using System;

namespace Lexepars.Parsers
{
    /// <summary>
    /// Takes the value parsed by `take and then skips over the general `skip.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    public class TakeSkipParser<TValue> : Parser<TValue>
    {
        /// <summary>
        /// Creates a new instance of <see cref="TakeSkipParser{TValue}"/>.
        /// </summary>
        /// <param name="take">The `take. Not null.</param>
        /// <param name="skip">The `skip. Not null.</param>
        public TakeSkipParser(IParser<TValue> take, IGeneralParser skip)
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
            var take = _take.Parse(tokens);

            if (!take.Success)
                return Failure<TValue>.From(take);

            var skip = _skip.ParseGenerally(take.UnparsedTokens);

            if (!skip.Success)
                return Failure<TValue>.From(skip);

            return new Success<TValue>(take.ParsedValue, skip.UnparsedTokens, skip.FailureMessages);
        }

        /// <summary>
        /// Parsing optimized for the case when the reply value is not needed.
        /// </summary>
        /// <param name="tokens">Stream of tokens to parse. Not null.</param>
        /// <returns>Parsing reply. Not null.</returns>
        public override IGeneralReply ParseGenerally(TokenStream tokens)
        {
            var take = _take.ParseGenerally(tokens);

            if (!take.Success)
                return take;

            return _skip.ParseGenerally(take.UnparsedTokens);
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
