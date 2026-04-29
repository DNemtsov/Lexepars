using Lexepars.Parsers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Lexepars.ParsersAsync
{
    /// <summary>
    /// Behaves like <see cref="SkipParser"/>, except could be used with parallel parsers.
    /// </summary>
    public class SkipParserAsync : AsyncParser
    {
        /// <summary>
        /// Creates a new instance of <see cref="SkipParserAsync"/>.
        /// </summary>
        /// <param name="items">The item parsers. Not null. Not empty.</param>
        public SkipParserAsync(params IAsyncGeneralParser[] items)
        {
            ArgumentCheck.NotNullOrEmptyOrWithNulls(items, nameof(items));

            _items = items;
        }

        /// <inheritdoc/>
        public async override Task<IGeneralReply> ParseGenerallyAsync(TokenStream tokens, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            IGeneralReply reply = null;

            var unparsedTokens = tokens;

            foreach (var item in _items)
            {
                reply = await item.ParseGenerallyAsync(unparsedTokens, cancellationToken);

                if (!reply.Success)
                    return reply;

                unparsedTokens = reply.UnparsedTokens;
            }

            return reply;
        }

        /// <inheritdoc/>
        protected override string BuildExpression()
        {
            var sb = new StringBuilder("<SKIP ");

            sb.Append(string.Join<IAsyncGeneralParser>(" ", _items));
            sb.Append(">");

            return sb.ToString();
        }

        private readonly IAsyncGeneralParser[] _items;
    }
}
