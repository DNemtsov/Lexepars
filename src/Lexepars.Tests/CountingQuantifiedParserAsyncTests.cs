using Lexepars.ParsersAsync;
using Lexepars.Parsers;
using Shouldly;
using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Lexepars.Tests
{
    public class CountingQuantifiedParserAsyncTests : BaseQuantifiedParserAsyncTests
    {
        [Fact]
        public async Task NOrMore()
        {
            var cts = new CancellationTokenSource();
            for (int n = 0; n < 10; ++n)
            {
                var parser = new CountingQuantifiedParserAsync(AsteriscParser, QuantificationRule.NOrMore, n);

                for (var i = n; i < n + 15; ++i)
                {
                    var r = await parser.ParseAsync(AsteriscStream(i), cts.Token);

                    r.Success.ShouldBe(true);
                    r.ParsedValue.ShouldBe(i);

                    var r3 = await parser.ParseGenerallyAsync(AsteriscStream(i), cts.Token);
                    r3.Success.ShouldBe(true);
                }

                for (var i = n - 1; i >= 0; --i)
                {
                    var r1 = await parser.ParseAsync(AsteriscStream(i), cts.Token);
                    r1.Success.ShouldBe(false);
                    var r2 = await parser.ParseGenerallyAsync(AsteriscStream(i), cts.Token);
                    r2.Success.ShouldBe(false);
                }
            }

            for (int n = 0; n < 10; ++n)
            {
                var parser = new CountingQuantifiedParserAsync(AsteriscParser, QuantificationRule.NOrMore, n, -1, SeparatorParser);

                for (var i = n; i < n + 15; ++i)
                {
                    var r = await parser.ParseAsync(AsteriscStream(i, true), cts.Token);

                    r.Success.ShouldBe(true);
                    r.ParsedValue.ShouldBe(i);

                    var r3 = await parser.ParseGenerallyAsync(AsteriscStream(i, true), cts.Token);
                    r3.Success.ShouldBe(true);
                }

                for (var i = n - 1; i >= 0; --i)
                {
                    var r1 = await parser.ParseAsync(AsteriscStream(i, true), cts.Token);
                    r1.Success.ShouldBe(false);
                    var r2 = await parser.ParseGenerallyAsync(AsteriscStream(i, true), cts.Token);
                    r2.Success.ShouldBe(false);
                }
            }
        }

        [Fact]
        public async Task ExactlyN()
        {
            var cts = new CancellationTokenSource();
            for (int n = 0; n < 15; ++n)
            {
                var parser = new CountingQuantifiedParserAsync(AsteriscParser, QuantificationRule.ExactlyN, n);

                for (var i = n - 1; i >= 0; --i)
                {
                    var r1 = await parser.ParseAsync(AsteriscStream(i), cts.Token);
                    r1.Success.ShouldBe(false);
                    var r2 = await parser.ParseGenerallyAsync(AsteriscStream(i), cts.Token);
                    r2.Success.ShouldBe(false);
                }

                var r = await parser.ParseAsync(AsteriscStream(n), cts.Token);

                    r.Success.ShouldBe(true);
                    r.ParsedValue.ShouldBe(n);

                var r3 = await parser.ParseGenerallyAsync(AsteriscStream(n), cts.Token);
                r3.Success.ShouldBe(true);

                for (var i = n + 1; i < n + 15; ++i)
                {
                    var r1 = await parser.ParseAsync(AsteriscStream(i), cts.Token);
                    r1.Success.ShouldBe(false);
                    var r2 = await parser.ParseGenerallyAsync(AsteriscStream(i), cts.Token);
                    r2.Success.ShouldBe(false);
                }
            }

            for (int n = 0; n < 15; ++n)
            {
                var parser = new CountingQuantifiedParserAsync(AsteriscParser, QuantificationRule.ExactlyN, n, -1, SeparatorParser);

                for (var i = n - 1; i >= 0; --i)
                {
                    var r1 = await parser.ParseAsync(AsteriscStream(i, true), cts.Token);
                    r1.Success.ShouldBe(false);
                    var r2 = await parser.ParseGenerallyAsync(AsteriscStream(i, true), cts.Token);
                    r2.Success.ShouldBe(false);
                }

                var r = await parser.ParseAsync(AsteriscStream(n, true), cts.Token);

                r.Success.ShouldBe(true);
                r.ParsedValue.ShouldBe(n);

                var r3 = await parser.ParseGenerallyAsync(AsteriscStream(n, true), cts.Token);
                r3.Success.ShouldBe(true);

                for (var i = n + 1; i < n + 15; ++i)
                {
                    var r1 = await parser.ParseAsync(AsteriscStream(i, true), cts.Token);
                    r1.Success.ShouldBe(false);
                    var r2 = await parser.ParseGenerallyAsync(AsteriscStream(i, true), cts.Token);
                    r2.Success.ShouldBe(false);
                }
            }
        }

        [Fact]
        public async Task NtoM()
        {
            var cts = new CancellationTokenSource();
            for (int n = 0; n < 15; ++n)
                for (int m = n; m < n + 10; ++m)
                {
                    var parser = new CountingQuantifiedParserAsync(AsteriscParser, QuantificationRule.NtoM, n, m);

                    for (var i = 0; i < n; ++i)
                    {
                        var r1 = await parser.ParseAsync(AsteriscStream(i), cts.Token);
                        r1.Success.ShouldBe(false);
                        var r2 = await parser.ParseGenerallyAsync(AsteriscStream(i), cts.Token);
                        r2.Success.ShouldBe(false);
                    }

                    for (var i = n; i <= m; ++i)
                    {
                        var r = await parser.ParseAsync(AsteriscStream(i), cts.Token);

                        r.Success.ShouldBe(true);
                        r.ParsedValue.ShouldBe(i);

                        var r3 = await parser.ParseGenerallyAsync(AsteriscStream(i), cts.Token);
                        r3.Success.ShouldBe(true);
                    }

                    for (var i = m + 1; i < m + 15; ++i)
                    {
                        var r1 = await parser.ParseAsync(AsteriscStream(i), cts.Token);
                        r1.Success.ShouldBe(false);
                        var r2 = await parser.ParseGenerallyAsync(AsteriscStream(i), cts.Token);
                        r2.Success.ShouldBe(false);
                    }
                }

            for (int n = 0; n < 15; ++n)
                for (int m = n; m < n + 10; ++m)
                {
                    var parser = new CountingQuantifiedParserAsync(AsteriscParser, QuantificationRule.NtoM, n, m, SeparatorParser);

                    for (var i = 0; i < n; ++i)
                    {
                        var r1 = await parser.ParseAsync(AsteriscStream(i, true), cts.Token);
                        r1.Success.ShouldBe(false);
                        var r2 = await parser.ParseGenerallyAsync(AsteriscStream(i, true), cts.Token);
                        r2.Success.ShouldBe(false);
                    }

                    for (var i = n; i <= m; ++i)
                    {
                        var r = await parser.ParseAsync(AsteriscStream(i, true), cts.Token);

                        r.Success.ShouldBe(true);
                        r.ParsedValue.ShouldBe(i);

                        var r3 = await parser.ParseGenerallyAsync(AsteriscStream(i, true), cts.Token);
                        r3.Success.ShouldBe(true);
                    }

                    for (var i = m + 1; i < m + 15; ++i)
                    {
                        var r1 = await parser.ParseAsync(AsteriscStream(i, true), cts.Token);
                        r1.Success.ShouldBe(false);
                        var r2 = await parser.ParseGenerallyAsync(AsteriscStream(i, true), cts.Token);
                        r2.Success.ShouldBe(false);
                    }
                }
        }

        [Fact]
        public async Task NOrLess()
        {
            var cts = new CancellationTokenSource();
            for (int n = 0; n < 15; ++n)
            {
                var parser = new CountingQuantifiedParserAsync(AsteriscParser, QuantificationRule.NOrLess, n);

                for (var i = 0; i <= n; ++i)
                {
                    var r = await parser.ParseAsync(AsteriscStream(i), cts.Token);

                    r.Success.ShouldBe(true);
                    r.ParsedValue.ShouldBe(i);

                    var r3 = await parser.ParseGenerallyAsync(AsteriscStream(i), cts.Token);
                    r3.Success.ShouldBe(true);
                }

                for (var i = n + 1; i <= n + 15; ++i)
                {
                    var r1 = await parser.ParseAsync(AsteriscStream(i), cts.Token);
                    r1.Success.ShouldBe(false);
                    var r2 = await parser.ParseGenerallyAsync(AsteriscStream(i), cts.Token);
                    r2.Success.ShouldBe(false);
                }
            }

            for (int n = 0; n < 15; ++n)
            {
                var parser = new CountingQuantifiedParserAsync(AsteriscParser, QuantificationRule.NOrLess, n, -1, SeparatorParser);

                for (var i = 0; i <= n; ++i)
                {
                    var r = await parser.ParseAsync(AsteriscStream(i, true), cts.Token);

                    r.Success.ShouldBe(true);
                    r.ParsedValue.ShouldBe(i);

                    var r3 = await parser.ParseGenerallyAsync(AsteriscStream(i, true), cts.Token);
                    r3.Success.ShouldBe(true);
                }

                for (var i = n + 1; i <= n + 15; ++i)
                {
                    var r1 = await parser.ParseAsync(AsteriscStream(i, true), cts.Token);
                    r1.Success.ShouldBe(false);
                    var r2 = await parser.ParseGenerallyAsync(AsteriscStream(i, true), cts.Token);
                    r2.Success.ShouldBe(false);
                }
            }
        }
        
        
        [Fact]
        public async Task CancelCountingQuantifiedParserAsync()
        {
            var cancellationTokenSource = new CancellationTokenSource();
            var cts = cancellationTokenSource.Token;
            cancellationTokenSource.Cancel();

            var parser = new CountingQuantifiedParserAsync(AsteriscParser, QuantificationRule.NOrLess, 1000);
            var r = await Assert.ThrowsAsync<OperationCanceledException>(() => parser.ParseAsync(AsteriscStream(1000), cts));

            var r2 = await Assert.ThrowsAsync<OperationCanceledException>(() => parser.ParseGenerallyAsync(AsteriscStream(1000), cts));
        }
    }
}
