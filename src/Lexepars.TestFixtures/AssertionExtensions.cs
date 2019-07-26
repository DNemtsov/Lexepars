namespace Lexepars.TestFixtures
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Shouldly;

    public static class AssertionExtensions
    {
        public static void ShouldList<T>(this IEnumerable<T> actual, params Action<T>[] itemExpectations)
        {
            var array = actual.ToArray();

            array.Length.ShouldBe(itemExpectations.Length);

            for (int i = 0; i < array.Length; i++)
                itemExpectations[i](array[i]);
        }
    }
}