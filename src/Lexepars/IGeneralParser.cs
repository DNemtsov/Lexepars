namespace Lexepars
{
    /// <summary>
    /// Parses a stream of tokens providing a <see cref="IGeneralReply"/> indicating success of failure as the result. There is no returned value with the reply, so this is suitable for validation situations or for the cases when the returned value is not needed because is gets skipped.
    /// </summary>
    public interface IGeneralParser
    {
        /// <summary>
        /// Parses the stream of tokens generally. Note that the result continuation for the parsers having them will not be called, as there is no parsed value expected.
        /// </summary>
        /// <param name="tokens">Stream of tokens to parse. Not null.</param>
        /// <returns><see cref="IGeneralReply"/> indicating success of failure. Not null.</returns>
        IGeneralReply ParseGenerally(TokenStream tokens);

        /// <summary>
        /// String representation of the parser hierarchy. Note that this is not an O(1) method because it traverses the entire parser hierarchy beneath the current instance.
        /// </summary>
        string Expression { get; }
    }
}
