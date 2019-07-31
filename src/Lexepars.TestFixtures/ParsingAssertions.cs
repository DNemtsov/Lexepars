namespace Lexepars.TestFixtures
{
    using System;
    using System.Collections.Generic;

    public static class ParsingAssertions
    {
        public static void ShouldSucceed(this MatchResult actual, string expected)
        {
            if (!actual.Success)
                throw new AssertionException("successful match", "match failed");

            if (actual.Value != expected)
                throw new AssertionException(expected, actual.Value);
        }

        public static void ShouldFail(this MatchResult actual)
        {
            if (actual.Success)
                throw new AssertionException("match failure", "successful match");

            if (!string.IsNullOrEmpty(actual.Value))
                throw new AssertionException(string.Empty, actual.Value);
        }

        public static void ShouldBe(this Token actual, TokenKind expectedKind, string expectedLexeme, int expectedLine, int expectedColumn)
        {
            actual.ShouldBe(expectedKind, expectedLexeme);

            var expectedPosition = new Position(expectedLine, expectedColumn);
            if (actual.Position != expectedPosition)
                throw new AssertionException("token at position " + expectedPosition,
                                             "token at position " + actual.Position);
        }

        public static void ShouldBe(this Token actual, TokenKind expectedKind, string expectedLexeme)
        {
            AssertEqual(expectedKind, actual.Kind);
            AssertTokenLexemeIsEqual(expectedLexeme, actual.Lexeme);
        }

        public static IReply<T> FailsToParse<T>(this IParser<T> parser, IEnumerable<Token> tokens)
        {
            var stream = new TokenStream(tokens);

            var reply = parser.Parse(stream);
            
            if (reply.Success)
                throw new AssertionException("parser failure", "parser completed successfully");

            var gReply = parser.ParseGenerally(stream);

            if (gReply.Success)
                throw new AssertionException("general parser failure", "general parser completed successfully");

            return reply;
        }

        public static IGeneralReply FailsToParse(this IGeneralParser parser, IEnumerable<Token> tokens)
        {
            var reply = parser.ParseGenerally(new TokenStream(tokens));

            if (reply.Success)
                throw new AssertionException("parser failure", "parser completed successfully");

            return reply;
        }

        public static TReply WithMessage<TReply>(this TReply reply, string expectedMessage)
            where TReply : IGeneralReply
        {
            var position = reply.UnparsedTokens.Position;
            var actual = position + ": " + reply.FailureMessages;
            
            if (actual != expectedMessage)
                throw new AssertionException($"message at {expectedMessage}", $"message at {actual}");

            return reply;
        }

        public static TReply WithNoMessage<TReply>(this TReply reply)
            where TReply : IGeneralReply
        {
            if (reply.FailureMessages != FailureMessages.Empty)
                throw new AssertionException("No messages was expected.", reply.FailureMessages);

            return reply;
        }

        public static IReply<T> PartiallyParses<T>(this IParser<T> parser, IEnumerable<Token> tokens)
        {
            var stream = new TokenStream(tokens);

            parser.ParseGenerally(stream).Succeeds();

            return parser.Parse(stream).Succeeds();
        }

        public static IGeneralReply PartiallyParses(this IGeneralParser parser, IEnumerable<Token> tokens)
        {
            var stream = new TokenStream(tokens);

            return parser.ParseGenerally(stream).Succeeds();
        }

        public static IReply<T> Parses<T>(this IParser<T> parser, IEnumerable<Token> tokens, bool atTheEndOfInput = true)
        {
            if (parser == null)
                throw new ArgumentNullException(nameof(parser));

            var stream = new TokenStream(tokens);

            var generalReply = parser.ParseGenerally(stream).Succeeds();

            if (atTheEndOfInput)
                generalReply.AtEndOfInput();

            var reply = parser.Parse(stream).Succeeds();

            if (atTheEndOfInput)
                reply.AtEndOfInput();

            return reply;
        }

        public static IReply<T> Parses<T>(this IParser<T> parser, params Token[] tokens)
            => parser.Parses((IEnumerable<Token>)tokens);

        public static IGeneralReply Parses(this IGeneralParser parser, IEnumerable<Token> tokens)
        {
            return parser.ParseGenerally(new TokenStream(tokens)).Succeeds().AtEndOfInput();
        }

        private static TReply Succeeds<TReply>(this TReply reply)
            where TReply : IGeneralReply
        {
            if (!reply.Success)
            {
                var message = "Position: " + reply.UnparsedTokens.Position
                              + Environment.NewLine
                              + "Error Message: " + reply.FailureMessages;
                throw new AssertionException(message, "parser success", "parser failed");
            }

            return reply;
        }

        public static TReply LeavingUnparsedTokens<TReply>(this TReply reply, params string[] expectedLexemes)
            where TReply : IGeneralReply
        {
            var stream = reply.UnparsedTokens;

            var actualLexemes = new List<string>();

            while (stream.Current.Kind != TokenKind.EndOfInput)
            {
                actualLexemes.Add(stream.Current.Lexeme);
                stream = stream.Advance();
            }

            void RaiseError()
            {
                throw new AssertionException("Parse resulted in unexpected remaining unparsed tokens.", string.Join(", ", expectedLexemes), string.Join(", ", actualLexemes));
            }

            if (actualLexemes.Count != expectedLexemes.Length)
                RaiseError();

            for (int i = 0; i < actualLexemes.Count; i++)
                if (actualLexemes[i] != expectedLexemes[i])
                    RaiseError();

            return reply;
        }

        public static TReply AtEndOfInput<TReply>(this TReply reply)
            where TReply : IGeneralReply
        {
            var nextTokenKind = reply.UnparsedTokens.Current.Kind;
            AssertEqual(TokenKind.EndOfInput, nextTokenKind);
            return reply.LeavingUnparsedTokens();
        }

        public static IReply<T> WithValue<T>(this IReply<T> reply, T expected)
        {
            if (!Equals(expected, reply.ParsedValue))
                throw new AssertionException($"parsed value: {expected}", $"parsed value: {reply.ParsedValue}");

            return reply;
        }

        public static IReply<T> WithValue<T>(this IReply<T> reply, Action<T> assertParsedValue)
        {
            assertParsedValue(reply.ParsedValue);

            return reply;
        }

        public static IReply<IEnumerable<T>> WithValues<T>(this IReply<IEnumerable<T>> reply, params T[] values)
        {
            using (var actual = reply.ParsedValue.GetEnumerator())
            {
                var expected = values.GetEnumerator();

                for (var i = 0; ; ++i)
                {
                    var aMoved = actual.MoveNext();
                    var eMoved = expected.MoveNext();

                    if (aMoved != eMoved)
                        throw new AssertionException("parsed and expected value collections have different sizes");

                    if (!aMoved)
                        break;

                    if (!Equals(expected.Current, actual.Current))
                        throw new AssertionException($"parsed value [{i}]: {expected}", $"parsed value [{i}]: {reply.ParsedValue}");
                }
            }


            return reply;
        }

        private static void AssertTokenLexemeIsEqual(string expected, string actual)
        {
            if (actual != expected)
                throw new AssertionException($"token with lexeme \"{expected}\"", $"token with lexeme \"{actual}\"");
        }

        private static void AssertEqual(TokenKind expected, TokenKind actual)
        {
            if (actual != expected)
                throw new AssertionException($"<{expected}> token", $"<{actual}> token");
        }
    }
}