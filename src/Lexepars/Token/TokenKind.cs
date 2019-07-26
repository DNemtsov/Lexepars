using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Lexepars
{
    /// <summary>
    /// The base class for all kinds of tokens. Should be inherited.
    /// </summary>
    public abstract class TokenKind
    {
        /// <summary>
        /// The standard token emitted at the end of the input text.
        /// </summary>
        public static readonly TokenKind EndOfInput = new EndOfInputTokenKind();

        /// <summary>
        /// The standard token emitted when all the tokens known to the lexer have failed to parse the input text.
        /// Occurrence of this token is a fatal error, so lexer should stop after emitting a token of this kind.
        /// </summary>
        public static readonly SpecialTokenKind Unknown = new UnknownTokenKind();

        /// <summary>
        /// Creates a new instance of <see cref="TokenKind"/>
        /// </summary>
        /// <param name="name">The display name of the token kind.</param>
        /// <param name="skippable">If true, the lexer won't be emitting the tokens of this kind when they are matched.</param>
        protected TokenKind(string name)
        {
            Name = name;
        }

        /// <summary>
        /// The display name of the token kind.
        /// </summary>
        public string Name { get; }

        public override string ToString() => Name;
    }

    /// <summary>
    /// The kind of token that is matched against the input text. Should be inherited.
    /// </summary>
    /// <remarks>Tokens of this kind should always have non-empty lexemes so that the input text parsing can advance.</remarks>
    public abstract class MatchableTokenKind : TokenKind
    {
        /// <summary>
        /// Creates a new instance of <see cref="MatchableTokenKind"/>
        /// </summary>
        /// <param name="name">The display name of the token kind.</param>
        /// <param name="skippable">If true, the lexer won't be emitting the tokens of this kind when they are matched.</param>
        protected MatchableTokenKind(string name)
            : base (name)
        { }

        /// <summary>
        /// Tries to match the token kind against the input text.
        /// </summary>
        /// <param name="text">The input text. Not null.</param>
        /// <param name="token">The token of this kind if matching succeeds. Null otherwise.</param>
        /// <returns></returns>
        public bool TryMatch(IInputText text, out Token token)
        {
            var match = Match(text);

            if (match.Success)
            {
                var matchValue = match.Value;

                if (string.IsNullOrEmpty(matchValue))
                    throw new InvalidOperationException("A successful match should always yield a non-empty lexeme.");

                token = new Token(this, text.Position, matchValue);
                return true;
            }

            token = null;
            return false;
        }

        /// <summary>
        /// The actual matching against the input text.
        /// </summary>
        /// <returns>The matching result. Not null. A successful match should always yield a non-empty lexeme.</returns>
        protected abstract MatchResult Match(IInputText text);

        public static implicit operator MatchableTokenKindSpec(MatchableTokenKind tokenKind)
            => new MatchableTokenKindSpec(tokenKind, false);

        public static MatchableTokenKindSpec ToMatchableTokenKindSpec(MatchableTokenKind tokenKind)
            => tokenKind;
    }

    /// <summary>
    /// The regex-based token kind.
    /// </summary>
    public class PatternTokenKind : MatchableTokenKind
    {
        private readonly TokenRegex _regex;

        /// <summary>
        /// Creates a new instance of <see cref="PatternTokenKind"/>
        /// </summary>
        /// <param name="name">The display name of the token kind.</param>
        /// <param name="pattern">The regular expression to be matched against the input text. Not null. Not empty. The \G anchor is always applied to make sure the sequence of matches is contiguous.</param>
        /// <param name="skippable">If true, the lexer won't be emitting the tokens of this kind when they are matched.</param>
        /// <param name="regexOptions">The options of the regular expression. <see cref="RegexOptions.Multiline"/> and <see cref="RegexOptions.IgnorePatternWhitespace"/> are always applied.</param>
        public PatternTokenKind(string name, string pattern, RegexOptions regexOptions = RegexOptions.None)
            : base(name)
        {
            _regex = new TokenRegex(pattern, regexOptions);
        }

        /// <summary>
        /// The actual matching method of the token kind against the input text.
        /// </summary>
        protected override sealed MatchResult Match(IInputText text) => text.Match(_regex);
    }

    /// <summary>
    /// Represents a language letter-only keyword separated by word boundaries.
    /// </summary>
    /// <remarks>Non-skippable.</remarks>
    public class KeywordTokenKind : PatternTokenKind
    {
        /// <summary>
        /// Creates a new instance of <see cref="KeywordTokenKind"/>
        /// </summary>
        /// <param name="keyword">The letter-only keyword. Not empty.</param>
        public KeywordTokenKind(string keyword)
            : base(keyword, keyword + @"\b")
        {
            if (keyword.Any(ch => !char.IsLetter(ch)))
                throw new ArgumentException("Keywords may only contain letters.", nameof(keyword));
        }
    }

    /// <summary>
    /// Represents a language operator that is matched directly against the input text.
    /// </summary>
    public class OperatorTokenKind : MatchableTokenKind
    {
        private readonly string _symbol;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="symbol">The operator string. Not empty.</param>
        /// <param name="skippable">If true, the lexer won't be emitting the tokens of this kind when they are matched.</param>
        public OperatorTokenKind(string symbol)
            : base(symbol)
        {
            _symbol = symbol ?? throw new ArgumentNullException(nameof(symbol));

            if (symbol.Length < 1)
                throw new ArgumentException("Should not be empty.", nameof(symbol));
        }

        /// <summary>
        /// The actual matching method of the token kind against the input text.
        /// </summary>
        protected override sealed MatchResult Match(IInputText text)
        {
            var peek = text.Peek(_symbol.Length);

            if (peek == _symbol)
                return MatchResult.Succeed(peek);

            return MatchResult.Fail;
        }
    }

    /// <summary>
    /// The base class for token kinds used for special purposes (i.e. end of input) that are directly created without any matching against the input text.
    /// </summary>
    /// <remarks>The text position will not be amended thus allowing to return however many tokens at the same position, potentially occupied by a matchable token as well.</remarks>
    public abstract class SpecialTokenKind : TokenKind
    {
        /// <summary>
        /// Creates a new instance of <see cref="SpecialTokenKind"/>
        /// </summary>
        /// <param name="name">The display name of the token kind.</param>
        public SpecialTokenKind(string name)
            : base(name)
        { }

        /// <summary>
        /// Creates a token of this kind at the current position of input text.
        /// </summary>
        /// <param name="text">The input text. Not null.</param>
        public Token CreateTokenAtCurrentPosition(IInputText text)
        {
            return new Token(this, text.Position, CreateLexeme(text));
        }

        /// <summary>
        /// Creates a lexeme for the token.
        /// </summary>
        /// <param name="text">The input text. Not null.</param>
        /// <remarks>The text position should not be amended.</remarks>
        protected abstract string CreateLexeme(IInputText text);
    }

    /// <summary>
    /// The standard token kind to be emitted when all the tokens known to the lexer have failed to match the input text.
    /// Occurrence of this token is a fatal error, so lexer should stop after emitting a token of this kind.
    /// </summary>
    public class UnknownTokenKind : SpecialTokenKind
    {
        /// <summary>
        /// Creates a new instance of <see cref="UnknownTokenKind"/>
        /// </summary>
        public UnknownTokenKind()
            : base("Unknown")
        { }

        /// <summary>
        /// Creates a lexeme for the token by peeking 50 characters starting at the current position.
        /// </summary>
        /// <param name="text">The input text. Not null.</param>
        /// <remarks>The text position is not be amended.</remarks>
        protected sealed override string CreateLexeme(IInputText text) => text.Peek(50);
    }

    /// <summary>
    /// The base class for token kinds without lexemes to be used for special purposes (i.e. Python-like offside-rule scopes) that are directly created without any matching against the input text.
    /// </summary>
    /// <remarks>The text position will not be amended thus allowing to return however many tokens at the same position, potentially occupied by a matchable token as well.</remarks>
    public class NullLexemeTokenKind : SpecialTokenKind
    {
        /// <summary>
        /// Creates a new instance of <see cref="NullLexemeTokenKind"/>
        /// </summary>
        public NullLexemeTokenKind(string name)
            : base(name)
        { }

        /// <summary>
        /// Returns null.
        /// </summary>
        /// <param name="text">The input text. Not used.</param>
        protected sealed override string CreateLexeme(IInputText text) => null;
    }

    /// <summary>
    /// The standard end-of-input token.
    /// </summary>
    public class EndOfInputTokenKind : NullLexemeTokenKind
    {
        /// <summary>
        /// Creates a new instance of <see cref="EndOfInputTokenKind"/>
        /// </summary>
        public EndOfInputTokenKind()
            : base("end of input")
        { }
    }
}