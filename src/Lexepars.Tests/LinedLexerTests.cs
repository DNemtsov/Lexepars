using Lexepars.TestFixtures;
using Shouldly;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace Lexepars.Tests
{
    public class LinedLexerTests
    {
        readonly MatchableTokenKind _lower = new PatternTokenKind("Lowercase", @"[a-z]+");
        readonly MatchableTokenKind _upper = new PatternTokenKind("Uppercase", @"[A-Z]+");

        public LinedLexerTests()
        {
            var space = new PatternTokenKind("space", @"\s");

            _lexer = new LinedLexer(_lower, _upper, LexerBase.Skip(space));
        }

        private readonly LinedLexer _lexer;

        IEnumerable<Token> Tokenize(string input)
        {
            return _lexer.Tokenize(new StringReader(input));
        }

        [Fact]
        public void ProvidesEmptyEnumerableForEmptyText()
        {
            Tokenize("").ShouldBeEmpty();
        }

        [Fact]
        public void UsesPrioritizedTokenMatchersToTokenize()
        {
            Tokenize("ABCdef\nGHI").ShouldBe(new[] { new Token(_upper, 1, 1, "ABC"), new Token(_lower, 1, 4, "def"), new Token(_upper, 2, 1, "GHI") });
        }

        [Fact]
        public void ProvidesTokenAtUnrecognizedInput()
        {
            Tokenize("ABC!def").ShouldBe(new[] { new Token(_upper, 1, 1, "ABC"), new Token(TokenKind.Unknown, 1, 4, "!def") });
        }

        [Fact]
        public void ProvidesTokenAtUnrecognizedMultilineInput()
        {
            Tokenize("ABC\r\n!def\rvfs\r").ShouldBe(new[] { new Token(_upper, 1, 1, "ABC"), new Token(TokenKind.Unknown, 2, 1, "!def\r") });
        }

        [Fact]
        public void DoesNotReadPastUnrecognizedTokenLine()
        {
            var oneMoreLine = "one more line";
            var input = "ABC\n!def\n" + oneMoreLine;

            using (var reader = new StringReader(input))
            {
                var tokens = _lexer.Tokenize(reader);

                tokens.ShouldBe(new[] { new Token(_upper, 1, 1, "ABC"), new Token(TokenKind.Unknown, 2, 1, "!def\n") });

                reader.ReadLine().ShouldBe(oneMoreLine);
            }
        }

        [Fact]
        public void SkipsPastSkippableTokens()
        {
            Tokenize(" ").ShouldBeEmpty();

            Tokenize(" ABC  def   GHI    jkl  ")
                .ShouldBe(new[] { new Token(_upper, 1, 2, "ABC"), new Token(_lower, 1, 7, "def"), new Token(_upper, 1, 13, "GHI"), new Token(_lower, 1, 20, "jkl") });
        }

        [Fact]
        public void TokenizesMultilineText()
        {
            Tokenize(" ").ShouldBeEmpty();

            Tokenize(@" ABC
                        def
                         GHI
                          jkl  ")
                .ShouldBe(new[] { new Token(_upper, 1, 2, "ABC"), new Token(_lower, 2, 25, "def"), new Token(_upper, 3, 26, "GHI"), new Token(_lower, 4, 27, "jkl") });
        }
    }
}