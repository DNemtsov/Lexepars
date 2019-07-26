namespace Lexepars
{
    public abstract class FailureMessage
    {
        public static FailureMessage Unknown()
            => new UnknownFailureMessage();

        public static FailureMessage Expected(string expectation)
            => new ExpectationFailureMessage(expectation);

        public static FailureMessage Backtrack(Position position, FailureMessages errors)
            => new BacktrackFailureMessage(position, errors);
    }

    public class UnknownFailureMessage : FailureMessage
    {
        internal UnknownFailureMessage() { }

        public override string ToString()
            => "Parsing failed.";
    }

    /// <summary>
    /// Parsers report this when a specific expectation was not met at the current position.
    /// </summary>
    public class ExpectationFailureMessage : FailureMessage
    {
        internal ExpectationFailureMessage(string expectation)
        {
            Expectation = expectation;
        }

        public string Expectation { get; }

        public override string ToString()
            => Expectation + " expected";
    }

    /// <summary>
    /// Parsers report this when they have backtracked after a failure occurred.
    /// The Position property describes the position where the original failure
    /// occurred.
    /// </summary>
    public class BacktrackFailureMessage : FailureMessage
    {
        internal BacktrackFailureMessage(Position position, FailureMessages failures)
        {
            Position = position;
            Failures = failures;
        }

        public Position Position { get; }
        public FailureMessages Failures { get; }

        public override string ToString() => $"{Position}: {Failures}";
    }
}