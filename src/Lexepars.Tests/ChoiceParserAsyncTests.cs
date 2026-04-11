using Lexepars.Async_Parsers;
using Lexepars.Parsers;
using Lexepars.TestFixtures;
using Newtonsoft.Json.Linq;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Lexepars.Tests
{
    public class ChoiceParserAsyncTests
    {

        private static readonly MatchableTokenKind aToken = new OperatorTokenKind("a");
        private static readonly MatchableTokenKind bToken = new OperatorTokenKind("b");
        private static readonly MatchableTokenKind cToken = new OperatorTokenKind("c");
        private static readonly MatchableTokenKind dToken = new OperatorTokenKind("d");
        private static readonly MatchableTokenKind separatorToken = new OperatorTokenKind(",");

        private static readonly Lexer lexer = new Lexer(aToken, bToken, cToken, dToken);

        private static readonly IAsyncGeneralParser separator = new TokenByKindParserAsync(separatorToken);

        private static readonly IAsyncParser<string> a = aToken.LexemeAsync();
        private static readonly IAsyncParser<string> b = bToken.LexemeAsync();
        private static readonly IAsyncParser<string> c = cToken.LexemeAsync();
        private static readonly IAsyncParser<string> d = dToken.LexemeAsync();
        
        [Fact]
        public async Task AllFail()
        {
            var cts = new CancellationTokenSource();
            var parser1 = new UnorderedParserAsync<string>(UnorderedParsingMode.FullSet, a, b, c);
            var parser2 = new UnorderedParserAsync<string>(UnorderedParsingMode.FullSet, a, d, c);
            var parser3 = new UnorderedParserAsync<string>(UnorderedParsingMode.FullSet, b, c, d);
            var parser4 = new UnorderedParserAsync<string>(UnorderedParsingMode.FullSet, b, a, c);
            var parser = new ChoiceParserAsync<string[]>(parser1, parser2, parser3, parser4);
            await parser.FailsToParseAsync(Tokenize("eee"));
        }

        [Fact]
        public async Task ConsumeAndFail()
        {
            var cts = new CancellationTokenSource();
            var parser1 = new UnorderedParserAsync<string>(UnorderedParsingMode.FullSet, a, b, c);
            var parser2 = new UnorderedParserAsync<string>(UnorderedParsingMode.FullSet, a, d, c);
            var parser3 = new UnorderedParserAsync<string>(UnorderedParsingMode.FullSet, b, c, d);
            var parser4 = new UnorderedParserAsync<string>(UnorderedParsingMode.FullSet, b, a, c);
            var parser = new ChoiceParserAsync<string[]>(parser1, parser2, parser3, parser4);
            await parser.FailsToParseAsync(Tokenize("bcd"));
        }

        [Fact]
        public async Task FirstSuccess()
        {
            var cts = new CancellationTokenSource();
            var parser1 = new UnorderedParserAsync<string>(UnorderedParsingMode.FullSet, a, b, c);
            var parser2 = new UnorderedParserAsync<string>(UnorderedParsingMode.FullSet, a, d, c);
            var parser3 = new UnorderedParserAsync<string>(UnorderedParsingMode.FullSet, b, c, d);
            var parser4 = new UnorderedParserAsync<string>(UnorderedParsingMode.FullSet, b, a, c);
            var parser = new ChoiceParserAsync<string[]>(parser1, parser2, parser3, parser4);
            await parser.Parses(Tokenize("abc"));
        }

        [Fact]
        public async Task NotFirstSuccess()
        {
            var cts = new CancellationTokenSource();
            var parser1 = new UnorderedParserAsync<string>(UnorderedParsingMode.FullSet, a, a, a);
            var parser2 = new UnorderedParserAsync<string>(UnorderedParsingMode.FullSet, a, a, a);
            var parser3 = new UnorderedParserAsync<string>(UnorderedParsingMode.FullSet, b, c, d);
            var parser4 = new UnorderedParserAsync<string>(UnorderedParsingMode.FullSet, b, a, c);
            var parser = new ChoiceParserAsync<string[]>(parser1, parser2, parser3, parser4);
            await parser.Parses(Tokenize("bcd"));
        }

        [Fact]
        public async Task MultipleSuccesses()
        {
            var cts = new CancellationTokenSource();
            var parser1 = new UnorderedParserAsync<string>(UnorderedParsingMode.FullSet, a, a, a);
            var parser2 = new UnorderedParserAsync<string>(UnorderedParsingMode.FullSet, c, d, b);
            var parser3 = new UnorderedParserAsync<string>(UnorderedParsingMode.FullSet, b, c, d);
            var parser4 = new UnorderedParserAsync<string>(UnorderedParsingMode.FullSet, b, a, c);
            var parser = new ChoiceParserAsync<string[]>(parser1, parser2, parser3, parser4);
            (await parser.Parses(Tokenize("bcd")))
                .ParsedValue
                .ShouldBe(new[] { "c", "d", "b" }); ;
        }

        [Fact]
        public async Task CancelQuantifiedParserAsync()
        {
            var cts = new CancellationTokenSource();
            cts.Cancel();
            var parser1 = new UnorderedParserAsync<string>(UnorderedParsingMode.FullSet, a, a, a);
            var parser2 = new UnorderedParserAsync<string>(UnorderedParsingMode.FullSet, c, d, b);
            var parser3 = new UnorderedParserAsync<string>(UnorderedParsingMode.FullSet, b, c, d);
            var parser4 = new UnorderedParserAsync<string>(UnorderedParsingMode.FullSet, b, a, c);
            var parser = new ChoiceParserAsync<string[]>(parser1, parser2, parser3, parser4);
            var r = await Assert.ThrowsAsync<OperationCanceledException>(() => parser.ParseAsync(new TokenStream(Tokenize("aaaa")), cts.Token));
        }

        private static IEnumerable<Token> Tokenize(string text) => lexer.Tokenize(text);
    }
}