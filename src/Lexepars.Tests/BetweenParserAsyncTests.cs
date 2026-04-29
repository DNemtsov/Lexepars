using Lexepars.ParsersAsync;
using Lexepars.Parsers;
using Lexepars.TestFixtures;
using Lexepars.Tests.Fixtures;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Lexepars.Tests
{
    public class BetweenParserAsyncTests
    {

        private static readonly Lexer lexer = SampleUnorderedParser.GetLexerAsync();

        // Success
        // Left failure
        // Middle Failure
        // Right failure

        [Fact]
        public async Task BetweenParserSuccess()
        {
            var cts = new CancellationTokenSource();
            var parser = new BetweenParserAsync<string[]>(
                SampleUnorderedParser.GetUnorderedParserAsync(),
                SampleUnorderedParser.GetAlternativeUnorderedParserAsync(),
                SampleUnorderedParser.GetThirdUnorderedParserAsync());
            var reply = await parser.Parses(Tokenize("abcdaaaabbbb"));
            reply.ParsedValue.ShouldBe(new string[] { "a", "a", "a", "a" });
        }

        [Fact]
        public async Task BetweenParserFailures()
        {
            var cts = new CancellationTokenSource();
            var parser = new BetweenParserAsync<string[]>(
                SampleUnorderedParser.GetUnorderedParserAsync(),
                SampleUnorderedParser.GetAlternativeUnorderedParserAsync(),
                SampleUnorderedParser.GetThirdUnorderedParserAsync());
            await parser.FailsToParseAsync(Tokenize("aaaaaaaabbbb"));
            await parser.FailsToParseAsync(Tokenize("abcdabcdbbbb"));
            await parser.FailsToParseAsync(Tokenize("abcdaaaaabcd"));
        }

        // Cancelled test
        [Fact]
        public async Task CancelledOptionParser()
        {
            var cancellationTokenSource = new CancellationTokenSource();
            var cts = cancellationTokenSource.Token;
            cancellationTokenSource.Cancel();

            var parser = new BetweenParserAsync<string[]>(
                SampleUnorderedParser.GetUnorderedParserAsync(), 
                SampleUnorderedParser.GetAlternativeUnorderedParserAsync(), 
                SampleUnorderedParser.GetThirdUnorderedParserAsync());
            var r = await Assert.ThrowsAsync<OperationCanceledException>(() => parser.ParseAsync(new TokenStream(Tokenize("b")), cts));

            var r2 = await Assert.ThrowsAsync<OperationCanceledException>(() => parser.ParseGenerallyAsync(new TokenStream(Tokenize("b")), cts));
        }

        private static IEnumerable<Token> Tokenize(string text) => lexer.Tokenize(text);
    }
}
