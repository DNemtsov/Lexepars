using System;

namespace Lexepars
{
    public sealed class Token : IEquatable<Token>
    {
        public TokenKind Kind { get; }
        public Position Position { get; }
        public string Lexeme { get; }

        public Token(TokenKind kind, Position position, string lexeme)
        {
            Kind = kind;
            Position = position;
            Lexeme = lexeme;
        }

        public Token(TokenKind kind, int line, int column, string lexeme)
        {
            Kind = kind;
            Position = new Position(line, column);
            Lexeme = lexeme;
        }

        public override string ToString()
        {
            return $"{Position}{(Lexeme != null ? $"'{Lexeme}'" : "null")}<{Kind}>";
        }

        public bool Equals(Token other)
        {
            if (ReferenceEquals(other, null))
                return false;

            return
                   this.Kind == other.Kind
                && this.Position == other.Position 
                && this.Lexeme == other.Lexeme;
        }

        public static bool operator ==(Token a, Token b)
        {
            if (ReferenceEquals(a, null))
                return ReferenceEquals(b, null);

            if (ReferenceEquals(b, null))
                return ReferenceEquals(a, null);

            return a.Equals(b);
        }

        public static bool operator !=(Token a, Token b) => !(a == b);

        public override bool Equals(object obj)
        {
            switch (obj)
            {
                case Token p:
                    return Equals(p);
                default:
                    return false;
            }
        }

        public override int GetHashCode()
        {
            return Kind.GetHashCode() ^ Position.GetHashCode() ^ Lexeme.GetHashCode();
        }
    }
}