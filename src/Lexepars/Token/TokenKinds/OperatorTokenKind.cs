using System;

namespace Lexepars.Token
{
    /// <summary>
    /// Represents a language operator that is matched directly against the input text.
    /// </summary>
    public class OperatorTokenKind : MatchableTokenKind
    {
        private readonly string _symbol;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="symbol">The operator string. Not empty.</param>
        /// <param name="skippable">If true, the lexer won't be emitting the tokens of this kind when they are matched.</param>
        public OperatorTokenKind(string symbol)
            : base(symbol)
        {
            _symbol = symbol ?? throw new ArgumentNullException(nameof(symbol));

            if (symbol.Length < 1)
                throw new ArgumentException("Should not be empty.", nameof(symbol));
        }

        /// <summary>
        /// The actual matching method of the token kind against the input text.
        /// </summary>
        protected override sealed MatchResult Match(IInputText text)
        {
            var peek = text.Peek(_symbol.Length);

            if (peek == _symbol)
                return MatchResult.Succeed(peek);

            return MatchResult.Fail;
        }
    }
}
