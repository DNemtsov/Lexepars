using Shouldly;
using System.Collections.Generic;
using System.Linq;

namespace Lexepars.Tests.Fixtures
{
    public abstract class LexerTestCase
    {
        public string InputText { get; }
        public IReadOnlyList<Token> LexerOutput { get; }

        public LexerTestCase(string inputText, IReadOnlyList<Token> lexerOutput)
        {
            InputText = inputText;
            LexerOutput = lexerOutput;
        }

        public void TestLexer()
        {
            var tokens = Tokenize(InputText).ToArray();

            tokens.ShouldBe(LexerOutput);
        }

        protected abstract IEnumerable<Token> Tokenize(string inputText);
    }
}
