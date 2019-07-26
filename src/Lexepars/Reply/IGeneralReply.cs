namespace Lexepars
{
    public interface IGeneralReply
    {
        TokenStream UnparsedTokens { get; }
        bool Success { get; }
        FailureMessages FailureMessages { get; }
    }
}
