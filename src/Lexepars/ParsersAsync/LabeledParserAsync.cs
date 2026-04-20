using Lexepars.Parsers;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Lexepars.ParsersAsync
{
    /// <summary>
    /// Behaves like <see cref="LabeledParser{TValue}"/>, except could be used with parallel parsers.
    /// </summary>
    /// <typeparam name="TValue">The type of the parsed value.</typeparam>
    public class LabeledParserAsync<TValue> : AsyncParser<TValue>
    {
        /// <summary>
        /// Creates a new instance of <see cref="LabeledParserAsync{TValue}"/>.
        /// </summary>
        /// <param name="parser">The `p parser. Not null.</param>
        /// <param name="expectation">Expectation message. Not null.</param>
        public LabeledParserAsync(IAsyncParser<TValue> parser, string expectation)
        {
            _parser = parser ?? throw new ArgumentNullException(nameof(parser));
            _failures = FailureMessages.Empty.With(FailureMessage.Expected(expectation));
        }

        /// <inheritdoc/>
        public async override Task<IReply<TValue>> ParseAsync(TokenStream tokens, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var oldPosition = tokens.Position;
            var reply = await _parser.ParseAsync(tokens, cancellationToken);
            var newPosition = reply.UnparsedTokens.Position;

            if (oldPosition != newPosition)
                return reply;

            if (reply.Success)
                return new Success<TValue>(reply.ParsedValue, reply.UnparsedTokens, _failures);
                
            return new Failure<TValue>(reply.UnparsedTokens, _failures);
        }

        /// <inheritdoc/>
        public async override Task<IGeneralReply> ParseGenerallyAsync(TokenStream tokens, CancellationToken cancellationToken) => await _parser.ParseGenerallyAsync(tokens, cancellationToken);

        /// <inheritdoc/>
        protected override string BuildExpression() => $"<LABEL {_parser.Expression} WITH {_failures}";

        private readonly IAsyncParser<TValue> _parser;
        private readonly FailureMessages _failures;
    }
}
