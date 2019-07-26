using System;

namespace Lexepars.Parsers
{
    /// <summary>
    /// Skips over the general `skip and takes the value parsed by `take.
    /// </summary>
    /// <typeparam name="TValue">The type of the parsed value.</typeparam>
    public class SkipTakeParser<TValue> : Parser<TValue>
    {
        /// <summary>
        /// Creates a new instance of <see cref="SkipTakeParser{TValue}"/>.
        /// </summary>
        /// <param name="skip">The `skip. Not null.</param>
        /// <param name="take">The `take. Not null.</param>
        public SkipTakeParser(IGeneralParser skip, IParser<TValue> take)
        {
            _take = take ?? throw new ArgumentNullException(nameof(take));
            _skip = skip ?? throw new ArgumentNullException(nameof(skip));
        }

        /// <summary>
        /// Parses the stream of tokens.
        /// </summary>
        /// <param name="tokens">Stream of tokens to parse. Not null.</param>
        /// <returns>Parsing reply. Not null.</returns>
        public override IReply<TValue> Parse(TokenStream tokens)
        {
            var skip = _skip.ParseGenerally(tokens);

            if (!skip.Success)
                return Failure<TValue>.From(skip);

            return _take.Parse(skip.UnparsedTokens);
        }

        /// <summary>
        /// Parsing optimized for the case when the reply value is not needed.
        /// </summary>
        /// <param name="tokens">Stream of tokens to parse. Not null.</param>
        /// <returns>Parsing reply. Not null.</returns>
        public override IGeneralReply ParseGenerally(TokenStream tokens)
        {
            var skip = _skip.ParseGenerally(tokens);

            if (!skip.Success)
                return skip;

            return _take.ParseGenerally(skip.UnparsedTokens);
        }

        /// <summary>
        /// Builds the parser expression.
        /// </summary>
        /// <returns>Expression string. Not null.</returns>
        protected override string BuildExpression() => $"<TAKE {_take.Expression} SKIP {_skip.Expression}>";

        private readonly IParser<TValue> _take;
        private readonly IGeneralParser _skip;
    }
}
