using System;

namespace Lexepars.OffsideRule
{
    public class Indent
    {
        public MatchableTokenKind Token { get; }

        public int PredefinedValue { get; }

        public ScopePerToken ScopePerToken { get; }

        public static Indent LexemeLength(MatchableTokenKind token, ScopePerToken scopeEmitType = ScopePerToken.None)
        {
            return new Indent(token, scopeEmitType, 0);
        }

        public static Indent Predefined(MatchableTokenKind token, int predefinedValue, ScopePerToken scopeEmitType = ScopePerToken.None)
        {
            if (predefinedValue < 1)
                throw new ArgumentOutOfRangeException(nameof(predefinedValue), "Should not be less than 1.");

            return new Indent(token, scopeEmitType, predefinedValue);
        }

        public static Indent Ignore(MatchableTokenKind token)
        {
            return new Indent(token, ScopePerToken.None, -1);
        }

        private Indent(MatchableTokenKind token, ScopePerToken scopeEmitType, int predefinedValue)
        {
            Token = token ?? throw new ArgumentNullException(nameof(token));

            PredefinedValue = predefinedValue;

            ScopePerToken = scopeEmitType;
        }
    }
}
