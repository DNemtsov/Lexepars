using System;

namespace Lexepars
{
    public interface IInputText
    {
        string Peek(int characters);

        void Advance(int characters);

        bool EndOfInput { get; }

        MatchResult Match(TokenRegex regex);

        MatchResult Match(Predicate<char> test);

        Position Position { get; }
    }
}
