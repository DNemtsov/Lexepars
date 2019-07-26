using System;

namespace Lexepars.Parsers
{
    /// <summary>
    /// If `left, `item, `right are sequentially called.
    /// If they all succeed, the result of the `item is returned.
    /// Otherwise, the first occured failure is returned and the remaining
    /// parsers are not called.
    /// </summary>
    /// <typeparam name="TValue">The type of the parsed value.</typeparam>
    public class BetweenParser<TValue> : Parser<TValue>
    {
        /// <summary>
        /// Creates a new instance of <see cref="BetweenParser{TValue}"/>.
        /// </summary>
        /// <param name="left">General parser of the left part. Not null.</param>
        /// <param name="item">Item parser. Not null.</param>
        /// <param name="right">General parser of the right part. Not null.</param>
        public BetweenParser(IGeneralParser left, IParser<TValue> item, IGeneralParser right)
        {
            _left = left ?? throw new ArgumentNullException(nameof(left));
            _item = item ?? throw new ArgumentNullException(nameof(item));
            _right = right ?? throw new ArgumentNullException(nameof(right));
        }

        /// <summary>
        /// Parses the stream of tokens.
        /// </summary>
        /// <param name="tokens">Stream of tokens to parse. Not null.</param>
        /// <returns>Parsing reply. Not null.</returns>
        public override IReply<TValue> Parse(TokenStream tokens)
        {
            var left = _left.ParseGenerally(tokens);

            if (!left.Success)
                return Failure<TValue>.From(left);

            var item = _item.Parse(left.UnparsedTokens);

            if (!item.Success)
                return item;

            var right = _right.ParseGenerally(item.UnparsedTokens);

            if (!right.Success)
                return Failure<TValue>.From(right);

            return new Success<TValue>(item.ParsedValue, right.UnparsedTokens, right.FailureMessages);
        }

        /// <summary>
        /// Parsing optimized for the case when the reply value is not needed. NOTE: Result continuation will not be called.
        /// </summary>
        /// <param name="tokens">The token stream to parse. Not null.</param>
        /// <returns>General parsing reply. Not null.</returns>
        public override IGeneralReply ParseGenerally(TokenStream tokens)
        {
            var left = _left.ParseGenerally(tokens);

            if (!left.Success)
                return left;

            var item = _item.ParseGenerally(left.UnparsedTokens);

            if (!item.Success)
                return item;

            return _right.ParseGenerally(item.UnparsedTokens);
        }

        /// <summary>
        /// Builds the parser expression.
        /// </summary>
        /// <returns>Expression string. Not null.</returns>
        protected override string BuildExpression() => $"<({_left.Expression}|{_item.Expression}|{_right.Expression})>";
        
        private readonly IGeneralParser _left;
        private readonly IParser<TValue> _item;
        private readonly IGeneralParser _right;
    }
}
