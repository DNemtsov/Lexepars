using Lexepars.Parsers;
using System.Threading;
using System.Threading.Tasks;

namespace Lexepars.Async_Parsers
{
    /// <summary>
    /// Behaves like <see cref="MonadicUnitParser{TValue}"/>, except could be used with parallel parsers.
    /// </summary>
    /// <typeparam name="TValue">The type of the parsed value.</typeparam>
    public class MonadicUnitParserAsync<TValue> : AsyncParser<TValue>
    {
        /// <summary>
        /// Creates a new instance of <see cref="MonadicUnitParserAsync{TValue}"/>.
        /// </summary>
        /// <param name="value">Value to be returned. Can be null.</param>
        public MonadicUnitParserAsync(TValue value)
        {
            _value = value;
        }

        /// <inheritdoc/>
        public override Task<IReply<TValue>> ParseAsync(TokenStream tokens, CancellationToken cancellationToken) => Task.FromResult<IReply<TValue>>(new Success<TValue>(_value, tokens));

        /// <inheritdoc/>
        public override Task<IGeneralReply> ParseGenerallyAsync(TokenStream tokens, CancellationToken cancellationToken) => Task.FromResult<IGeneralReply>(new GeneralSuccess(tokens));

        private readonly TValue _value;

        /// <inheritdoc/>
        protected override string BuildExpression() => $"<= {_value}>";
    }
}
