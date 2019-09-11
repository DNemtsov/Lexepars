namespace Lexepars.OffsideRule
{
    public sealed class LexemeLengthIndent : Indent
    {
        public LexemeLengthIndent(MatchableTokenKind token, ScopePerToken scopeEmitType)
            : base(token, scopeEmitType)
        {
        }

        public override int CalculateNewIndentLevel(int oldIndentLevel, Token token) => oldIndentLevel + token.Lexeme?.Length ?? 0;
    }
}
