namespace Lexepars
{
    /// <summary>
    /// The standard token kind to be emitted when all the tokens known to the lexer have failed to match the input text.
    /// Occurrence of this token is a fatal error, so lexer should stop after emitting a token of this kind.
    /// </summary>
    public class UnknownTokenKind : SpecialTokenKind
    {
        /// <summary>
        /// Creates a new instance of <see cref="UnknownTokenKind"/>
        /// </summary>
        public UnknownTokenKind()
            : base("Unknown")
        { }

        /// <summary>
        /// Creates a lexeme for the token by peeking 50 characters starting at the current position.
        /// </summary>
        /// <param name="text">The input text. Not null.</param>
        /// <remarks>The text position is not be amended.</remarks>
        protected sealed override string CreateLexeme(IInputText text) => text.Peek(50);
    }
}
