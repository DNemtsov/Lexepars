using Lexepars.Parsers;
using System;
using System.Linq;
using System.Reflection;

namespace Lexepars
{
    /// <summary>
    /// Base class offering shorthand methods to combine parsers and specify rules building grammars.
    /// Is complemented by the token kind parsing extension methods contained in <see cref="TokenKindExtensions"/>.
    /// </summary>
    /// <remarks>Serves as the hub of all parser combinator functionality.</remarks>
    public abstract class Grammar
    {
        /// <summary>
        /// Creates a new instance of <see cref="ChoiceParser{TValue}"/>.
        /// </summary>
        /// <typeparam name="TValue">The type of parsed value.</typeparam>
        /// <param name="parsers">The alternative parsers. Not null. Not empty.</param>
        /// <returns>The new instance of <see cref="ChoiceParser{TValue}"/>. Not null.</returns>
        public static ChoiceParser<TValue> Choice<TValue>(params IParser<TValue>[] parsers) => new ChoiceParser<TValue>(parsers);

        /// <summary>
        /// Creates a new instance of <see cref="LabeledParser{TValue}"/>.
        /// </summary>
        /// <typeparam name="TValue">The type of the parsed value.</typeparam>
        /// <param name="parser">The `p parser. Not null.</param>
        /// <param name="expectation">Expectation message. Not null.</param>
        /// <returns>The new instance of <see cref="LabeledParser{TValue}"/>. Not null.</returns>
        public static LabeledParser<TValue> Label<TValue>(IParser<TValue> item, string expectation) => new LabeledParser<TValue>(item, expectation);

        /// <summary>
        /// Creates a new instance of <see cref="AttemptParser{TValue}"/>.
        /// </summary>
        /// <typeparam name="TValue">The type of parsed value.</typeparam>
        /// <param name="parser">The attempted parser. Not null.</param>
        /// <returns>The new instance of <see cref="AttemptParser{TValue}"/>. Not null.</returns>
        public static AttemptParser<TValue> Attempt<TValue>(IParser<TValue> item) => new AttemptParser<TValue>(item);

        /// <summary>
        /// Creates a new instance of <see cref="QuantifiedParser{TValue}"/> configured to <see cref="QuantificationRule.NOrMore"/> with N being 0.
        /// </summary>
        /// <typeparam name="TValue">The type of the parsed value.</typeparam>
        /// <param name="item">The item parser. Not null.</param>
        /// <param name="itemSeparator">Optional item separator parser. Is null by default.</param>
        /// <returns>The new instance of <see cref="QuantifiedParser{TValue}"/>. Not null.</returns>
        public static QuantifiedParser<TValue> ZeroOrMore<TValue>(IParser<TValue> item, IGeneralParser separator = null) => new QuantifiedParser<TValue>(item, QuantificationRule.NOrMore, 0, -1, separator);

        /// <summary>
        /// Creates a new instance of <see cref="QuantifiedParser{TValue}"/> configured to <see cref="QuantificationRule.NOrMore"/> with N being 1.
        /// </summary>
        /// <typeparam name="TValue">The type of the parsed value.</typeparam>
        /// <param name="item">The item parser. Not null.</param>
        /// <param name="itemSeparator">Optional item separator parser. Is null by default.</param>
        /// <returns>The new instance of <see cref="QuantifiedParser{TValue}"/>. Not null.</returns>
        public static QuantifiedParser<TValue> OneOrMore<TValue>(IParser<TValue> item, IGeneralParser separator = null) => new QuantifiedParser<TValue>(item, QuantificationRule.NOrMore, 1, -1, separator);

        /// <summary>
        /// Creates a new instance of <see cref="QuantifiedParser{TValue}"/> configured to <see cref="QuantificationRule.NOrMore"/>.
        /// </summary>
        /// <typeparam name="TValue">The type of the parsed value.</typeparam>
        /// <param name="n">The minimum number of occurrences of the `item. Non-negative.</param>
        /// <param name="item">The `item parser. Not null.</param>
        /// <param name="itemSeparator">Optional item separator parser. Is null by default.</param>
        /// <returns>The new instance of <see cref="QuantifiedParser{TValue}"/>. Not null.</returns>
        public static QuantifiedParser<TValue> NOrMore<TValue>(int n, IParser<TValue> item, IGeneralParser separator = null) => new QuantifiedParser<TValue>(item, QuantificationRule.NOrMore, n, -1, separator);

