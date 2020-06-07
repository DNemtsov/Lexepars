using System;

namespace Lexepars.Parsers
{

    /// <summary>
    /// Binds a result-mapping function to the `interimParser.  Is used in chaining situations such as <see cref="ParserExtensions.Select{TInterim, TValue}(IParser{TInterim}, Func{TInterim, TValue})"/>.
    /// </summary>
    /// <typeparam name="TInterim">The type of the interim parsed result.</typeparam>
    /// <typeparam name="TResult">The type of the final parsed result.</typeparam>
    public class MonadicBindParser<TInterim, TResult> : Parser<TResult>
    {
        /// <summary>
        /// Creates a new instance of <see cref="MonadicBindParser{TInterim, TResult}"/>.
        /// </summary>
        /// <param name="parser">Parser to provide the interim value. Not null.</param>
        /// <param name="resultContinuation">Result continuation callback to project the interim value to the result. Not null.</param>
        public MonadicBindParser(IParser<TInterim> parser, Func<TInterim, TResult> resultContinuation)
        {
            _parser = parser ?? throw new ArgumentNullException(nameof(parser));
            _resultContinuation = resultContinuation ?? throw new ArgumentNullException(nameof(resultContinuation));
        }

        /// <inheritdoc/>
        public override IReply<TResult> Parse(TokenStream tokens)
        {
            var reply = _parser.Parse(tokens);

            if (!reply.Success)
                return Failure<TResult>.From(reply);

            var parsedValue = _resultContinuation(reply.ParsedValue);

            return new Success<TResult>(parsedValue, reply.UnparsedTokens);
        }

        /// <inheritdoc/>
        public override IGeneralReply ParseGenerally(TokenStream tokens) => _parser.ParseGenerally(tokens);

        /// <inheritdoc/>
        protected override string BuildExpression() => $"<BIND {_parser.Expression} TO {typeof(TResult)}>";
        
        private readonly IParser<TInterim> _parser;
        private readonly Func<TInterim, TResult> _resultContinuation;
    }

    /// <summary>
    /// Binds a result-mapping function to the `parser1 to the second parser continuation to the final result continuation. Is used in chaining situations such as <see cref="ParserExtensions.SelectMany{TInterim1, TInterim2, TValue}(IParser{TInterim1}, Func{TInterim1, IParser{TInterim2}}, Func{TInterim1, TInterim2, TValue})"/>.
    /// </summary>
    /// <typeparam name="TInterim1">The type of the first interim parsed result.</typeparam>
    /// <typeparam name="TInterim2">The type of the second interim parsed result.</typeparam>
    /// <typeparam name="TResult">The type of the final parsed result.</typeparam>
    public class MonadicBindParser<TInterim1, TInterim2, TResult> : Parser<TResult>
    {
        /// <summary>
        /// Creates a new instance of <see cref="MonadicBindParser{TInterim1, TInterim2, TResult}"/>.
        /// </summary>
        /// <param name="parser1">First parser.</param>
        /// <param name="parser2Continuation">First result to second parser continuation.</param>
        /// <param name="resultContinuation">Final result continuation.</param>
        public MonadicBindParser(IParser<TInterim1> parser1, Func<TInterim1, IParser<TInterim2>> parser2Continuation, Func<TInterim1, TInterim2, TResult> resultContinuation)
        {
            _parser1 = parser1 ?? throw new ArgumentNullException(nameof(parser1));

            _parser2Continuation = parser2Continuation ?? throw new ArgumentNullException(nameof(parser2Continuation));
            _resultContinuation = resultContinuation ?? throw new ArgumentNullException(nameof(resultContinuation));
        }

        /// <inheritdoc/>
        public override IReply<TResult> Parse(TokenStream tokens)
        {
            var reply1 = _parser1.Parse(tokens);

            if (!reply1.Success)
                return Failure<TResult>.From(reply1);

            var value1 = reply1.ParsedValue;

            var parser2 = _parser2Continuation(value1);

            var reply2 = parser2.Parse(reply1.UnparsedTokens);

            if (!reply2.Success)
                return Failure<TResult>.From(reply2);

            var value2 = reply2.ParsedValue;

            var result = _resultContinuation(value1, value2);

            return new Success<TResult>(result, reply2.UnparsedTokens);
        }

        
        /// <inheritdoc/>
        protected override string BuildExpression() => $"<BIND2 {_parser1} TO {typeof(TInterim1)} TO {typeof(TInterim2)}>";

        private readonly IParser<TInterim1> _parser1;

        private readonly Func<TInterim1, IParser<TInterim2>> _parser2Continuation;
        private readonly Func<TInterim1, TInterim2, TResult> _resultContinuation;
    }
}
