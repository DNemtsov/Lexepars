using Lexepars.TestFixtures;
using System.Collections.Generic;
using Xunit;

namespace Lexepars.Tests
{
    public class NameValuePairParserTests : Grammar
    {
        [Fact]
        public void ParsesNameValuePairs()
        {
            var parser = NameValuePair(NvpLexer.Name.Lexeme(), NvpLexer.Delimiter.Kind(), NvpLexer.Value.Lexeme());

            parser.Parses(Tokenize("A=B")).AtEndOfInput();

            parser.FailsToParse(Tokenize("AA=B")).WithMessage("(1, 2): = expected");
            parser.FailsToParse(Tokenize("A==B")).WithMessage("(1, 3): B expected");
            parser.FailsToParse(Tokenize("=B")).WithMessage("(1, 1): A expected");
            parser.FailsToParse(Tokenize("A=")).WithMessage("(1, 3): B expected");
        }

        private class NvpLexer : Lexer
        {
            public NvpLexer()
                : base(Name, Delimiter, Value)
            {
            }

            public static readonly MatchableTokenKind Name = new OperatorTokenKind("A");
            public static readonly MatchableTokenKind Delimiter = new OperatorTokenKind("=");
            public static readonly MatchableTokenKind Value = new OperatorTokenKind("B");
        }

        IEnumerable<Token> Tokenize(string text)
            => new NvpLexer().Tokenize(text);
    }
}
