namespace Lexepars.Tests
{
    using Shouldly;
    using Xunit;

    public class FailureMessageTests
    {
        [Fact]
        public void CanIndicateGenericErrors()
        {
            var failure = FailureMessage.Unknown();
            failure.ToString().ShouldBe("Parsing failed.");
        }

        [Fact]
        public void CanIndicateSpecificExpectation()
        {
            var failure = (ExpectationFailureMessage)FailureMessage.Expected("statement");
            failure.Expectation.ShouldBe("statement");
            failure.ToString().ShouldBe("statement expected");
        }

        [Fact]
        public void CanIndicateErrorsWhichCausedBacktracking()
        {
            var position = new Position(3, 4);
            FailureMessages failures = FailureMessages.Empty
                .With(FailureMessage.Expected("a"))
                .With(FailureMessage.Expected("b"));

            var failure = (BacktrackFailureMessage) FailureMessage.Backtrack(position, failures);
            failure.Position.ShouldBe(position);
            failure.Failures.ShouldBe(failures);
            failure.ToString().ShouldBe("(3, 4): a or b expected");
        }
    }
}