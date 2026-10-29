using System;
using System.Collections.Generic;
using SheepStrike.Domain.Core;

namespace SheepStrike.Domain.Voxels
{
    /// <summary>
    /// Which voxels the player can actually see from a direction: a tiny orthographic depth buffer.
    /// Every voxel stamps its square footprint onto a grid on the view plane; the voxel nearest the viewer
    /// wins each cell, and only winners are visible. Linear in the voxel count, cheap enough to run every tick;
    /// the buffer is reused between runs so it does not churn the garbage collector.
    /// </summary>
    public sealed class VisibleSurface
    {
        private const float CellSize = 0.5f;
        private const float FootprintRadius = 0.5f;
        private const float Epsilon = 0.001f;

        private struct Winner
        {
            public float Depth;
            public VoxelPos Pos;
        }

        private readonly Dictionary<long, Winner> _buffer = new Dictionary<long, Winner>();

        public void Compute(IEnumerable<VoxelPos> voxels, ViewDirection view, HashSet<VoxelPos> visible)
        {
            Basis(view, out var u, out var v);
            _buffer.Clear();
            foreach (var pos in voxels) Stamp(pos, view, u, v);

            visible.Clear();
            foreach (var winner in _buffer.Values) visible.Add(winner.Pos);
        }

        private void Stamp(VoxelPos pos, ViewDirection view, float[] u, float[] v)
        {
            float depth = view.Dot(pos.X, pos.Y, pos.Z);
            float pu = u[0] * pos.X + u[1] * pos.Y + u[2] * pos.Z;
            float pv = v[0] * pos.X + v[1] * pos.Y + v[2] * pos.Z;

            int minU = Cell(pu - FootprintRadius + Epsilon), maxU = Cell(pu + FootprintRadius - Epsilon);
            int minV = Cell(pv - FootprintRadius + Epsilon), maxV = Cell(pv + FootprintRadius - Epsilon);
            for (int cu = minU; cu <= maxU; cu++)
            {
                for (int cv = minV; cv <= maxV; cv++)
                {
                    long key = ((long)cu << 32) ^ (uint)cv;
                    if (!_buffer.TryGetValue(key, out var current) || depth > current.Depth)
                    {
                        _buffer[key] = new Winner { Depth = depth, Pos = pos };
                    }
                }
            }
        }

        private static int Cell(float coordinate) => (int)Math.Floor(coordinate / CellSize);

        /// <summary>Two unit vectors spanning the plane perpendicular to the view.</summary>
        private static void Basis(ViewDirection view, out float[] u, out float[] v)
        {
            float[] d = { view.X, view.Y, view.Z };
            float[] up = Math.Abs(view.Y) > 0.9f ? new[] { 1f, 0f, 0f } : new[] { 0f, 1f, 0f };
            u = Normalize(Cross(up, d));
            v = Cross(d, u);
        }

        private static float[] Cross(float[] a, float[] b) =>
            new[] { a[1] * b[2] - a[2] * b[1], a[2] * b[0] - a[0] * b[2], a[0] * b[1] - a[1] * b[0] };

        private static float[] Normalize(float[] a)
        {
            float length = (float)Math.Sqrt(a[0] * a[0] + a[1] * a[1] + a[2] * a[2]);
            return new[] { a[0] / length, a[1] / length, a[2] / length };
        }
    }
}
