using Lexepars.Parsers;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Lexepars.ParsersAsync
{
    public delegate IAsyncParser<T> ExtendParserBuilder<T>(T left);
    public delegate T AtomNodeBuilder<out T>(string atom);
    public delegate T UnaryNodeBuilder<T>(string symbol, T operand);
    public delegate T BinaryNodeBuilder<T>(T left, string symbol, T right);

    /// <summary>
    /// Behaves like <see cref="OperatorPrecedenceParser{TValue}"/>, except could be used with parallel parsers.
    /// </summary>
    public enum Associativity
    {
        /// <summary>
        /// If op is o, then A o B o C becomes ((A o B) o C)
        /// </summary>
        Left,
        /// <summary>
        /// If op is o, then A o B o C becomes (A o (B o C))
        /// </summary>
        Right
    }

    /// <summary>
    /// Parses expressions comprised of atoms (e.g. constants, variable names), unary (prefix and postfix), binary (left and right associative) operators, grouping units (e.g. parentheses)
    /// </summary>
    /// <typeparam name="TValue">The type of the parsed value.</typeparam>
    public class OperatorPrecedenceParserAsync<TValue> : AsyncParser<TValue>
    {
        private readonly IDictionary<TokenKind, IAsyncParser<TValue>> _unitParsers;
        private readonly IDictionary<TokenKind, ExtendParserBuilder<TValue>> _extendParsers;
        private readonly IDictionary<TokenKind, int> _extendParserPrecedence;

        /// <summary>
        /// Creates a new instance of <see cref="OperatorPrecedenceParserAsync{TValue}"/>.
        /// </summary>
        public OperatorPrecedenceParserAsync()
        {
            _unitParsers = new Dictionary<TokenKind, IAsyncParser<TValue>>();
            _extendParsers = new Dictionary<TokenKind, ExtendParserBuilder<TValue>>();
            _extendParserPrecedence = new Dictionary<TokenKind, int>();
        }

        /// <summary>
        /// Registers a grouping unit, e.g. opening parenthesis
        /// </summary>
        /// <param name="kind">Grouping unit token kind.</param>
        /// <param name="unitParser">Unit parser.</param>
        public void Unit(TokenKind kind, IAsyncParser<TValue> unitParser)
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
            Unit(kind, kind.BindLexemeAsync(l => atomNodeBuilder(l)));
        }

        /// <summary>
        /// Registers a prefix unary operator.
        /// </summary>
        /// <param name="operatorTokenKind">Operator token kind.</param>
        /// <param name="precedence">Precedence. The bigger the number the more priority in the expression the operator has.</param>
        /// <param name="unaryNodeBuilder">Unary node builder function.</param>
        public void Prefix(TokenKind operatorTokenKind, int precedence, UnaryNodeBuilder<TValue> unaryNodeBuilder)
        {
            Unit(operatorTokenKind, from symbol in operatorTokenKind.LexemeAsync() from operand in OperandAtPrecedenceLevel(precedence) select unaryNodeBuilder(symbol, operand));
        }

        /// <summary>
        /// Registers an operator extension.
        /// </summary>
        /// <param name="operatorTokenKind">Operator token kind.</param>
        /// <param name="precedence"></param>
        /// <param name="createExtendParser"></param>
        public void Extend(TokenKind operatorTokenKind, int precedence, ExtendParserBuilder<TValue> createExtendParser)
        {
            _extendParsers[operatorTokenKind] = createExtendParser;
            _extendParserPrecedence[operatorTokenKind] = precedence;
        }

        /// <summary>
        /// Registers a postfix unary operator.
        /// </summary>
        /// <param name="operatorTokenKind">Operator token kind.</param>
        /// <param name="precedence">Precedence. The bigger the number the more priority in the expression the operator has.</param>
        /// <param name="unaryNodeBuilder">Unary node builder function.</param>
        public void Postfix(TokenKind operatorTokenKind, int precedence, UnaryNodeBuilder<TValue> unaryNodeBuilder)
        {
            Extend(operatorTokenKind, precedence, left => from symbol in operatorTokenKind.LexemeAsync() select unaryNodeBuilder(symbol, left));
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

            Extend(kind, precedence, left => from symbol in kind.LexemeAsync() from right in OperandAtPrecedenceLevel(rightOperandPrecedence) select binaryNodeBuilder(left, symbol, right));

        }

        /// <inheritdoc/>
        public override Task<IReply<TValue>> ParseAsync(TokenStream tokens, CancellationToken cancellationToken) => Parse(tokens, 0);

        private IAsyncParser<TValue> OperandAtPrecedenceLevel(int precedence)
        {
            return new LambdaParserAsync<TValue>(tokens => Parse(tokens, precedence));
        }
        private async Task<IReply<TValue>> Parse(TokenStream tokens, int precedence)
        {
            var cts = new CancellationTokenSource();
            var token = tokens.Current;

            if (!_unitParsers.ContainsKey(token.Kind))
                return new Failure<TValue>(tokens, FailureMessage.Unknown());

            var reply = await _unitParsers[token.Kind].ParseAsync(tokens, cts.Token);

            if (!reply.Success)
                return reply;

            tokens = reply.UnparsedTokens;
            token = tokens.Current;

            while (precedence < GetPrecedence(token))
            {
                //Continue parsing at this precedence level.

                reply = await _extendParsers[token.Kind](reply.ParsedValue).ParseAsync(tokens, cts.Token);

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

        /// <inheritdoc/>
        protected override string BuildExpression() => "<OPP>";
    }
}
