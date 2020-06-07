using System;
using System.Collections.Generic;

namespace Lexepars.Parsers
{
    /// <summary>
    /// Specifies the expected number of the `item occurrences. 
    /// </summary>
    public enum QuantificationRule
    {
        /// <summary>
        /// N or more times.
        /// </summary>
        NOrMore,

        /// <summary>
        /// Exactly N times.
        /// </summary>
        ExactlyN,

        /// <summary>
        /// N or less times.
        /// </summary>
        NOrLess,

        /// <summary>
        /// From N to M times.
        /// </summary>
        NtoM
    }

    /// <summary>
    /// Parses the sequence of `items optionally divided by the `separator.
    /// Repeatedly applies `item until it fails or the number of applications
    /// reaches the threshold specified by the <see cref="QuantificationRule"/>.
    /// Returns the list of values obtained by the successful applications
    /// of `item. At the end of the sequence, the `item must fail without consuming
    /// any input, otherwise <see cref="QuantifiedParser{TValue}"/> will fail
    /// with the failure reported by the `item. If the number of successful `item
    /// applications does not satisfy the quantification rule,
    /// <see cref="QuantifiedParser{TValue}"/> fails with a corresponding
    /// message. Regardless of the success, all input from successful
    /// applications of `item remain consumed.
    /// </summary>
    /// <typeparam name="TValue">The type of the parsed value.</typeparam>
    public class QuantifiedParser<TValue> : Parser<IList<TValue>>
    {
        private readonly IParser<TValue> _item;
        private readonly QuantificationRule _quantificationRule;
        private readonly int _n;
        private readonly int _m;
        private readonly IGeneralParser _separator;

        /// <summary>
        /// Creates a new instance of <see cref="QuantifiedParser{TValue}"/>.
        /// </summary>
        /// <param name="item">The ~item parser. Not null.</param>
        /// <param name="quantificationRule">Quantification rule.</param>
        /// <param name="n">N parameter of the quantification rule. Non-negative.</param>
        /// <param name="m">M parameter of the quantification rule. If used by the <paramref name="quantificationRule"/>,
        /// should be not less than N, othervise should be set to -1. ></param>
        /// <param name="separator">Optional item separator parser. Is null by default.</param>
        public QuantifiedParser(IParser<TValue> item, QuantificationRule quantificationRule, int n, int m = -1, IGeneralParser separator = null)
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
                        throw new ArgumentOutOfRangeException(nameof(m), "should not be less than nTimes");
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
        public override IReply<IList<TValue>> Parse(TokenStream tokens)
        {
            var oldPosition = tokens.Position;
            var reply = _item.Parse(tokens);
            var newPosition = reply.UnparsedTokens.Position;

            var times = 0;

            var list = new List<TValue>();

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
                            return new Failure<IList<TValue>>(
                                reply.UnparsedTokens,
                                FailureMessages.Empty.With(FailureMessage.Expected($"{_item.Expression} occurring no more than exactly {_n} times"))
                            );
                        break;
                    case QuantificationRule.NtoM:
                        if (times > _m)
                            return new Failure<IList<TValue>>(
                                reply.UnparsedTokens,
                                FailureMessages.Empty.With(FailureMessage.Expected($"{_item.Expression} occurring no more than between {_n} and {_m} times"))
                            );
                        break;
                    case QuantificationRule.NOrLess:
                        if (times > _n)
                            return new Failure<IList<TValue>>(
                                reply.UnparsedTokens,
                                FailureMessages.Empty.With(FailureMessage.Expected($"{_item.Expression} occurring no more than {_n} times"))
                            );
                        break;
                }

                list.Add(reply.ParsedValue);

                var unparsedTokens = reply.UnparsedTokens;

                if (separatorParserIsPresent)
                {
                    var positionBeforeSeparator = newPosition;

                    var separatorReply = _separator.ParseGenerally(reply.UnparsedTokens);

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
                            return Failure<TValue[]>.From(separatorReply);
                    }

                    newPosition = positionAfterSeparator;

                    separatorWasParsed = separatorReply.Success;
                }

                oldPosition = newPosition;

                if (separatorParserIsPresent && !separatorWasParsed)
                    break;

                reply = _item.Parse(unparsedTokens);

                if (!reply.Success && separatorParserIsPresent)
                    return new Failure<IList<TValue>>(reply.UnparsedTokens, reply.FailureMessages);

