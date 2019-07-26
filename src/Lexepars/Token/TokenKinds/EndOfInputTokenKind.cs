namespace Lexepars
{
    /// <summary>
    /// The standard end-of-input token.
    /// </summary>
    public class EndOfInputTokenKind : NullLexemeTokenKind
    {
        /// <summary>
        /// Creates a new instance of <see cref="EndOfInputTokenKind"/>
        /// </summary>
        public EndOfInputTokenKind()
            : base("end of input")
        { }
    }
}
