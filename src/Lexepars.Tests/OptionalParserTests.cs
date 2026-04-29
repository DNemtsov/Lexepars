using Lexepars.ParsersAsync;
using Lexepars.TestFixtures;
using Lexepars.Tests.Fixtures;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Lexepars.Tests
{
    public class OptionalParserTests
    {
        private static readonly Lexer lexer = SampleUnorderedParser.GetLexerAsync();

        // Succeed on failure with no input consumed
        [Fact]
        public async Task SuccessOnFailWithNoInputConsumed()
        {
            var cts = new CancellationTokenSource();
            OptionalParserAsync<string[]> parser = new OptionalParserAsync<string[]>(SampleUnorderedParser.GetUnorderedParserAsync());
            await parser.PartiallyParsesAsync(Tokenize("zzzz"));
        }

        // Fail on failure with consuming input
        [Fact]
        public async Task OptionParserFailure()
        {
            var cts = new CancellationTokenSource();
            var parser = new OptionalParserAsync<string[]>(SampleUnorderedParser.GetUnorderedParserAsync());
            await parser.FailsToParseAsync(Tokenize("abX"));
        }

        // Succeed on success with consuming input
        [Fact]
        public async Task OptionParserSuccess()
        {
            var cts = new CancellationTokenSource();
            var parser = new OptionalParserAsync<string[]>(SampleUnorderedParser.GetUnorderedParserAsync());
            await parser.Parses(Tokenize("abcd"));
        }

        // Cancelled test
        [Fact]
        public async Task CancelledOptionParser()
        {
            var cancellationTokenSource = new CancellationTokenSource();
            var cts = cancellationTokenSource.Token;
            cancellationTokenSource.Cancel();

            var parser = new OptionalParserAsync<string[]>(SampleUnorderedParser.GetUnorderedParserAsync());
            var r = await Assert.ThrowsAsync<OperationCanceledException>(() => parser.ParseAsync(new TokenStream(Tokenize("b")), cts));

            var r2 = await Assert.ThrowsAsync<OperationCanceledException>(() => parser.ParseGenerallyAsync(new TokenStream(Tokenize("b")), cts));
        }
        private static IEnumerable<Token> Tokenize(string text) => lexer.Tokenize(text);
    }
}
