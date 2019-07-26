namespace Lexepars
{
    using System.Collections.Generic;
    using System.Linq;

    public class FailureMessages
    {
        public static readonly FailureMessages Empty = new FailureMessages();

        private readonly FailureMessage head;
        private readonly FailureMessages tail;

        private FailureMessages()
        {
            head = null;
            tail = null;
        }

        private FailureMessages(FailureMessage head, FailureMessages tail)
        {
            this.head = head;
            this.tail = tail;
        }

        public FailureMessages With(FailureMessage errorMessage)
        {
            return new FailureMessages(errorMessage, this);
        }

        public FailureMessages Merge(FailureMessages failure)
        {
            var result = this;

            foreach (var f in failure.All<FailureMessage>())
                result = result.With(f);

            return result;
        }

        public override string ToString()
        {
            var failedExpectations = new List<string>(All<ExpectationFailureMessage>()
                                              .Select(f => f.Expectation)
                                              .Distinct()
                                              .OrderBy(expectation => expectation));

            var backtrackFailures = All<BacktrackFailureMessage>().ToArray();

            if (!failedExpectations.Any() && !backtrackFailures.Any())
            {
                var unknownFailure = All<UnknownFailureMessage>().FirstOrDefault();
                if (unknownFailure != null)
                    return unknownFailure.ToString();

                return string.Empty;
            }

            var parts = new List<string>();

            if (failedExpectations.Any())
            {
                var suffixes = Separators(failedExpectations.Count - 1).Concat(new[] { " expected" });

                parts.Add(string.Join("", failedExpectations.Zip(suffixes, (expectation, suffix) => expectation + suffix)));
            }

            if (backtrackFailures.Any())
                parts.Add(string.Join(" ", backtrackFailures.Select(backtrack => $"[{backtrack}]")));

            return string.Join(" ", parts);
        }

        private static IEnumerable<string> Separators(int count)
        {
            if (count <= 0)
                return Enumerable.Empty<string>();
            return Enumerable.Repeat(", ", count - 1).Concat(new[] { " or " });
        }

        private IEnumerable<T> All<T>() where T : FailureMessage
        {
            if (this == Empty)
                yield break;

            if (head is T)
                yield return (T)head;

            foreach (T message in tail.All<T>())
                yield return message;
        }
    }
}