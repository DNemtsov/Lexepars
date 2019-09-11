using System;
using System.Collections.Generic;
using System.Linq;

namespace Lexepars.OffsideRule
{
    public class OffsideRuleTokenizationContext
    {
        public OffsideRuleTokenizationContext(LinedInputText text, IEnumerable<FlowExtent> flowExtents)
        {
            Text = text ?? throw new ArgumentNullException(nameof(text));
            IndentLevels = new Stack<int>();

            Flow = new Flow(flowExtents);
        }

        public LinedInputText Text { get; }

        protected Stack<int> IndentLevels { get; }

        protected Flow Flow { get; }

        protected int CurrentIndentLevel { get; set; }

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
        /// It is called after a new indent token has been matched.
        /// </summary>
        /// <param name="indent">The indent to process. Not null.</param>
        /// <param name="token">The token matched.</param>
        /// <returns>Whether to continue matching indents.</returns>
        public virtual void OnProcessIndent(Indent indent, Token token)
        {
            CurrentIndentLevel = indent.CalculateNewIndentLevel(CurrentIndentLevel, token);
        }

        public virtual bool StopIndentLexing() => false;

        public virtual void OnIndentLexingComplete()
        {
        }

        public virtual void OnProcessToken(Token token)
        {
        }

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

        public virtual int ResetIndentation()
        {
            var scopesToClose = IndentLevels.Count;

            CurrentIndentLevel = 0;
            IndentLevels.Clear();

            return scopesToClose;
        }
    }
}
