using Lexepars.TestFixtures;
using System;
using System.Collections.Generic;

namespace Lexepars.Tests.Fixtures
{
    public abstract class LexerParserTestCase<TParsingResult> : LexerTestCase
    {
        public Action<TParsingResult> ParserOutputValidation { get; }

        public LexerParserTestCase(string inputText, IReadOnlyList<Token> lexerOutput, Action<TParsingResult> parserOutputValidation)
            : base(inputText, lexerOutput)
        {
            ParserOutputValidation = parserOutputValidation;
        }

        protected abstract IParser<TParsingResult> CreateParser();

        public void TestParser()
        {
            if (ParserOutputValidation == null)
                throw new InvalidOperationException($"{nameof(ParserOutputValidation)} is not specified.");

            CreateParser().Parses(LexerOutput).WithValue(ParserOutputValidation);
        }
    }
}
