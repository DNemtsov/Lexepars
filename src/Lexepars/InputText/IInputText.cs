using System;

namespace Lexepars
{
    /// <summary>
    /// Represents the input text for parsing.
    /// </summary>
    public interface IInputText
    {
        /// <summary>
        /// Peeks the text without advancing the current position.
        /// </summary>
        /// <param name="characters">Number of characters to peek.</param>
        string Peek(int characters);

        /// <summary>
        /// Advances the current position.
        /// </summary>
        /// <param name="characters">Number of characters to advance.</param>
        void Advance(int characters);

        /// <summary>
        /// Indicates whether the end of input is reached.
        /// </summary>
        bool EndOfInput { get; }

        /// <summary>
        /// Matches the regular expression against the current position.
        /// </summary>
        /// <param name="regex">Regular expression to match.</param>
        /// <returns>Match result.</returns>
        MatchResult Match(TokenRegex regex);

        /// <summary>
        /// Matches the current character against the predicate.
        /// </summary>
        /// <param name="test">Predicate to match.</param>
        /// <returns></returns>
        MatchResult Match(Predicate<char> test);

        /// <summary>
        /// Returns the current position.
        /// </summary>
        Position Position { get; }
    }
}
