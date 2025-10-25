using System;

namespace Code.GameManagement
{
    public readonly struct BoardLocation : IEquatable<BoardLocation>
    {
        public readonly int Row;
        public readonly int Column;

        public BoardLocation(int row, int column)
        {
            Row = row;
            Column = column;
        }

        public override string ToString() =>
            $"(r: {Row}, c: {Column})";

        public bool Equals(BoardLocation other) =>
            Row == other.Row && Column == other.Column;

        public override bool Equals(object obj) =>
            obj is BoardLocation other && Equals(other);

        public override int GetHashCode() =>
            HashCode.Combine(Row, Column);
    }
}