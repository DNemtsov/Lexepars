using Lexepars.Parsers;
using Lexepars.TestFixtures;
using Xunit;

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
    }
}