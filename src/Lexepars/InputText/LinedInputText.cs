using System;
using System.IO;
using System.Text;

namespace Lexepars
{
    public class LinedInputText : ILinedInputText
    {
        private string _lineBuffer;
        private readonly TextReader _reader;

        private int _line;
        private int _index;

        public LinedInputText(TextReader reader)
        {
            _reader = reader ?? throw new ArgumentNullException(nameof(reader));
        }

        public string Peek(int characters)
        {
            if (_lineBuffer == null || characters <= 0)
                return string.Empty;

            var s = _index + characters >= _lineBuffer.Length
                       ? _lineBuffer.Substring(_index)
                       : _lineBuffer.Substring(_index, characters);

            return s;
        }

        public bool ReadLine()
        {
            _lineBuffer = ReadLinePreservingNewlineCharacters(_reader)?.ToString();

            if (_lineBuffer == null)
            {
                EndOfInput = true;
                return false;
            }

            _index = 0;
            ++_line;

            return true;
        }

        public void Advance(int characters)
        {
            if (characters <= 0)
                return;

            if (_lineBuffer == null)
                return;

            int index = Math.Min(_index + characters, _lineBuffer.Length);

            _index = index;
        }

        public bool EndOfInput { get; private set; }

        public bool EndOfLine => _lineBuffer == null || _index >= _lineBuffer.Length;

        public MatchResult Match(TokenRegex regex)
        {
            if (_lineBuffer == null)
                return MatchResult.Fail;

            return regex.Match(_lineBuffer, _index);
        }

        public MatchResult Match(Predicate<char> test)
        {
            if (_lineBuffer == null)
                return MatchResult.Fail;

            int i = _index;

            while (i < _lineBuffer.Length && test(_lineBuffer[i]))
                i++;

            var value = Peek(i - _index);

            if (value.Length > 0)
                return MatchResult.Succeed(value);

            return MatchResult.Fail;
        }

        public Position Position => new Position(_line, _index + 1);

        public override string ToString()
        {
            if (_lineBuffer == null)
                return string.Empty;

            const int maxVisibleLength = 50;

            var sb = new StringBuilder($"{Position}");

            if (_lineBuffer.Length - _index >= maxVisibleLength)
            {
                sb.Append(_lineBuffer.Substring(_index, maxVisibleLength));
                sb.Append("...");
            }
            else
                sb.Append(_lineBuffer.Substring(_index));

            return sb.ToString();
        }

        /// <summary>Reads a line of characters from the current stream and returns the data as a string. The difference with the standard implementation is that it does not strip the newline characters.</summary>
        /// <returns>The next line from the input stream, or null if all characters have been read.</returns>
        /// <exception cref="IOException">An I/O error occurs. </exception>
        /// <exception cref="OutOfMemoryException">There is insufficient memory to allocate a buffer for the returned string. </exception>
        /// <exception cref="ObjectDisposedException">The <see cref="TextReader" /> is closed. </exception>
        /// <exception cref="ArgumentOutOfRangeException">The number of characters in the next line is larger than <see cref="int.MaxValue" /></exception>
        protected static StringBuilder ReadLinePreservingNewlineCharacters(TextReader textReader)
        {
            int currentCharacter;

            var stringBuilder = new StringBuilder();

            const int lineFeed = 10;
            const int carriageReturn = 13;

            while (true)
            {
                currentCharacter = textReader.Read();

                if (currentCharacter == -1)
                {
                    if (stringBuilder.Length == 0)
                        return null;

                    return stringBuilder;
                }

                stringBuilder.Append((char)currentCharacter);

                if (currentCharacter == lineFeed || currentCharacter == carriageReturn)
                    break;
            }

            if (currentCharacter == carriageReturn && textReader.Peek() == lineFeed)
            {
                textReader.Read();
                stringBuilder.Append((char)lineFeed);
            }

            return stringBuilder;
        }
    }
}