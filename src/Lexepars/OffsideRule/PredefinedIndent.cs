using System;

namespace Lexepars.OffsideRule
{
    public sealed class PredefinedIndent : Indent
    {
        private readonly int _indent;

        public PredefinedIndent(MatchableTokenKind token, ScopePerToken scopeEmitType, int indent)
            : base (token, scopeEmitType)
        {
            if (indent < 1)
                throw new ArgumentException("Predefined indeet value should not be less than 1.", nameof(indent));

            _indent = indent;
        }

        public override int CalculateNewIndentLevel(int oldIndentLevel, Token token) => oldIndentLevel + _indent;
    }
}
