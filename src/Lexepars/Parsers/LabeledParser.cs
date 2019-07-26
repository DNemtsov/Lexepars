using System;

namespace Lexepars.Parsers
{
    /// <summary>
    /// When `p consumes any input, Label(`p, e) is the same as `p.
    /// When `p fails not consuming any input, Label(`p, e) is the same
    /// as `p, except any messages are replaced with expectation e.
    /// </summary>
    /// <typeparam name="TValue">The type of the parsed value.</typeparam>
    public class LabeledParser<TValue> : Parser<TValue>
    {
        /// <summary>
        ///  Creates a new instance of <see cref="LabeledParser{TValue}"/>.
        /// </summary>
        /// <param name="parser">The `p parser. Not null.</param>
        /// <param name="expectation">Expectation message. Not null.</param>
        public LabeledParser(IParser<TValue> parser, string expectation)
        {
            _parser = parser ?? throw new ArgumentNullException(nameof(parser));
            _failures = FailureMessages.Empty.With(FailureMessage.Expected(expectation));
        }

        /// <summary>
        /// Parses the stream of tokens.
        /// </summary>
        /// <param name="tokens">Stream of tokens to parse. Not null.</param>
        /// <returns>Parsing reply. Not null.</returns>
        public override IReply<TValue> Parse(TokenStream tokens)
        {
            var oldPosition = tokens.Position;
            var reply = _parser.Parse(tokens);
            var newPosition = reply.UnparsedTokens.Position;

            if (oldPosition != newPosition)
                return reply;

            if (reply.Success)
                return new Success<TValue>(reply.ParsedValue, reply.UnparsedTokens, _failures);
                
            return new Failure<TValue>(reply.UnparsedTokens, _failures);
        }

        /// <summary>
        /// Parsing optimized for the case when the reply value is not needed. NOTE: Result continuation will not be called.
        /// </summary>
        /// <param name="tokens">The token stream to parse. Not null.</param>
        /// <returns>General parsing reply. Not null.</returns>
        public override IGeneralReply ParseGenerally(TokenStream tokens) => _parser.ParseGenerally(tokens);

        /// <summary>
        /// Builds the parser expression.
        /// </summary>
        /// <returns>Expression string. Not null.</returns>
        protected override string BuildExpression() => $"<LABEL {_parser.Expression} WITH {_failures}";

        private readonly IParser<TValue> _parser;
        private readonly FailureMessages _failures;
    }
}
