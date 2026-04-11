using Lexepars.Parsers;
using Lexepars.Async_Parsers;
using Lexepars.TestFixtures;
using Xunit;
using System.Threading.Tasks;

namespace Lexepars.Tests
{
    public class LambdaParserTests
    {
        [Fact]
        public void CreatesParsersFromLambdas()
        {
            var succeeds = new LambdaParser<string>(tokens => new Success<string>("AA", tokens.Advance().Advance()));
            succeeds.PartiallyParses(new CharLexer().Tokenize("AABB")).LeavingUnparsedTokens("B", "B").WithValue("AA");

            var fails = new LambdaParser<string>(tokens => new Failure<string>(tokens, FailureMessage.Unknown()));
            fails.FailsToParse(new CharLexer().Tokenize("AABB")).LeavingUnparsedTokens("A", "A", "B", "B").WithMessage("(1, 1): Parsing failed.");
        }

        [Fact]
        public async Task CreatesParsersFromLambdasAsync()
        {
            var succeeds = new LambdaParserAsync<string>(tokens => Task.FromResult<IReply<string>>(new Success<string>("AA", tokens.Advance().Advance())));
            var result = await succeeds.PartiallyParsesAsync(new CharLexer().Tokenize("AABB"));
            result.LeavingUnparsedTokens("B", "B").WithValue("AA");

            var fails = new LambdaParserAsync<string>(tokens => Task.FromResult<IReply<string>>(new Failure<string>(tokens, FailureMessage.Unknown())));
            result = await fails.FailsToParseAsync(new CharLexer().Tokenize("AABB"));
            result.LeavingUnparsedTokens("A", "A", "B", "B").WithMessage("(1, 1): Parsing failed.");
        }
    }
}