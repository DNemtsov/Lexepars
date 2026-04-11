using System;
using System.Threading;
using System.Threading.Tasks;
using Lexepars.Parsers;

namespace Lexepars.Async_Parsers
{
    /// <summary>
    /// Behaves like <see cref="AttemptParser{TValue}"/>, except could be used with parallel parsers.
    /// <remarks>This combinator is used whenever arbitrary look ahead is needed.</remarks>
    /// </summary>
    /// <typeparam name="TValue">The type of the parsed value.</typeparam>
    public class AttemptParserAsync<TValue> : AsyncParser<TValue>
    {
        /// <summary>
        /// Creates a new instance of <see cref="AttemptParserAsync{TValue}"/>.
        /// </summary>
        /// <param name="parser">The `p asynhcronous parser. Not null.</param>
        public AttemptParserAsync(IAsyncParser<TValue> parser)
        {
            _parser = parser ?? throw new ArgumentNullException(nameof(parser));
        }

        private readonly IAsyncParser<TValue> _parser;

        /// <inheritdoc/>
        protected override string BuildExpression() => $"<ATTEMPT {_parser.Expression}>";

        /// <inheritdoc/>
        public async override Task<IReply<TValue>> ParseAsync(TokenStream tokens, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var start = tokens.Position;
            var reply = await _parser.ParseAsync(tokens, cancellationToken);
            var newPosition = reply.UnparsedTokens.Position;

            if (reply.Success || start == newPosition)
                return reply;

            return new Failure<TValue>(tokens, FailureMessage.Backtrack(newPosition, reply.FailureMessages));
        }
    }
}