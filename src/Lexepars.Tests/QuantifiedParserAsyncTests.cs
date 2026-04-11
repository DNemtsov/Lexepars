using Lexepars.Async_Parsers;
using Lexepars.Parsers;
using Lexepars.Tests.Fixtures;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Lexepars.Tests
{
    public abstract class BaseQuantifiedParserAsyncTests
    {
        protected const string Asterisc = "*";
        protected const string Separator = ",";
        protected static readonly MatchableTokenKind AsteriscToken = new OperatorTokenKind(Asterisc);
        protected static readonly MatchableTokenKind SeparatorToken = new OperatorTokenKind(Separator);
        protected static readonly Lexer AsteriscLexer = new Lexer(AsteriscToken, SeparatorToken);
        protected static readonly IAsyncParser<string> AsteriscParser = AsteriscToken.LexemeAsync();
        protected static readonly IAsyncGeneralParser SeparatorParser = SeparatorToken.KindAsync();

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

    public class QuantifiedParserAsyncTests : BaseQuantifiedParserAsyncTests
    {
        [Fact]
        public async Task NOrMore()
        {
            var cancellationTokenSource = new CancellationTokenSource();
            for (int n = 0; n < 10; ++n)
            {
                var parser = new QuantifiedParserAsync<string>(AsteriscParser, QuantificationRule.NOrMore, n);

                for (var i = n; i < n + 15; ++i)
                {
                    var r = await parser.ParseAsync(AsteriscStream(i), cancellationTokenSource.Token);

                    r.Success.ShouldBe(true);
                    r.ParsedValue.Count.ShouldBe(i);

                    var r2 = await parser.ParseGenerallyAsync(AsteriscStream(i), cancellationTokenSource.Token);
                    r2.Success.ShouldBe(true);
                }

                for (var i = n - 1; i >= 0; --i)
                {
                    var r1 = await parser.ParseAsync(AsteriscStream(i), cancellationTokenSource.Token);
                    r1.Success.ShouldBe(false);
                    var r2 = await parser.ParseGenerallyAsync(AsteriscStream(i), cancellationTokenSource.Token);
                    r2.Success.ShouldBe(false);
                }
            }

            for (int n = 0; n < 10; ++n)
            {
                var parser = new QuantifiedParserAsync<string>(AsteriscParser, QuantificationRule.NOrMore, n, -1, SeparatorParser);

                for (var i = n; i < n + 15; ++i)
                {
                    var r = await parser.ParseAsync(AsteriscStream(i, true), cancellationTokenSource.Token);

                    r.Success.ShouldBe(true);
                    r.ParsedValue.Count.ShouldBe(i);

                    var r2 = await parser.ParseGenerallyAsync(AsteriscStream(i, true), cancellationTokenSource.Token);
                    r2.Success.ShouldBe(true);
                }

                for (var i = n - 1; i >= 0; --i)
                {
                    var r1 = await parser.ParseAsync(AsteriscStream(i, true), cancellationTokenSource.Token);
                    r1.Success.ShouldBe(false);
                    var r2 = await parser.ParseGenerallyAsync(AsteriscStream(i, true), cancellationTokenSource.Token);
                    r2.Success.ShouldBe(false);
                }
            }
        }

        [Fact]
        public async Task ExactlyN()
        {
            var cancellationTokenSource = new CancellationTokenSource();
            for (int n = 0; n < 15; ++n)
            {
                var parser = new QuantifiedParserAsync<string>(AsteriscParser, QuantificationRule.ExactlyN, n);

                for (var i = n - 1; i >= 0; --i)
                {
                    var r1 = await parser.ParseAsync(AsteriscStream(i), cancellationTokenSource.Token);
                    r1.Success.ShouldBe(false);
                    var r2 = await parser.ParseGenerallyAsync(AsteriscStream(i), cancellationTokenSource.Token);
                    r2.Success.ShouldBe(false);
                }

                var r = await parser.ParseAsync(AsteriscStream(n), cancellationTokenSource.Token);

                    r.Success.ShouldBe(true);
                    r.ParsedValue.Count.ShouldBe(n);

                var r3 = await parser.ParseGenerallyAsync(AsteriscStream(n), cancellationTokenSource.Token);
                r3.Success.ShouldBe(true);

                for (var i = n + 1; i < n + 15; ++i)
                {
                    var r1 = await parser.ParseAsync(AsteriscStream(i), cancellationTokenSource.Token);
                    r1.Success.ShouldBe(false);
                    var r2 = await parser.ParseGenerallyAsync(AsteriscStream(i), cancellationTokenSource.Token);
                    r2.Success.ShouldBe(false);
                }
            }

            for (int n = 0; n < 15; ++n)
            {
                var parser = new QuantifiedParserAsync<string>(AsteriscParser, QuantificationRule.ExactlyN, n, -1, SeparatorParser);

                for (var i = n - 1; i >= 0; --i)
                {
                    var r1 = await parser.ParseAsync(AsteriscStream(i, true), cancellationTokenSource.Token);
                    r1.Success.ShouldBe(false);
                    var r2 = await parser.ParseGenerallyAsync(AsteriscStream(i, true), cancellationTokenSource.Token);
                    r2.Success.ShouldBe(false);
                }

                var r = await parser.ParseAsync(AsteriscStream(n, true), cancellationTokenSource.Token);

                r.Success.ShouldBe(true);
                r.ParsedValue.Count.ShouldBe(n);

                var r3 = await parser.ParseGenerallyAsync(AsteriscStream(n, true), cancellationTokenSource.Token);
                r3.Success.ShouldBe(true);

                for (var i = n + 1; i < n + 15; ++i)
                {
                    var r1 = await parser.ParseAsync(AsteriscStream(i, true), cancellationTokenSource.Token);
                    r1.Success.ShouldBe(false);
                    var r2 = await parser.ParseGenerallyAsync(AsteriscStream(i, true), cancellationTokenSource.Token);
                    r2.Success.ShouldBe(false);
                }
            }
        }

        [Fact]
        public async Task NtoM()
        {
            var cancellationTokenSource = new CancellationTokenSource();
            for (int n = 0; n < 15; ++n)
                for (int m = n; m < n + 10; ++m)
                {
                    var parser = new QuantifiedParserAsync<string>(AsteriscParser, QuantificationRule.NtoM, n, m);

                    for (var i = 0; i < n; ++i)
                    {
                        var r1 = await parser.ParseAsync(AsteriscStream(i), cancellationTokenSource.Token);
                        r1.Success.ShouldBe(false);
                        var r2 = await parser.ParseGenerallyAsync(AsteriscStream(i), cancellationTokenSource.Token);
                        r2.Success.ShouldBe(false);
                    }

                    for (var i = n; i <= m; ++i)
                    {
                        var r = await parser.ParseAsync(AsteriscStream(i), cancellationTokenSource.Token);

                        r.Success.ShouldBe(true);
                        r.ParsedValue.Count.ShouldBe(i);

                        var r3 = await parser.ParseGenerallyAsync(AsteriscStream(i), cancellationTokenSource.Token);
                        r3.Success.ShouldBe(true);
                    }

                    for (var i = m + 1; i < m + 15; ++i)
                    {
                        var r1 = await parser.ParseAsync(AsteriscStream(i), cancellationTokenSource.Token);
                        r1.Success.ShouldBe(false);
                        var r2 = await parser.ParseGenerallyAsync(AsteriscStream(i), cancellationTokenSource.Token);
                        r2.Success.ShouldBe(false);
                    }
                }

            for (int n = 0; n < 15; ++n)
                for (int m = n; m < n + 10; ++m)
                {
                    var parser = new QuantifiedParserAsync<string>(AsteriscParser, QuantificationRule.NtoM, n, m, SeparatorParser);

                    for (var i = 0; i < n; ++i)
                    {
                        var r1 = await parser.ParseAsync(AsteriscStream(i, true), cancellationTokenSource.Token);
                        r1.Success.ShouldBe(false);
                        var r2 = await parser.ParseGenerallyAsync(AsteriscStream(i, true), cancellationTokenSource.Token);
                        r2.Success.ShouldBe(false);
                    }

                    for (var i = n; i <= m; ++i)
                    {
                        var r = await parser.ParseAsync(AsteriscStream(i, true), cancellationTokenSource.Token);

                        r.Success.ShouldBe(true);
                        r.ParsedValue.Count.ShouldBe(i);

                        var r3 = await parser.ParseGenerallyAsync(AsteriscStream(i, true), cancellationTokenSource.Token);
                        r3.Success.ShouldBe(true);
                    }

                    for (var i = m + 1; i < m + 15; ++i)
                    {
                        var r1 = await parser.ParseAsync(AsteriscStream(i, true), cancellationTokenSource.Token);
                        r1.Success.ShouldBe(false);
                        var r2 = await parser.ParseGenerallyAsync(AsteriscStream(i, true), cancellationTokenSource.Token);
                        r2.Success.ShouldBe(false);
                    }
                }
        }

        [Fact]
        public async Task NOrLess()
        {
            var cancellationTokenSource = new CancellationTokenSource();
            for (int n = 0; n < 15; ++n)
            {
                var parser = new QuantifiedParserAsync<string>(AsteriscParser, QuantificationRule.NOrLess, n);

                for (var i = 0; i <= n; ++i)
                {
                    var r = await parser.ParseAsync(AsteriscStream(i), cancellationTokenSource.Token);

                    r.Success.ShouldBe(true);
                    r.ParsedValue.Count.ShouldBe(i);

                    var r3 = await parser.ParseGenerallyAsync(AsteriscStream(i), cancellationTokenSource.Token);
                    r3.Success.ShouldBe(true);
                }

                for (var i = n + 1; i <= n + 15; ++i)
                {
                    var r1 = await parser.ParseAsync(AsteriscStream(i), cancellationTokenSource.Token);
                    r1.Success.ShouldBe(false);
                    var r2 = await parser.ParseGenerallyAsync(AsteriscStream(i), cancellationTokenSource.Token);
                    r2.Success.ShouldBe(false);
                }
            }

            for (int n = 0; n < 15; ++n)
            {
                var parser = new QuantifiedParserAsync<string>(AsteriscParser, QuantificationRule.NOrLess, n, -1, SeparatorParser);

                for (var i = 0; i <= n; ++i)
                {
                    var r = await parser.ParseAsync(AsteriscStream(i, true), cancellationTokenSource.Token);

                    r.Success.ShouldBe(true);
                    r.ParsedValue.Count.ShouldBe(i);

                    var r3 = await parser.ParseGenerallyAsync(AsteriscStream(i, true), cancellationTokenSource.Token);
                    r3.Success.ShouldBe(true);
                }

                for (var i = n + 1; i <= n + 15; ++i)
                {
                    var r1 = await parser.ParseAsync(AsteriscStream(i, true), cancellationTokenSource.Token);
                    r1.Success.ShouldBe(false);
                    var r2 = await parser.ParseGenerallyAsync(AsteriscStream(i, true), cancellationTokenSource.Token);
                    r2.Success.ShouldBe(false);
                }
            }
        }

        [Fact]
        public async Task CancelQuantifiedParserAsync()
        {
            var cancellationTokenSource = new CancellationTokenSource();
            var cts = cancellationTokenSource.Token;
            cancellationTokenSource.Cancel();

            var parser = new QuantifiedParserAsync<string>(AsteriscParser, QuantificationRule.NOrLess, 1000);
            var r = await Assert.ThrowsAsync<OperationCanceledException>(() => parser.ParseAsync(AsteriscStream(1000), cts));

            var r2 = await Assert.ThrowsAsync<OperationCanceledException>(() => parser.ParseGenerallyAsync(AsteriscStream(1000), cts));
        }
    }
}
