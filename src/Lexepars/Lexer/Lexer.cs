using System.Collections.Generic;

namespace Lexepars
{
    public class Lexer : LexerBase<string>
    {
        public Lexer(params MatchableTokenKindSpec[] kinds)
            : base(null, kinds)
        { }

        public override IEnumerable<Token> Tokenize(string input)
        {
            var text = new InputText(input);

            while (!text.EndOfInput)
            {
                var current = GetToken(text);

                if (current == null)
                {
                    yield return CreateUnknownToken(text);
                    yield break;
                }

                text.Advance(current.Lexeme.Length);

                if (SkippableTokenKinds.Contains(current.Kind))
                    continue;

                yield return current;
            }
        }
    }
}