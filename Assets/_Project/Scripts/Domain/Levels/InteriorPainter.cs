using System.Collections.Generic;
using System.Linq;
using SheepStrike.Domain.Core;

namespace SheepStrike.Domain.Levels
{
    /// <summary>
    /// Repaints an animal's hidden interior in concentric shells, so peeling it reveals new colours instead of
    /// one solid block. Interior colours are borrowed from the animal's own surface (skipping its dominant colour),
    /// so the level never gains a colour and the sheep mix stays as easy to read as before.
    /// </summary>
    public static class InteriorPainter
    {
        /// <param name="paletteSize">How many surface colours to reuse inside; 0 leaves the model untouched.</param>
        /// <param name="shellThickness">How many voxel layers each interior colour band spans.</param>
        public static Dictionary<VoxelPos, GameColor> Paint(IReadOnlyDictionary<VoxelPos, GameColor> voxels, int paletteSize, int shellThickness)
        {
            var painted = new Dictionary<VoxelPos, GameColor>(voxels.Count);
            foreach (var pair in voxels) painted.Add(pair.Key, pair.Value);
            if (paletteSize <= 0 || shellThickness <= 0) return painted;

            var depth = DepthFromSurface(voxels);
            var palette = InteriorPalette(voxels, depth, paletteSize);
            if (palette.Count == 0) return painted;

            foreach (var pair in depth)
            {
                if (pair.Value <= 1) continue;
                int band = (pair.Value - 2) / shellThickness;
                painted[pair.Key] = palette[band % palette.Count];
            }
            return painted;
        }

        /// <summary>Breadth-first from every exposed voxel inwards: the surface is depth 1.</summary>
        private static Dictionary<VoxelPos, int> DepthFromSurface(IReadOnlyDictionary<VoxelPos, GameColor> voxels)
        {
            var depth = new Dictionary<VoxelPos, int>(voxels.Count);
            var frontier = new Queue<VoxelPos>();
            foreach (var pos in voxels.Keys)
            {
                if (!IsExposed(voxels, pos)) continue;
                depth[pos] = 1;
                frontier.Enqueue(pos);
            }

            while (frontier.Count > 0)
            {
                var pos = frontier.Dequeue();
                foreach (var direction in VoxelPos.FaceDirections)
                {
                    var next = pos + direction;
                    if (!voxels.ContainsKey(next) || depth.ContainsKey(next)) continue;
                    depth[next] = depth[pos] + 1;
                    frontier.Enqueue(next);
                }
            }
            return depth;
        }

        /// <summary>The surface colours after the dominant one, most common first; the dominant one only if nothing else exists.</summary>
        private static List<GameColor> InteriorPalette(IReadOnlyDictionary<VoxelPos, GameColor> voxels, Dictionary<VoxelPos, int> depth, int paletteSize)
        {
            var bySurfaceCount = depth.Where(p => p.Value == 1)
                .GroupBy(p => voxels[p.Key])
                .OrderByDescending(g => g.Count())
                .ThenBy(g => g.Key)
                .Select(g => g.Key)
                .ToList();
            if (bySurfaceCount.Count <= 1) return bySurfaceCount;
            return bySurfaceCount.Skip(1).Take(paletteSize).ToList();
        }

        private static bool IsExposed(IReadOnlyDictionary<VoxelPos, GameColor> voxels, VoxelPos pos)
        {
            foreach (var direction in VoxelPos.FaceDirections)
            {
                if (!voxels.ContainsKey(pos + direction)) return true;
            }
            return false;
        }
    }
}
