using Lexepars.Parsers;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Lexepars.ParsersAsync
{
    /// <summary>
    /// Behaves like <see cref="LambdaParser{TValue}"/>, except could be used with parallel parsers.
    /// </summary>
    /// <typeparam name="TValue">The type of the parsed value.</typeparam>
    public class LambdaParserAsync<TValue> : AsyncParser<TValue>
    {
        private readonly Func<TokenStream, Task<IReply<TValue>>> _parse;

        /// <summary>
        /// Creates a new instance of <see cref="LambdaParserAsync{TValue}"/>.
        /// </summary>
        /// <param name="parse"></param>
        public LambdaParserAsync(Func<TokenStream, Task<IReply<TValue>>> parse)
        {
            _parse = parse;
        }

        /// <inheritdoc/>
        public override Task<IReply<TValue>> ParseAsync(TokenStream tokens, CancellationToken cancellationToken)
        {
            return _parse(tokens);
        }

        /// <inheritdoc/>
        protected override string BuildExpression() => $"<(t) {typeof(TValue)}>";
    }
}