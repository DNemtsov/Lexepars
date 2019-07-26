using System.Linq;
using System.Text;

namespace Lexepars.Parsers
{
    /// <summary>
    /// Sequentially tries its parsers against the input.
    /// Parsers are applied from left to right.
    /// If a parser succeeds, its reply is returned.
    /// If a parser fails without consuming input, the next parser
    /// is attempted.  If a parser fails after consuming input,
    /// subsequent parsers will not be attempted. As long as
    /// parsers fail and consume no input, their failure messages are merged.
    /// </summary>
    /// <typeparam name="TValue">The type of the parsed value.</typeparam>
    /// <remarks>
    /// The next parser is only tried when the previous one didn't
    /// consume any input, i.e. the look-ahead is 1 (one token on the current position).
    /// This non-backtracking behaviour allows for both an efficient
    /// implementation of the parser combinators and the generation
    /// of good failure messages.
    /// </remarks>
    public class ChoiceParser<TValue> : Parser<TValue>
    {
        /// <summary>
        /// Creates a new instance of <see cref="ChoiceParser{TItem}"/>.
        /// </summary>
        /// <param name="parsers">The alternative parsers. Not null. Not empty.</param>
        public ChoiceParser(params IParser<TValue>[] parsers)
        {
            ArgumentCheck.NotNullOrEmptyOrWithNulls(parsers, nameof(parsers));

            _parsers = parsers;
        }

        /// <summary>
        /// Parses the stream of tokens.
        /// </summary>
        /// <param name="tokens">Stream of tokens to parse. Not null.</param>
        /// <returns>Parsing reply. Not null.</returns>
        public override IReply<TValue> Parse(TokenStream tokens)
        {
            var oldPosition = tokens.Position;
            var reply = _parsers[0].Parse(tokens);
            var newPosition = reply.UnparsedTokens.Position;

            var failures = FailureMessages.Empty;

            for (var i = 1; i < _parsers.Length; ++i)
            {
                if (reply.Success)
                    break;

                if (oldPosition != newPosition)
                    break;

                failures = failures.Merge(reply.FailureMessages);
                reply = _parsers[i].Parse(tokens);
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

        /// <summary>
        /// Parsing optimized for the case when the reply value is not needed.
        /// </summary>
        /// <param name="tokens">The token stream to parse. Not null.</param>
        /// <returns>General parsing reply. Not null.</returns>
        public override IGeneralReply ParseGenerally(TokenStream tokens)
        {
            var oldPosition = tokens.Position;
            var reply = _parsers[0].ParseGenerally(tokens);
            var newPosition = reply.UnparsedTokens.Position;

            var failures = FailureMessages.Empty;

            for (var i = 1; i < _parsers.Length; ++i)
            {
                if (reply.Success)
                    break;

                if (oldPosition != newPosition)
                    break;

                failures = failures.Merge(reply.FailureMessages);
                reply = _parsers[i].ParseGenerally(tokens);
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

        /// <summary>
        /// Builds the parser expression.
        /// </summary>
        /// <returns>Expression string. Not null.</returns>
        protected override string BuildExpression()
        {
            var sb = new StringBuilder("<CHOICE ");

            sb.Append(string.Join(" OR ", _parsers.Select(p => p.Expression)));
            sb.Append(">");

            return sb.ToString();
        }

        private readonly IParser<TValue>[] _parsers;
    }
}