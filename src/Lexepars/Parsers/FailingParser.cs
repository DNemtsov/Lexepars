namespace Lexepars.Parsers
{
    /// <summary>
    /// Always returns a failure result. May be used to specifically prohibit some structures from occurring in the input. Can be equipped with a custom failure message.
    /// </summary>
    /// <typeparam name="TValue">The type of the parsed value.</typeparam>
    public class FailingParser<TValue> : Parser<TValue>
    {
        private readonly FailureMessage _message;

        /// <summary>
        /// Creates a new instance of <see cref="FailingParser{TValue}"/>.
        /// </summary>
        /// <param name="message">The failure message to appear on the result.</param>
        public FailingParser(FailureMessage message = null)
        {
            _message = message ?? FailureMessage.Unknown();
        }

        ///<inheritdoc/>
        public override IReply<TValue> Parse(TokenStream tokens) => new Failure<TValue>(tokens, _message);

        ///<inheritdoc/>
        public override IGeneralReply ParseGenerally(TokenStream tokens) => new GeneralFailure(tokens, _message);

        ///<inheritdoc/>
        protected override string BuildExpression() => "<FAIL>";
    }
}