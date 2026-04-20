using Lexepars.Parsers;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Lexepars.ParsersAsync
{
    /// <summary>
    /// Behaves like <see cref="BetweenParser{TValue}"/>, except could be used with parallel parsers.
    /// </summary>
    /// <typeparam name="TValue">The type of the parsed value.</typeparam>
    public class BetweenParserAsync<TValue> : AsyncParser<TValue>
    {
        /// <summary>
        /// Creates a new instance of <see cref="BetweenParserAsync{TValue}"/>.
        /// </summary>
        /// <param name="left">General parser of the left part. Not null.</param>
        /// <param name="item">Item parser. Not null.</param>
        /// <param name="right">General parser of the right part. Not null.</param>
        public BetweenParserAsync(IAsyncGeneralParser left, IAsyncParser<TValue> item, IAsyncGeneralParser right)
        {
            _left = left ?? throw new ArgumentNullException(nameof(left));
            _item = item ?? throw new ArgumentNullException(nameof(item));
            _right = right ?? throw new ArgumentNullException(nameof(right));
        }

        /// <inheritdoc/>
        public async override Task<IReply<TValue>> ParseAsync(TokenStream tokens, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var left = await _left.ParseGenerallyAsync(tokens, cancellationToken);

            if (!left.Success)
                return Failure<TValue>.From(left);

            var item = await _item.ParseAsync(left.UnparsedTokens, cancellationToken);

            if (!item.Success)
                return item;

            var right = await _right.ParseGenerallyAsync(item.UnparsedTokens, cancellationToken);

            if (!right.Success)
                return Failure<TValue>.From(right);

            return new Success<TValue>(item.ParsedValue, right.UnparsedTokens, right.FailureMessages);
        }

        /// <inheritdoc/>
        public override async Task<IGeneralReply> ParseGenerallyAsync(TokenStream tokens, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var left = await _left.ParseGenerallyAsync(tokens, cancellationToken);

            if (!left.Success)
                return left;

            var item = await _item.ParseGenerallyAsync(left.UnparsedTokens, cancellationToken);

            if (!item.Success)
                return item;

            return await _right.ParseGenerallyAsync(item.UnparsedTokens, cancellationToken);
        }

        /// <inheritdoc/>
        protected override string BuildExpression() => $"<({_left.Expression}|{_item.Expression}|{_right.Expression})>";

        private readonly IAsyncGeneralParser _left;
        private readonly IAsyncParser<TValue> _item;
        private readonly IAsyncGeneralParser _right;
    }
}
