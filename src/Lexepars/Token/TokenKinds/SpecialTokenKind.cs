namespace Lexepars
{
    /// <summary>
    /// The base class for token kinds used for special purposes (i.e. end of input) that are directly created without any matching against the input text.
    /// </summary>
    /// <remarks>The text position will not be amended thus allowing to return however many tokens at the same position, potentially occupied by a matchable token as well.</remarks>
    public abstract class SpecialTokenKind : TokenKind
    {
        /// <summary>
        /// Creates a new instance of <see cref="SpecialTokenKind"/>
        /// </summary>
        /// <param name="name">The display name of the token kind.</param>
        public SpecialTokenKind(string name)
            : base(name)
        { }

        /// <summary>
        /// Creates a token of this kind at the current position of input text.
        /// </summary>
        /// <param name="text">The input text. Not null.</param>
        public Token CreateTokenAtCurrentPosition(IInputText text)
        {
            return new Token(this, text.Position, CreateLexeme(text));
        }

        /// <summary>
        /// Creates a lexeme for the token.
        /// </summary>
        /// <param name="text">The input text. Not null.</param>
        /// <remarks>The text position should not be amended.</remarks>
        protected abstract string CreateLexeme(IInputText text);
    }
}
