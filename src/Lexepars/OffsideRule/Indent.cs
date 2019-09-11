using System;

namespace Lexepars.OffsideRule
{
    public abstract class Indent
    {
        public MatchableTokenKind Token { get; }

        public ScopePerToken ScopePerToken { get; }

        public static Indent LexemeLength(MatchableTokenKind token, ScopePerToken scopeEmitType = ScopePerToken.None) => new LexemeLengthIndent(token, scopeEmitType);

        public static Indent Predefined(MatchableTokenKind token, int predefinedValue, ScopePerToken scopeEmitType = ScopePerToken.None) => new PredefinedIndent(token, scopeEmitType, predefinedValue);

        public static Indent Neutral(MatchableTokenKind token) => new NeutralIndent(token);
        
        public abstract int CalculateNewIndentLevel(int oldIndentLevel, Token token);

        protected Indent(MatchableTokenKind token, ScopePerToken scopeEmitType)
        {
            Token = token ?? throw new ArgumentNullException(nameof(token));
            ScopePerToken = scopeEmitType;
        }
    }
}
