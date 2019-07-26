using System;

namespace Lexepars.Parsers
{
    /// <summary>
    /// Optional(`p) is equivalent to `p whenever `p succeeds or when `p fails after consuming input.
    /// If `p fails without consuming input, Optional(`p) succeeds with the default value.
    /// </summary>
    /// <typeparam name="TValue">The type of the parsed value.</typeparam>
    public class OptionalParser<TValue> : Parser<TValue>
    {
        /// <summary>
        /// Creates a new instance of <see cref="OptionalParser{TValue}"/>.
        /// </summary>
        /// <param name="parser">The `p parser. Not null.</param>
        /// <param name="defaultValue">The default value that is returned in case `p fails.</param>
        public OptionalParser(IParser<TValue> parser, TValue defaultValue = default(TValue))
        {
            _parser = parser ?? throw new ArgumentNullException(nameof(parser));
            _defaultValue = defaultValue;
        }

        /// <summary>
        /// Parses the stream of tokens.
        /// </summary>
        /// <param name="tokens">Stream of tokens to parse. Not null.</param>
        /// <returns>Parsing reply. Not null.</returns>
        public override IReply<TValue> Parse(TokenStream tokens)
        {
            var oldPosition = tokens.Position;
            var reply = _parser.Parse(tokens);
            var newPosition = reply.UnparsedTokens.Position;

            if (reply.Success)
                return reply;

            if (oldPosition == newPosition)
                return new Success<TValue>(_defaultValue, reply.UnparsedTokens);

            return reply;
        }

        /// <summary>
        /// Parsing optimized for the case when the reply value is not needed. NOTE: Result continuation will not be called.
        /// </summary>
        /// <param name="tokens">The token stream to parse. Not null.</param>
        /// <returns>General parsing reply. Not null.</returns>
        public override IGeneralReply ParseGenerally(TokenStream tokens)
        {
            var oldPosition = tokens.Position;
            var reply = _parser.ParseGenerally(tokens);
            var newPosition = reply.UnparsedTokens.Position;

            if (reply.Success || oldPosition == newPosition)
                return new GeneralSuccess(reply.UnparsedTokens);

            return reply;
        }

        /// <summary>
        /// Builds the parser expression.
        /// </summary>
        /// <returns>Expression string. Not null.</returns>
        protected override string BuildExpression() => $"<? {_parser.Expression} ?? {_defaultValue}>";
        
        private readonly IParser<TValue> _parser;
        private readonly TValue _defaultValue;
    }
}
