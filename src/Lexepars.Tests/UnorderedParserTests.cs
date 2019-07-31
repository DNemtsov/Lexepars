using Lexepars.Parsers;
using Lexepars.TestFixtures;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Lexepars.Tests
{
    public class UnorderedParserTests
    {
        private static readonly MatchableTokenKind aToken = new OperatorTokenKind("a");
        private static readonly MatchableTokenKind bToken = new OperatorTokenKind("b");
        private static readonly MatchableTokenKind cToken = new OperatorTokenKind("c");
        private static readonly MatchableTokenKind dToken = new OperatorTokenKind("d");

        private static readonly MatchableTokenKind separatorToken = new OperatorTokenKind(",");

        private static readonly Lexer lexer = new Lexer(separatorToken, aToken, bToken, cToken, dToken);

        private static readonly IParser<string> a = aToken.Lexeme();
        private static readonly IParser<string> b = bToken.Lexeme();
        private static readonly IParser<string> c = cToken.Lexeme();
        private static readonly IParser<string> d = dToken.Lexeme();
        private static readonly IGeneralParser separator = new TokenByKindParser(separatorToken);

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
            Func<UnorderedParser<string>> nullParsers = () => new UnorderedParser<string>(UnorderedParsingMode.FullSet, null);

            nullParsers.ShouldThrow<ArgumentNullException>().ParamName.ShouldBe("items");

            Func<UnorderedParser<string>> emptyParsers = () => new UnorderedParser<string>(UnorderedParsingMode.FullSet, Array.Empty<IParser<string>>());

            emptyParsers.ShouldThrow<ArgumentException>("items should not be empty").ParamName.ShouldBe("items");

            Func<UnorderedParser<string>> parsersContainNull = () => new UnorderedParser<string>(UnorderedParsingMode.FullSet, a, null);

            emptyParsers.ShouldThrow<ArgumentException>("items should not have null items").ParamName.ShouldBe("items");
        }

        [Fact]
        public void ParsesDistinctItemsInAnyOrder()
        {
            IParser<string[]> CreateParser(UnorderedParsingMode mode) => new UnorderedParser<string>(mode, a, b, c, d);

            foreach (var parser in new[] { CreateParser(UnorderedParsingMode.FullSet), CreateParser(UnorderedParsingMode.NonemptySubset) })
            {
                foreach (var input in GetPermutations("abcd"))
                {
                    parser.Parses(Tokenize(input))
                        .ParsedValue
                        .ShouldBe(new[] { "a", "b", "c", "d" });
                }
            }
        }

        [Fact]
        public void ParsesDistinctItemsInAnyOrderSeparated()
        {
            IParser<string[]> CreateParser(UnorderedParsingMode mode) => new UnorderedParser<string>(separator, mode, a, b, c, d);

            foreach (var parser in new[] { CreateParser(UnorderedParsingMode.FullSet), CreateParser(UnorderedParsingMode.NonemptySubset) })
            {
                foreach (var input in GetPermutationsSeparated("abcd"))
                {
                    parser.Parses(Tokenize(input))
                        .ParsedValue
                        .ShouldBe(new[] { "a", "b", "c", "d" });

                }
            }
        }

        [Fact]
        public void ParsesNonemptySubsetsOfItems()
        {
            var parser = new UnorderedParser<string>(separator, UnorderedParsingMode.NonemptySubset, a, b, c);

            foreach (var input in
                GetPermutations("abc")
                .Concat(GetPermutations("ab"))
                .Concat(GetPermutations("bc"))
                .Concat(GetPermutations("ac"))
                .Concat(GetPermutations("a"))
                .Concat(GetPermutations("b"))
                .Concat(GetPermutations("c")))
            {
                parser.Parses(Tokenize(input));
            }
        }

        [Fact]
        public void ParsesNonemptySubsetsOfItemsSeparated()
        {
            var parser = new UnorderedParser<string>(separator, UnorderedParsingMode.NonemptySubset, a, b, c);

            foreach (var input in 
                GetPermutationsSeparated("abc")
                .Concat(GetPermutationsSeparated("ab"))
                .Concat(GetPermutationsSeparated("bc"))
                .Concat(GetPermutationsSeparated("ac"))
                .Concat(GetPermutationsSeparated("a"))
                .Concat(GetPermutationsSeparated("b"))
                .Concat(GetPermutationsSeparated("c")))
            {
                parser.Parses(Tokenize(input));
            }
        }

        [Fact]
        public void ParsesDuplicatedItemsInAnyOrder()
        {
            IParser<string[]> CreateParser(UnorderedParsingMode mode) => new UnorderedParser<string>(mode, a, b, a, d);

            foreach (var parser in new[] { CreateParser(UnorderedParsingMode.FullSet), CreateParser(UnorderedParsingMode.NonemptySubset) })
            {
                foreach (var input in GetPermutations("abad"))
                {
                    parser.Parses(Tokenize(input))
                        .ParsedValue
                        .ShouldBe(new[] { "a", "b", "a", "d" });
                }
            }
        }

        [Fact]
        public void ParsesDuplicatedItemsInAnyOrderSeparated()
        {
            IParser<string[]> CreateParser(UnorderedParsingMode mode) => new UnorderedParser<string>(separator, mode, a, b, a, d);

            foreach (var parser in new[] { CreateParser(UnorderedParsingMode.FullSet), CreateParser(UnorderedParsingMode.NonemptySubset) })
            {
                foreach (var input in GetPermutationsSeparated("abad"))
                {
                    parser.Parses(Tokenize(input))
                        .ParsedValue
                        .ShouldBe(new[] { "a", "b", "a", "d" });
                }
            }
        }

        [Fact]
        public void ParsesAllIdenticalItems()
        {
            IParser<string[]> CreateParser(UnorderedParsingMode mode) => new UnorderedParser<string>(mode, a, a, a, a);

            foreach (var parser in new[] { CreateParser(UnorderedParsingMode.FullSet), CreateParser(UnorderedParsingMode.NonemptySubset) })
            {
                parser.Parses(Tokenize("aaaa"))
                    .ParsedValue
                    .ShouldBe(new[] { "a", "a", "a", "a" });

            }
        }

        [Fact]
        public void ParsesAllIdenticalItemsSeparated()
        {
            IParser<string[]> CreateParser(UnorderedParsingMode mode) => new UnorderedParser<string>(separator, mode, a, a, a, a);

            foreach (var parser in new[] { CreateParser(UnorderedParsingMode.FullSet), CreateParser(UnorderedParsingMode.NonemptySubset) })
            {
                parser.Parses(Tokenize("a,a,a,a"))
                    .ParsedValue
                    .ShouldBe(new[] { "a", "a", "a", "a" });
            }
        }


        [Fact]
        public void FailsOnUnexpectedItem()
        {
            var parser =  new UnorderedParser<string>(UnorderedParsingMode.FullSet, a, b, c);

            parser
            .FailsToParse(Tokenize("abX"))
            .FailureMessages
            .ToString()
            .ShouldBe("c expected");

            parser
                .FailsToParse(Tokenize("abX"))
                .FailureMessages
                .ToString()
                .ShouldBe("c expected");

            parser
                .FailsToParse(Tokenize("aXb"))
                .FailureMessages
                .ToString()
                .ShouldBe("b or c expected");

            parser
                .FailsToParse(Tokenize("Xab"))
                .FailureMessages
                .ToString()
                .ShouldBe("a, b or c expected");
        }

        [Fact]
        public void FailsOnUnexpectedItemSeparated()
        {
            IParser<string[]> CreateParser(UnorderedParsingMode mode) => new UnorderedParser<string>(separator, mode, a, b, c);

            foreach (var parser in new[] { CreateParser(UnorderedParsingMode.FullSet), CreateParser(UnorderedParsingMode.NonemptySubset) })
            {
                parser
                    .FailsToParse(Tokenize("a,b,X"))
                    .FailureMessages
                    .ToString()
                    .ShouldBe("c expected");

                parser
                    .FailsToParse(Tokenize("a,X,b"))
                    .FailureMessages
                    .ToString()
                    .ShouldBe("b or c expected");

                parser
                    .FailsToParse(Tokenize("X,a,b"))
                    .FailureMessages
                    .ToString()
                    .ShouldBe("a, b or c expected");

                parser
                    .FailsToParse(Tokenize(",a,b"))
                    .FailureMessages
                    .ToString()
                    .ShouldBe("a, b or c expected");

                parser
                    .FailsToParse(Tokenize("a,b,"))
                    .FailureMessages
                    .ToString()
                    .ShouldBe("c expected");
            }
        }

        [Fact]
        public void SucceedsWithNoItems()
        {
            var parser = new UnorderedParser<string>(UnorderedParsingMode.AnySubset, a, b, c, d);

            parser.Parses(Tokenize(""))
                .ParsedValue
                .ShouldBe(new string[4]);
        }

        [Fact]
        public void SucceedsWithNoKnownItems()
        {
            var parser = new UnorderedParser<string>(UnorderedParsingMode.AnySubset, a, b, c, d);

            parser.Parses(Tokenize("X"), false)
                .ParsedValue
                .ShouldBe(new string[4]);
        }

        [Fact]
        public void SucceedsWithNoItemsSeparated()
        {
            var parser = new UnorderedParser<string>(separator, UnorderedParsingMode.AnySubset, a, b, c, d);

            parser.Parses(Tokenize(""))
                .ParsedValue
                .ShouldBe(new string[4]);
        }

        [Fact]
        public void SucceedsWithNoKnownItemsSeparated()
        {
            var parser = new UnorderedParser<string>(separator, UnorderedParsingMode.AnySubset, a, b, c, d);

            parser.Parses(Tokenize("X"), false)
                .ParsedValue
                .ShouldBe(new string[4]);
        }

        [Fact]
        public void FailsOnMissingItem()
        {
            var parser = new UnorderedParser<string>(UnorderedParsingMode.FullSet, a, b, c, d);

            foreach (var input in GetPermutations("acd"))
            {
                parser.FailsToParse(Tokenize(input))
                    .FailureMessages
                    .ToString()
                    .ShouldBe("b expected");
            }
        }

        [Fact]
        public void FailsOnMissingItemSeparated()
        {
            var parser = new UnorderedParser<string>(separator, UnorderedParsingMode.FullSet, a, b, c, d);

            foreach (var input in GetPermutationsSeparated("abc"))
            {
                parser.FailsToParse(Tokenize(input))
                    .FailureMessages
                    .ToString()
                    .ShouldBe(", expected");
            }
        }

        private static IEnumerable<Token> Tokenize(string text) => lexer.Tokenize(text);
    }
}
