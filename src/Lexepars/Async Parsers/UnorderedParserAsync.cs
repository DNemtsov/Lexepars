using Lexepars.Parsers;
using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Lexepars.Async_Parsers
{
    /// <summary>
    /// Behaves like <see cref="UnorderedParserAsync{TValue}"/>, except could be used with parallel parsers.
    /// </summary>
    /// <typeparam name="TValue">The type of the parsed value.</typeparam>
    public class UnorderedParserAsync<TValue> : AsyncParser<TValue[]>
        {
            /// <summary>
            /// Creates a new instance of <see cref="UnorderedParserAsync{TValue}"/>.
            /// </summary>
            /// <param name="separator">Item separator. Not null.</param>
            /// <param name="mode">Parsing mode.</param>
            /// <param name="items">The item parsers. Not null. Not empty.</param>
            public UnorderedParserAsync(IAsyncGeneralParser separator, UnorderedParsingMode mode, params IAsyncParser<TValue>[] items)
            {
                ArgumentCheck.NotNullOrEmptyOrWithNulls(items, nameof(items));

                _separator = separator ?? throw new ArgumentNullException(nameof(separator));
                _mode = mode;
                _items = items;
            }

            /// <summary>
            /// Creates a new instance of <see cref="UnorderedParserAsync{TValue}"/>.
            /// </summary>
            /// <param name="mode">Parsing mode.</param>
            /// <param name="items">The item parsers. Not null. Not empty.</param>
            public UnorderedParserAsync(UnorderedParsingMode mode = UnorderedParsingMode.FullSet, params IAsyncParser<TValue>[] items)
            {
                ArgumentCheck.NotNullOrEmptyOrWithNulls(items, nameof(items));

                _mode = mode;
                _items = items;
            }

            /// <summary>
            /// Creates a new instance of <see cref="UnorderedParserAsync{TValue}"/> configured for <see cref="UnorderedParsingMode.FullSet"/>
            /// </summary>
            /// <param name="items">The item parsers. Not null. Not empty.</param>
            public UnorderedParserAsync(params IAsyncParser<TValue>[] items)
            {
                ArgumentCheck.NotNullOrEmptyOrWithNulls(items, nameof(items));

                _items = items;
            }

            private readonly IAsyncParser<TValue>[] _items;
            private readonly IAsyncGeneralParser _separator;
            private readonly UnorderedParsingMode _mode;

            /// <inheritdoc/>
            public async override Task<IReply<TValue[]>> ParseAsync(TokenStream tokens, CancellationToken cancellationToken)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var tokensToParse = tokens ?? throw new ArgumentNullException(nameof(tokens));

                var parsersLength = _items.Length;

                var result = new TValue[parsersLength];
                var parsersUsed = new bool[parsersLength];
                var parsersToGo = parsersLength;

                var separatorParserIsPresent = _separator != null;

                var failures = FailureMessages.Empty;

                var atLeastOneItemParsed = false;

                var separatorWasTheLastTokenParsed = false;

                for (var i = 0; i < parsersLength; ++i)
                {
                    if (parsersUsed[i])
                        continue;

                    if (separatorParserIsPresent && atLeastOneItemParsed && !separatorWasTheLastTokenParsed)
                    {
                        var positionBeforeSeparator = tokensToParse.Position;
                        var separatorReply = await _separator.ParseGenerallyAsync(tokensToParse, cancellationToken);
                        var positionAfterSeparator = separatorReply.UnparsedTokens.Position;

                        separatorWasTheLastTokenParsed = separatorReply.Success;

                        if (separatorWasTheLastTokenParsed)
                        {
                            if (positionBeforeSeparator == positionAfterSeparator)
                                throw new Exception($"Separator parser {_separator.Expression} encountered a potential infinite loop at position {positionBeforeSeparator}.");
                        }
                        else
                        {
                            if (positionBeforeSeparator != positionAfterSeparator || _mode == UnorderedParsingMode.FullSet)
                                return Failure<TValue[]>.From(separatorReply);
                        }

                        tokensToParse = separatorReply.UnparsedTokens;
                    }

                    var oldPosition = tokensToParse.Position;
                    var reply = await _items[i].ParseAsync(tokensToParse, cancellationToken);
                    var newPosition = reply.UnparsedTokens.Position;

                    tokensToParse = reply.UnparsedTokens;

                    if (reply.Success)
                    {
                        if (newPosition == oldPosition)
                            throw new Exception($"Item parser {_items[i].Expression} encountered a potential infinite loop at position {newPosition}.");

                        result[i] = reply.ParsedValue;
                        parsersUsed[i] = true;
                        --parsersToGo;

                        i = -1; // start from the beginning of _parsers
                        atLeastOneItemParsed = true;
                        separatorWasTheLastTokenParsed = false;
                        failures = FailureMessages.Empty;

                        continue;
                    }
                    else if (newPosition != oldPosition)
                        return Failure<TValue[]>.From(reply);

                    failures = failures.Merge(reply.FailureMessages);

                    var triedAllParsers = i == parsersLength - 1;

                    if (newPosition != oldPosition || (triedAllParsers && (separatorWasTheLastTokenParsed || _mode == UnorderedParsingMode.FullSet)))
                        return new Failure<TValue[]>(reply.UnparsedTokens, failures);
                }

                if ((parsersToGo > 0 && _mode == UnorderedParsingMode.FullSet) || (!atLeastOneItemParsed && _mode == UnorderedParsingMode.NonemptySubset))
                    return new Failure<TValue[]>(tokensToParse, failures);

                return new Success<TValue[]>(result, tokensToParse);
            }

            /// <inheritdoc/>
            protected override string BuildExpression()
            {
                var sb = new StringBuilder("<UNORDERED ");

                sb.Append(string.Join(" | ", _items.Select(p => p.Expression)));
                sb.Append(">");

                return sb.ToString();
            }
        }
}