                newPosition = reply.UnparsedTokens.Position;
            }

            //The item parser finally failed or the separator parser parsed the next separator, but there was no item following it
            if (oldPosition != newPosition || separatorParserIsPresent && separatorWasParsed)
                return new Failure<IList<TValue>>(reply.UnparsedTokens, reply.FailureMessages);

            switch (_quantificationRule)
            {
                case QuantificationRule.NOrMore:
                    if (times < _n)
                        return new Failure<IList<TValue>>(
                            reply.UnparsedTokens,
                            FailureMessages.Empty.With(FailureMessage.Expected($"{_item.Expression} occurring {_n}+ times")));
                    break;
                case QuantificationRule.ExactlyN:
                    if (times != _n)
                        return new Failure<IList<TValue>>(
                            reply.UnparsedTokens,
                            FailureMessages.Empty.With(FailureMessage.Expected($"{_item.Expression} occurring no {(times > _n ? "more" : "less")} than exactly {_n} times")));
                    break;
                case QuantificationRule.NtoM:
                    if (times < _n)
                        return new Failure<IList<TValue>>(
                            reply.UnparsedTokens,
                            FailureMessages.Empty.With(FailureMessage.Expected($"{_item.Expression} occurring no less than between {_n} and {_m} times")));
                    break;
                case QuantificationRule.NOrLess:
                    if (times > _n)
                        return new Failure<IList<TValue>>(
                            reply.UnparsedTokens,
                            FailureMessages.Empty.With(FailureMessage.Expected($"{_item.Expression} occurring no more than {_n} times")));
                    break;
            }

            return new Success<IList<TValue>>(list, reply.UnparsedTokens, reply.FailureMessages);
        }

        /// <inheritdoc/>
        public override IGeneralReply ParseGenerally(TokenStream tokens)
        {
            if (tokens == null)
                throw new ArgumentNullException(nameof(tokens));

            var oldPosition = tokens.Position;
            var reply = _item.ParseGenerally(tokens);
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
                            return new GeneralFailure(
                                reply.UnparsedTokens,
                                FailureMessages.Empty.With(FailureMessage.Expected($"{_item.Expression} occurring no more than exactly {_n} times")));
                        break;
                    case QuantificationRule.NtoM:
                        if (times > _m)
                            return new GeneralFailure(
                                reply.UnparsedTokens,
                                FailureMessages.Empty.With(FailureMessage.Expected($"{_item.Expression} occurring no more than between {_n} and {_m} times")));
                        break;
                    case QuantificationRule.NOrLess:
                        if (times > _n)
                            return new GeneralFailure(
                                reply.UnparsedTokens,
                                FailureMessages.Empty.With(FailureMessage.Expected($"{_item.Expression} occurring no more than {_n} times")));
                        break;
                }

                var unparsedTokens = reply.UnparsedTokens;

                if (separatorParserIsPresent)
                {
                    var positionBeforeSeparator = newPosition;

                    var separatorReply = _separator.ParseGenerally(reply.UnparsedTokens);

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
                            return Failure<TValue[]>.From(separatorReply);
                    }

                    newPosition = positionAfterSeparator;

                    separatorWasParsed = separatorReply.Success;
                }

                oldPosition = newPosition;

                if (separatorParserIsPresent && !separatorWasParsed)
                    break;

                reply = _item.ParseGenerally(unparsedTokens);

                if (!reply.Success && separatorParserIsPresent)
                    return new GeneralFailure(reply.UnparsedTokens, reply.FailureMessages);

                newPosition = reply.UnparsedTokens.Position;
            }

            //The item parser finally failed or the separator parser parsed the next separator, but there was no item following it
            if (oldPosition != newPosition || separatorParserIsPresent && separatorWasParsed)
                return new GeneralFailure(reply.UnparsedTokens, reply.FailureMessages);

            switch (_quantificationRule)
            {
                case QuantificationRule.NOrMore:
                    if (times < _n)
                        return new GeneralFailure(
                            reply.UnparsedTokens,
                            FailureMessages.Empty.With(FailureMessage.Expected($"{_item.Expression} occurring {_n}+ times"))
                        );
                    break;
                case QuantificationRule.ExactlyN:
                    if (times != _n)
                        return new GeneralFailure(
                            reply.UnparsedTokens,
                            FailureMessages.Empty.With(FailureMessage.Expected(
                                $"{_item.Expression} occurring no {(times > _n ? "more" : "less")} than exactly {_n} times")
                            ));
                    break;
                case QuantificationRule.NtoM:
                    if (times < _n)
                        return new GeneralFailure(
                            reply.UnparsedTokens,
                            FailureMessages.Empty.With(FailureMessage.Expected($"{_item.Expression} occurring no less than between {_n} and {_m} times"))
                        );
                    break;
                case QuantificationRule.NOrLess:
                    if (times > _n)
                        return new GeneralFailure(
                            reply.UnparsedTokens,
                            FailureMessages.Empty.With(FailureMessage.Expected($"{_item.Expression} occurring no more than {_n} times"))
                        );
                    break;
            }

            return new GeneralSuccess(reply.UnparsedTokens);
        }

        /// <summary>
        /// Builds the parser expression.
        /// </summary>
        /// <returns>Expression string. Not null.</returns>
        protected override string BuildExpression()
        {
            switch (_quantificationRule)
            {
                case QuantificationRule.NtoM:
                    return $"<{_n} TO {_m} TIMES {_item.Expression}>";
                case QuantificationRule.ExactlyN:
                    return $"<{_n} TIMES {_item.Expression}>";
            }

            return $"<{_n}+ TIMES {_item.Expression}>";
        }
    }
}
