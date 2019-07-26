namespace Lexepars
{
    /// <summary>
    /// The base class for all kinds of tokens. Should be inherited.
    /// </summary>
    public abstract class TokenKind
    {
        /// <summary>
        /// The standard token emitted at the end of the input text.
        /// </summary>
        public static readonly TokenKind EndOfInput = new EndOfInputTokenKind();

        /// <summary>
        /// The standard token emitted when all the tokens known to the lexer have failed to parse the input text.
        /// Occurrence of this token is a fatal error, so lexer should stop after emitting a token of this kind.
        /// </summary>
        public static readonly SpecialTokenKind Unknown = new UnknownTokenKind();

        /// <summary>
        /// Creates a new instance of <see cref="TokenKind"/>
        /// </summary>
        /// <param name="name">The display name of the token kind.</param>
        /// <param name="skippable">If true, the lexer won't be emitting the tokens of this kind when they are matched.</param>
        protected TokenKind(string name)
        {
            Name = name;
        }

        /// <summary>
        /// The display name of the token kind.
        /// </summary>
        public string Name { get; }

        public override string ToString() => Name;
    }
}