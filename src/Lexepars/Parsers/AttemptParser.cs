using System;

namespace Lexepars.Parsers
{
    /// <summary>
    /// Behaves like `p, except that it pretends that it hasn't consumed any input when `p fails.
    /// <remarks>This combinator is used whenever arbitrary look ahead is needed.</remarks>
    /// </summary>
    /// <typeparam name="TValue">The type of the parsed value.</typeparam>
    public class AttemptParser<TValue> : Parser<TValue>
    {
        /// <summary>
        /// Creates a new instance of <see cref="AttemptParser{TValue}"/>.
        /// </summary>
        /// <param name="parser">The `p parser. Not null.</param>
        public AttemptParser(IParser<TValue> parser)
        {
            _parser = parser ?? throw new ArgumentNullException(nameof(parser));
        }

        private readonly IParser<TValue> _parser;

        /// <summary>
        /// Builds the parser expression.
        /// </summary>
        /// <returns>Expression string. Not null.</returns>
        protected override string BuildExpression() => $"<ATTEMPT {_parser.Expression}>";

        /// <summary>
        /// Parses the stream of tokens.
        /// </summary>
        /// <param name="tokens">Stream of tokens to parse. Not null.</param>
        /// <returns>Parsing reply. Not null.</returns>
        public override IReply<TValue> Parse(TokenStream tokens)
        {
            var start = tokens.Position;
            var reply = _parser.Parse(tokens);
            var newPosition = reply.UnparsedTokens.Position;

            if (reply.Success || start == newPosition)
                return reply;

            return new Failure<TValue>(tokens, FailureMessage.Backtrack(newPosition, reply.FailureMessages));
        }

        /// <summary>
        /// Parsing optimized for the case when the reply value is not needed. NOTE: Result continuation will not be called.
        /// </summary>
        /// <param name="tokens">The token stream to parse. Not null.</param>
        /// <returns>General parsing reply. Not null.</returns>
        public override IGeneralReply ParseGenerally(TokenStream tokens)
        {
            var start = tokens.Position;
            var reply = _parser.ParseGenerally(tokens);
            var newPosition = reply.UnparsedTokens.Position;

            if (reply.Success || start == newPosition)
                return reply;

            return new GeneralFailure(tokens, FailureMessage.Backtrack(newPosition, reply.FailureMessages));
        }
    }
}