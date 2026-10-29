using System;

namespace SheepStrike.Domain.Core
{
    /// <summary>Integer cell coordinate inside a voxel model.</summary>
    public readonly struct VoxelPos : IEquatable<VoxelPos>
    {
        public static readonly VoxelPos[] FaceDirections =
        {
            new VoxelPos(1, 0, 0), new VoxelPos(-1, 0, 0),
            new VoxelPos(0, 1, 0), new VoxelPos(0, -1, 0),
            new VoxelPos(0, 0, 1), new VoxelPos(0, 0, -1)
        };

        public readonly int X;
        public readonly int Y;
        public readonly int Z;

        public VoxelPos(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static VoxelPos operator +(VoxelPos a, VoxelPos b) => new VoxelPos(a.X + b.X, a.Y + b.Y, a.Z + b.Z);

        public bool Equals(VoxelPos other) => X == other.X && Y == other.Y && Z == other.Z;

        public override bool Equals(object obj) => obj is VoxelPos other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = X * 73856093;
                hash ^= Y * 19349663;
                hash ^= Z * 83492791;
                return hash;
            }
        }

        public override string ToString() => $"({X},{Y},{Z})";
    }
}
