namespace Lexepars.OffsideRule
{
    public class OffsideRuleSpecialTokenConfig : SpecialTokenConfig
    {
        public ScopeTokenKind ScopeBegin { get; }

        public ScopeTokenKind ScopeEnd { get; }

        public ScopeTokenKind ScopeInconsistent { get; }

        public OffsideRuleSpecialTokenConfig(NullLexemeTokenKind unknown, ScopeTokenKind scopeBegin, ScopeTokenKind scopeEnd, ScopeTokenKind scopeInconsistent)
            : base(unknown)
        {
            ScopeBegin = scopeBegin;
            ScopeEnd = scopeEnd;
            ScopeInconsistent = scopeInconsistent;
        }
    }
}
