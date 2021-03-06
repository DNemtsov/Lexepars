﻿namespace Lexepars.Tests
{
    using System.Collections.Generic;
    using System.Globalization;
    using Lexepars.Parsers;
    using Lexepars.TestFixtures;
    using Shouldly;
    using Xunit;

    public class OperatorPrecedenceParserTests : Grammar
    {
        readonly OperatorPrecedenceParser<IExpression> expression;

        public OperatorPrecedenceParserTests()
        {
            expression = new OperatorPrecedenceParser<IExpression>();

            expression.Atom(SampleLexer.Digit, digit => new Constant(int.Parse(digit, CultureInfo.InvariantCulture)));
            expression.Atom(SampleLexer.Name, name => new Identifier(name));

            expression.Unit(SampleLexer.LeftParen, Between(SampleLexer.LeftParen.Kind(), expression, SampleLexer.RightParen.Kind()));

            expression.Binary(SampleLexer.Add, 3, (left, symbol, right) => new Form(symbol, left, right));
            expression.Binary(SampleLexer.Subtract, 3, (left, symbol, right) => new Form(symbol, left, right));
            expression.Binary(SampleLexer.Multiply, 4, (left, symbol, right) => new Form(symbol, left, right));
            expression.Binary(SampleLexer.Divide, 4, (left, symbol, right) => new Form(symbol, left, right));
            expression.Binary(SampleLexer.Exponent, 5, (left, symbol, right) => new Form(symbol, left, right), Associativity.Right);
            expression.Prefix(SampleLexer.Subtract, 6, (subtract, operand) => new Form(new Identifier(subtract), operand));
            expression.Postfix(SampleLexer.Increment, 7, (increment, operand) => new Form(new Identifier(increment), operand));
            expression.Postfix(SampleLexer.Decrement, 7, (decrement, operand) => new Form(new Identifier(decrement), operand));

            expression.Extend(SampleLexer.LeftParen, 8, callable =>
                                 from arguments in Between(SampleLexer.LeftParen.Kind(), ZeroOrMore(expression, SampleLexer.Comma.Kind()), SampleLexer.RightParen.Kind())
                                 select new Form(callable, arguments));
        }

        [Fact]
        public void ParsesRegisteredTokensIntoCorrespondingAtoms()
        {
            Parses("1", "1");
            Parses("square", "square");
        }

        [Fact]
        public void ParsesUnitExpressionsStartedByRegisteredTokens()
        {
            Parses("(0)", "0");
            Parses("(square)", "square");
            Parses("(1+4)/(2-3)*4", "(* (/ (+ 1 4) (- 2 3)) 4)");
        }

        [Fact]
        public void ParsesPrefixExpressionsStartedByRegisteredToken()
        {
            Parses("-1", "(- 1)");
            Parses("-(-1)", "(- (- 1))");
        }

        [Fact]
        public void ParsesPostfixExpressionsEndedByRegisteredToken()
        {
            Parses("1++", "(++ 1)");
            Parses("1++--", "(-- (++ 1))");
        }

        [Fact]
        public void ParsesExpressionsThatExtendTheLeftSideExpressionWhenTheRegisteredTokenIsEncountered()
        {
            Parses("square(1)", "(square 1)");
            Parses("square(1,2)", "(square 1 2)");
        }

        [Fact]
        public void ParsesBinaryOperationsRespectingPrecedenceAndAssociativity()
        {
            Parses("1+2", "(+ 1 2)");
            Parses("1-2", "(- 1 2)");
            Parses("1*2", "(* 1 2)");
            Parses("1/2", "(/ 1 2)");
            Parses("1^2", "(^ 1 2)");

            Parses("1+2+3", "(+ (+ 1 2) 3)");
            Parses("1-2-3", "(- (- 1 2) 3)");
            Parses("1*2*3", "(* (* 1 2) 3)");
            Parses("1/2/3", "(/ (/ 1 2) 3)");
            Parses("1^2^3", "(^ 1 (^ 2 3))");

            Parses("1*2/3-4", "(- (/ (* 1 2) 3) 4)");
            Parses("1/2*3-4", "(- (* (/ 1 2) 3) 4)");
            Parses("1+2-3*4", "(- (+ 1 2) (* 3 4))");
            Parses("1-2+3*4", "(+ (- 1 2) (* 3 4))");
            Parses("1^2^3*4", "(* (^ 1 (^ 2 3)) 4)");
            Parses("1^2^3*4", "(* (^ 1 (^ 2 3)) 4)");
            Parses("1*2/3^4", "(/ (* 1 2) (^ 3 4))");
            Parses("1^2+3^4", "(+ (^ 1 2) (^ 3 4))");
        }

        [Fact]
        public void ProvidesErrorAtAppropriatePositionWhenUnitParsersFail()
        {
            //Upon unit-parser failures, stop!
            
            //The "(" unit-parser is invoked but fails.  The next token, "*", has
            //high precedence, but that should not provoke parsing to continue.
            
            expression.FailsToParse(Tokenize("(*")).LeavingUnparsedTokens("*").WithMessage("(1, 2): Parsing failed.");
        }

        [Fact]
        public void ProvidesErrorAtAppropriatePositionWhenExtendParsersFail()
        {
            //Upon extend-parser failures, stop!
            
            //The "2" unit-parser succeeds.  The next token, "-" has
            //high-enough precedence to continue, so the "-" extend-parser
            //is invoked and immediately fails.  The next token, "*", has
            //high precedence, but that should not provoke parsing to continue.

            expression.FailsToParse(Tokenize("2-*")).LeavingUnparsedTokens("*").WithMessage("(1, 3): Parsing failed.");
        }

        void Parses(string input, string expectedTree) => expression.Parses(Tokenize(input)).WithValue(e => e.ToString().ShouldBe(expectedTree));

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
            {}
        }

        interface IExpression
        {}

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