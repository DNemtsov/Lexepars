using Lexepars.Parsers;
using System.Threading;
using System.Threading.Tasks;

namespace Lexepars.Async_Parsers
{
    /// <summary>
    /// Base class for a typical asynchronous parser. Has to be inherited from.
    /// </summary>
    public abstract class AsyncParser : IAsyncGeneralParser
    {
        /// <inheritdoc/>
        public abstract Task<IGeneralReply> ParseGenerallyAsync(TokenStream tokens, CancellationToken cancellationToken);

        /// <summary>
        /// Returns the parser expression.
        /// </summary>
        /// <returns>Parser expression string. Not null.</returns>
        public override string ToString() => Expression;

        /// <summary>
        /// Builds the parser expression.
        /// </summary>
        /// <returns>Expression string. Not null.</returns>
        protected abstract string BuildExpression();

        /// <inheritdoc/>
        public string Expression
        {
            get
            {
                if (_nameRecursionGuard)
                    return "<~>";

                _nameRecursionGuard = true;

                var name = BuildExpression();

                _nameRecursionGuard = false;

                return name;
            }
        }

        private bool _nameRecursionGuard;
    }

    /// <summary>
    /// Base class for a typical asynchronous parser. Has to be inherited from.
    /// </summary>
    public abstract class AsyncParser<TValue> : AsyncParser, IAsyncParser<TValue>
    {
        /// <inheritdoc/>
        public abstract Task<IReply<TValue>> ParseAsync(TokenStream tokens, CancellationToken cancellationToken);

        /// <inheritdoc/>
        public async override Task<IGeneralReply> ParseGenerallyAsync(TokenStream tokens, CancellationToken cancellationToken) => await ParseAsync(tokens, cancellationToken);
    }
}
