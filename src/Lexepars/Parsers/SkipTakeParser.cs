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

        /// <inheritdoc/>
        public override IReply<TValue> Parse(TokenStream tokens)
        {
            var skip = _skip.ParseGenerally(tokens);

            if (!skip.Success)
                return Failure<TValue>.From(skip);

            return _take.Parse(skip.UnparsedTokens);
        }

        /// <inheritdoc/>
        public override IGeneralReply ParseGenerally(TokenStream tokens)
        {
            var skip = _skip.ParseGenerally(tokens);

            if (!skip.Success)
                return skip;

            return _take.ParseGenerally(skip.UnparsedTokens);
        }

        /// <inheritdoc/>
        protected override string BuildExpression() => $"<TAKE {_take.Expression} SKIP {_skip.Expression}>";

        private readonly IParser<TValue> _take;
        private readonly IGeneralParser _skip;
    }
}
