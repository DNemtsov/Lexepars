using Lexepars.Async_Parsers;
using Lexepars.Parsers;
using Microsoft.VisualBasic;

namespace Lexepars.Tests.Fixtures
{
    /// <summary>
    /// Returns <see cref="UnorderedParserAsync<string>"/> to be used in tests.
    /// </summary>
    public class SampleUnorderedParser
    {
        private static readonly MatchableTokenKind aToken = new OperatorTokenKind("a");
        private static readonly MatchableTokenKind bToken = new OperatorTokenKind("b");
        private static readonly MatchableTokenKind cToken = new OperatorTokenKind("c");
        private static readonly MatchableTokenKind dToken = new OperatorTokenKind("d");

        private static readonly MatchableTokenKind separatorToken = new OperatorTokenKind(",");

        private static readonly Lexer lexer = new Lexer(separatorToken, aToken, bToken, cToken, dToken);

        private static readonly IAsyncParser<string> a = aToken.LexemeAsync();
        private static readonly IAsyncParser<string> b = bToken.LexemeAsync();
        private static readonly IAsyncParser<string> c = cToken.LexemeAsync();
        private static readonly IAsyncParser<string> d = dToken.LexemeAsync();
        private static readonly IAsyncGeneralParser separator = new TokenByKindParserAsync(separatorToken);

        private static readonly UnorderedParserAsync<string> parser = new UnorderedParserAsync<string>(UnorderedParsingMode.FullSet, a, b, c, d);

        private static readonly UnorderedParserAsync<string> alternativeParser = new UnorderedParserAsync<string>(UnorderedParsingMode.FullSet, a, a, a, a);
        private static readonly UnorderedParserAsync<string> thirdParser = new UnorderedParserAsync<string>(UnorderedParsingMode.FullSet, b, b, b, b);
        public static IAsyncParser<string[]> GetUnorderedParserAsync()
        {
            return parser;
        }

        public static IAsyncParser<string[]> GetAlternativeUnorderedParserAsync()
        {
            return alternativeParser;
        }

        public static IAsyncParser<string[]> GetThirdUnorderedParserAsync()
        {
            return thirdParser;
        }

        public static Lexer GetLexerAsync()
        {
            return lexer;
        }
    }
}
