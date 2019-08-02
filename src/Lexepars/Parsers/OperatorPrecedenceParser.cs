using System.Collections.Generic;

namespace Lexepars.Parsers
{
    public delegate IParser<T> ExtendParserBuilder<T>(T left);
    public delegate T AtomNodeBuilder<out T>(string atom);
    public delegate T UnaryNodeBuilder<T>(string symbol, T operand);
    public delegate T BinaryNodeBuilder<T>(T left, string symbol, T right);

    /// <summary>
    /// Operator associativity.
    /// For example, for left-associative operator + the expression 1 + 2 + 3 is equivalent to ((1 + 2) + 3);
    /// if + were right-associative, the same expression would be equivalent to (1 + (2 + 3)).
    /// </summary>
    public enum Associativity
    {
        Left,
        Right
    }

    /// <summary>
    /// Parses expressions comprised of atoms (e.g. constants, variable names), unary (prefix and postfix), binary (left and right associative) operators, grouping units (e.g. parentheses)
    /// </summary>
    /// <typeparam name="TValue">The type of the parsed value.</typeparam>
    public class OperatorPrecedenceParser<TValue> : Parser<TValue>
    {
        private readonly IDictionary<TokenKind, IParser<TValue>> _unitParsers;
        private readonly IDictionary<TokenKind, ExtendParserBuilder<TValue>> _extendParsers;
        private readonly IDictionary<TokenKind, int> _extendParserPrecedence;

        /// <summary>
        /// Creates a new instance of <see cref="OperatorPrecedenceParser{TValue}"/>.
        /// </summary>
        public OperatorPrecedenceParser()
        {
            _unitParsers = new Dictionary<TokenKind, IParser<TValue>>();
            _extendParsers = new Dictionary<TokenKind, ExtendParserBuilder<TValue>>();
            _extendParserPrecedence = new Dictionary<TokenKind, int>();
        }

        /// <summary>
        /// Registers a grouping unit, e.g. opening parenthesis
        /// </summary>
        /// <param name="kind">Grouping unit token kind.</param>
        /// <param name="unitParser">Unit parser.</param>
        public void Unit(TokenKind kind, IParser<TValue> unitParser)
        {
            _unitParsers[kind] = unitParser;
        }

        /// <summary>
        /// Registers an atom (e.g. numeric coefficient, variable name)
        /// </summary>
        /// <param name="kind">Atom unit token kind.</param>
        /// <param name="atomNodeBuilder">Atom node builder function.</param>
        public void Atom(TokenKind kind, AtomNodeBuilder<TValue> atomNodeBuilder)
        {
            Unit(kind, kind.BindLexeme(l => atomNodeBuilder(l)));
        }

        /// <summary>
        /// Registers a prefix unary operator.
        /// </summary>
        /// <param name="kind">Operator token kind.</param>
        /// <param name="precedence">Precedence. The bigger the number the more priority in the expression the operator has.</param>
        /// <param name="unaryNodeBuilder">Unary node builder function.</param>
        public void Prefix(TokenKind kind, int precedence, UnaryNodeBuilder<TValue> unaryNodeBuilder)
        {
            Unit(kind, from symbol in kind.Lexeme() from operand in OperandAtPrecedenceLevel(precedence) select unaryNodeBuilder(symbol, operand));
        }

        public void Extend(TokenKind operation, int precedence, ExtendParserBuilder<TValue> createExtendParser)
        {
            _extendParsers[operation] = createExtendParser;
            _extendParserPrecedence[operation] = precedence;
        }

        /// <summary>
        /// Registers a postfix unary operator.
        /// </summary>
        /// <param name="kind">Operator token kind.</param>
        /// <param name="precedence">Precedence. The bigger the number the more priority in the expression the operator has.</param>
        /// <param name="unaryNodeBuilder">Unary node builder function.</param>
        public void Postfix(TokenKind kind, int precedence, UnaryNodeBuilder<TValue> unaryNodeBuilder)
        {
            Extend(kind, precedence, left => from symbol in kind.Lexeme() select unaryNodeBuilder(symbol, left));
        }

        /// <summary>
        /// Registers a postfix unary operator.
        /// </summary>
        /// <param name="kind">Operator token kind.</param>
        /// <param name="precedence">Precedence. The bigger the number the more priority in the expression the operator has.</param>
        /// <param name="binaryNodeBuilder">Binary node builder function.</param>
        /// <param name="associativity">Operator associativity.</param>
        public void Binary(TokenKind kind, int precedence, BinaryNodeBuilder<TValue> binaryNodeBuilder, Associativity associativity = Associativity.Left)
        {
            var rightOperandPrecedence = associativity == Associativity.Left ? precedence : precedence - 1;

            Extend(kind, precedence, left => from symbol in kind.Lexeme() from right in OperandAtPrecedenceLevel(rightOperandPrecedence) select binaryNodeBuilder(left, symbol, right));
        }

        /// <summary>
        /// Parses the stream of tokens.
        /// </summary>
        /// <param name="tokens">Stream of tokens to parse. Not null.</param>
        /// <returns>Parsing reply. Not null.</returns>
        public override IReply<TValue> Parse(TokenStream tokens) => Parse(tokens, 0);

        private IParser<TValue> OperandAtPrecedenceLevel(int precedence) => new LambdaParser<TValue>(tokens => Parse(tokens, precedence));

        private IReply<TValue> Parse(TokenStream tokens, int precedence)
        {
            var token = tokens.Current;

            if (!_unitParsers.ContainsKey(token.Kind))
                return new Failure<TValue>(tokens, FailureMessage.Unknown());

            var reply = _unitParsers[token.Kind].Parse(tokens);

            if (!reply.Success)
                return reply;

            tokens = reply.UnparsedTokens;
            token = tokens.Current;

            while (precedence < GetPrecedence(token))
            {
                //Continue parsing at this precedence level.

                reply = _extendParsers[token.Kind](reply.ParsedValue).Parse(tokens);

                if (!reply.Success)
                    return reply;

                tokens = reply.UnparsedTokens;
                token = tokens.Current;
            }

            return reply;
        }

        private int GetPrecedence(Token token)
        {
            var kind = token.Kind;

            if (_extendParserPrecedence.ContainsKey(kind))
                return _extendParserPrecedence[kind];

            return 0;
        }

        protected override string BuildExpression() => "<OPP>";
    }
}
