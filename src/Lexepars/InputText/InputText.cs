using System;

namespace Lexepars
{
    /// <summary>
    /// Represents the default implementaion of the input text.
    /// </summary>
    public class InputText : IInputText
    {
        private int _index;
        private readonly string _input;
        private int _line = 1;
        private int _column = 1;

        private readonly string _newLine;

        /// <summary>
        /// Creates a new instance of <see cref="InputText"/>.
        /// </summary>
        /// <param name="input">Input string.</param>
        /// <param name="newLine">New line delimiter.</param>
        public InputText(string input, string newLine = null)
        {
            _input = input ?? throw new ArgumentNullException(nameof(input));
            _newLine = newLine ?? Environment.NewLine;
        }

        /// <inheritdoc/>
        public string Peek(int characters)
        {
            var s = _index + characters >= _input.Length
                       ? _input.Substring(_index)
                       : _input.Substring(_index, characters);

            return s;
        }

        /// <inheritdoc/>
        public void Advance(int characters)
        {
            if (characters == 0)
                return;

            int index = Math.Min(_index + characters, _input.Length);

            var newLineLength = _newLine.Length;

            var addedLinesCount = 0;
            var column = 1;

            var lastComparisonIndex = Math.Min(_index + characters, _input.Length) - newLineLength;

            for (var i = _index; i <= lastComparisonIndex; ++i)
            {
                var match = true;

                for (var j = 0; j < newLineLength; ++j)
                {
                    if (_input[i + j] != _newLine[j])
                    {
                        match = false;
                        break;
                    }
                }

                ++column;

                if (!match)
                    continue;

                ++addedLinesCount;
                column = 1;
            }

            _line += addedLinesCount;
            _column = addedLinesCount > 0 ? column : _column + index - _index;
            _index = index;
        }

        /// <inheritdoc/>
        public bool EndOfInput => _index >= _input.Length;

        /// <inheritdoc/>
        public MatchResult Match(TokenRegex regex) => regex.Match(_input, _index);

        /// <inheritdoc/>
        public MatchResult Match(Predicate<char> test)
        {
            int i = _index;

            while (i < _input.Length && test(_input[i]))
                i++;

            var value = Peek(i - _index);

            if (value.Length > 0)
                return MatchResult.Succeed(value);

            return MatchResult.Fail;
        }

        /// <inheritdoc/>
        public Position Position => new Position(_line, _column);

        /// <inheritdoc/>
        public override string ToString()
        {
            const int maxVisibleLength = 50;

            if (_input.Length - _index >= maxVisibleLength)
                return _input.Substring(_index, maxVisibleLength) + "...";

            return _input.Substring(_index);
        }
    }
}