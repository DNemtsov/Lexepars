namespace Lexepars
{
    /// <summary>
    /// The base class for token kinds without lexemes to be used for special purposes (i.e. Python-like offside-rule scopes) that are directly created without any matching against the input text.
    /// </summary>
    /// <remarks>The text position will not be amended thus allowing to return however many tokens at the same position, potentially occupied by a matchable token as well.</remarks>
    public class NullLexemeTokenKind : SpecialTokenKind
    {
        /// <summary>
        /// Creates a new instance of <see cref="NullLexemeTokenKind"/>
        /// </summary>
        public NullLexemeTokenKind(string name)
            : base(name)
        { }

        /// <summary>
        /// Returns null.
        /// </summary>
        /// <param name="text">The input text. Not used.</param>
        protected sealed override string CreateLexeme(IInputText text) => null;
    }
}
