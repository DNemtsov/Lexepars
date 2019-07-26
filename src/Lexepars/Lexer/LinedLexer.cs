using System.Collections.Generic;
using System.IO;

namespace Lexepars
{
    public class LinedLexer : LexerBase<TextReader>
    {
        public LinedLexer(params MatchableTokenKindSpec[] kinds)
            : base(null, kinds)
        { }

        public override IEnumerable<Token> Tokenize(TextReader textReader)
        {
            var text = new LinedInputText(textReader);

            while (text.ReadLine())
                while (!text.EndOfLine)
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