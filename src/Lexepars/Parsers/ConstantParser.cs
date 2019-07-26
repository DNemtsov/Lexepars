﻿namespace Lexepars.Parsers
{
    /// <summary>
    /// Parses the token of the desired kind and returns the specified constant value.
    /// </summary>
    /// <typeparam name="TValue">The type of the constant value.</typeparam>
    public class ConstantParser<TValue> : Parser<TValue>
    {
        /// <summary>
        /// Creates a new instance of <see cref="ConstantParser{TValue}"/>.
        /// </summary>
        /// <param name="kind">The kind of token. Not null.</param>
        /// <param name="value">The value to be returned.</param>
        public ConstantParser(TokenKind kind, TValue value)
        {
            _kind = kind;
            _value = value;
        }

        /// <summary>
        /// Parses the stream of tokens.
        /// </summary>
        /// <param name="tokens">Stream of tokens to parse. Not null.</param>
        /// <returns>Parsing reply. Not null.</returns>
        public override IReply<TValue> Parse(TokenStream tokens)
        {
            if (tokens.Current.Kind == _kind)
                return new Success<TValue>(_value, tokens.Advance());

            return new Failure<TValue>(tokens, FailureMessage.Expected(_kind.Name));
        }

        /// <summary>
        /// Parsing optimized for the case when the reply value is not needed.
        /// </summary>
        /// <param name="tokens">Stream of tokens to parse. Not null.</param>
        /// <returns>Parsing reply. Not null.</returns>
        public override IGeneralReply ParseGenerally(TokenStream tokens)
        {
            if (tokens.Current.Kind == _kind)
                return new GeneralSuccess(tokens.Advance());

            return new GeneralFailure(tokens, FailureMessage.Expected(_kind.Name));
        }

        /// <summary>
        /// Builds the parser expression.
        /// </summary>
        /// <returns>Expression string. Not null.</returns>
        protected override string BuildExpression() => $"<C {_kind} := {_value}>";

        private readonly TokenKind _kind;
        private readonly TValue _value;
    }
}
