using System;

namespace Lexepars.Parsers
{
    /// <summary>
    /// General parser which succeeds if the current token belongs to the specified kind.
    /// </summary>
    public class TokenByKindParser : Parser
    {
        private readonly TokenKind _kind;

        /// <summary>
        /// Creates a new instance of <see cref="TokenByKindParser"/>.
        /// </summary>
        /// <param name="kind">The kind of token. Not null.</param>
        public TokenByKindParser(TokenKind kind)
            => _kind = kind ?? throw new ArgumentNullException(nameof(kind));

        /// <inheritdoc/>
        public override IGeneralReply ParseGenerally(TokenStream tokens)
        {
            if (tokens.Current.Kind == _kind)
                return new GeneralSuccess(tokens.Advance());

            return new GeneralFailure(tokens, FailureMessage.Expected(_kind.Name));
        }

        /// <inheritdoc/>
        protected override string BuildExpression() => $"<*{_kind}*>";
    }
}