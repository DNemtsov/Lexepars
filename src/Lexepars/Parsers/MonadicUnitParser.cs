namespace Lexepars.Parsers
{
    /// <summary>
    /// Simply returns a successful result with the value provided. Useful for the monadic chaining situations.
    /// </summary>
    /// <typeparam name="TValue">The type of the parsed value.</typeparam>
    public class MonadicUnitParser<TValue> : Parser<TValue>
    {
        /// <summary>
        /// Creates a new instance of <see cref="MonadicUnitParser{TValue}"/>.
        /// </summary>
        /// <param name="value">Value to be returned. Can be null.</param>
        public MonadicUnitParser(TValue value)
        {
            _value = value;
        }

        /// <inheritdoc/>
        public override IReply<TValue> Parse(TokenStream tokens) => new Success<TValue>(_value, tokens);

        /// <inheritdoc/>
        public override IGeneralReply ParseGenerally(TokenStream tokens) => new GeneralSuccess(tokens);

        private readonly TValue _value;

        /// <inheritdoc/>
        protected override string BuildExpression() => $"<= {_value}>";
    }
}
