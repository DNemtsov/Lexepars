namespace Lexepars
{
    using System.Text.RegularExpressions;

    public class TokenRegex
    {
        private readonly string _pattern;
        private readonly Regex _regex;

        public TokenRegex(string pattern, RegexOptions regexOptions = RegexOptions.None)
        {
            regexOptions |= RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace;

            _pattern = pattern;
            _regex = new Regex("\\G(\n" + pattern + "\n)", regexOptions);
        }

        public MatchResult Match(string input, int index)
        {
            var match = _regex.Match(input, index);

            if (match.Success)
                return MatchResult.Succeed(match.Value);

            return MatchResult.Fail;
        }

        public override string ToString()
        {
            return _pattern;
        }
    }
}