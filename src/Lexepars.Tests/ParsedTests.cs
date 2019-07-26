namespace Lexepars.Tests
{
    using Shouldly;
    using Xunit;
    
    public class ParsedTests
    {
        private readonly TokenStream _unparsed = new TokenStream(new CharLexer().Tokenize("0"));

        [Fact]
        public void HasAParsedValue()
        {
            new Success<string>("parsed", _unparsed).ParsedValue.ShouldBe("parsed");
        }

        [Fact]
        public void HasNoErrorMessageByDefault()
        {
            new Success<string>("x", _unparsed).FailureMessages.ShouldBe(FailureMessages.Empty);
        }

        [Fact]
        public void CanIndicatePotentialErrors()
        {
            var potentialErrors = FailureMessages.Empty
                .With(FailureMessage.Expected("A"))
                .With(FailureMessage.Expected("B"));

            new Success<object>("x", _unparsed, potentialErrors).FailureMessages.ShouldBe(potentialErrors);
        }

        [Fact]
        public void HasRemainingUnparsedTokens()
        {
            new Success<string>("parsed", _unparsed).UnparsedTokens.ShouldBe(_unparsed);
        }

        [Fact]
        public void ReportsNonerrorState()
        {
            new Success<string>("parsed", _unparsed).Success.ShouldBeTrue();
        }
    }
}