namespace Lexepars.Tests
{
    using System;
    using Shouldly;
    using Xunit;

    public class FailureTests
    {
        readonly TokenStream x;
        readonly TokenStream endOfInput;

        public FailureTests()
        {
            var lexer = new Lexer(new OperatorTokenKind(":("));
            x = new TokenStream(lexer.Tokenize("x"));
            endOfInput = new TokenStream(lexer.Tokenize(""));
        }

        [Fact]
        public void CanIndicateErrorsAtTheCurrentPosition()
        {
            new Failure<object>(endOfInput, FailureMessage.Unknown()).FailureMessages.ToString().ShouldBe("Parsing failed.");
            new Failure<object>(endOfInput, FailureMessage.Expected("statement")).FailureMessages.ToString().ShouldBe("statement expected");
        }

        [Fact]
        public void CanIndicateMultipleErrorsAtTheCurrentPosition()
        {
            var errors = FailureMessages.Empty
                .With(FailureMessage.Expected("A"))
                .With(FailureMessage.Expected("B"));

            new Failure<object>(endOfInput, errors).FailureMessages.ToString().ShouldBe("A or B expected");
        }

        [Fact]
        public void ThrowsWhenAttemptingToGetParsedValue()
        {
            Func<object> inspectParsedValue = () => new Failure<object>(x, FailureMessage.Unknown()).ParsedValue;
            inspectParsedValue.ShouldThrow<NotSupportedException>("(1, 1): Parsing failed.");
        }

        [Fact]
        public void HasRemainingUnparsedTokens()
        {
            new Failure<object>(x, FailureMessage.Unknown()).UnparsedTokens.ShouldBe(x);
            new Failure<object>(endOfInput, FailureMessage.Unknown()).UnparsedTokens.ShouldBe(endOfInput);
        }

        [Fact]
        public void ReportsErrorState()
        {
            new Failure<object>(x, FailureMessage.Unknown()).Success.ShouldBeFalse();
        }
    }
}