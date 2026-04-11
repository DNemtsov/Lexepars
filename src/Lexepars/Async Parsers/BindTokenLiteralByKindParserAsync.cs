using Lexepars.Async_Parsers;
using Lexepars.Parsers;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Lexepars.Async_Parsers
{
    /// <summary>
    /// Behaves like <see cref="BindTokenLexemeByKindParser{TValue}"/>, except could be used with parallel parsers.
    /// </summary>
    /// <typeparam name="TValue">The type of the parsed value.</typeparam>
    public class BindTokenLexemeByKindParserAsync<TValue> : AsyncParser<TValue>
    {
        /// <summary>
        /// Creates a new instance of <see cref="BindTokenLexemeByKindParserAsync{TValue}"/>.
        /// </summary>
        /// <param name="kind">Token kind to parse. Not null.</param>
        /// <param name="lexemeMapping">Lexeme mapping function. Not null.</param>
        public BindTokenLexemeByKindParserAsync(TokenKind kind, Func<string, TValue> lexemeMapping)
        {
            _kind = kind ?? throw new ArgumentNullException(nameof(kind));
            _lexemeMapping = lexemeMapping ?? throw new ArgumentNullException(nameof(lexemeMapping));
        }

        /// <inheritdoc/>
        public async override Task<IReply<TValue>> ParseAsync(TokenStream tokens, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (tokens.Current.Kind != _kind)
                return new Failure<TValue>(tokens, FailureMessage.Expected(_kind.Name));

            var parsedValue = _lexemeMapping(tokens.Current.Lexeme);

            return new Success<TValue>(parsedValue, tokens.Advance());
        }

        /// <inheritdoc/>
        public async override Task<IGeneralReply> ParseGenerallyAsync(TokenStream tokens, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (tokens.Current.Kind != _kind)
                return new GeneralFailure(tokens, FailureMessage.Expected(_kind.Name));

            return new GeneralSuccess(tokens.Advance());
        }

        /// <inheritdoc/>
        protected override string BuildExpression() => $"<BTL *{_kind}* TO {typeof(TValue)}>";

        private readonly TokenKind _kind;
        private readonly Func<string, TValue> _lexemeMapping;
    }
}
