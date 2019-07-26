using System;

namespace Lexepars
{
    /// <summary>
    /// Token kind specification used to initialize lexers.
    /// </summary>
    public class MatchableTokenKindSpec
    {
        /// <summary>
        /// The token kind. Not null.
        /// </summary>
        public MatchableTokenKind TokenKind { get; }

        /// <summary>
        /// If true, the lexer won't be emitting the tokens of this kind when they are matched.
        /// </summary>
        public bool Skipped { get; }

        public MatchableTokenKindSpec(MatchableTokenKind tokenKind, bool skipped)
        {
            TokenKind = tokenKind ?? throw new ArgumentNullException(nameof(tokenKind));
            Skipped = skipped;
        }
    }
}
