using Lexepars.Parsers;
using System;

namespace Lexepars
{
    public class GrammarRule<T> : Parser<T>, INamedInternal
    {
        private IParser<T> _parser;

        public GrammarRule(string name = null)
        {
            _name = name;
        }

        protected override string BuildExpression() => _parser?.Expression;

        void INamedInternal.SetName(string name)
        {
            _name = name;
        }

        private string _name;

        public IParser<T> Rule
        {
            get => _parser;

            set
            {
                if (_parser != null)
                    throw new InvalidOperationException($"Rule {Expression} is already initialized with {_parser.Expression}.");

                _parser = value ?? throw new ArgumentNullException(nameof(value));
            }
        }

        public string Name => _name ?? Expression;

        public override IReply<T> Parse(TokenStream tokens)
        {
            if (_parser == null)
                throw new InvalidOperationException($"Rule {Expression} is not initialized.");

            return _parser.Parse(tokens);
        }
    }
}