using Lexepars.Parsers;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Lexepars.ParsersAsync
{
    /// <summary>
    /// Behaves like <see cref="NameValuePairParser{TName, TValue}"/>, except could be used with parallel parsers.
    /// </summary>
    /// <typeparam name="TName">Name parsed value type.</typeparam>
    /// <typeparam name="TValue">Value parsed value type. Sorry for the tautology:)</typeparam>
    public class NameValuePairParserAsync<TName, TValue> : AsyncParser<KeyValuePair<TName, TValue>>
    {
        /// <summary>
        /// Creates a new instance of <see cref="NameValuePairParserAsync{TName, TValue}"/>.
        /// </summary>
        /// <param name="name">Name parser.</param>
        /// <param name="delimiter">Delimiter parser.</param>
        /// <param name="value">Value parser.</param>
        public NameValuePairParserAsync(IAsyncParser<TName> name, IAsyncGeneralParser delimiter, IAsyncParser<TValue> value)
        {
            _name = name ?? throw new ArgumentNullException(nameof(name));
            _delimiter = delimiter ?? throw new ArgumentNullException(nameof(delimiter));
            _value = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <inheritdoc/>
        protected override string BuildExpression() => $"<N {_name.Expression} D {_delimiter.Expression} V {_value.Expression}>";

        /// <inheritdoc/>
        public async override Task<IReply<KeyValuePair<TName, TValue>>> ParseAsync(TokenStream tokens, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var name = await _name.ParseAsync(tokens, cancellationToken);

            if (!name.Success)
                return Failure<KeyValuePair<TName, TValue>>.From(name);

            var delimiter = await _delimiter.ParseGenerallyAsync(name.UnparsedTokens, cancellationToken);

            if (!delimiter.Success)
                return Failure<KeyValuePair<TName, TValue>>.From(delimiter);

            var value = await _value.ParseAsync(delimiter.UnparsedTokens, cancellationToken);

            if (!value.Success)
                return Failure<KeyValuePair<TName, TValue>>.From(value);

            return new Success<KeyValuePair<TName, TValue>>(new KeyValuePair<TName, TValue>(name.ParsedValue, value.ParsedValue), value.UnparsedTokens, value.FailureMessages);
        }

        /// <inheritdoc/>
        public async override Task<IGeneralReply> ParseGenerallyAsync(TokenStream tokens, CancellationToken cancellationToken)
        {
            var name = await _name.ParseGenerallyAsync(tokens, cancellationToken);

            if (!name.Success)
                return name;

            var delimiter = await _delimiter.ParseGenerallyAsync(name.UnparsedTokens, cancellationToken);

            if (!delimiter.Success)
                return delimiter;

            return await _value.ParseAsync(delimiter.UnparsedTokens, cancellationToken);
        }

        private readonly IAsyncParser<TName> _name;
        private readonly IAsyncGeneralParser _delimiter;
        private readonly IAsyncParser<TValue> _value;
    }
}
