using System;

namespace SheepStrike.Domain.Shooters
{
    /// <summary>Where a pen slot sits: X along the main row (0 = left end), Row 0 for the main row, 1+ above it.</summary>
    public readonly struct PenPosition : IEquatable<PenPosition>
    {
        public PenPosition(int x, int row)
        {
            X = x;
            Row = row;
        }

        public int X { get; }
        public int Row { get; }

        public bool Equals(PenPosition other) => X == other.X && Row == other.Row;

        public override bool Equals(object obj) => obj is PenPosition other && Equals(other);

        public override int GetHashCode() => X * 31 + Row;

        public override string ToString() => $"({X},{Row})";
    }
}
