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
            Tokenize("ABCdefGHI")
                .ShouldList(t => t.ShouldBe(upper, "ABC", 1, 1),
                            t => t.ShouldBe(lower, "def", 1, 4),
                            t => t.ShouldBe(upper, "GHI", 1, 7));
        }

        [Fact]
        public void ProvidesTokenAtUnrecognizedInput()
        {
            Tokenize("ABC!def")
                .ShouldList(t => t.ShouldBe(upper, "ABC", 1, 1),
                            t => t.ShouldBe(TokenKind.Unknown, "!def", 1, 4));
        }

        [Fact]
        public void SkipsPastSkippableTokens()
        {
            Tokenize(" ").ShouldBeEmpty();

            Tokenize(" ABC  def   GHI    jkl  ")
                .ShouldList(t => t.ShouldBe(upper, "ABC", 1, 2),
                            t => t.ShouldBe(lower, "def", 1, 7),
                            t => t.ShouldBe(upper, "GHI", 1, 13),
                            t => t.ShouldBe(lower, "jkl", 1, 20));
        }
    }
}