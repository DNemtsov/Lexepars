using System;

namespace Lexepars
{
    /// <summary>
    /// Represents a language letter-only keyword separated by word boundaries.
    /// </summary>
    /// <remarks>Non-skippable.</remarks>
    public class KeywordTokenKind : PatternTokenKind
    {
        /// <summary>
        /// Creates a new instance of <see cref="KeywordTokenKind"/>
        /// </summary>
        /// <param name="keyword">The letter-only keyword. Not empty.</param>
        public KeywordTokenKind(string keyword)
            : base(keyword, keyword + @"\b")
        {
            if (keyword.Any(ch => !char.IsLetter(ch)))
                throw new ArgumentException("Keywords may only contain letters.", nameof(keyword));
        }
    }
}
