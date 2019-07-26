using System;

namespace Lexepars.Parsers
{
    /// <summary>
    /// Parses the token of the desired kind and returns its lexeme.
    /// </summary>
    public class ReturnTokenLexemeParser : Parser<string>
    {
        /// <summary>
        /// Creates a new instance of <see cref="ReturnTokenLexemeParser"/>.
        /// </summary>
        /// <param name="kind">The kind of token. Not null.</param>
        public ReturnTokenLexemeParser(TokenKind kind)
        {
            _kind = kind ?? throw new ArgumentNullException(nameof(kind));
        }

        public override IReply<string> Parse(TokenStream tokens)
        {
            var currentToken = tokens.Current;

            if (currentToken.Kind != _kind)
                return new Failure<string>(tokens, FailureMessage.Expected(_kind.Name));

            return new Success<string>(currentToken.Lexeme, tokens.Advance());
        }

        /// <summary>
        /// Parsing optimized for the case when the reply value is not needed. NOTE: Result continuation will not be called.
        /// </summary>
        /// <param name="tokens">Tokens to parse</param>
        public override IGeneralReply ParseGenerally(TokenStream tokens)
        {
            if (tokens.Current.Kind != _kind)
                return new GeneralFailure(tokens, FailureMessage.Expected(_kind.Name));

            return new GeneralSuccess(tokens.Advance());
        }

        /// <summary>
        /// Builds the parser expression.
        /// </summary>
        /// <returns>Expression string. Not null.</returns>
        protected override string BuildExpression() => $"<'{_kind}'>";

        private readonly TokenKind _kind;
    }
}
