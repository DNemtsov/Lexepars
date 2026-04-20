using Lexepars.Parsers;
using System.Threading;
using System.Threading.Tasks;

namespace Lexepars.ParsersAsync
{
    /// <summary>
    /// Behaves like <see cref="ConstantParser{TValue}"/>, except could be used with parallel parsers.
    /// </summary>
    /// <typeparam name="TValue">The type of the constant value.</typeparam>
    public class ConstantParserAsync<TValue> : AsyncParser<TValue>
    {
        /// <summary>
        /// Creates a new instance of <see cref="ConstantParserAsync{TValue}"/>.
        /// </summary>
        /// <param name="kind">The kind of token. Not null.</param>
        /// <param name="value">The value to be returned.</param>
        public ConstantParserAsync(TokenKind kind, TValue value)
        {
            _kind = kind;
            _value = value;
        }

        /// <inheritdoc/>
        public override Task<IReply<TValue>> ParseAsync(TokenStream tokens, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (tokens.Current.Kind == _kind)
                return Task.FromResult<IReply<TValue>>(new Success<TValue>(_value, tokens.Advance()));

            return Task.FromResult<IReply<TValue>>(new Failure<TValue>(tokens, FailureMessage.Expected(_kind.Name)));
        }

        /// <inheritdoc/>
        public override Task<IGeneralReply> ParseGenerallyAsync(TokenStream tokens, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (tokens.Current.Kind == _kind)
                return Task.FromResult<IGeneralReply>(new GeneralSuccess(tokens.Advance()));

            return Task.FromResult<IGeneralReply>(new GeneralFailure(tokens, FailureMessage.Expected(_kind.Name)));
        }

        /// <inheritdoc/>
        protected override string BuildExpression() => $"<C {_kind} := {_value}>";

        private readonly TokenKind _kind;
        private readonly TValue _value;
    }
}
