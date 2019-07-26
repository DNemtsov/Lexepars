namespace Lexepars.Tests
{
    using Lexepars.TestFixtures;
    using Lexepars.Tests.Fixtures;
    using Shouldly;
    using System;
    using System.Text.RegularExpressions;
    using Xunit;

    public class TokenKindTests
    {
        readonly MatchableTokenKind lower = new PatternTokenKind("Lowercase", @"[a-z]+");
        readonly MatchableTokenKind upper = new PatternTokenKind("Uppercase", @"[A-Z]+");
        readonly MatchableTokenKind caseInsensitive = new PatternTokenKind("Case Insensitive", @"[a-z]+", RegexOptions.IgnoreCase);
        readonly TextTestFixture abcDEF = new TextTestFixture("abcDEF");

        [Fact]
        public void ProducesNullTokenUponFailedMatch()
        {
            upper.TryMatch((InputText)abcDEF, out Token token).ShouldBeFalse();
            token.ShouldBeNull();
        }

        [Fact]
        public void ProducesTokenUponSuccessfulMatch()
        {
            lower.TryMatch((InputText)abcDEF, out Token token).ShouldBeTrue();
            token.ShouldBe(lower, "abc", 1, 1);

            upper.TryMatch(abcDEF.Advance(3), out token).ShouldBeTrue();
            token.ShouldBe(upper, "DEF", 1, 4);

            caseInsensitive.TryMatch((InputText)abcDEF, out token).ShouldBeTrue();
            token.ShouldBe(caseInsensitive, "abcDEF", 1, 1);
        }

        [Fact]
        public void HasDescriptiveName()
        {
            lower.Name.ShouldBe("Lowercase");
            upper.Name.ShouldBe("Uppercase");
            caseInsensitive.Name.ShouldBe("Case Insensitive");
        }

        [Fact]
        public void UsesDescriptiveNameForToString()
        {
            lower.ToString().ShouldBe("Lowercase");
            upper.ToString().ShouldBe("Uppercase");
            caseInsensitive.ToString().ShouldBe("Case Insensitive");
        }

        [Fact]
        public void ProvidesConvenienceSubclassForDefiningKeywords()
        {
            var foo = new KeywordTokenKind("foo");

            foo.Name.ShouldBe("foo");

            foo.TryMatch(new InputText("bar"), out Token token).ShouldBeFalse();
            token.ShouldBeNull();

            foo.TryMatch(new InputText("foo"), out token).ShouldBeTrue();
            token.ShouldBe(foo, "foo", 1, 1);

            foo.TryMatch(new InputText("foo bar"), out token).ShouldBeTrue();
            token.ShouldBe(foo, "foo", 1, 1);

            foo.TryMatch(new InputText("foobar"), out token).ShouldBeFalse();
            token.ShouldBeNull();

            Func<TokenKind> notJustLetters = () => new KeywordTokenKind(" oops ");

            notJustLetters.ShouldThrow<ArgumentException>("Keywords may only contain letters.\r\nParameter name: keyword");
        }

        [Fact]
        public void ProvidesConvenienceSubclassForDefiningOperators()
        {
            var star = new OperatorTokenKind("*");
            var doubleStar = new OperatorTokenKind("**");

            star.Name.ShouldBe("*");

            star.TryMatch(new InputText("a"), out Token token).ShouldBeFalse();
            token.ShouldBeNull();

            star.TryMatch(new InputText("*"), out token).ShouldBeTrue();
            token.ShouldBe(star, "*", 1, 1);

            star.TryMatch(new InputText("* *"), out token).ShouldBeTrue();
            token.ShouldBe(star, "*", 1, 1);

            star.TryMatch(new InputText("**"), out token).ShouldBeTrue();
            token.ShouldBe(star, "*", 1, 1);

            doubleStar.Name.ShouldBe("**");

            doubleStar.TryMatch(new InputText("a"), out token).ShouldBeFalse();
            token.ShouldBeNull();

            doubleStar.TryMatch(new InputText("*"), out token).ShouldBeFalse();
            token.ShouldBeNull();

            doubleStar.TryMatch(new InputText("* *"), out token).ShouldBeFalse();
            token.ShouldBeNull();

            doubleStar.TryMatch(new InputText("**"), out token).ShouldBeTrue();
            token.ShouldBe(doubleStar, "**", 1, 1);
        }

        [Fact]
        public void ProvidesConvenienceSubclassForTokensThatDoNotMatchLexemesFromTheInput()
        {
            TokenKind.EndOfInput.ShouldBeOfType<EndOfInputTokenKind>();

            TokenKind.EndOfInput.Name.ShouldBe("end of input");
        }
    }
}