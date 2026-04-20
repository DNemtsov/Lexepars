using Lexepars.ParsersAsync;
using Lexepars.TestFixtures;
using Lexepars.Tests.Fixtures;
using Newtonsoft.Json.Linq;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Sdk;

namespace Lexepars.Tests
{
    public class SkipTakeTests
    {
        private static readonly Lexer lexer = SampleUnorderedParser.GetLexerAsync();

        [Fact]
        public async Task SkipSuccess()
        {
            var cts = new CancellationTokenSource();
            var parser = new SkipParserAsync(SampleUnorderedParser.GetUnorderedParserAsync());
            
            var stream = new TokenStream(Tokenize("abcdabcd"));
            var oldPosition = stream.Position;

            var generalReply = (await parser.ParseGenerallyAsync(stream, cts.Token));

            if (!generalReply.Success)
                throw new AssertionException("Parser failed", "Parsing failure");

            if(oldPosition == generalReply.UnparsedTokens.Position)
                throw new AssertionException("Parser failed", "Failed to skip");
        }

        [Fact]

        public async Task SkipFail()
        {
            var cts = new CancellationTokenSource();
            var parser = new SkipParserAsync(SampleUnorderedParser.GetUnorderedParserAsync());

            var stream = new TokenStream(Tokenize("zzzzabcd"));
            var oldPosition = stream.Position;

            var generalReply = (await parser.ParseGenerallyAsync(stream, cts.Token));

            if (generalReply.Success)
                throw new AssertionException("Parser success", "Parser succeeded when should have failed");
        }

        [Fact]
        public async Task SkipTakeSuccess()
        {
            var parser = new SkipTakeParserAsync<string[]>(SampleUnorderedParser.GetUnorderedParserAsync(), SampleUnorderedParser.GetAlternativeUnorderedParserAsync());

            var reply = await parser.Parses(Tokenize("abcdaaaa"));
            reply.ParsedValue.ShouldBe(new string[] { "a", "a", "a", "a" });
        }

        [Fact]
        public async Task TakeSkipSuccess()
        {
            var cts = new CancellationTokenSource();
            var parser = new TakeSkipParserAsync<string[]>(SampleUnorderedParser.GetUnorderedParserAsync(), SampleUnorderedParser.GetAlternativeUnorderedParserAsync());

            var stream = new TokenStream(Tokenize("abcdaaaa"));
            var reply = await parser.ParseAsync(stream, cts.Token);
            if(!reply.Success)
                throw new AssertionException("Parser failed", "Parsing failure");
            reply.ParsedValue.ShouldBe(new string[] { "a", "b", "c", "d" });
            reply.AtEndOfInput();
        }

        [Fact]
        public async Task TakeSkipParsingFailure()
        {
            var cts = new CancellationTokenSource();
            var parser = new TakeSkipParserAsync<string[]>(SampleUnorderedParser.GetUnorderedParserAsync(), SampleUnorderedParser.GetAlternativeUnorderedParserAsync());

            var stream = new TokenStream(Tokenize("zzzzaaaa"));
            var reply = await parser.ParseAsync(stream, cts.Token);
            if (reply.Success)
                throw new AssertionException("Parser success", "Parser succeeded when should have failed");
        }

        [Fact]
        public async Task SkipTakeFailsToParse()
        {
            var parser = new SkipTakeParserAsync<string[]>(SampleUnorderedParser.GetUnorderedParserAsync(), SampleUnorderedParser.GetAlternativeUnorderedParserAsync());

            await parser.FailsToParseAsync(Tokenize("abcdaaab"));
        }

        [Fact]
        public async Task SkipTakeFailsToSkip()
        {
            var parser = new SkipTakeParserAsync<string[]>(SampleUnorderedParser.GetUnorderedParserAsync(), SampleUnorderedParser.GetAlternativeUnorderedParserAsync());

            await parser.FailsToParseAsync(Tokenize("abcaaaaa"));
        }

        private static IEnumerable<Token> Tokenize(string text) => lexer.Tokenize(text);
    }
}
