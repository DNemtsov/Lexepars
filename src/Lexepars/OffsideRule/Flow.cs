using System;
using System.Collections.Generic;

namespace Lexepars.OffsideRule
{
    public sealed class FlowExtent
    {
        public TokenKind Begin { get; }

        public TokenKind End { get; }

        public FlowExtent(TokenKind begin, TokenKind end)
        {
            Begin = begin;
            End = end;
        }
    }

    public sealed class Flow
    {
        public Flow(IEnumerable<FlowExtent> extents)
        {
            var extentTracks = new Dictionary<TokenKind, FlowExtentTrack>();

            foreach (var extent in extents)
            {
                var spinCount = new FlowSpinCount();

                if (extentTracks.ContainsKey(extent.Begin))
                    throw new ArgumentException($"Extent-begin token kind '{extent.Begin}' is not unique.");

                if (extentTracks.ContainsKey(extent.End))
                    throw new ArgumentException($"Extent-end token kind '{extent.End}' is not unique.");

                extentTracks.Add(extent.Begin, new FlowExtentTrack(true, spinCount));
                extentTracks.Add(extent.End, new FlowExtentTrack(false, spinCount));
            }

            _extentTracks = extentTracks;
        }

        public enum State
        {
            NoExtent,
            WithinExtent,
            IncorrectExtent
        }

        public State ProcessTokenKind(TokenKind tokenKind)
        {
            if (_extentTracks.TryGetValue(tokenKind, out FlowExtentTrack track))
            {
                if (track.BeginOrEnd)
                {
                    ++track.SpinCount.Value;
                    ++_summarySpinCount;
                }
                else
                {
                    if (track.SpinCount.Value == 0)
                        return State.IncorrectExtent;

                    --track.SpinCount.Value;
                    --_summarySpinCount;
                }
            }
            else if (_summarySpinCount == 0)
                return State.NoExtent;

            return State.WithinExtent;
        }

        private class FlowSpinCount
        {
            public int Value;
        }

        private struct FlowExtentTrack
        {
            public FlowExtentTrack(bool beginOrEnd, FlowSpinCount spinCount)
            {
                BeginOrEnd = beginOrEnd;
                SpinCount = spinCount;
            }

            public readonly bool BeginOrEnd;

            public readonly FlowSpinCount SpinCount;
        }

        private readonly Dictionary<TokenKind, FlowExtentTrack> _extentTracks;

        private int _summarySpinCount;
    }
}