        /// <summary>
        /// Creates a new instance of <see cref="QuantifiedParser{TValue}"/> configured to <see cref="QuantificationRule.NOrLess"/>.
        /// </summary>
        /// <typeparam name="TValue">The type of the parsed value.</typeparam>
        /// <param name="n">The maximum number of occurrences of the `item. Non-negative.</param>
        /// <param name="item">The `item parser. Not null.</param>
        /// <param name="itemSeparator">Optional item separator parser. Is null by default.</param>
        /// <returns>The new instance of <see cref="QuantifiedParser{TValue}"/>. Not null.</returns>
        public static QuantifiedParser<TValue> NOrLess<TValue>(int n, IParser<TValue> item, IGeneralParser separator = null) => new QuantifiedParser<TValue>(item, QuantificationRule.NOrLess, n, -1, separator);

        /// <summary>
        /// Creates a new instance of <see cref="QuantifiedParser{TValue}"/> configured to <see cref="QuantificationRule.NtoM"/>.
        /// </summary>
        /// <typeparam name="TValue">The type of the parsed value.</typeparam>
        /// <param name="n">The minimum number of occurrences of the `item. Non-negative.</param>
        /// <param name="m">The maximum number of occurrences of the `item. Non-negative.</param>
        /// <param name="item">The `item parser. Not null.</param>
        /// <param name="itemSeparator">Optional item separator parser. Is null by default.</param>
        /// <returns>The new instance of <see cref="QuantifiedParser{TValue}"/>. Not null.</returns>
        public static QuantifiedParser<TValue> NToM<TValue>(int n, int m, IParser<TValue> item, IGeneralParser separator = null) => new QuantifiedParser<TValue>(item, QuantificationRule.NtoM, n, m, separator);

        /// <summary>
        /// Creates a new instance of <see cref="QuantifiedParser{TValue}"/> configured to <see cref="QuantificationRule.ExactlyN"/>.
        /// </summary>
        /// <typeparam name="TValue">The type of the parsed value.</typeparam>
        /// <param name="n">The exact number of occurrences of the `item. Non-negative.</param>
        /// <param name="item">The `item parser. Not null.</param>
        /// <param name="itemSeparator">Optional item separator parser. Is null by default.</param>
        /// <returns>The new instance of <see cref="QuantifiedParser{TValue}"/>. Not null.</returns>
        public static QuantifiedParser<TValue> ExactlyN<TValue>(int n, IParser<TValue> item, IGeneralParser separator = null) => new QuantifiedParser<TValue>(item, QuantificationRule.ExactlyN, n, -1, separator);

        /// <summary>
        /// Creates a new instance of <see cref="BetweenParser{TValue}"/>.
        /// </summary>
        /// <typeparam name="TValue">The type of the parsed value.</typeparam>
        /// <param name="left">General parser of the left part. Not null.</param>
        /// <param name="item">Item parser. Not null.</param>
        /// <param name="right">General parser of the right part. Not null.</param>
        /// <returns>The new instance of <see cref="BetweenParser{TValue}"/>. Not null.</returns>
        public static BetweenParser<TValue> Between<TValue>(IGeneralParser left, IParser<TValue> item, IGeneralParser right) => new BetweenParser<TValue>(left, item, right);

        /// <summary>
        /// Creates a new instance of <see cref="OptionalParser{TValue}"/>.
        /// </summary>
        /// <typeparam name="TValue">The type of parsed value.</typeparam>
        /// <param name="parser">The `p parser. Not null.</param>
        /// <param name="defaultValue">The default value that is returned in case `p fails.</param>
        /// <returns>The new instance of <see cref="OptionalParser{TValue}"/>. Not null.</returns>
        public static OptionalParser<TValue> Optional<TValue>(IParser<TValue> parser, TValue defaultValue = default(TValue)) => new OptionalParser<TValue>(parser, defaultValue);

        /// <summary>
        /// Creates a new instance of <see cref="NameValuePairParser{TName, TValue}"/>.
        /// </summary>
        /// <typeparam name="TName">The type of the parsed name.</typeparam>
        /// <typeparam name="TValue">The type of the parsed value.</typeparam>
        /// <param name="name">The `name parser. Not null.</param>
        /// <param name="delimiter">The `delimiter parser. Not null.</param>
        /// <param name="value">The `value parser. Not null.</param>
        /// <returns></returns>
        public static NameValuePairParser<TName, TValue> NameValuePair<TName, TValue>(IParser<TName> name, IGeneralParser delimiter, IParser<TValue> value)
            => new NameValuePairParser<TName, TValue>(name, delimiter, value);

