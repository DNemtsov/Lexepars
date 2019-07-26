using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace Lexepars.OffsideRule
{
    public class OffsideRuleSpecialTokenConfig : SpecialTokenConfig
    {
        public ScopeTokenKind ScopeBegin { get; }

        public ScopeTokenKind ScopeEnd { get; }

        public ScopeTokenKind ScopeInconsistent { get; }

        public OffsideRuleSpecialTokenConfig(NullLexemeTokenKind unknown, ScopeTokenKind scopeBegin, ScopeTokenKind scopeEnd, ScopeTokenKind scopeInconsistent)
            : base(unknown)
        {
            ScopeBegin = scopeBegin;
            ScopeEnd = scopeEnd;
            ScopeInconsistent = scopeInconsistent;
        }
    }

    public enum ScopeState
    {
        Begin,
        End,
        Inconsistent
    }

    public class OffsideRuleTokenizationContext
    {
        public OffsideRuleTokenizationContext(LinedInputText text, IEnumerable<FlowExtent> flowExtents)
        {
            Text = text ?? throw new ArgumentNullException(nameof(text));
            IndentLevels = new Stack<int>();

            Flow = new Flow(flowExtents);
        }

        public LinedInputText Text { get; }

        public Stack<int> IndentLevels { get; }

        protected Flow Flow { get; }

        public virtual int CurrentIndentLevel { get; protected set; }

        protected bool TrackScopesInCurrentLine { get; set; }

        public virtual void OnLineTokenizingBegin()
        {
            CurrentIndentLevel = 0;
            TrackScopesInCurrentLine = true;
        }

        public virtual Flow.State OnProcessTheFlow(TokenKind tokenKind)
        {
            if (Flow == null || !TrackScopesInCurrentLine)
                return Flow.State.NoExtent;

            var flowState = Flow.ProcessTokenKind(tokenKind);

            if (flowState != Flow.State.NoExtent)
                TrackScopesInCurrentLine = false;

            return flowState;
        }

        /// <summary>
        /// Is called after a new indent token has been matched.
        /// </summary>
        /// <param name="indent">The indent to process. Not null.</param>
        /// <param name="token">The token matched.</param>
        /// <returns>Whether to continue match indents.</returns>
        public virtual void OnProcessIndent(Indent indent, Token token)
        {
            var indentPredefinedValue = indent.PredefinedValue;

            if (indentPredefinedValue == 0)
                CurrentIndentLevel += token.Lexeme?.Length ?? 0;
            else if (indentPredefinedValue > 0)
                CurrentIndentLevel += indentPredefinedValue;
        }

        public virtual bool NoMoreIndents() => false;

        public virtual void OnIndentLexingComplete()
        { }

        public virtual void OnProcessToken(Token token)
        { }

        public virtual IEnumerable<ScopeState> BalanceScope()
        {
            if (!TrackScopesInCurrentLine)
                return Enumerable.Empty<ScopeState>();

            if (IndentLevels.Count == 0 || IndentLevels.Peek() < CurrentIndentLevel)
            {
                IndentLevels.Push(CurrentIndentLevel);

                return new[] { ScopeState.Begin };
            }

            if (IndentLevels.Peek() > CurrentIndentLevel)
            {
                var scopesOpened = IndentLevels.Count;

                do
                {
                    IndentLevels.Pop();

                    if (IndentLevels.Count == 0 || IndentLevels.Peek() < CurrentIndentLevel)
                        return new[] { ScopeState.Inconsistent };
                }
                while (IndentLevels.Peek() > CurrentIndentLevel);

                var scopesToClose = scopesOpened - IndentLevels.Count;

                return Enumerable.Repeat(ScopeState.End, scopesToClose);
            }

            return Enumerable.Empty<ScopeState>();
        }
    }

    public abstract class OffsideRuleLexer<TTokenizationContext> : LexerBase<TextReader>
        where TTokenizationContext : OffsideRuleTokenizationContext
    {
        public OffsideRuleLexer(OffsideRuleSpecialTokenConfig specialTokens, IReadOnlyList<Indent> indents, FlowExtent[] flowExtents, params MatchableTokenKindSpec[] tokenKinds)
            : base(specialTokens, tokenKinds)
        {
            ArgumentCheck.NotNullOrEmptyOrWithNulls(indents, nameof(indents));
            ArgumentCheck.NotNullOrEmptyOrWithNulls(tokenKinds, nameof(tokenKinds));

            _indents = indents;

            ScopeBegin = specialTokens?.ScopeBegin ?? ScopeTokenKind.ScopeBegin;
            ScopeEnd = specialTokens?.ScopeEnd ?? ScopeTokenKind.ScopeEnd;
            ScopeInconsistent = specialTokens?.ScopeInconsistent ?? ScopeTokenKind.ScopeInconsistent;

            FlowExtents = flowExtents;
        }

        protected abstract TTokenizationContext CreateTokenizationContext(LinedInputText text, IEnumerable<FlowExtent> flowExtents);

        private readonly IReadOnlyList<Indent> _indents;

        protected ScopeTokenKind ScopeBegin { get; }

        protected ScopeTokenKind ScopeEnd { get; }

        protected ScopeTokenKind ScopeInconsistent { get; }

        protected IEnumerable<FlowExtent> FlowExtents { get; }

        public override IEnumerable<Token> Tokenize(TextReader textReader)
        {
            var text = new LinedInputText(textReader);

            var context = CreateTokenizationContext(text, FlowExtents);

            while (text.ReadLine())
            {
                context.OnLineTokenizingBegin();

                for (int i = 0, indentsCount = _indents.Count; i < indentsCount; ++i)
                {
                    if (text.EndOfLine)
                        break;

                    var indent = _indents[i];

                    if (!indent.Token.TryMatch(text, out Token token))
                        continue;

                    if (context.OnProcessTheFlow(token.Kind) == Flow.State.IncorrectExtent)
                    {
                        yield return ScopeInconsistent.CreateTokenAtCurrentPosition(text);
                        yield break;
                    }

                    context.OnProcessIndent(indent, token);

                    var skipCurrentToken = ShouldSkipToken(token, context);

                    if (!skipCurrentToken && (indent.ScopePerToken == ScopePerToken.None || indent.ScopePerToken == ScopePerToken.EmitAfter))
                        yield return token;

                    if (indent.ScopePerToken != ScopePerToken.None)
                    {
                        foreach (var scope in context.BalanceScope())
                            switch (scope)
                            {
                                case ScopeState.Begin:
                                    yield return ScopeBegin.CreateTokenAtCurrentPosition(text);
                                    break;
                                case ScopeState.End:
                                    yield return ScopeEnd.CreateTokenAtCurrentPosition(text);
                                    break;
                                case ScopeState.Inconsistent:
                                    yield return ScopeInconsistent.CreateTokenAtCurrentPosition(text);
                                    yield break;
                            }
                    }

                    if (!skipCurrentToken && indent.ScopePerToken == ScopePerToken.EmitBefore)
                        yield return token;

                    text.Advance(token.Lexeme.Length);

                    if (context.NoMoreIndents())
                        break;

                    i = -1; // start from the beginning of _indents
                }

                context.OnIndentLexingComplete();

                while (!text.EndOfLine)
                {
                    var current = GetToken(text, context);

                    context.OnProcessToken(current);

                    if (current == null)
                    {
                        yield return CreateUnknownToken(text);
                        yield break;
                    }

                    if (context.OnProcessTheFlow(current.Kind) == Flow.State.IncorrectExtent)
                    {
                        yield return ScopeInconsistent.CreateTokenAtCurrentPosition(text);
                        yield break;
                    }

                    foreach (var scope in context.BalanceScope())
                        switch (scope)
                        {
                            case ScopeState.Begin:
                                yield return ScopeBegin.CreateTokenAtCurrentPosition(text);
                                break;
                            case ScopeState.End:
                                yield return ScopeEnd.CreateTokenAtCurrentPosition(text);
                                break;
                            case ScopeState.Inconsistent:
                                yield return ScopeInconsistent.CreateTokenAtCurrentPosition(text);
                                yield break;
                        }

                    text.Advance(current.Lexeme.Length);

                    if (ShouldSkipToken(current, context))
                        continue;

                    yield return current;
                }
            }

            for (var i = 0; i < context.IndentLevels.Count; ++i)
                yield return ScopeEnd.CreateTokenAtCurrentPosition(text);
        }

        protected virtual bool ShouldSkipToken(Token token, TTokenizationContext context)
            => SkippableTokenKinds.Contains(token.Kind);

        protected virtual Token GetToken(ILinedInputText text, TTokenizationContext context)
        {
            return GetToken(text);
        }
    }

    public class OffsideRuleLexer : OffsideRuleLexer<OffsideRuleTokenizationContext>
    {
        public OffsideRuleLexer(OffsideRuleSpecialTokenConfig specialTokens, IReadOnlyList<Indent> indents, FlowExtent[] flowExtents, params MatchableTokenKindSpec[] tokenKinds)
            : base(specialTokens, indents, flowExtents, tokenKinds)
        { }

        protected override OffsideRuleTokenizationContext CreateTokenizationContext(LinedInputText text, IEnumerable<FlowExtent> flowExtents)
        {
            return new OffsideRuleTokenizationContext(text, flowExtents);
        }
    }

    public enum ScopePerToken
    {
        None,
        EmitBefore,
        EmitAfter,
        EmitInstead
    }

    public class Indent
    {
        public MatchableTokenKind Token { get; }

        public int PredefinedValue { get; }

        public ScopePerToken ScopePerToken { get; }

        public static Indent LexemeLength(MatchableTokenKind token, ScopePerToken scopeEmitType = ScopePerToken.None)
        {
            return new Indent(token, scopeEmitType, 0);
        }

        public static Indent Predefined(MatchableTokenKind token, int predefinedValue, ScopePerToken scopeEmitType = ScopePerToken.None)
        {
            if (predefinedValue < 1)
                throw new ArgumentOutOfRangeException(nameof(predefinedValue), "Should not be less than 1.");

            return new Indent(token, scopeEmitType, predefinedValue);
        }

        public static Indent Ignore(MatchableTokenKind token)
        {
            return new Indent(token, ScopePerToken.None, -1);
        }

        private Indent(MatchableTokenKind token, ScopePerToken scopeEmitType, int predefinedValue)
        {
            Token = token ?? throw new ArgumentNullException(nameof(token));

            PredefinedValue = predefinedValue;

            ScopePerToken = scopeEmitType;
        }
    }
}
