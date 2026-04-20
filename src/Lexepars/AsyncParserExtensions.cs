using Lexepars.ParsersAsync;
using Lexepars.Parsers;
using System;

namespace Lexepars
{
    /// <summary>
    /// Extension methods for the <see cref="IAsyncParser{TValue}"/>.
    /// </summary>
    public static class ParserExtensionsAsync
    {
        /// <summary>
        /// Allows LINQ syntax to construct a new parser from a simpler parser, using a single 'from' clause.
        /// </summary>
        /// <typeparam name="TInterim">The type of the interim parsed result.</typeparam>
        /// <typeparam name="TValue">The type of the final parsed result.</typeparam>
        /// <param name="interimParser">The interim parser. Not null.</param>
        /// <param name="resultMapping">Function mapping the interim parsing result to the final parsing result. Not null.</param>
        public static IAsyncParser<TValue> Select<TInterim, TValue>(this IAsyncParser<TInterim> interimParser, Func<TInterim, TValue> resultMapping) => new MonadicBindParserAsync<TInterim, TValue>(interimParser, resultMapping);

        /// <summary>
        /// Allows LINQ syntax to contruct a new parser from an ordered sequence of simpler parsers, using multiple 'from' clauses.
        /// </summary>
        /// <typeparam name="TInterim1">The type of the first interim parsed result.</typeparam>
        /// <typeparam name="TInterim2">The type of the second interim parsed result.</typeparam>
        /// <typeparam name="TValue">The type of the final parsed result.</typeparam>
        /// <param name="interimParser1">First interim parser. Not null.</param>
        /// <param name="result1ToParser2Mapping">Function mapping the first interim parsing result to the second interim parser. Not null.</param>
        /// <param name="resultMapping">Function mapping the interim parsing result to the final parsing result. Not null.</param>
        public static IAsyncParser<TValue> SelectMany<TInterim1, TInterim2, TValue>
            (this IAsyncParser<TInterim1> interimParser1, Func<TInterim1, IAsyncParser<TInterim2>> result1ToParser2Mapping, Func<TInterim1, TInterim2, TValue> resultMapping) 
            => new MonadicBindParserAsync<TInterim1, TInterim2, TValue>(interimParser1, result1ToParser2Mapping, resultMapping);

        /// <summary>
        /// Binds a result-mapping function to the `interimParser.
        /// </summary>
        /// <typeparam name="TInterim">The type of the interim parsed result.</typeparam>
        /// <typeparam name="TValue">The type of the final parsed result.</typeparam>
        /// <param name="interimParser">The interim parser. Not null.</param>
        /// <param name="resultMapping">Function mapping the interim parsing result to the final parsing result. Not null.</param>
        public static MonadicBindParserAsync<TInterim, TValue> Bind<TInterim, TValue>(this IAsyncParser<TInterim> interimParser, Func<TInterim, TValue> resultMapping) => new MonadicBindParserAsync<TInterim, TValue>(interimParser, resultMapping);

        [Obsolete(message: "Use TakeAndThenSkip instead.")]
        public static IAsyncParser<TValue> ThenSkip<TValue>(this IAsyncParser<TValue> item, IAsyncGeneralParser following) => new TakeSkipParserAsync<TValue>(item, following);

        /// <summary>
        /// Takes the `item and then skips the `skip.
        /// </summary>
        /// <typeparam name="TValue">The type of the parsed result.</typeparam>
        /// <param name="item">The item parser. Not null.</param>
        /// <param name="skip">The parser of the part to be skipped. Not null.</param>
        public static IAsyncParser<TValue> TakeAndThenSkip<TValue>(this IAsyncParser<TValue> item, IAsyncGeneralParser skip) => new TakeSkipParserAsync<TValue>(item, skip);

        [Obsolete(message: "Use SkipAndThenTake instead.")]
        public static IAsyncParser<TValue> Take<TValue>(this IAsyncGeneralParser preceding, IAsyncParser<TValue> item) => new SkipTakeParserAsync<TValue>(preceding, item);

        /// <summary>
        /// Skips the `skip and then takes the `item.
        /// </summary>
        /// <typeparam name="TValue">The type of the parsed result.</typeparam>
        /// <param name="skip">The parser of the part to be skipped. Not null.</param>
        /// <param name="item">The item parser. Not null.</param>
        public static SkipTakeParserAsync<TValue> SkipAndThenTake<TValue>(this IAsyncGeneralParser skip, IAsyncParser<TValue> item) => new SkipTakeParserAsync<TValue>(skip, item);
    }
}