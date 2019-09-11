namespace Lexepars.OffsideRule
{
    public sealed class NeutralIndent : Indent
    {
        public NeutralIndent(MatchableTokenKind token)
            : base(token, ScopePerToken.None)
        {
        }

        public override int CalculateNewIndentLevel(int oldIndentLevel, Token token) => oldIndentLevel;
    }
}
