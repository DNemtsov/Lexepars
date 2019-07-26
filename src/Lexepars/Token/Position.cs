namespace Lexepars
{
    using System;

    public struct Position : IEquatable<Position>
    {
        public int Line { get; }
        public int Column { get; }

        public Position(int line, int column)
        {
            Line = line;
            Column = column;
        }

        public override bool Equals(object obj)
        {
            switch (obj)
            {
                case Position p:
                    return this == p;
                default:
                    return false;
            }
        }

        public bool Equals(Position other) => this == other;

        public override int GetHashCode()
        {
            return Line ^ Column;
        }

        public static bool operator ==(Position a, Position b) => a.Column == b.Column && a.Line == b.Line;
        public static bool operator !=(Position a, Position b) => !(a == b);

        public override string ToString() => $"({Line}, {Column})";
    }
}
