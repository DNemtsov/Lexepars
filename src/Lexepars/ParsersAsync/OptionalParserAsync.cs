using System;
using System.Threading;
using System.Threading.Tasks;
using Lexepars.Parsers;

namespace Lexepars.ParsersAsync
{
    /// <summary>
    /// Behaves like <see cref="OptionalParser{TValue}"/>, except could be used with parallel parsers.
    /// </summary>
    /// <typeparam name="TValue">The type of the parsed value.</typeparam>
    public class OptionalParserAsync<TValue> : AsyncParser<TValue>
    {
        /// <summary>
        /// Creates a new instance of <see cref="OptionalParserAsync{TValue}"/>.
        /// </summary>
        /// <param name="parser">The `p parser. Not null.</param>
        /// <param name="defaultValue">The default value that is returned in case `p fails.</param>
        public OptionalParserAsync(IAsyncParser<TValue> parser, TValue defaultValue = default(TValue))
        {
            _parser = parser ?? throw new ArgumentNullException(nameof(parser));
            _defaultValue = defaultValue;
        }

        /// <inheritdoc/>
        public async override Task<IReply<TValue>> ParseAsync(TokenStream tokens, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var oldPosition = tokens.Position;
            var reply = await _parser.ParseAsync(tokens, cancellationToken);
            var newPosition = reply.UnparsedTokens.Position;

            if (reply.Success)
                return reply;

            if (oldPosition == newPosition)
                return new Success<TValue>(_defaultValue, reply.UnparsedTokens);

            return reply;
        }

        /// <inheritdoc/>
        public async override Task<IGeneralReply> ParseGenerallyAsync(TokenStream tokens, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var oldPosition = tokens.Position;
            var reply = await _parser.ParseGenerallyAsync(tokens, cancellationToken);
            var newPosition = reply.UnparsedTokens.Position;

            if (reply.Success || oldPosition == newPosition)
                return new GeneralSuccess(reply.UnparsedTokens);

            return reply;
        }

        /// <inheritdoc/>
        protected override string BuildExpression() => $"<? {_parser.Expression} ?? {_defaultValue}>";
        
        private readonly IAsyncParser<TValue> _parser;
        private readonly TValue _defaultValue;
    }
}
