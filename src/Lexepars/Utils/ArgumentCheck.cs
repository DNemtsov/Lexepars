using System;
using System.Collections.Generic;
using System.Linq;

namespace Lexepars
{
    public static class ArgumentCheck
    {
        public static void NotNullOrEmptyOrWithNulls<T>(IReadOnlyCollection<T> collection, string nameOfCollection)
        {
            if (collection == null)
                throw new ArgumentNullException(nameOfCollection);

            if (collection.Count < 1)
                throw new ArgumentException($"{nameOfCollection} should not be empty", nameOfCollection);

            if (collection.Any(item => item == null))
                throw new ArgumentException($"{nameOfCollection} should not have null items", nameOfCollection);
        }

        public static void NotNullOrWithNulls<T>(IReadOnlyCollection<T> collection, string nameOfCollection)
        {
            if (collection == null)
                throw new ArgumentNullException(nameOfCollection);

            if (collection.Any(item => item == null))
                throw new ArgumentException($"{nameOfCollection} should not have null items", nameOfCollection);
        }
    }
}
