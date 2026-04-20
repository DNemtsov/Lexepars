using Lexepars.ParsersAsync;
using Lexepars.Parsers;
using Lexepars.TestFixtures;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Lexepars.Tests
{
    public class UnorderedParserAsyncAsyncTests
    {
        private static readonly MatchableTokenKind aToken = new OperatorTokenKind("a");
        private static readonly MatchableTokenKind bToken = new OperatorTokenKind("b");
        private static readonly MatchableTokenKind cToken = new OperatorTokenKind("c");
        private static readonly MatchableTokenKind dToken = new OperatorTokenKind("d");

        private static readonly MatchableTokenKind separatorToken = new OperatorTokenKind(",");

        private static readonly Lexer lexer = new Lexer(separatorToken, aToken, bToken, cToken, dToken);

        private static readonly IAsyncParser<string> a = aToken.LexemeAsync();
        private static readonly IAsyncParser<string> b = bToken.LexemeAsync();
        private static readonly IAsyncParser<string> c = cToken.LexemeAsync();
        private static readonly IAsyncParser<string> d = dToken.LexemeAsync();
        private static readonly IAsyncGeneralParser separator = new TokenByKindParserAsync(separatorToken);

        [Fact]
        public async Task ParsesNonemptySubsetsOfItemsAsync()
        {
            var parser = new UnorderedParserAsync<string>(separator, UnorderedParsingMode.NonemptySubset, a, b, c);

            foreach (var input in
                GetPermutations("abc")
                .Concat(GetPermutations("ab"))
                .Concat(GetPermutations("bc"))
                .Concat(GetPermutations("ac"))
                .Concat(GetPermutations("a"))
                .Concat(GetPermutations("b"))
                .Concat(GetPermutations("c")))
            {
                
                await parser.Parses(Tokenize(input));
            }
        }

        private static IEnumerable<string> GetPermutations(string str)
        {
            int x = str.Length - 1;

            IEnumerable<string> GetPermutationsRecursive(char[] list, int k, int m)
            {
                if (k == m)
                {
                    yield return new string(list, 0, list.Length);
                    yield break;
                }

                void SwapChars(ref char a, ref char b)
                {
                    if (a == b)
                        return;

                    a ^= b;
                    b ^= a;
                    a ^= b;
                }

                for (int i = k; i <= m; i++)
                {
                    SwapChars(ref list[k], ref list[i]);

                    foreach (var p in GetPermutationsRecursive(list, k + 1, m))
                        yield return p;

                    SwapChars(ref list[k], ref list[i]);
                }
            }

            return GetPermutationsRecursive(str.ToCharArray(), 0, x);
        }

        private static IEnumerable<string> GetPermutationsSeparated(string str) => GetPermutations(str).Select(s => string.Join(",", s.ToCharArray()));

        [Fact]
        public void ChecksConstructorArguments()
        {
            Func<UnorderedParserAsync<string>> nullParsers = () => new UnorderedParserAsync<string>(UnorderedParsingMode.FullSet, null);

            nullParsers.ShouldThrow<ArgumentNullException>().ParamName.ShouldBe("items");

            Func<UnorderedParserAsync<string>> emptyParsers = () => new UnorderedParserAsync<string>(UnorderedParsingMode.FullSet, Array.Empty<IAsyncParser<string>>());

            emptyParsers.ShouldThrow<ArgumentException>("items should not be empty").ParamName.ShouldBe("items");

            Func<UnorderedParserAsync<string>> parsersContainNull = () => new UnorderedParserAsync<string>(UnorderedParsingMode.FullSet, a, null);

            emptyParsers.ShouldThrow<ArgumentException>("items should not have null items").ParamName.ShouldBe("items");
        }

        [Fact]
        public async Task ParsesDistinctItemsInAnyOrder()
        {
            IAsyncParser<string[]> CreateParser(UnorderedParsingMode mode) => new UnorderedParserAsync<string>(mode, a, b, c, d);

            foreach (var parser in new[] { CreateParser(UnorderedParsingMode.FullSet), CreateParser(UnorderedParsingMode.NonemptySubset) })
            {
                foreach (var input in GetPermutations("abcd"))
                {
                    (await parser.Parses(Tokenize(input)))
                        .ParsedValue
                        .ShouldBe(new[] { "a", "b", "c", "d" });
                }
            }
        }

        [Fact]
        public async Task ParsesDistinctItemsInAnyOrderSeparated()
        {
            IAsyncParser<string[]> CreateParser(UnorderedParsingMode mode) => new UnorderedParserAsync<string>(separator, mode, a, b, c, d);

            foreach (var parser in new[] { CreateParser(UnorderedParsingMode.FullSet), CreateParser(UnorderedParsingMode.NonemptySubset) })
            {
                foreach (var input in GetPermutationsSeparated("abcd"))
                {
                    (await parser.Parses(Tokenize(input)))
                        .ParsedValue
                        .ShouldBe(new[] { "a", "b", "c", "d" });

                }
            }
        }

        [Fact]
        public async Task ParsesNonemptySubsetsOfItems()
        {
            var parser = new UnorderedParserAsync<string>(separator, UnorderedParsingMode.NonemptySubset, a, b, c);

            foreach (var input in
                GetPermutations("abc")
                .Concat(GetPermutations("ab"))
                .Concat(GetPermutations("bc"))
                .Concat(GetPermutations("ac"))
                .Concat(GetPermutations("a"))
                .Concat(GetPermutations("b"))
                .Concat(GetPermutations("c")))
            {
                await parser.Parses(Tokenize(input));
            }
        }

        [Fact]
        public async Task ParsesNonemptySubsetsOfItemsSeparated()
        {
            var parser = new UnorderedParserAsync<string>(separator, UnorderedParsingMode.NonemptySubset, a, b, c);

            foreach (var input in 
                GetPermutationsSeparated("abc")
                .Concat(GetPermutationsSeparated("ab"))
                .Concat(GetPermutationsSeparated("bc"))
                .Concat(GetPermutationsSeparated("ac"))
                .Concat(GetPermutationsSeparated("a"))
                .Concat(GetPermutationsSeparated("b"))
                .Concat(GetPermutationsSeparated("c")))
            {
                await parser.Parses(Tokenize(input));
            }
        }

        [Fact]
        public async Task ParsesDuplicatedItemsInAnyOrder()
        {
            IAsyncParser<string[]> CreateParser(UnorderedParsingMode mode) => new UnorderedParserAsync<string>(mode, a, b, a, d);

            foreach (var parser in new[] { CreateParser(UnorderedParsingMode.FullSet), CreateParser(UnorderedParsingMode.NonemptySubset) })
            {
                foreach (var input in GetPermutations("abad"))
                {
                    (await parser.Parses(Tokenize(input)))
                        .ParsedValue
                        .ShouldBe(new[] { "a", "b", "a", "d" });
                }
            }
        }

        [Fact]
        public async Task ParsesDuplicatedItemsInAnyOrderSeparated()
        {
            IAsyncParser<string[]> CreateParser(UnorderedParsingMode mode) => new UnorderedParserAsync<string>(separator, mode, a, b, a, d);

            foreach (var parser in new[] { CreateParser(UnorderedParsingMode.FullSet), CreateParser(UnorderedParsingMode.NonemptySubset) })
            {
                foreach (var input in GetPermutationsSeparated("abad"))
                {
                    (await parser.Parses(Tokenize(input)))
                        .ParsedValue
                        .ShouldBe(new[] { "a", "b", "a", "d" });
                }
            }
        }

        [Fact]
        public async Task ParsesAllIdenticalItems()
        {
            IAsyncParser<string[]> CreateParser(UnorderedParsingMode mode) => new UnorderedParserAsync<string>(mode, a, a, a, a);

            foreach (var parser in new[] { CreateParser(UnorderedParsingMode.FullSet), CreateParser(UnorderedParsingMode.NonemptySubset) })
            {
                (await parser.Parses(Tokenize("aaaa")))
                    .ParsedValue
                    .ShouldBe(new[] { "a", "a", "a", "a" });

            }
        }

        [Fact]
        public async Task ParsesAllIdenticalItemsSeparated()
        {
            IAsyncParser<string[]> CreateParser(UnorderedParsingMode mode) => new UnorderedParserAsync<string>(separator, mode, a, a, a, a);

            foreach (var parser in new[] { CreateParser(UnorderedParsingMode.FullSet), CreateParser(UnorderedParsingMode.NonemptySubset) })
            {
                (await parser.Parses(Tokenize("a,a,a,a")))
                    .ParsedValue
                    .ShouldBe(new[] { "a", "a", "a", "a" });
            }
        }


        [Fact]
        public async Task FailsOnUnexpectedItem()
        {
            var parser =  new UnorderedParserAsync<string>(UnorderedParsingMode.FullSet, a, b, c);

            (await parser
            .FailsToParseAsync(Tokenize("abX")))
            .FailureMessages
            .ToString()
            .ShouldBe("c expected");

            (await parser
                .FailsToParseAsync(Tokenize("abX")))
                .FailureMessages
                .ToString()
                .ShouldBe("c expected");

            (await parser
                .FailsToParseAsync(Tokenize("aXb")))
                .FailureMessages
                .ToString()
                .ShouldBe("b or c expected");

            (await parser
                .FailsToParseAsync(Tokenize("Xab")))
                .FailureMessages
                .ToString()
                .ShouldBe("a, b or c expected");
        }

        [Fact]
        public async Task FailsOnUnexpectedItemSeparated()
        {
            IAsyncParser<string[]> CreateParser(UnorderedParsingMode mode) => new UnorderedParserAsync<string>(separator, mode, a, b, c);

            foreach (var parser in new[] { CreateParser(UnorderedParsingMode.FullSet), CreateParser(UnorderedParsingMode.NonemptySubset) })
            {
                (await parser
                    .FailsToParseAsync(Tokenize("a,b,X")))
                    .FailureMessages
                    .ToString()
                    .ShouldBe("c expected");

                (await parser
                    .FailsToParseAsync(Tokenize("a,X,b")))
                    .FailureMessages
                    .ToString()
                    .ShouldBe("b or c expected");

                (await parser
                    .FailsToParseAsync(Tokenize("X,a,b")))
                    .FailureMessages
                    .ToString()
                    .ShouldBe("a, b or c expected");

                (await parser
                    .FailsToParseAsync(Tokenize(",a,b")))
                    .FailureMessages
                    .ToString()
                    .ShouldBe("a, b or c expected");

                (await parser
                    .FailsToParseAsync(Tokenize("a,b,")))
                    .FailureMessages
                    .ToString()
                    .ShouldBe("c expected");
            }
        }

        [Fact]
        public async Task SucceedsWithNoItems()
        {
            var parser = new UnorderedParserAsync<string>(UnorderedParsingMode.AnySubset, a, b, c, d);

            (await parser.Parses(Tokenize("")))
                .ParsedValue
                .ShouldBe(new string[4]);
        }

        [Fact]
        public async Task SucceedsWithNoKnownItems()
        {
            var parser = new UnorderedParserAsync<string>(UnorderedParsingMode.AnySubset, a, b, c, d);

            (await parser.Parses(Tokenize("X"), false))
                .ParsedValue
                .ShouldBe(new string[4]);
        }

        [Fact]
        public async Task SucceedsWithNoItemsSeparated()
        {
            var parser = new UnorderedParserAsync<string>(separator, UnorderedParsingMode.AnySubset, a, b, c, d);

            (await parser.Parses(Tokenize("")))
                .ParsedValue
                .ShouldBe(new string[4]);
        }

        [Fact]
        public async Task SucceedsWithNoKnownItemsSeparated()
        {
            var parser = new UnorderedParserAsync<string>(separator, UnorderedParsingMode.AnySubset, a, b, c, d);

            (await parser.Parses(Tokenize("X"), false))
                .ParsedValue
                .ShouldBe(new string[4]);
        }

        [Fact]
        public async Task FailsOnMissingItem()
        {
            var parser = new UnorderedParserAsync<string>(UnorderedParsingMode.FullSet, a, b, c, d);

            foreach (var input in GetPermutations("acd"))
            {
                (await parser.FailsToParseAsync(Tokenize(input)))
                    .FailureMessages
                    .ToString()
                    .ShouldBe("b expected");
            }
        }

        [Fact]
        public async Task FailsOnMissingItemSeparated()
        {
            var parser = new UnorderedParserAsync<string>(separator, UnorderedParsingMode.FullSet, a, b, c, d);

            foreach (var input in GetPermutationsSeparated("abc"))
            {
                (await parser.FailsToParseAsync(Tokenize(input)))
                    .FailureMessages
                    .ToString()
                    .ShouldBe(", expected");
            }
        }

        private static IEnumerable<Token> Tokenize(string text) => lexer.Tokenize(text);
    }
}
