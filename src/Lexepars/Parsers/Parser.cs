namespace Lexepars.Parsers
{
    /// <summary>
    /// Base class for a typical general parser. Has to be inherited.
    /// </summary>
    public abstract class Parser : IGeneralParser
    {
        /// <summary>
        /// Parsing optimized for the case when the reply value is not needed.
        /// </summary>
        /// <param name="tokens">Tokens to parse.</param>
        public abstract IGeneralReply ParseGenerally(TokenStream tokens);

        public override string ToString() => Expression;

        protected abstract string BuildExpression();

        public string Expression
        {
            get
            {
                if (_nameRecursionGuard)
                    return "<~>";

                _nameRecursionGuard = true;

                var name = BuildExpression();

                _nameRecursionGuard = false;

                return name;
            }
        }

        private bool _nameRecursionGuard;
    }

    /// <summary>
    /// Base class for a typical parser. Has to be inherited.
    /// </summary>
    public abstract class Parser<TValue> : Parser, IParser<TValue>
    {
        /// <summary>
        /// Parses the stream of tokens.
        /// </summary>
        /// <param name="tokens">Stream of tokens to parse. Not null.</param>
        /// <returns>Parsing reply. Not null.</returns>
        public abstract IReply<TValue> Parse(TokenStream tokens);

        /// <summary>
        /// Parsing optimized for the case when the reply value is not needed.
        /// </summary>
        /// <param name="tokens">The token stream to parse. Not null.</param>
        /// <returns>General parsing reply. Not null.</returns>
        public override IGeneralReply ParseGenerally(TokenStream tokens) => Parse(tokens);
    }
}
