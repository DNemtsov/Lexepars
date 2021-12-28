using Lexepars.Parsers;
using Shouldly;
using System.Linq;
using System.Text;
using Xunit;

namespace Lexepars.Tests
{
    public class CountingQuantifiedParserTests : BaseQuantifiedParserTests
    {
        [Fact]
        public void NOrMore()
        {
            for (int n = 0; n < 10; ++n)
            {
                var parser = new CountingQuantifiedParser(AsteriscParser, QuantificationRule.NOrMore, n);

                for (var i = n; i < n + 15; ++i)
                {
                    var r = parser.Parse(AsteriscStream(i));

                    r.Success.ShouldBe(true);
                    r.ParsedValue.ShouldBe(i);

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
                var parser = new CountingQuantifiedParser(AsteriscParser, QuantificationRule.NOrMore, n, -1, SeparatorParser);

                for (var i = n; i < n + 15; ++i)
                {
                    var r = parser.Parse(AsteriscStream(i, true));

                    r.Success.ShouldBe(true);
                    r.ParsedValue.ShouldBe(i);

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
                var parser = new CountingQuantifiedParser(AsteriscParser, QuantificationRule.ExactlyN, n);

                for (var i = n - 1; i >= 0; --i)
                {
                    parser.Parse(AsteriscStream(i)).Success.ShouldBe(false);
                    parser.ParseGenerally(AsteriscStream(i)).Success.ShouldBe(false);
                }

                var r = parser.Parse(AsteriscStream(n));

                    r.Success.ShouldBe(true);
                    r.ParsedValue.ShouldBe(n);

                parser.ParseGenerally(AsteriscStream(n)).Success.ShouldBe(true);

                for (var i = n + 1; i < n + 15; ++i)
                {
                    parser.Parse(AsteriscStream(i)).Success.ShouldBe(false);
                    parser.ParseGenerally(AsteriscStream(i)).Success.ShouldBe(false);
                }
            }

            for (int n = 0; n < 15; ++n)
            {
                var parser = new CountingQuantifiedParser(AsteriscParser, QuantificationRule.ExactlyN, n, -1, SeparatorParser);

                for (var i = n - 1; i >= 0; --i)
                {
                    parser.Parse(AsteriscStream(i, true)).Success.ShouldBe(false);
                    parser.ParseGenerally(AsteriscStream(i, true)).Success.ShouldBe(false);
                }

                var r = parser.Parse(AsteriscStream(n, true));

                r.Success.ShouldBe(true);
                r.ParsedValue.ShouldBe(n);

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
                    var parser = new CountingQuantifiedParser(AsteriscParser, QuantificationRule.NtoM, n, m);

                    for (var i = 0; i < n; ++i)
                    {
                        parser.Parse(AsteriscStream(i)).Success.ShouldBe(false);
                        parser.ParseGenerally(AsteriscStream(i)).Success.ShouldBe(false);
                    }

                    for (var i = n; i <= m; ++i)
                    {
                        var r = parser.Parse(AsteriscStream(i));

                        r.Success.ShouldBe(true);
                        r.ParsedValue.ShouldBe(i);

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
                    var parser = new CountingQuantifiedParser(AsteriscParser, QuantificationRule.NtoM, n, m, SeparatorParser);

                    for (var i = 0; i < n; ++i)
                    {
                        parser.Parse(AsteriscStream(i, true)).Success.ShouldBe(false);
                        parser.ParseGenerally(AsteriscStream(i, true)).Success.ShouldBe(false);
                    }

                    for (var i = n; i <= m; ++i)
                    {
                        var r = parser.Parse(AsteriscStream(i, true));

                        r.Success.ShouldBe(true);
                        r.ParsedValue.ShouldBe(i);

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
                var parser = new CountingQuantifiedParser(AsteriscParser, QuantificationRule.NOrLess, n);

                for (var i = 0; i <= n; ++i)
                {
                    var r = parser.Parse(AsteriscStream(i));

                    r.Success.ShouldBe(true);
                    r.ParsedValue.ShouldBe(i);

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
                var parser = new CountingQuantifiedParser(AsteriscParser, QuantificationRule.NOrLess, n, -1, SeparatorParser);

                for (var i = 0; i <= n; ++i)
                {
                    var r = parser.Parse(AsteriscStream(i, true));

                    r.Success.ShouldBe(true);
                    r.ParsedValue.ShouldBe(i);

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
            var modern = Grammar.CountZeroOrMore(AsteriscParser, SeparatorParser);

            {
                var str = "**";

                var r = classic.Parse(Tokenize(str));

                r.Success.ShouldBe(true);
                r.ParsedValue.Count().ShouldBe(1);
                r.UnparsedTokens.Position.ShouldBe(new Position(1, 2));

                var rr = modern.Parse(Tokenize(str));

                rr.Success.ShouldBe(r.Success);
                rr.ParsedValue.ShouldBe(r.ParsedValue.Count());
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
                rr.ParsedValue.ShouldBe(r.ParsedValue.Count());
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
                rr.ParsedValue.ShouldBe(r.ParsedValue.Count());
                rr.UnparsedTokens.Position.ShouldBe(r.UnparsedTokens.Position);
            }
        }
    }
}
