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

        /// <inheritdoc/>
        protected override string BuildExpression() => $"<ATTEMPT {_parser.Expression}>";

        /// <inheritdoc/>
        public override IReply<TValue> Parse(TokenStream tokens)
        {
            var start = tokens.Position;
            var reply = _parser.Parse(tokens);
            var newPosition = reply.UnparsedTokens.Position;

            if (reply.Success || start == newPosition)
                return reply;

            return new Failure<TValue>(tokens, FailureMessage.Backtrack(newPosition, reply.FailureMessages));
        }

        /// <inheritdoc/>
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