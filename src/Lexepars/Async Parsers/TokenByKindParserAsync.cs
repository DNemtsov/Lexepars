using Lexepars.Parsers;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Lexepars.Async_Parsers
{
    /// <summary>
    /// Behaves like <see cref="TokenByKindParser"/>, except could be used with parallel parsers.
    /// </summary>
    public class TokenByKindParserAsync : AsyncParser
    {
        private readonly TokenKind _kind;

        /// <summary>
        /// Creates a new instance of <see cref="TokenByKindParserAsync"/>.
        /// </summary>
        /// <param name="kind">The kind of token. Not null.</param>
        public TokenByKindParserAsync(TokenKind kind)
            => _kind = kind ?? throw new ArgumentNullException(nameof(kind));

        /// <inheritdoc/>
        public override Task<IGeneralReply> ParseGenerallyAsync(TokenStream tokens, CancellationToken cancellationToken)
        {
            if (tokens.Current.Kind == _kind)
                return Task.FromResult<IGeneralReply>(new GeneralSuccess(tokens.Advance()));

            return Task.FromResult<IGeneralReply>(new GeneralFailure(tokens, FailureMessage.Expected(_kind.Name)));
        }

        /// <inheritdoc/>
        protected override string BuildExpression() => $"<*{_kind}*>";
    }
}