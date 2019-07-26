using Lexepars.Parsers;
using Lexepars.TestFixtures;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Lexepars.Tests
{
    public class ParserQueryTests
    {
        static readonly IParser<string> Next = new LambdaParser<string>(tokens => new Success<string>(tokens.Current.Lexeme, tokens.Advance()));

        static IEnumerable<Token> Tokenize(string input) => new CharLexer().Tokenize(input);

        [Fact]
        public void CanBuildParserWhichSimulatesSuccessfulParsingOfGivenValueWithoutConsumingInput()
        {
            var parser = new MonadicUnitParser<int>(1);

            parser.PartiallyParses(Tokenize("input")).LeavingUnparsedTokens("i", "n", "p", "u", "t").WithValue(1);
        }

        [Fact]
        public void CanBuildParserFromSingleSimplerParser()
        {
            var parser = from x in Next
                         select x.ToUpperInvariant();

            parser.PartiallyParses(Tokenize("xy")).LeavingUnparsedTokens("y").WithValue("X");
        }

        [Fact]
        public void CanBuildParserFromOrderedSequenceOfSimplerParsers()
        {
            var parser = (from a in Next
                          from b in Next
                          from c in Next
                          select (a + b + c).ToUpperInvariant());

            parser.PartiallyParses(Tokenize("abcdef")).LeavingUnparsedTokens("d", "e", "f").WithValue("ABC");
        }

        [Fact]
        public void PropagatesErrorsWithoutRunningRemainingParsers()
        {
            IParser<string> fail = Grammar.Fail<string>();

            var tokens = Tokenize("xy").ToArray();

            (from _ in fail
             from x in Next
             from y in Next
             select Tuple.Create(x, y)).FailsToParse(tokens).LeavingUnparsedTokens("x", "y");

            (from x in Next
             from _ in fail
             from y in Next
             select Tuple.Create(x, y)).FailsToParse(tokens).LeavingUnparsedTokens("y");

            (from x in Next
             from y in Next
             from _ in fail
             select Tuple.Create(x, y)).FailsToParse(tokens).AtEndOfInput();
        }
    }
}