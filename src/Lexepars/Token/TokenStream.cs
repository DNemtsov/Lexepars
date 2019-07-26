namespace Lexepars
{
    using System;
    using System.Collections.Generic;

    public class TokenStream
    {
        private readonly Lazy<TokenStream> _rest;

        public TokenStream(IEnumerable<Token> tokens)
        {
            var enumerator = tokens.GetEnumerator();

            Current = enumerator.MoveNext()
                          ? enumerator.Current
                          : new Token(TokenKind.EndOfInput, new Position(1, 1), "");

            _rest = new Lazy<TokenStream>(() => LazyAdvance(enumerator));
        }

        protected TokenStream(Token current, IEnumerator<Token> enumerator)
        {
            Current = current;
            _rest = enumerator == null
                ? new Lazy<TokenStream>(() => this)
                : new Lazy<TokenStream>(() => LazyAdvance(enumerator));
        }

        public Token Current { get; }

        public virtual TokenStream Advance()
        {
            return _rest.Value;
        }

        public Position Position => Current.Position;

        public override string ToString() => $">{Current}";

        private TokenStream LazyAdvance(IEnumerator<Token> enumerator)
        {
            if (enumerator.MoveNext())
                return CreateInstance(enumerator.Current, enumerator);

            if (Current.Kind == TokenKind.EndOfInput)
                return this;

            var endPosition = new Position(Position.Line, Position.Column + Current.Lexeme?.Length ?? 0);

            var endToken = new Token(TokenKind.EndOfInput, endPosition, string.Empty);

            return CreateInstance(endToken, null);
        }

        protected virtual TokenStream CreateInstance(Token current, IEnumerator<Token> enumerator) => new TokenStream(current, enumerator);
    }
}
