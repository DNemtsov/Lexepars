namespace Lexepars.Parsers
{
    /// <summary>
    /// Base class for a typical general parser. Has to be inherited from.
    /// </summary>
    public abstract class Parser : IGeneralParser
    {
        /// <inheritdoc/>
        public abstract IGeneralReply ParseGenerally(TokenStream tokens);

        /// <summary>
        /// Returns the parser expression.
        /// </summary>
        /// <returns>Parser expression string. Not null.</returns>
        public override string ToString() => Expression;

        /// <summary>
        /// Builds the parser expression.
        /// </summary>
        /// <returns>Expression string. Not null.</returns>
        protected abstract string BuildExpression();

        /// <inheritdoc/>
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
    /// Base class for a typical parser. Has to be inherited from.
    /// </summary>
    public abstract class Parser<TValue> : Parser, IParser<TValue>
    {
        /// <inheritdoc/>
        public abstract IReply<TValue> Parse(TokenStream tokens);

        /// <inheritdoc/>
        public override IGeneralReply ParseGenerally(TokenStream tokens) => Parse(tokens);
    }
}
