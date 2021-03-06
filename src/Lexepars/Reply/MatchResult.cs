namespace Lexepars
{
    public class MatchResult
    {
        public static readonly MatchResult Fail = new MatchResult(false, string.Empty);

        public static MatchResult Succeed(string value)
        {
            return new MatchResult(true, value);
        }

        private MatchResult(bool success, string value)
        {
            Success = success;
            Value = value;
        }

        public bool Success { get; }
        public string Value { get; }
    }
}