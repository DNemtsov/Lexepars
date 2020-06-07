namespace Lexepars
{
    /// <summary>
    /// Parses the stream of tokens with a <see cref="IReply{TValue}"/> as the result.
    /// </summary>
    /// <typeparam name="TValue">The type of parsed value.</typeparam>
    public interface IParser<out TValue> : IGeneralParser
    {
        /// <summary>
        /// Parses the stream of tokens.
        /// </summary>
        /// <param name="tokens">Stream of tokens to parse. Not null.</param>
        /// <returns><see cref="IReply{TValue}"/> either indicating failure or success with the value. Not null.</returns>
        IReply<TValue> Parse(TokenStream tokens);
    }
}