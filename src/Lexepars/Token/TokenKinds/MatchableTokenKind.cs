using System;

namespace Lexepars
{
    /// <summary>
    /// The kind of token that is matched against the input text. Should be inherited.
    /// </summary>
    /// <remarks>Tokens of this kind should always have non-empty lexemes so that the input text parsing can advance.</remarks>
    public abstract class MatchableTokenKind : TokenKind
    {
        /// <summary>
        /// Creates a new instance of <see cref="MatchableTokenKind"/>
        /// </summary>
        /// <param name="name">The display name of the token kind.</param>
        /// <param name="skippable">If true, the lexer won't be emitting the tokens of this kind when they are matched.</param>
        protected MatchableTokenKind(string name)
            : base(name)
        { }

        /// <summary>
        /// Tries to match the token kind against the input text.
        /// </summary>
        /// <param name="text">The input text. Not null.</param>
        /// <param name="token">The token of this kind if matching succeeds. Null otherwise.</param>
        /// <returns></returns>
        public bool TryMatch(IInputText text, out Token token)
        {
            var match = Match(text);

            if (match.Success)
            {
                var matchValue = match.Value;

                if (string.IsNullOrEmpty(matchValue))
                    throw new InvalidOperationException("A successful match should always yield a non-empty lexeme.");

                token = new Token(this, text.Position, matchValue);
                return true;
            }

            token = null;
            return false;
        }

        /// <summary>
        /// The actual matching against the input text.
        /// </summary>
        /// <returns>The matching result. Not null. A successful match should always yield a non-empty lexeme.</returns>
        protected abstract MatchResult Match(IInputText text);

        public static implicit operator MatchableTokenKindSpec(MatchableTokenKind tokenKind)
            => new MatchableTokenKindSpec(tokenKind, false);

        public static MatchableTokenKindSpec ToMatchableTokenKindSpec(MatchableTokenKind tokenKind)
            => tokenKind;
    }
}
