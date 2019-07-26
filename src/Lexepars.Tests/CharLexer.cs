namespace Lexepars.Tests
{
    public class CharLexer : Lexer
    {
        public static readonly MatchableTokenKind Character = new PatternTokenKind("Character", @".");
        public static readonly MatchableTokenKind LeftParen = new OperatorTokenKind("(");
        public static readonly MatchableTokenKind RightParen = new OperatorTokenKind(")");
        public CharLexer()
            : base(LeftParen, RightParen, Character) { }
    }
}