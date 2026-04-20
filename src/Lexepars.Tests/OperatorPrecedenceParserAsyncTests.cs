namespace Lexepars.Tests
{
    using Lexepars.ParsersAsync;
    using Lexepars.TestFixtures;
    using Shouldly;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;
    using Xunit;

    public class OperatorPrecedenceParserAsyncTests : Grammar
    {
        readonly OperatorPrecedenceParserAsync<IExpression> expression;

        public OperatorPrecedenceParserAsyncTests()
        {
            expression = new OperatorPrecedenceParserAsync<IExpression>();

            expression.Atom(SampleLexer.Digit, digit => new Constant(int.Parse(digit, CultureInfo.InvariantCulture)));
            expression.Atom(SampleLexer.Name, name => new Identifier(name));

            expression.Unit(SampleLexer.LeftParen, BetweenAsync(SampleLexer.LeftParen.KindAsync(), expression, SampleLexer.RightParen.KindAsync()));

            expression.Binary(SampleLexer.Add, 3, (left, symbol, right) => new Form(symbol, left, right));
            expression.Binary(SampleLexer.Subtract, 3, (left, symbol, right) => new Form(symbol, left, right));
            expression.Binary(SampleLexer.Multiply, 4, (left, symbol, right) => new Form(symbol, left, right));
            expression.Binary(SampleLexer.Divide, 4, (left, symbol, right) => new Form(symbol, left, right));
            expression.Binary(SampleLexer.Exponent, 5, (left, symbol, right) => new Form(symbol, left, right), Associativity.Right);
            expression.Prefix(SampleLexer.Subtract, 6, (subtract, operand) => new Form(new Identifier(subtract), operand));
            expression.Postfix(SampleLexer.Increment, 7, (increment, operand) => new Form(new Identifier(increment), operand));
            expression.Postfix(SampleLexer.Decrement, 7, (decrement, operand) => new Form(new Identifier(decrement), operand));            

            expression.Extend(SampleLexer.LeftParen, 8, callable =>
                                 from arguments in BetweenAsync(SampleLexer.LeftParen.KindAsync(), ZeroOrMoreAsync(expression, SampleLexer.Comma.KindAsync()), SampleLexer.RightParen.KindAsync())
                                 select (IExpression)new Form(callable, arguments));
        }

        [Fact]
        public async Task ParsesRegisteredTokensIntoCorrespondingAtoms()
        {
            await Parses("1", "1");
            await Parses("square", "square");
        }

        [Fact]
        public async Task ParsesUnitExpressionsStartedByRegisteredTokens()
        {
            await Parses("(0)", "0");
            await Parses("(square)", "square");
            await Parses("(1+4)/(2-3)*4", "(* (/ (+ 1 4) (- 2 3)) 4)");
        }

        [Fact]
        public async Task ParsesPrefixExpressionsStartedByRegisteredToken()
        {
            await Parses("-1", "(- 1)");
            await Parses("-(-1)", "(- (- 1))");
        }

        [Fact]
        public async Task ParsesPostfixExpressionsEndedByRegisteredToken()
        {
            await Parses("1++", "(++ 1)");
            await Parses("1++--", "(-- (++ 1))");
        }

        [Fact]
        public async Task ParsesExpressionsThatExtendTheLeftSideExpressionWhenTheRegisteredTokenIsEncountered()
        {
            await Parses("square(1)", "(square 1)");
            await Parses("square(1,2)", "(square 1 2)");
        }

        [Fact]
        public async Task ParsesBinaryOperationsRespectingPrecedenceAndAssociativity()
        {
            await Parses("1+2", "(+ 1 2)");
            await Parses("1-2", "(- 1 2)");
            await Parses("1*2", "(* 1 2)");
            await Parses("1/2", "(/ 1 2)");
            await Parses("1^2", "(^ 1 2)");

            await Parses("1+2+3", "(+ (+ 1 2) 3)");
            await Parses("1-2-3", "(- (- 1 2) 3)");
            await Parses("1*2*3", "(* (* 1 2) 3)");
            await Parses("1/2/3", "(/ (/ 1 2) 3)");
            await Parses("1^2^3", "(^ 1 (^ 2 3))");

            await Parses("1*2/3-4", "(- (/ (* 1 2) 3) 4)");
            await Parses("1/2*3-4", "(- (* (/ 1 2) 3) 4)");
            await Parses("1+2-3*4", "(- (+ 1 2) (* 3 4))");
            await Parses("1-2+3*4", "(+ (- 1 2) (* 3 4))");
            await Parses("1^2^3*4", "(* (^ 1 (^ 2 3)) 4)");
            await Parses("1^2^3*4", "(* (^ 1 (^ 2 3)) 4)");
            await Parses("1*2/3^4", "(/ (* 1 2) (^ 3 4))");
            await Parses("1^2+3^4", "(+ (^ 1 2) (^ 3 4))");
        }

        [Fact]
        public async Task ProvidesErrorAtAppropriatePositionWhenUnitParsersFail()
        {
            //Upon unit-parser failures, stop!

            //The "(" unit-parser is invoked but fails.  The next token, "*", has
            //high precedence, but that should not provoke parsing to continue.

            (await expression.FailsToParseAsync(Tokenize("(*"))).LeavingUnparsedTokens("*").WithMessage("(1, 2): Parsing failed.");
        }

        [Fact]
        public async Task ProvidesErrorAtAppropriatePositionWhenExtendParsersFail()
        {
            //Upon extend-parser failures, stop!

            //The "2" unit-parser succeeds.  The next token, "-" has
            //high-enough precedence to continue, so the "-" extend-parser
            //is invoked and immediately fails.  The next token, "*", has
            //high precedence, but that should not provoke parsing to continue.

            (await expression.FailsToParseAsync(Tokenize("2-*"))).LeavingUnparsedTokens("*").WithMessage("(1, 3): Parsing failed.");
        }

        async Task Parses(string input, string expectedTree)
        {
            var tks = new CancellationTokenSource();
            var r = await (expression.Parses(Tokenize(input)));
            r.WithValue(e => e.ToString().ShouldBe(expectedTree));
        }

        static IEnumerable<Token> Tokenize(string input) => new SampleLexer().Tokenize(input);

        class SampleLexer : Lexer
        {
            public static readonly MatchableTokenKind Digit = new PatternTokenKind("Digit", @"[0-9]");
            public static readonly MatchableTokenKind Name = new PatternTokenKind("Name", @"[a-z]+");
            public static readonly MatchableTokenKind Increment = new OperatorTokenKind("++");
            public static readonly MatchableTokenKind Decrement = new OperatorTokenKind("--");
            public static readonly MatchableTokenKind Add = new OperatorTokenKind("+");
            public static readonly MatchableTokenKind Subtract = new OperatorTokenKind("-");
            public static readonly MatchableTokenKind Multiply = new OperatorTokenKind("*");
            public static readonly MatchableTokenKind Divide = new OperatorTokenKind("/");
            public static readonly MatchableTokenKind Exponent = new OperatorTokenKind("^");
            public static readonly MatchableTokenKind LeftParen = new OperatorTokenKind("(");
            public static readonly MatchableTokenKind RightParen = new OperatorTokenKind(")");
            public static readonly MatchableTokenKind Comma = new OperatorTokenKind(",");

            public SampleLexer()
                : base(Digit, Name, Increment, Decrement, Add,
                       Subtract, Multiply, Divide, Exponent,
                       LeftParen, RightParen, Comma)
            { }
        }

        interface IExpression
        { }

        class Constant : IExpression
        {
            readonly int value;

            public Constant(int value)
            {
                this.value = value;
            }

            public override string ToString() => value.ToString(CultureInfo.InvariantCulture);
        }

        class Identifier : IExpression
        {
            readonly string identifier;

            public Identifier(string identifier)
            {
                this.identifier = identifier;
            }

            public override string ToString() => identifier;
        }

        class Form : IExpression
        {
            readonly IExpression head;
            readonly IEnumerable<IExpression> expressions;

            public Form(string head, params IExpression[] expressions)
                : this(new Identifier(head), expressions)
            {
            }

            public Form(IExpression head, params IExpression[] expressions)
                : this(head, (IEnumerable<IExpression>)expressions)
            {
            }

            public Form(IExpression head, IEnumerable<IExpression> expressions)
            {
                this.head = head;
                this.expressions = expressions;
            }

            public override string ToString() => $"({head} {string.Join(" ", expressions)})";
        }
    }
}