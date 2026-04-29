using Lexepars.Parsers;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Lexepars.ParsersAsync
{

    /// <summary>
    /// Behaves like <see cref="MonadicBindParser{TInterim, TResult}"/>, except could be used with parallel parsers.
    /// </summary>
    /// <typeparam name="TInterim">The type of the interim parsed result.</typeparam>
    /// <typeparam name="TResult">The type of the final parsed result.</typeparam>
    public class MonadicBindParserAsync<TInterim, TResult> : AsyncParser<TResult>
    {
        /// <summary>
        /// Creates a new instance of <see cref="MonadicBindParserAsync{TInterim, TResult}"/>.
        /// </summary>
        /// <param name="parser">Parser to provide the interim value. Not null.</param>
        /// <param name="resultContinuation">Result continuation callback to project the interim value to the result. Not null.</param>
        public MonadicBindParserAsync(IAsyncParser<TInterim> parser, Func<TInterim, TResult> resultContinuation)
        {
            _parser = parser ?? throw new ArgumentNullException(nameof(parser));
            _resultContinuation = resultContinuation ?? throw new ArgumentNullException(nameof(resultContinuation));
        }

        /// <inheritdoc/>
        public async override Task<IReply<TResult>> ParseAsync(TokenStream tokens, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var reply = await _parser.ParseAsync(tokens, cancellationToken);

            if (!reply.Success)
                return Failure<TResult>.From(reply);

            var parsedValue = _resultContinuation(reply.ParsedValue);

            return new Success<TResult>(parsedValue, reply.UnparsedTokens);
        }

        /// <inheritdoc/>
        public async override Task<IGeneralReply> ParseGenerallyAsync(TokenStream tokens, CancellationToken cancellationToken) => await _parser.ParseGenerallyAsync(tokens, cancellationToken);

        /// <inheritdoc/>
        protected override string BuildExpression() => $"<BIND {_parser.Expression} TO {typeof(TResult)}>";
        
        private readonly IAsyncParser<TInterim> _parser;
        private readonly Func<TInterim, TResult> _resultContinuation;
    }

    /// <summary>
    /// Binds a result-mapping function to the `parser1 to the second parser continuation to the final result continuation. Is used in chaining situations such as <see cref="ParserExtensions.SelectMany{TInterim1, TInterim2, TValue}(IParser{TInterim1}, Func{TInterim1, IParser{TInterim2}}, Func{TInterim1, TInterim2, TValue})"/>.
    /// </summary>
    /// <typeparam name="TInterim1">The type of the first interim parsed result.</typeparam>
    /// <typeparam name="TInterim2">The type of the second interim parsed result.</typeparam>
    /// <typeparam name="TResult">The type of the final parsed result.</typeparam>
    public class MonadicBindParserAsync<TInterim1, TInterim2, TResult> : AsyncParser<TResult>
    {
        /// <summary>
        /// Creates a new instance of <see cref="MonadicBindParserAsync{TInterim1, TInterim2, TResult}"/>.
        /// </summary>
        /// <param name="parser1">First parser.</param>
        /// <param name="parser2Continuation">First result to second parser continuation.</param>
        /// <param name="resultContinuation">Final result continuation.</param>
        public MonadicBindParserAsync(IAsyncParser<TInterim1> parser1, Func<TInterim1, IAsyncParser<TInterim2>> parser2Continuation, Func<TInterim1, TInterim2, TResult> resultContinuation)
        {
            _parser1 = parser1 ?? throw new ArgumentNullException(nameof(parser1));

            _parser2Continuation = parser2Continuation ?? throw new ArgumentNullException(nameof(parser2Continuation));
            _resultContinuation = resultContinuation ?? throw new ArgumentNullException(nameof(resultContinuation));
        }

        /// <inheritdoc/>
        public async override Task<IReply<TResult>> ParseAsync(TokenStream tokens, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var reply1 = await _parser1.ParseAsync(tokens, cancellationToken);

            if (!reply1.Success)
                return Failure<TResult>.From(reply1);

            var value1 = reply1.ParsedValue;

            var parser2 = _parser2Continuation(value1);

            var reply2 = await parser2.ParseAsync(reply1.UnparsedTokens, cancellationToken);

            if (!reply2.Success)
                return Failure<TResult>.From(reply2);

            var value2 = reply2.ParsedValue;

            var result = _resultContinuation(value1, value2);

            return new Success<TResult>(result, reply2.UnparsedTokens);
        }

        
        /// <inheritdoc/>
        protected override string BuildExpression() => $"<BIND2 {_parser1} TO {typeof(TInterim1)} TO {typeof(TInterim2)}>";

        private readonly IAsyncParser<TInterim1> _parser1;

        private readonly Func<TInterim1, IAsyncParser<TInterim2>> _parser2Continuation;
        private readonly Func<TInterim1, TInterim2, TResult> _resultContinuation;
    }
}
