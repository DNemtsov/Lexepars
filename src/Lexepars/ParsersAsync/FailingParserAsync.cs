using Lexepars.Parsers;
using System.Threading;
using System.Threading.Tasks;

namespace Lexepars.ParsersAsync
{
    /// <summary>
    /// Behaves like <see cref="FailingParser{TValue}"/>, except could be used with parallel parsers.
    /// </summary>
    /// <typeparam name="TValue">The type of the parsed value.</typeparam>
    public class FailingParserAsync<TValue> : AsyncParser<TValue>
    {
        private readonly FailureMessage _message;

        /// <summary>
        /// Creates a new instance of <see cref="FailingParserAsync{TValue}"/>.
        /// </summary>
        /// <param name="message">The failure message to appear on the result.</param>
        public FailingParserAsync(FailureMessage message = null)
        {
            _message = message ?? FailureMessage.Unknown();
        }

        ///<inheritdoc/>
        public override Task<IReply<TValue>> ParseAsync(TokenStream tokens, CancellationToken cancellationToken) => Task.FromResult<IReply<TValue>>(new Failure<TValue>(tokens, _message));

        ///<inheritdoc/>
        public override Task<IGeneralReply> ParseGenerallyAsync(TokenStream tokens, CancellationToken cancellationToken) => Task.FromResult<IGeneralReply>(new GeneralFailure(tokens, _message));

        ///<inheritdoc/>
        protected override string BuildExpression() => "<FAIL>";
    }
}