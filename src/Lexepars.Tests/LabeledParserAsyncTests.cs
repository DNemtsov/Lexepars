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

namespace Lexepars.Tests
{
    public class LabeledParserAsyncTests
    {
        private static readonly Lexer lexer = SampleUnorderedParser.GetLexerAsync();

        [Fact]
        public async Task LabeledSuccess()
        {
            var parser = new LabeledParserAsync<string[]>(SampleUnorderedParser.GetUnorderedParserAsync(), "Test expectation");
            await parser.Parses(Tokenize("abcd"));
        }

        [Fact]
        public async Task LabeledFailWithNoInputConsumed()
        {
            var expectation = "Test expectation";
            var parser = new LabeledParserAsync<string[]>(SampleUnorderedParser.GetAlternativeUnorderedParserAsync(), expectation);
            var reply = await parser.FailsToParseAsync(Tokenize("bbbb"));
            reply.FailureMessages.ToString().ShouldBe("Test expectation expected");
        }

        [Fact]
        public async Task LabeledFailWithInputConsumed()
        {
            var expectation = "Test expectation";
            var parser = new LabeledParserAsync<string[]>(SampleUnorderedParser.GetUnorderedParserAsync(), expectation);
            var reply = await parser.FailsToParseAsync(Tokenize("aaaa"));
            reply.FailureMessages.ToString().ShouldBe("b, c or d expected");
        }

        private static IEnumerable<Token> Tokenize(string text) => lexer.Tokenize(text);
    }
}
