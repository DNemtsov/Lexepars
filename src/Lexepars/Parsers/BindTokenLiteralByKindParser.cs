using System;

namespace Lexepars.Parsers
{
    /// <summary>
    /// Parses token by kind and binds its lexeme to the mapping function.
    /// </summary>
    /// <typeparam name="TValue">The type of the parsed value.</typeparam>
    public class BindTokenLexemeByKindParser<TValue> : Parser<TValue>
    {
        /// <summary>
        /// Creates a new instance of <see cref="BindTokenLexemeByKindParser{TValue}"/>.
        /// </summary>
        /// <param name="kind">Token kind to parse. Not null.</param>
        /// <param name="lexemeMapping">Lexeme mapping function. Not null.</param>
        public BindTokenLexemeByKindParser(TokenKind kind, Func<string, TValue> lexemeMapping)
        {
            _kind = kind ?? throw new ArgumentNullException(nameof(kind));
            _lexemeMapping = lexemeMapping ?? throw new ArgumentNullException(nameof(lexemeMapping));
        }

        /// <inheritdoc/>
        public override IReply<TValue> Parse(TokenStream tokens)
        {
            if (tokens.Current.Kind != _kind)
                return new Failure<TValue>(tokens, FailureMessage.Expected(_kind.Name));

            var parsedValue = _lexemeMapping(tokens.Current.Lexeme);

            return new Success<TValue>(parsedValue, tokens.Advance());
        }

        /// <inheritdoc/>
        public override IGeneralReply ParseGenerally(TokenStream tokens)
        {
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
