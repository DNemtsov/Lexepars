using Lexepars.Parsers;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Lexepars.ParsersAsync
{
    /// <summary>
    /// Behaves like <see cref="SkipTakeParser{TValue}"/>, except could be used with parallel parsers.
    /// </summary>
    /// <typeparam name="TValue">The type of the parsed value.</typeparam>
    public class SkipTakeParserAsync<TValue> : AsyncParser<TValue>
    {
        /// <summary>
        /// Creates a new instance of <see cref="SkipTakeParserAsync{TValue}"/>.
        /// </summary>
        /// <param name="skip">The `skip. Not null.</param>
        /// <param name="take">The `take. Not null.</param>
        public SkipTakeParserAsync(IAsyncGeneralParser skip, IAsyncParser<TValue> take)
        {
            _take = take ?? throw new ArgumentNullException(nameof(take));
            _skip = skip ?? throw new ArgumentNullException(nameof(skip));
        }

        /// <inheritdoc/>
        public async override Task<IReply<TValue>> ParseAsync(TokenStream tokens, CancellationToken cancellationToken)
        {
            var skip = await _skip.ParseGenerallyAsync(tokens, cancellationToken);

            if (!skip.Success)
                return Failure<TValue>.From(skip);

            return await _take.ParseAsync(skip.UnparsedTokens, cancellationToken);
        }

        /// <inheritdoc/>
        public async override Task<IGeneralReply> ParseGenerallyAsync(TokenStream tokens, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var skip = await _skip.ParseGenerallyAsync(tokens, cancellationToken);

            if (!skip.Success)
                return skip;

            return await _take.ParseGenerallyAsync(skip.UnparsedTokens, cancellationToken);
        }

        /// <inheritdoc/>
        protected override string BuildExpression() => $"<TAKE {_take.Expression} SKIP {_skip.Expression}>";

        private readonly IAsyncParser<TValue> _take;
        private readonly IAsyncGeneralParser _skip;
    }
}
