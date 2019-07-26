namespace Lexepars
{
    /// <summary>
    /// General non-value-specific parser.
    /// </summary>
    public interface IGeneralParser
    {
        IGeneralReply ParseGenerally(TokenStream tokens);
        string Expression { get; }
    }
}
