using Lexepars.Parsers;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Lexepars.Async_Parsers
{
    /// <summary>
    /// Behaves like <see cref="CountingQuantifiedParser"/>, except could be used with parallel parsers.
    /// </summary>
    public class CountingQuantifiedParserAsync : AsyncParser<int>
    {
        private readonly IAsyncGeneralParser _item;
        private readonly QuantificationRule _quantificationRule;
        private readonly int _n;
        private readonly int _m;
        private readonly IAsyncGeneralParser _separator;

        /// <summary>
        /// Creates a new instance of <see cref="QuantifiedParserAsync{TValue}"/>.
        /// </summary>
        /// <param name="item">The ~item parser. Not null.</param>
        /// <param name="quantificationRule">Quantification rule.</param>
        /// <param name="n">N parameter of the quantification rule. Non-negative.</param>
        /// <param name="m">M parameter of the quantification rule. If used by the <paramref name="quantificationRule"/>,
        /// should be not less than N, othervise should be set to -1. ></param>
        /// <param name="separator">Optional item separator parser. Is null by default.</param>
        public CountingQuantifiedParserAsync(IAsyncGeneralParser item, QuantificationRule quantificationRule, int n, int m = -1, IAsyncGeneralParser separator = null)
        {
            _item = item ?? throw new ArgumentNullException(nameof(item));

            if (n < 0)
                throw new ArgumentOutOfRangeException(nameof(n), "should be non-negative");

            switch (quantificationRule)
            {
                case QuantificationRule.ExactlyN:
                case QuantificationRule.NOrMore:
                    if (m != -1)
                        throw new ArgumentOutOfRangeException(nameof(m), "this value is not used in this mode and should be left -1");
                    break;
                case QuantificationRule.NtoM:
                    if (n > m)
                        throw new ArgumentOutOfRangeException(nameof(m), "should not be less than n");
                    break;
            }

            if (item == separator)
                throw new ArgumentException("parser for the item and the separator cannot be the same one", nameof(separator));

            _quantificationRule = quantificationRule;

            _n = n;
            _m = m;

            _separator = separator;
        }

        /// <inheritdoc/>
        public async override Task<IReply<int>> ParseAsync(TokenStream tokens, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var oldPosition = tokens.Position;
            var reply = await _item.ParseGenerallyAsync(tokens, cancellationToken);
            var newPosition = reply.UnparsedTokens.Position;

            var times = 0;

            var separatorParserIsPresent = _separator != null;
            var separatorWasParsed = false;

            while (reply.Success)
            {
                if (oldPosition == newPosition)
                    throw new Exception($"Item parser {_item.Expression} encountered a potential infinite loop at position {newPosition}.");

                ++times;

                switch (_quantificationRule)
                {
                    case QuantificationRule.ExactlyN:
                        if (times > _n)
                            return new Failure<int>(
                                reply.UnparsedTokens,
                                FailureMessages.Empty.With(FailureMessage.Expected($"{_item.Expression} occurring exactly {_n} times"))
                            );
                        break;
                    case QuantificationRule.NtoM:
                        if (times > _m)
                            return new Failure<int>(
                                reply.UnparsedTokens,
                                FailureMessages.Empty.With(FailureMessage.Expected($"{_item.Expression} occurring between {_n} and {_m} times"))
                            );
                        break;
                    case QuantificationRule.NOrLess:
                        if (times > _n)
                            return new Failure<int>(
                                reply.UnparsedTokens,
                                FailureMessages.Empty.With(FailureMessage.Expected($"{_item.Expression} occurring no more than {_n} times"))
                            );
                        break;
                }

                var unparsedTokens = reply.UnparsedTokens;

                if (separatorParserIsPresent)
                {
                    var positionBeforeSeparator = newPosition;

                    var separatorReply = await _separator.ParseGenerallyAsync(reply.UnparsedTokens, cancellationToken);

                    unparsedTokens = separatorReply.UnparsedTokens;

                    var positionAfterSeparator = unparsedTokens.Position;

                    if (separatorReply.Success)
                    {
                        if (positionBeforeSeparator == positionAfterSeparator)
                            throw new Exception($"Separator parser {_separator.Expression} encountered a potential infinite loop at position {positionBeforeSeparator}.");
                    }
                    else
                    {
                        if (positionBeforeSeparator != positionAfterSeparator)
                            return Failure<int>.From(separatorReply);
                    }

                    newPosition = positionAfterSeparator;

                    separatorWasParsed = separatorReply.Success;
                }

                oldPosition = newPosition;

                if (separatorParserIsPresent && !separatorWasParsed)
                    break;

                reply = await _item.ParseGenerallyAsync(unparsedTokens, cancellationToken);

                if (!reply.Success && separatorParserIsPresent)
                    return new Failure<int>(reply.UnparsedTokens, reply.FailureMessages);

                newPosition = reply.UnparsedTokens.Position;
            }

            //The item parser finally failed or the separator parser parsed the next separator, but there was no item following it
            if (oldPosition != newPosition || separatorParserIsPresent && separatorWasParsed)
                return new Failure<int>(reply.UnparsedTokens, reply.FailureMessages);

            switch (_quantificationRule)
            {
                case QuantificationRule.NOrMore:
                    if (times < _n)
                        return new Failure<int>(
                            reply.UnparsedTokens,
                            FailureMessages.Empty.With(FailureMessage.Expected($"{_item.Expression} occurring {_n}+ times")));
                    break;
                case QuantificationRule.ExactlyN:
                    if (times != _n)
                        return new Failure<int>(
                            reply.UnparsedTokens,
                            FailureMessages.Empty.With(FailureMessage.Expected($"{_item.Expression} occurring exactly {_n} times")));
                    break;
                case QuantificationRule.NtoM:
                    if (times < _n)
                        return new Failure<int>(
                            reply.UnparsedTokens,
                            FailureMessages.Empty.With(FailureMessage.Expected($"{_item.Expression} occurring between {_n} and {_m} times")));
                    break;
                case QuantificationRule.NOrLess:
                    if (times > _n)
                        return new Failure<int>(
                            reply.UnparsedTokens,
                            FailureMessages.Empty.With(FailureMessage.Expected($"{_item.Expression} occurring no more than {_n} times")));
                    break;
            }

            return new Success<int>(times, reply.UnparsedTokens, reply.FailureMessages);
        }

        /// <inheritdoc/>
        public async override Task<IGeneralReply> ParseGenerallyAsync(TokenStream tokens, CancellationToken cancellationToken) => await ParseAsync(tokens, cancellationToken);

        /// <summary>
        /// Builds the parser expression.
        /// </summary>
        /// <returns>Expression string. Not null.</returns>
        protected override string BuildExpression()
        {
            switch (_quantificationRule)
            {
                case QuantificationRule.NtoM:
                    return $"<COUNT [{_n};{_m}] TIMES {_item.Expression}>";
                case QuantificationRule.ExactlyN:
                    return $"<COUNT {_n} TIMES {_item.Expression}>";
                case QuantificationRule.NOrLess:
                    return $"<COUNT {_n}- TIMES {_item.Expression}>";
                default:
                    return $"<COUNT {_n}+ TIMES {_item.Expression}>";
            }
        }
    }
}