        public static TakeSkipParser<TResult> OccupiesEntireInput<TResult>(IParser<TResult> parser) => new TakeSkipParser<TResult>(parser, EndOfInput);

        /// <summary>
        /// Creates a new instance of <see cref="SkipParser"/>.
        /// </summary>
        /// <param name="items">The item parsers. Not null. Not empty.</param>
        /// <returns>The new instance of <see cref="TokenByKindParser"/>. Not null.</returns>
        public static SkipParser Skip(params IGeneralParser[] items) => new SkipParser(items);

        /// <summary>
        /// Creates a new instance of <see cref="TokenByKindParser"/> expecting <see cref="TokenKind.EndOfInput"/>.
        /// </summary>
        /// <returns>The new instance of <see cref="TokenByKindParser"/>. Not null.</returns>
        public static TokenByKindParser EndOfInput => new TokenByKindParser(TokenKind.EndOfInput);

        /// <summary>
        ///  Creates a new instance of <see cref="FailingParser{TValue}"/>.
        /// </summary>
        /// <typeparam name="TValue">The type of the parsed value.</typeparam>
        /// <returns>The new instance of <see cref="FailingParser{TValue}"/>. Not null.</returns>
        public static FailingParser<TValue> Fail<TValue>() => new FailingParser<TValue>();

        /// <summary>
        ///  Creates a new instance of <see cref="UnorderedParser{TValue}"/>.
        /// </summary>
        /// <param name="separator">Item separator. Not null.</param>
        /// <param name="items">The item parsers. Not null. Not empty.</param>
        /// <returns>The new instance of <see cref="UnorderedParser{TValue}"/>. Not null.</returns>
        public static UnorderedParser<TValue> Unordered<TValue>(IGeneralParser separator, UnorderedParsingMode mode, params IParser<TValue>[] items) => new UnorderedParser<TValue>(separator, mode, items);

        /// <summary>
        ///  Creates a new instance of <see cref="UnorderedParser{TValue}"/>.
        /// </summary>
        /// <param name="mode">Parsing mode.</param>
        /// <param name="items">The item parsers. Not null. Not empty.</param>
        /// <returns>The new instance of <see cref="UnorderedParser{TValue}"/>. Not null.</returns>
        public static UnorderedParser<TValue> Unordered<TValue>(UnorderedParsingMode mode = UnorderedParsingMode.RequireAllItems, params IParser<TValue>[] items) => new UnorderedParser<TValue>(mode, items);

        protected void InferGrammarRuleNames()
        {
            var rules =
                GetType()
                    .GetRuntimeFields()
                    .Where(grammarRuleField => grammarRuleField.FieldType.GetInterface(nameof(INamedInternal)) != null)
                    .Select(grammarRuleField => new { Rule = (INamedInternal)grammarRuleField.GetValue(this), grammarRuleField.Name })
                    .Where(ruleName => ruleName.Rule != null);

            foreach (var rule in rules)
            {
                if (rule.Rule.Name == null)
                    rule.Rule.SetName(rule.Name);
            }
        }
    }

    public abstract class Grammar<T> : Grammar, IParser<T>
    {
        public IReply<T> Parse(TokenStream tokens) => EntryRule.Parse(tokens);

        public IGeneralReply ParseGenerally(TokenStream tokens) => EntryRule.ParseGenerally(tokens);

        public string Name { get; }

        protected Grammar(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        protected Grammar(string name, GrammarRule<T> entryRule)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            _entryRule = entryRule ?? throw new ArgumentNullException(nameof(entryRule));
        }

        private GrammarRule<T> _entryRule;

        public GrammarRule<T> EntryRule
        {
            get => _entryRule;

            protected set
            {
                if (_entryRule != null)
                    throw new InvalidOperationException($"Rule {Expression} is already initialized with {_entryRule.Expression}.");

                _entryRule = value ?? throw new ArgumentNullException(nameof(value));
            }
        }



        public string Expression => EntryRule?.Expression;
    }
}