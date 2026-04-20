using Lexepars.ParsersAsync;
using Lexepars.Parsers;
using Lexepars.TestFixtures;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Xunit;

namespace Lexepars.Tests
{
    public class AttemptParserTests
    {
        private static readonly MatchableTokenKind aToken = new OperatorTokenKind("a");
        private static readonly MatchableTokenKind bToken = new OperatorTokenKind("b");

        private static readonly Lexer lexer = new Lexer(aToken, bToken);

        [Fact]
        public async Task NotConsumingInputOnFail()
        {
            var cts = new CancellationTokenSource();
            var parser = new AttemptParserAsync<string>(new OperatorTokenKind("a").LexemeAsync());
            var tokens = new TokenStream(Tokenize("b"));
            var oldPosition = tokens.Position;
            var reply = await parser.ParseAsync(tokens, cts.Token);
            if(reply.Success)
                throw new AssertionException("parser failure", "parser completed successfully");
            if(reply.UnparsedTokens.Position != oldPosition)
                throw new AssertionException("parser failure", "input consumed");

            var gReply = await parser.ParseGenerallyAsync(tokens, cts.Token);
            if (gReply.Success)
                throw new AssertionException("parser failure", "parser completed successfully");
            if (gReply.UnparsedTokens.Position != oldPosition)
                throw new AssertionException("parser failure", "input consumed");
        }

        [Fact]
        public async Task AttemptParserSuccess()
        {
            var cts = new CancellationTokenSource();
            var parser = new AttemptParserAsync<string>(new LambdaParserAsync<string>(tokens => Task.FromResult<IReply<string>>(new Success<string>("AA", tokens.Advance().Advance()))));
            await parser.Parses(Tokenize("a"));
        }

        [Fact]
        public async Task CancelAttemptParser()
        {
            var cancellationTokenSource = new CancellationTokenSource();
            var cts = cancellationTokenSource.Token;
            cancellationTokenSource.Cancel();

            var parser = new AttemptParserAsync<string>(new OperatorTokenKind("a").LexemeAsync());
            var r = await Assert.ThrowsAsync<OperationCanceledException>(() => parser.ParseAsync(new TokenStream(Tokenize("b")), cts));

            var r2 = await Assert.ThrowsAsync<OperationCanceledException>(() => parser.ParseGenerallyAsync(new TokenStream(Tokenize("b")), cts));
        }
        private static IEnumerable<Token> Tokenize(string text) => lexer.Tokenize(text);
    }
}