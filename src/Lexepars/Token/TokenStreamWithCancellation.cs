namespace Lexepars
{
    using System.Collections.Generic;
    using System.Threading;

    public class TokenStreamWithCancellation : TokenStream
    {
        private readonly CancellationToken _cancellationToken;

        public TokenStreamWithCancellation(IEnumerable<Token> tokens, CancellationToken cancellationToken)
            : base(tokens)
        {
            _cancellationToken = cancellationToken;
        }

        private TokenStreamWithCancellation(Token current, IEnumerator<Token> enumerator, CancellationToken cancellationToken)
            : base(current, enumerator)
        {
            _cancellationToken = cancellationToken;
        }

        public override TokenStream Advance()
        {
            _cancellationToken.ThrowIfCancellationRequested();

            return base.Advance();
        }

        protected override TokenStream CreateInstance(Token current, IEnumerator<Token> enumerator)
            => new TokenStreamWithCancellation(current, enumerator, _cancellationToken);
    }
}