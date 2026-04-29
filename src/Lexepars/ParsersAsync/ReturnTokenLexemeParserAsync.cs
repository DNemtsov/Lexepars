using System;
using System.Threading;
using System.Threading.Tasks;

namespace Lexepars.ParsersAsync
{
    /// <summary>
    /// Behaves like <see cref="ReturnTokenLexemeParserAsync"/>, except could be used with parallel parsers.
    /// </summary>
    public class ReturnTokenLexemeParserAsync : AsyncParser<string>
    {
        /// <summary>
        /// Creates a new instance of <see cref="ReturnTokenLexemeParserAsync"/>.
        /// </summary>
        /// <param name="kind">The kind of token. Not null.</param>
        public ReturnTokenLexemeParserAsync(TokenKind kind)
        {
            _kind = kind ?? throw new ArgumentNullException(nameof(kind));
        }

        /// <inheritdoc/>
        public override Task<IReply<string>> ParseAsync(TokenStream tokens, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var currentToken = tokens.Current;

            if (currentToken.Kind != _kind)
                return Task.FromResult<IReply<string>>(new Failure<string>(tokens, FailureMessage.Expected(_kind.Name)));

            return Task.FromResult<IReply<string>>(new Success<string>(currentToken.Lexeme, tokens.Advance()));
        }

        /// <inheritdoc/>
        public override Task<IGeneralReply> ParseGenerallyAsync(TokenStream tokens, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (tokens.Current.Kind != _kind)
                return Task.FromResult<IGeneralReply>(new GeneralFailure(tokens, FailureMessage.Expected(_kind.Name)));

            return Task.FromResult<IGeneralReply>(new GeneralSuccess(tokens.Advance()));
        }

        /// <inheritdoc/>
        protected override string BuildExpression() => $"<'{_kind}'>";

        private readonly TokenKind _kind;
    }
}
