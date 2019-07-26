namespace Lexepars.Tests
{
    using Lexepars.TestFixtures;
    using Shouldly;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using Xunit;

    public sealed class TokenStreamTests : IDisposable
    {
        readonly TokenKind lower = new PatternTokenKind("Lowercase", @"[a-z]+");
        readonly TokenKind upper = new PatternTokenKind("Uppercase", @"[A-Z]+");

        IEnumerable<Token> NoTokens()
        {
            yield break;
        }

        IEnumerable<Token> OneToken()
        {
            yield return new Token(upper, new Position(1, 1), "ABC");
        }

        IEnumerable<Token> Tokens()
        {
            yield return new Token(upper, new Position(1, 1), "ABC");
            yield return new Token(lower, new Position(1, 4), "def");
            yield return new Token(upper, new Position(1, 7), "GHI");
            yield return new Token(TokenKind.EndOfInput, new Position(1, 10), "");
        }

        private readonly CancellationTokenSource _cancellationTokenSouce = new CancellationTokenSource();

        public void Dispose() => _cancellationTokenSouce.Dispose();

        IEnumerable<TokenStream> CreateAllTokenStreamVarieties(IEnumerable<Token> stream)
        {
            yield return new TokenStream(stream);
            yield return new TokenStreamWithCancellation(stream, default(CancellationToken));
            yield return new TokenStreamWithCancellation(stream, _cancellationTokenSouce.Token);
        }

        [Fact]
        public void ProvidesEndOfInputTokenWhenGivenEmptyEnumerator()
        {
            foreach (var stream in CreateAllTokenStreamVarieties(NoTokens()))
            {
                stream.Current.ShouldBe(TokenKind.EndOfInput, "", 1, 1);
                stream.Advance().ShouldBeSameAs(stream);
            }
        }

        [Fact]
        public void ProvidesCurrentToken()
        {
            foreach (var stream in CreateAllTokenStreamVarieties(Tokens()))
                stream.Current.ShouldBe(upper, "ABC", 1, 1);
        }

        [Fact]
        public void AdvancesToTheNextToken()
        {
            foreach (var stream in CreateAllTokenStreamVarieties(Tokens()))
                stream.Advance().Current.ShouldBe(lower, "def", 1, 4);
        }

        [Fact]
        public void ProvidesEndOfInputTokenAfterEnumeratorIsExhausted()
        {
            foreach (var stream in CreateAllTokenStreamVarieties(OneToken()))
            {
                var end = stream.Advance();

                end.Current.ShouldBe(TokenKind.EndOfInput, "", 1, 4);
                end.Advance().ShouldBeSameAs(end);
            }
        }

        [Fact]
        public void TryingToAdvanceBeyondEndOfInputResultsInNoMovement()
        {
            foreach (var stream in CreateAllTokenStreamVarieties(NoTokens()))
                stream.ShouldBeSameAs(stream.Advance());
        }

        [Fact]
        public void DoesNotChangeStateAsUnderlyingEnumeratorIsTraversed()
        {
            foreach (var stream in CreateAllTokenStreamVarieties(Tokens()))
            {
                var first = stream;

                first.Current.ShouldBe(upper, "ABC", 1, 1);

                var second = first.Advance();
                first.Current.ShouldBe(upper, "ABC", 1, 1);
                second.Current.ShouldBe(lower, "def", 1, 4);

                var third = second.Advance();
                first.Current.ShouldBe(upper, "ABC", 1, 1);
                second.Current.ShouldBe(lower, "def", 1, 4);
                third.Current.ShouldBe(upper, "GHI", 1, 7);

                var fourth = third.Advance();
                first.Current.ShouldBe(upper, "ABC", 1, 1);
                second.Current.ShouldBe(lower, "def", 1, 4);
                third.Current.ShouldBe(upper, "GHI", 1, 7);
                fourth.Current.ShouldBe(TokenKind.EndOfInput, "", 1, 10);

                fourth.Advance().ShouldBeSameAs(fourth);
            }
        }

        [Fact]
        public void AllowsRepeatableTraversalWhileTraversingUnderlyingEnumeratorItemsAtMostOnce()
        {
            foreach (var stream in CreateAllTokenStreamVarieties(Tokens()))
            {
                stream.Current.ShouldBe(upper, "ABC", 1, 1);
                stream.Advance().Current.ShouldBe(lower, "def", 1, 4);
                stream.Advance().Advance().Current.ShouldBe(upper, "GHI", 1, 7);
                stream.Advance().Advance().Advance().Current.ShouldBe(TokenKind.EndOfInput, "", 1, 10);

                stream.Advance().ShouldBeSameAs(stream.Advance());
            }
        }

        [Fact]
        public void ProvidesPositionOfCurrentToken()
        {
            foreach (var stream in CreateAllTokenStreamVarieties(Tokens()))
            {
                stream.Position.Line.ShouldBe(1);
                stream.Position.Column.ShouldBe(1);

                stream.Advance().Position.Line.ShouldBe(1);
                stream.Advance().Position.Column.ShouldBe(4);

                stream.Advance().Advance().Position.Line.ShouldBe(1);
                stream.Advance().Advance().Position.Column.ShouldBe(7);

                stream.Advance().Advance().Advance().Position.Line.ShouldBe(1);
                stream.Advance().Advance().Advance().Position.Column.ShouldBe(10);
            }
        }

        [Fact]
        public void TokenStreamWithCancellationThrowsWithNoTokensLeft()
        {
            using (var source = new CancellationTokenSource())
            {
                var stream = new TokenStreamWithCancellation(NoTokens(), source.Token);

                source.Cancel();

                Should
                    .Throw<OperationCanceledException>(() => stream.Advance())
                    .CancellationToken
                    .ShouldBe(source.Token);
            }
        }

        [Fact]
        public void TokenStreamWithCancellationThrowsWithTokensLeft()
        {
            using (var source = new CancellationTokenSource())
            {
                var stream = new TokenStreamWithCancellation(Tokens(), source.Token);

                stream.Advance();

                source.Cancel();

                Should
                    .Throw<OperationCanceledException>(() => stream.Advance())
                    .CancellationToken
                    .ShouldBe(source.Token);
            }
        }
    }
}