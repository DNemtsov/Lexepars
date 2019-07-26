using System;
using Lexepars.Parsers;

namespace Lexepars
{
    public static class TokenKindExtensions
    {
        /// <summary>
        /// Creates a <see cref="ConstantParser"/>.
        /// </summary>
        /// <typeparam name="TValue">The type of the parsed result.</typeparam>
        /// <param name="constant">The constant value to be returned.</param>
        /// <returns>The new instance of <see cref="ConstantParser"/>. Not null.</returns>
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
    }
}
