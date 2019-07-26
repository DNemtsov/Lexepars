namespace Lexepars
{
    public class GeneralSuccess : IGeneralReply
    {
        public GeneralSuccess(TokenStream unparsedTokens)
        {
            UnparsedTokens = unparsedTokens;
        }
        
        public TokenStream UnparsedTokens { get; }
        public bool Success => true;

        public virtual FailureMessages FailureMessages => FailureMessages.Empty;
    }
}
