namespace Lexepars.Tests
{
    using Lexepars.TestFixtures;
    using Shouldly;
    using System.Collections.Generic;
    using Xunit;

    public class LexerTests
    {
        readonly MatchableTokenKind lower = new PatternTokenKind("Lowercase", @"[a-z]+");
        readonly MatchableTokenKind upper = new PatternTokenKind("Uppercase", @"[A-Z]+");
        readonly MatchableTokenKind space = new PatternTokenKind("Space", @"\s");

        IEnumerable<Token> Tokenize(string input)
        {
            return new Lexer(lower, upper, LexerBase.Skip(space)).Tokenize(input);
        }

        [Fact]
        public void ProvidesEmptyEnumerableForEmptyText()
        {
            Tokenize("").ShouldBeEmpty();
        }

        [Fact]
        public void UsesPrioritizedTokenMatchersToTokenize()
        {
            Tokenize("ABCdefGHI").ShouldBe(new[] { new Token(upper, 1, 1, "ABC"), new Token(lower, 1, 4, "def"), new Token(upper, 1, 7, "GHI") });
        }

        [Fact]
        public void ProvidesTokenAtUnrecognizedInput()
        {
            Tokenize("ABC!def").ShouldBe(new[] { new Token(upper, 1, 1, "ABC"), new Token(TokenKind.Unknown, 1, 4, "!def") });
        }

        [Fact]
        public void SkipsPastSkippableTokens()
        {
            Tokenize(" ").ShouldBeEmpty();

            Tokenize(" ABC  def   GHI    jkl  ").ShouldBe(new[] { new Token(upper, 1, 2, "ABC"), new Token(lower, 1, 7, "def"), new Token(upper, 1, 13, "GHI"), new Token(lower, 1, 20, "jkl") });
        }
    }
}