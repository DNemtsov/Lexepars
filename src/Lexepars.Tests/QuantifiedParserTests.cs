﻿using Lexepars.Parsers;
using Lexepars.Tests.Fixtures;
using Shouldly;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace Lexepars.Tests
{
    public abstract class BaseQuantifiedParserTests
    {
        protected const string Asterisc = "*";
        protected const string Separator = ",";
        protected static readonly MatchableTokenKind AsteriscToken = new OperatorTokenKind(Asterisc);
        protected static readonly MatchableTokenKind SeparatorToken = new OperatorTokenKind(Separator);
        protected static readonly Lexer AsteriscLexer = new Lexer(AsteriscToken, SeparatorToken);
        protected static readonly IParser<string> AsteriscParser = AsteriscToken.Lexeme();
        protected static readonly IGeneralParser SeparatorParser = SeparatorToken.Kind();

        protected static TokenStream AsteriscStream(int n, bool separated = false)
        {
            string GenerateAsteriscs(int nn)
            {
                var sb = new StringBuilder();

                for (var i = 0; i < nn; ++i)
                {
                    sb.Append(Asterisc);
                    if (separated && i < nn - 1)
                        sb.Append(Separator);
                }

                return sb.ToString();
            }

            return Tokenize(GenerateAsteriscs(n));
        }

        protected static TokenStream Tokenize(string text) => new TokenStream(AsteriscLexer.Tokenize(text));

        /// <summary>
        /// ZeroOrMore(p) repeatedly applies parser p until it fails, returing
        /// the list of values returned by successful applications of p.  At the
        /// end of the sequence, p must fail without consuming input, otherwise the
        /// sequence will return the failure reported by p.
        /// </summary>
        protected static IParser<IEnumerable<T>> ClassicZeroOrMore<T>(IParser<T> item)
        {
            return new ClassicZeroOrMoreParser<T>(item);
        }

        /// <summary>
        /// OneOrMore(p) behaves like ZeroOrMore(p), except that p must succeed at least one time.
        /// </summary>
        protected static IParser<IEnumerable<T>> ClassicOneOrMore<T>(IParser<T> item)
        {
            return from first in item
                   from rest in ClassicZeroOrMore(item)
                   select List(first, rest);
        }

        /// <summary>
        /// ZeroOrMore(p, s) parses zero or more occurrences of p separated by occurrences of s,
        /// returning the list of values returned by successful applications of p.
        /// </summary>
        protected static IParser<IEnumerable<T>> ClassicZeroOrMore<T>(IParser<T> item, IGeneralParser separator)
        {
            return Grammar.Choice(Grammar.OneOrMore(item, separator), new MonadicUnitParser<IEnumerable<T>>(Enumerable.Empty<T>()));
        }

        /// <summary>
        /// OneOrMore(p, s) behaves like ZeroOrMore(p, s), except that p must succeed at least one time.
        /// </summary>
        protected static IParser<IEnumerable<T>> ClassicOneOrMore<T, S>(IParser<T> item, IParser<S> separator)
        {
            return from first in item
                   from rest in ClassicZeroOrMore(from sep in separator
                                                  from next in item
                                                  select next)
                   select List(first, rest);
        }

        protected static IEnumerable<T> List<T>(T first, IEnumerable<T> rest)
        {
            yield return first;

            foreach (var item in rest)
                yield return item;
        }
    }

    public class QuantifiedParserTests : BaseQuantifiedParserTests
    {
        [Fact]
        public void NOrMore()
        {
            for (int n = 0; n < 10; ++n)
            {
                var parser = new QuantifiedParser<string>(AsteriscParser, QuantificationRule.NOrMore, n);

                for (var i = n; i < n + 15; ++i)
                {
                    var r = parser.Parse(AsteriscStream(i));

                    r.Success.ShouldBe(true);
                    r.ParsedValue.Count.ShouldBe(i);

                    parser.ParseGenerally(AsteriscStream(i)).Success.ShouldBe(true);
                }

                for (var i = n - 1; i >= 0; --i)
                {
                    parser.Parse(AsteriscStream(i)).Success.ShouldBe(false);
                    parser.ParseGenerally(AsteriscStream(i)).Success.ShouldBe(false);
                }
            }

            for (int n = 0; n < 10; ++n)
            {
                var parser = new QuantifiedParser<string>(AsteriscParser, QuantificationRule.NOrMore, n, -1, SeparatorParser);

                for (var i = n; i < n + 15; ++i)
                {
                    var r = parser.Parse(AsteriscStream(i, true));

                    r.Success.ShouldBe(true);
                    r.ParsedValue.Count.ShouldBe(i);

                    parser.ParseGenerally(AsteriscStream(i, true)).Success.ShouldBe(true);
                }

                for (var i = n - 1; i >= 0; --i)
                {
                    parser.Parse(AsteriscStream(i, true)).Success.ShouldBe(false);
                    parser.ParseGenerally(AsteriscStream(i, true)).Success.ShouldBe(false);
                }
            }
        }

        [Fact]
        public void ExactlyN()
        {
            for (int n = 0; n < 15; ++n)
            {
                var parser = new QuantifiedParser<string>(AsteriscParser, QuantificationRule.ExactlyN, n);

                for (var i = n - 1; i >= 0; --i)
                {
                    parser.Parse(AsteriscStream(i)).Success.ShouldBe(false);
                    parser.ParseGenerally(AsteriscStream(i)).Success.ShouldBe(false);
                }

                var r = parser.Parse(AsteriscStream(n));

                    r.Success.ShouldBe(true);
                    r.ParsedValue.Count.ShouldBe(n);

                parser.ParseGenerally(AsteriscStream(n)).Success.ShouldBe(true);

                for (var i = n + 1; i < n + 15; ++i)
                {
                    parser.Parse(AsteriscStream(i)).Success.ShouldBe(false);
                    parser.ParseGenerally(AsteriscStream(i)).Success.ShouldBe(false);
                }
            }

            for (int n = 0; n < 15; ++n)
            {
                var parser = new QuantifiedParser<string>(AsteriscParser, QuantificationRule.ExactlyN, n, -1, SeparatorParser);

                for (var i = n - 1; i >= 0; --i)
                {
                    parser.Parse(AsteriscStream(i, true)).Success.ShouldBe(false);
                    parser.ParseGenerally(AsteriscStream(i, true)).Success.ShouldBe(false);
                }

                var r = parser.Parse(AsteriscStream(n, true));

                r.Success.ShouldBe(true);
                r.ParsedValue.Count.ShouldBe(n);

                parser.ParseGenerally(AsteriscStream(n, true)).Success.ShouldBe(true);

                for (var i = n + 1; i < n + 15; ++i)
                {
                    parser.Parse(AsteriscStream(i, true)).Success.ShouldBe(false);
                    parser.ParseGenerally(AsteriscStream(i, true)).Success.ShouldBe(false);
                }
            }
        }

        [Fact]
        public void NtoM()
        {
            for (int n = 0; n < 15; ++n)
                for (int m = n; m < n + 10; ++m)
                {
                    var parser = new QuantifiedParser<string>(AsteriscParser, QuantificationRule.NtoM, n, m);

                    for (var i = 0; i < n; ++i)
                    {
                        parser.Parse(AsteriscStream(i)).Success.ShouldBe(false);
                        parser.ParseGenerally(AsteriscStream(i)).Success.ShouldBe(false);
                    }

                    for (var i = n; i <= m; ++i)
                    {
                        var r = parser.Parse(AsteriscStream(i));

                        r.Success.ShouldBe(true);
                        r.ParsedValue.Count.ShouldBe(i);

                        parser.ParseGenerally(AsteriscStream(i)).Success.ShouldBe(true);
                    }

                    for (var i = m + 1; i < m + 15; ++i)
                    {
                        parser.Parse(AsteriscStream(i)).Success.ShouldBe(false);
                        parser.ParseGenerally(AsteriscStream(i)).Success.ShouldBe(false);
                    }
                }

            for (int n = 0; n < 15; ++n)
                for (int m = n; m < n + 10; ++m)
                {
                    var parser = new QuantifiedParser<string>(AsteriscParser, QuantificationRule.NtoM, n, m, SeparatorParser);

                    for (var i = 0; i < n; ++i)
                    {
                        parser.Parse(AsteriscStream(i, true)).Success.ShouldBe(false);
                        parser.ParseGenerally(AsteriscStream(i, true)).Success.ShouldBe(false);
                    }

                    for (var i = n; i <= m; ++i)
                    {
                        var r = parser.Parse(AsteriscStream(i, true));

                        r.Success.ShouldBe(true);
                        r.ParsedValue.Count.ShouldBe(i);

                        parser.ParseGenerally(AsteriscStream(i, true)).Success.ShouldBe(true);
                    }

                    for (var i = m + 1; i < m + 15; ++i)
                    {
                        parser.Parse(AsteriscStream(i, true)).Success.ShouldBe(false);
                        parser.ParseGenerally(AsteriscStream(i, true)).Success.ShouldBe(false);
                    }
                }
        }

        [Fact]
        public void NOrLess()
        {
            for (int n = 0; n < 15; ++n)
            {
                var parser = new QuantifiedParser<string>(AsteriscParser, QuantificationRule.NOrLess, n);

                for (var i = 0; i <= n; ++i)
                {
                    var r = parser.Parse(AsteriscStream(i));

                    r.Success.ShouldBe(true);
                    r.ParsedValue.Count.ShouldBe(i);

                    parser.ParseGenerally(AsteriscStream(i)).Success.ShouldBe(true);
                }

                for (var i = n + 1; i <= n + 15; ++i)
                {
                    parser.Parse(AsteriscStream(i)).Success.ShouldBe(false);
                    parser.ParseGenerally(AsteriscStream(i)).Success.ShouldBe(false);
                }
            }

            for (int n = 0; n < 15; ++n)
            {
                var parser = new QuantifiedParser<string>(AsteriscParser, QuantificationRule.NOrLess, n, -1, SeparatorParser);

                for (var i = 0; i <= n; ++i)
                {
                    var r = parser.Parse(AsteriscStream(i, true));

                    r.Success.ShouldBe(true);
                    r.ParsedValue.Count.ShouldBe(i);

                    parser.ParseGenerally(AsteriscStream(i, true)).Success.ShouldBe(true);
                }

                for (var i = n + 1; i <= n + 15; ++i)
                {
                    parser.Parse(AsteriscStream(i, true)).Success.ShouldBe(false);
                    parser.ParseGenerally(AsteriscStream(i, true)).Success.ShouldBe(false);
                }
            }
        }

        [Fact]
        public void HandlesSeparatorErrors()
        {
            var classic = ClassicZeroOrMore(AsteriscParser, SeparatorParser);
            var modern = Grammar.ZeroOrMore(AsteriscParser, SeparatorParser);

            {
                var str = "**";

                var r = classic.Parse(Tokenize(str));

                r.Success.ShouldBe(true);
                r.ParsedValue.Count().ShouldBe(1);
                r.UnparsedTokens.Position.ShouldBe(new Position(1, 2));

                var rr = modern.Parse(Tokenize(str));

                rr.Success.ShouldBe(r.Success);
                rr.ParsedValue.Count.ShouldBe(r.ParsedValue.Count());
                rr.UnparsedTokens.Position.ShouldBe(r.UnparsedTokens.Position);
            }
            {
                var str = "*,";

                var r = classic.Parse(Tokenize(str));
                r.Success.ShouldBe(false);
                r.UnparsedTokens.Position.ShouldBe(new Position(1, 3));

                var rr = modern.Parse(Tokenize(str));

                rr.Success.ShouldBe(r.Success);
                rr.UnparsedTokens.Position.ShouldBe(r.UnparsedTokens.Position);
            }
            {
                var str = "*,*";

                var r = classic.Parse(Tokenize(str));
                r.Success.ShouldBe(true);
                r.ParsedValue.Count().ShouldBe(2);
                r.UnparsedTokens.Position.ShouldBe(new Position(1, 4));

                var rr = modern.Parse(Tokenize(str));

                rr.Success.ShouldBe(r.Success);
                rr.ParsedValue.Count.ShouldBe(r.ParsedValue.Count());
                rr.UnparsedTokens.Position.ShouldBe(r.UnparsedTokens.Position);
            }
            {
                var str = ",*";

                var r = classic.Parse(Tokenize(str));

                r.Success.ShouldBe(true);
                r.ParsedValue.Count().ShouldBe(0);
                r.UnparsedTokens.Position.ShouldBe(new Position(1, 1));

                var rr = modern.Parse(Tokenize(str));

                rr.Success.ShouldBe(r.Success);
                rr.ParsedValue.Count.ShouldBe(r.ParsedValue.Count());
                rr.UnparsedTokens.Position.ShouldBe(r.UnparsedTokens.Position);
            }
        }
    }
}
