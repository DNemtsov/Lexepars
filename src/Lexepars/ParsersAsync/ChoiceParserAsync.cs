using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Lexepars.ParsersAsync
{
    /// <summary>
    /// Tries all the parsers in parallel, then sequentially checks the results.
    /// Parsers are applied from left to right.
    /// If a parser succeeds, its reply is returned.
    /// If a parser fails without consuming input, the next parser
    /// is attempted.  If a parser fails after consuming input,
    /// subsequent parsers will be discarded. As long as
    /// parsers fail and consume no input, their failure messages are merged.
    /// </summary>
    /// <typeparam name="TValue">The type of the parsed value.</typeparam>
    public class ChoiceParserAsync<TValue> : AsyncParser<TValue>
    {
        /// <summary>
        /// Creates a new instance of <see cref="ChoiceParserAsync{TItem}"/>.
        /// </summary>
        /// <param name="parsers">The alternative parsers. Not null. Not empty.</param>
        public ChoiceParserAsync(params IAsyncParser<TValue>[] parsers)
        {
            ArgumentCheck.NotNullOrEmptyOrWithNulls(parsers, nameof(parsers));

            _parsers = parsers;
        }

        /// <inheritdoc/>
        public async override Task<IReply<TValue>> ParseAsync(TokenStream tokens, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            var oldPosition = tokens.Position;
            var tasks = new List<Task<IReply<TValue>>>();
            
            for(int i = 0; i < _parsers.Length; i++)
            {
                tasks.Add(_parsers[i].ParseAsync(tokens, token));
            }

            var failures = FailureMessages.Empty;
            await Task.WhenAll(tasks);
            var reply = await tasks[0];
            var newPosition = reply.UnparsedTokens.Position;

            for (int i = 1; i < _parsers.Length; i++)
            {
                if (reply.Success)
                    break;

                if (oldPosition != newPosition)
                    break;

                failures = failures.Merge(reply.FailureMessages);
                reply = await tasks[i];
                newPosition = reply.UnparsedTokens.Position;
            }

            if (oldPosition == newPosition)
            {
                failures = failures.Merge(reply.FailureMessages);

                if (reply.Success)
                    return new Success<TValue>(reply.ParsedValue, reply.UnparsedTokens, failures);

                return new Failure<TValue>(reply.UnparsedTokens, failures);
            }

            return reply;
        }

        /// <inheritdoc/>
        public async override Task<IGeneralReply> ParseGenerallyAsync(TokenStream tokens, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var oldPosition = tokens.Position;
            var tasks = new List<Task<IGeneralReply>>();
            for (int i = 0; i < _parsers.Length; i++)
            {
                tasks.Add(_parsers[i].ParseGenerallyAsync(tokens, cancellationToken));
            }
            var failures = FailureMessages.Empty;
            await Task.WhenAll(tasks);
            var reply = await tasks[0];
            var newPosition = reply.UnparsedTokens.Position;

            for (var i = 1; i < _parsers.Length; ++i)
            {
                if (reply.Success)
                    break;

                if (oldPosition != newPosition)
                    break;

                failures = failures.Merge(reply.FailureMessages);
                reply = await tasks[i];
                newPosition = reply.UnparsedTokens.Position;
            }

            if (oldPosition == newPosition)
            {
                if (reply.Success)
                    return new GeneralSuccess(reply.UnparsedTokens);

                return new Failure<TValue>(reply.UnparsedTokens, failures.Merge(reply.FailureMessages));
            }

            return reply;
        }

        /// <inheritdoc/>
        protected override string BuildExpression()
        {
            var sb = new StringBuilder("<CHOICE ");

            sb.Append(string.Join(" OR ", _parsers.Select(p => p.Expression)));
            sb.Append(">");

            return sb.ToString();
        }

        private readonly IAsyncParser<TValue>[] _parsers;
    }
}