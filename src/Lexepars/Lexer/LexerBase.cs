using System.Collections.Generic;

namespace Lexepars
{
    public class SpecialTokenConfig
    {
        public SpecialTokenKind Unknown { get; }

        public SpecialTokenConfig(SpecialTokenKind unknown)
        {
            Unknown = unknown;
        }
    }

    public abstract class LexerBase
    {
        private readonly SpecialTokenKind _unknown;

        protected LexerBase(SpecialTokenConfig specialTokens, IReadOnlyList<MatchableTokenKindSpec> tokenKindSpecs)
        {
            ArgumentCheck.NotNullOrEmptyOrWithNulls(tokenKindSpecs, nameof(tokenKindSpecs));

            var tokenKinds = new List<MatchableTokenKind>(tokenKindSpecs.Count);

            foreach (var tokenKindSpec in tokenKindSpecs)
            {
                var tokenKind = tokenKindSpec.TokenKind;

                tokenKinds.Add(tokenKind);

                if (tokenKindSpec.Skipped)
                    SkippableTokenKinds.Add(tokenKind);
            }

            TokenKinds = tokenKinds;

            _unknown = specialTokens?.Unknown ?? TokenKind.Unknown;
        }

        protected IEnumerable<MatchableTokenKind> TokenKinds { get; }

        protected ISet<TokenKind> SkippableTokenKinds { get; } = new HashSet<TokenKind>();

        protected virtual Token GetToken(IInputText text)
        {
            foreach (var kind in TokenKinds)
                if (kind.TryMatch(text, out Token token))
                    return token;

            return null;
        }

        protected static Token GetToken(IInputText text, IEnumerable<MatchableTokenKind> tokenKinds)
        {
            foreach (var kind in tokenKinds)
                if (kind.TryMatch(text, out Token token))
                    return token;

            return null;
        }

        protected Token CreateUnknownToken(IInputText text) => _unknown.CreateTokenAtCurrentPosition(text);

        public static MatchableTokenKindSpec Skip(MatchableTokenKind tokenKind)
            => new MatchableTokenKindSpec(tokenKind, true);
    }

    public abstract class LexerBase<TInput>: LexerBase
    {
        protected LexerBase(SpecialTokenConfig specialTokens, IReadOnlyList<MatchableTokenKindSpec> tokenKindSpecs)
            : base(specialTokens, tokenKindSpecs)
        { }

        public abstract IEnumerable<Token> Tokenize(TInput input);
    }
}
