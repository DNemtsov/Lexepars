namespace Lexepars.Parsers
{
    public class MonadicUnitParser<T> : Parser<T>
    {
        public MonadicUnitParser(T value)
        {
            _value = value;
        }

        public override IReply<T> Parse(TokenStream tokens) => new Success<T>(_value, tokens);

        /// <summary>
        /// Parsing optimized for the case when the reply value is not needed.
        /// </summary>
        /// <param name="tokens">Tokens to parse</param>
        public override IGeneralReply ParseGenerally(TokenStream tokens) => new GeneralSuccess(tokens);

        private readonly T _value;

        protected override string BuildExpression() => $"<= {_value}>";
    }
}
