using System;
using System.Collections.Generic;

namespace Lexepars.Parsers
{
    public class NameValuePairParser<TName, TValue> : Parser<KeyValuePair<TName, TValue>>
    {
        public NameValuePairParser(IParser<TName> name, IGeneralParser delimiter, IParser<TValue> value)
        {
            _name = name ?? throw new ArgumentNullException(nameof(name));
            _delimiter = delimiter ?? throw new ArgumentNullException(nameof(delimiter));
            _value = value ?? throw new ArgumentNullException(nameof(value));
        }

        protected override string BuildExpression() => $"<N {_name.Expression} D {_delimiter.Expression} V {_value.Expression}>";

        public override IReply<KeyValuePair<TName, TValue>> Parse(TokenStream tokens)
        {
            var name = _name.Parse(tokens);

            if (!name.Success)
                return Failure<KeyValuePair<TName, TValue>>.From(name);

            var delimiter = _delimiter.ParseGenerally(name.UnparsedTokens);

            if (!delimiter.Success)
                return Failure<KeyValuePair<TName, TValue>>.From(delimiter);

            var value = _value.Parse(delimiter.UnparsedTokens);

            if (!value.Success)
                return Failure<KeyValuePair<TName, TValue>>.From(value);

            return new Success<KeyValuePair<TName, TValue>>(new KeyValuePair<TName, TValue>(name.ParsedValue, value.ParsedValue), value.UnparsedTokens, value.FailureMessages);
        }

        public override IGeneralReply ParseGenerally(TokenStream tokens)
        {
            var name = _name.ParseGenerally(tokens);

            if (!name.Success)
                return name;

            var delimiter = _delimiter.ParseGenerally(name.UnparsedTokens);

            if (!delimiter.Success)
                return delimiter;

            return _value.Parse(delimiter.UnparsedTokens);
        }

        private readonly IParser<TName> _name;
        private readonly IGeneralParser _delimiter;
        private readonly IParser<TValue> _value;
    }
}
