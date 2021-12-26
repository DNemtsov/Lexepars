namespace Lexepars
{
    /// <summary>
    /// Represents the input text for parsing which is divided by the line boundaries.
    /// </summary>
    /// <remarks>It is suitable for the big input files to avoid loading them entirely into memory. Multiline tokens are not supported by definition, and thus the multiline value should be tokenized as a set of one-line-value tokens to be assembled back together during parsing.</remarks>
    public interface ILinedInputText : IInputText
    {
        /// <summary>
        /// Reads the next line into the buffer.
        /// </summary>
        /// <remarks>Has to be called for the first line as well.</remarks>
        /// <returns>True if the line was read successfully. False means the end of file has been reached.</returns>
        bool ReadLine();

        /// <summary>
        /// Indicates whether the end of the current line in the buffer has been reached.
        /// </summary>
        bool EndOfLine { get; }
    }
}
