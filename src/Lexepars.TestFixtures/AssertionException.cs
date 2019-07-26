namespace Lexepars.TestFixtures
{
    using System;

    public class AssertionException : Exception
    {
        public AssertionException(object expected, object actual)
            : base(ExpectationDetails(expected, actual))
        {
        }

        public AssertionException(string message)
            : base(message)
        {
        }

        public AssertionException()
        {
        }

        public AssertionException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public AssertionException(string message, object expected, object actual)
            : base(Environment.NewLine + message + ExpectationDetails(expected, actual))
        {
        }

        private static string ExpectationDetails(object expected, object actual)
            => $"{Environment.NewLine}Expected: {expected}{Environment.NewLine}But was:  {actual}";
    }
}