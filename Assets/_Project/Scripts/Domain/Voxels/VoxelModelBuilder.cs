using System.Collections.Generic;
using SheepStrike.Domain.Core;

namespace SheepStrike.Domain.Voxels
{
    /// <summary>Small fluent helper to describe voxel animals out of boxes and painted cells.</summary>
    public sealed class VoxelModelBuilder
    {
        private readonly Dictionary<VoxelPos, GameColor> _cells = new Dictionary<VoxelPos, GameColor>();

        public int Count => _cells.Count;

        public VoxelModelBuilder Box(int x, int y, int z, int width, int height, int depth, GameColor color)
        {
            for (int ix = x; ix < x + width; ix++)
            for (int iy = y; iy < y + height; iy++)
            for (int iz = z; iz < z + depth; iz++)
            {
                _cells[new VoxelPos(ix, iy, iz)] = color;
            }
            return this;
        }

        /// <summary>Recolours only cells that already exist, e.g. eyes painted on a head.</summary>
        public VoxelModelBuilder Paint(int x, int y, int z, int width, int height, int depth, GameColor color)
        {
            for (int ix = x; ix < x + width; ix++)
            for (int iy = y; iy < y + height; iy++)
            for (int iz = z; iz < z + depth; iz++)
            {
                var pos = new VoxelPos(ix, iy, iz);
                if (_cells.ContainsKey(pos))
                {
                    _cells[pos] = color;
                }
            }
            return this;
        }

        public VoxelModelBuilder Carve(int x, int y, int z, int width, int height, int depth)
        {
            for (int ix = x; ix < x + width; ix++)
            for (int iy = y; iy < y + height; iy++)
            for (int iz = z; iz < z + depth; iz++)
            {
                _cells.Remove(new VoxelPos(ix, iy, iz));
            }
            return this;
        }

        public VoxelModel Build() => new VoxelModel(_cells);
    }
}
