using System;
using Lexepars.Parsers;

namespace Lexepars
{
    /// <summary>
    /// Extension methods for the <see cref="TokenKind"/>.
    /// </summary>
    public static class TokenKindExtensions
    {
        /// <summary>
        /// Creates a <see cref="ConstantParser{TValue}"/>.
        /// </summary>
        /// <typeparam name="TValue">The type of the parsed result.</typeparam>
        /// <param name="tokenKind">The kind of token. Not null.</param>
        /// <param name="constant">The constant value to be returned.</param>
        /// <returns>The new instance of <see cref="ConstantParser{TValue}"/>. Not null.</returns>
        public static ConstantParser<TValue> Constant<TValue>(this TokenKind tokenKind, TValue constant) => new ConstantParser<TValue>(tokenKind, constant);

        /// <summary>
        /// Creates a <see cref="TokenByKindParser"/>.
        /// </summary>
        /// <returns>The new instance of <see cref="TokenByKindParser"/>. Not null.</returns>
        public static TokenByKindParser Kind(this TokenKind tokenKind) => new TokenByKindParser(tokenKind);

        /// <summary>
        /// Creates a <see cref="ReturnTokenLexemeParser"/>.
        /// </summary>
        /// <returns>The new instance of <see cref="ReturnTokenLexemeParser"/>. Not null.</returns>
        public static ReturnTokenLexemeParser Lexeme(this TokenKind tokenKind) => new ReturnTokenLexemeParser(tokenKind);

        /// <summary>
        /// Creates a <see cref="BindTokenLexemeByKindParser{TValue}"/>.
        /// </summary>
        /// <typeparam name="TValue">The type of the parsed result.</typeparam>
        /// <returns>The new instance of <see cref="BindTokenLexemeByKindParser{TValue}"/>. Not null.</returns>
        public static BindTokenLexemeByKindParser<TValue> BindLexeme<TValue>(this TokenKind kind, Func<string, TValue> resultContinuation) => new BindTokenLexemeByKindParser<TValue>(kind, resultContinuation);

        /// <summary>
        /// Creates a <see cref="SkipTakeParser{TValue}"/> skipping the specified-kind token of the and then taking the `item.
        /// </summary>
        /// <typeparam name="TValue">The type of the parsed result.</typeparam>
        /// <returns>The new instance of <see cref="SkipTakeParser{TValue}"/>. Not null.</returns>
        public static SkipTakeParser<TValue> SkipThenTake<TValue>(this TokenKind tokenKind, IParser<TValue> item) => new SkipTakeParser<TValue>(new TokenByKindParser(tokenKind), item);
    }
}
