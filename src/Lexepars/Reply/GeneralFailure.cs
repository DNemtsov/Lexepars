namespace Lexepars
{
    public class GeneralFailure : IGeneralReply
    {
        public GeneralFailure(TokenStream unparsedTokens, FailureMessage message)
            : this(unparsedTokens, FailureMessages.Empty.With(message)) { }

        public GeneralFailure(TokenStream unparsedTokens, FailureMessages messages)
        {
            UnparsedTokens = unparsedTokens;
            FailureMessages = messages;
        }

        public static GeneralFailure From(IGeneralReply r)
        {
            return new GeneralFailure(r.UnparsedTokens, r.FailureMessages);
        }

        public TokenStream UnparsedTokens { get; }
        public bool Success => false;
        public FailureMessages FailureMessages { get; }
    }
}
