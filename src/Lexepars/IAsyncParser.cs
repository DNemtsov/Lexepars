using System.Threading;
using System.Threading.Tasks;

namespace Lexepars
{
    /// <summary>
    /// Parses the stream of tokens with a <see cref="IReply{TValue}"/> as the result.
    /// </summary>
    /// <typeparam name="TValue">The type of parsed value.</typeparam>
    public interface IAsyncParser<TValue> : IAsyncGeneralParser
    {
        /// <summary>
        /// Parses the stream of tokens asynchronously.
        /// </summary>
        /// <param name="tokens">Stream of tokens to parse. Not null.</param>
        /// /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns><see cref="IReply{TValue}"/> either indicating failure or success with the value. Not null.</returns>
        Task<IReply<TValue>> ParseAsync(TokenStream tokens, CancellationToken cancellationToken);
    }
}