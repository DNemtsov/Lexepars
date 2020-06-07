using System;

namespace Lexepars.Parsers
{
    /// <summary>
    /// Uses the provided lambda expression to perform parsing.
    /// </summary>
    /// <typeparam name="TValue">The type of the parsed value.</typeparam>
    public class LambdaParser<TValue> : Parser<TValue>
    {
        private readonly Func<TokenStream, IReply<TValue>> _parse;

        /// <summary>
        /// Creates a new instance of <see cref="LambdaParser{TValue}"/>.
        /// </summary>
        /// <param name="parse"></param>
        public LambdaParser(Func<TokenStream, IReply<TValue>> parse)
        {
            _parse = parse;
        }

        /// <inheritdoc/>
        public override IReply<TValue> Parse(TokenStream tokens)
        {
            return _parse(tokens);
        }

        /// <inheritdoc/>
        protected override string BuildExpression() => $"<(t) {typeof(TValue)}>";
    }
}