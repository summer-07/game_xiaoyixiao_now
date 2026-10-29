using System;

namespace SheepStrike.Domain.Core
{
    /// <summary>Unit vector, in voxel-model coordinates, pointing from the model towards the viewer.</summary>
    public readonly struct ViewDirection
    {
        public readonly float X;
        public readonly float Y;
        public readonly float Z;

        public ViewDirection(float x, float y, float z)
        {
            float length = (float)Math.Sqrt(x * x + y * y + z * z);
            if (length <= 0f) throw new ArgumentException("A view direction cannot be zero.");
            X = x / length;
            Y = y / length;
            Z = z / length;
        }

        public float Dot(float x, float y, float z) => X * x + Y * y + Z * z;
    }
}
