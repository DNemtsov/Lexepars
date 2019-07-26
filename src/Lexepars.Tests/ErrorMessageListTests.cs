namespace Lexepars.Tests
{
    using Lexepars;
    using Shouldly;
    using Xunit;

    public class FailureMessagesTests
    {
        [Fact]
        public void ShouldProvideSharedEmptyInstance()
        {
            FailureMessages.Empty.ShouldBeSameAs(FailureMessages.Empty);
        }

        [Fact]
        public void CanBeEmpty()
        {
            FailureMessages.Empty.ToString().ShouldBe("");
        }

        [Fact]
        public void CreatesNewCollectionWhenAddingItems()
        {
            FailureMessages list = FailureMessages.Empty.With(FailureMessage.Expected("expectation"));

            list.ToString().ShouldBe("expectation expected");
            list.ShouldNotBeSameAs(FailureMessages.Empty);
        }

        [Fact]
        public void CanIncludeUnknownErrors()
        {
            FailureMessages.Empty
                .With(FailureMessage.Unknown())
                .ToString().ShouldBe("Parsing failed.");
        }

        [Fact]
        public void CanIncludeMultipleExpectations()
        {
            FailureMessages.Empty
                .With(FailureMessage.Expected("A"))
                .With(FailureMessage.Expected("B"))
                .ToString().ShouldBe("A or B expected");

            FailureMessages.Empty
                .With(FailureMessage.Expected("A"))
                .With(FailureMessage.Expected("B"))
                .With(FailureMessage.Expected("C"))
                .ToString().ShouldBe("A, B or C expected");

            FailureMessages.Empty
                .With(FailureMessage.Expected("A"))
                .With(FailureMessage.Expected("B"))
                .With(FailureMessage.Expected("C"))
                .With(FailureMessage.Expected("D"))
                .ToString().ShouldBe("A, B, C or D expected");
        }

        [Fact]
        public void OmitsDuplicateExpectationsFromExpectationLists()
        {
            FailureMessages.Empty
                .With(FailureMessage.Expected("A"))
                .With(FailureMessage.Expected("A"))
                .With(FailureMessage.Expected("B"))
                .With(FailureMessage.Expected("C"))
                .With(FailureMessage.Unknown())
                .With(FailureMessage.Expected("C"))
                .With(FailureMessage.Expected("C"))
                .With(FailureMessage.Expected("A"))
                .ToString().ShouldBe("A, B or C expected");
        }

        [Fact]
        public void CanIncludeBacktrackErrors()
        {
            var deepBacktrack = FailureMessage.Backtrack(new Position(3, 4),
                                                       FailureMessages.Empty
                                                           .With(FailureMessage.Expected("A"))
                                                           .With(FailureMessage.Expected("B")));

            var shallowBacktrack = FailureMessage.Backtrack(new Position(2, 3),
                                                          FailureMessages.Empty
                                                              .With(FailureMessage.Expected("C"))
                                                              .With(FailureMessage.Expected("D"))
                                                              .With(deepBacktrack));
            
            var unrelatedBacktrack = FailureMessage.Backtrack(new Position(1, 2),
                                                       FailureMessages.Empty
                                                           .With(FailureMessage.Expected("E"))
                                                           .With(FailureMessage.Expected("F")));

            FailureMessages.Empty
                .With(deepBacktrack)
                .ToString().ShouldBe("[(3, 4): A or B expected]");

            FailureMessages.Empty
                .With(shallowBacktrack)
                .ToString().ShouldBe("[(2, 3): C or D expected [(3, 4): A or B expected]]");

            FailureMessages.Empty
                .With(FailureMessage.Expected("G"))
                .With(FailureMessage.Expected("H"))
                .With(shallowBacktrack)
                .With(unrelatedBacktrack)
                .ToString().ShouldBe("G or H expected [(1, 2): E or F expected] [(2, 3): C or D expected [(3, 4): A or B expected]]");
        }

        [Fact]
        public void CanMergeTwoLists()
        {
            var first = FailureMessages.Empty
                .With(FailureMessage.Expected("A"))
                .With(FailureMessage.Expected("B"))
                .With(FailureMessage.Unknown())
                .With(FailureMessage.Expected("C"));

            var second = FailureMessages.Empty
                .With(FailureMessage.Expected("D"))
                .With(FailureMessage.Expected("B"))
                .With(FailureMessage.Unknown())
                .With(FailureMessage.Expected("E"));

            first.Merge(second)
                .ToString().ShouldBe("A, B, C, D or E expected");
        }

        [Fact]
        public void OmitsUnknownErrorsWhenAdditionalErrorsExist()
        {
            FailureMessages.Empty
                .With(FailureMessage.Expected("A"))
                .With(FailureMessage.Expected("B"))
                .With(FailureMessage.Unknown())
                .With(FailureMessage.Expected("C"))
                .ToString().ShouldBe("A, B or C expected");
        }
    }
}