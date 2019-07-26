namespace Lexepars.Parsers
{
    public class FailingParser<T> : Parser<T>
    {
        /// <summary>
        /// Parsing optimized for the case when the reply value is not needed.
        /// </summary>
        /// <param name="tokens">Stream of tokens to parse. Not null.</param>
        /// <returns>Parsing reply. Not null.</returns>
        public override IReply<T> Parse(TokenStream tokens) => new Failure<T>(tokens, FailureMessage.Unknown());

        /// <summary>
        /// Parsing optimized for the case when the reply value is not needed.
        /// </summary>
        /// <param name="tokens">Tokens to parse</param>
        public override IGeneralReply ParseGenerally(TokenStream tokens) => new GeneralFailure(tokens, FailureMessage.Unknown());

        /// <summary>
        /// Builds the parser expression.
        /// </summary>
        /// <returns>Expression string. Not null.</returns>
        protected override string BuildExpression() => "<FAIL>";
    }
}