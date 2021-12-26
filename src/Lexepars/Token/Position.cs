namespace Lexepars
{
    using System;

    /// <summary>
    /// Represents a position in the input text.
    /// </summary>
    public struct Position : IEquatable<Position>
    {
        /// <summary>
        /// Line number.
        /// </summary>
        public int Line { get; }

        /// <summary>
        /// Column number.
        /// </summary>
        public int Column { get; }

        /// <summary>
        /// Creates a new instance of <see cref="Position"/>.
        /// </summary>
        /// <param name="line"></param>
        /// <param name="column"></param>
        public Position(int line, int column)
        {
            Line = line;
            Column = column;
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public bool Equals(Position other) => this == other;

        /// <inheritdoc/>
        public override int GetHashCode() => Line ^ Column;

        public static bool operator ==(Position a, Position b) => a.Column == b.Column && a.Line == b.Line;

        public static bool operator !=(Position a, Position b) => !(a == b);

        /// <summary>
        /// Return the line and column numbers.
        /// </summary>
        /// <returns></returns>
        public override string ToString() => $"({Line}, {Column})";
    }
}
