namespace Lexepars
{
    public class Success<T> : GeneralSuccess, IReply<T>
    {
        public Success(T value, TokenStream unparsedTokens)
            : this(value, unparsedTokens, FailureMessages.Empty)
        { }

        public Success(T value, TokenStream unparsedTokens, FailureMessages potentialFailures)
            : base (unparsedTokens)
        {
            ParsedValue = value;
            FailureMessages = potentialFailures;
        }

        public T ParsedValue { get; }
        public override FailureMessages FailureMessages { get; }
    }
}