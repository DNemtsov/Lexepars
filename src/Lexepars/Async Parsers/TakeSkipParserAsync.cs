using Lexepars.Parsers;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Lexepars.Async_Parsers
{
    /// <summary>
    /// Behaves like <see cref="TakeSkipParser{TValue}"/>, except could be used with parallel parsers.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    public class TakeSkipParserAsync<TValue> : AsyncParser<TValue>
    {
        /// <summary>
        /// Creates a new instance of <see cref="TakeSkipParser{TValue}"/>.
        /// </summary>
        /// <param name="take">The `take. Not null.</param>
        /// <param name="skip">The `skip. Not null.</param>
        public TakeSkipParserAsync(IAsyncParser<TValue> take, IAsyncGeneralParser skip)
        {
            _take = take ?? throw new ArgumentNullException(nameof(take));
            _skip = skip ?? throw new ArgumentNullException(nameof(skip));
        }

        /// <inheritdoc/>
        public async override Task<IReply<TValue>> ParseAsync(TokenStream tokens, CancellationToken cancellationToken)
        {
            var take = await _take.ParseAsync(tokens, cancellationToken);

            if (!take.Success)
                return Failure<TValue>.From(take);

            var skip = await _skip.ParseGenerallyAsync(take.UnparsedTokens, cancellationToken);

            if (!skip.Success)
                return Failure<TValue>.From(skip);

            return new Success<TValue>(take.ParsedValue, skip.UnparsedTokens, skip.FailureMessages);
        }

        /// <inheritdoc/>
        public async override Task<IGeneralReply> ParseGenerallyAsync(TokenStream tokens, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var take = await _take.ParseGenerallyAsync(tokens, cancellationToken);

            if (!take.Success)
                return take;

            return await _skip.ParseGenerallyAsync(take.UnparsedTokens, cancellationToken);
        }

        /// <summary>
        /// Builds the parser expression.
        /// </summary>
        /// <returns>Expression string. Not null.</returns>
        protected override string BuildExpression() => $"<TAKE {_take.Expression} SKIP {_skip.Expression}>";

        private readonly IAsyncParser<TValue> _take;
        private readonly IAsyncGeneralParser _skip;
    }
}
