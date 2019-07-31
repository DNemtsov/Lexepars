using System.Text.RegularExpressions;

namespace Lexepars
{
    /// <summary>
    /// The regex-based token kind.
    /// </summary>
    public class PatternTokenKind : MatchableTokenKind
    {
        private readonly TokenRegex _regex;

        /// <summary>
        /// Creates a new instance of <see cref="PatternTokenKind"/>
        /// </summary>
        /// <param name="name">The display name of the token kind.</param>
        /// <param name="pattern">The regular expression to be matched against the input text. Not null. Not empty. The \G anchor is always applied to make sure the sequence of matches is contiguous.</param>
        /// <param name="skippable">If true, the lexer won't be emitting the tokens of this kind when they are matched.</param>
        /// <param name="regexOptions">The options of the regular expression. <see cref="RegexOptions.Multiline"/> and <see cref="RegexOptions.IgnorePatternWhitespace"/> are always applied.</param>
        public PatternTokenKind(string name, string pattern, RegexOptions regexOptions = RegexOptions.None)
            : base(name)
        {
            _regex = new TokenRegex(pattern, regexOptions);
        }

        /// <summary>
        /// The actual matching method of the token kind against the input text.
        /// </summary>
        protected override sealed MatchResult Match(IInputText text) => text.Match(_regex);
    }
}
