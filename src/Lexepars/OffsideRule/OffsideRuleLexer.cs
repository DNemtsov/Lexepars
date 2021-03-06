﻿using System.Collections.Generic;
using System.IO;

namespace Lexepars.OffsideRule
{
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

                    if (context.StopIndentLexing())
                        break;

                    i = -1; // start from the beginning of the _indents
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

            var scopesToClose = context.ResetIndentation();

            for (int i = 0; i < scopesToClose; ++i)
                yield return ScopeEnd.CreateTokenAtCurrentPosition(text);
        }

        protected virtual bool ShouldSkipToken(Token token, TTokenizationContext context) => SkippableTokenKinds.Contains(token.Kind);

        protected virtual Token GetToken(ILinedInputText text, TTokenizationContext context) => GetToken(text);
    }

    public class OffsideRuleLexer : OffsideRuleLexer<OffsideRuleTokenizationContext>
    {
        public OffsideRuleLexer(OffsideRuleSpecialTokenConfig specialTokens, IReadOnlyList<Indent> indents, FlowExtent[] flowExtents, params MatchableTokenKindSpec[] tokenKinds)
            : base(specialTokens, indents, flowExtents, tokenKinds)
        {
        }

        protected override OffsideRuleTokenizationContext CreateTokenizationContext(LinedInputText text, IEnumerable<FlowExtent> flowExtents)
        {
            return new OffsideRuleTokenizationContext(text, flowExtents);
        }
    }
}
