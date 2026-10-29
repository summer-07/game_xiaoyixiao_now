using System;
using System.Collections.Generic;
using System.Linq;
using SheepStrike.Domain.Core;

namespace SheepStrike.Domain.Levels
{
    /// <summary>
    /// Repaints an animal's hidden interior so peeling it reveals new colours instead of one solid block.
    /// The surface is never changed. Inside, voxels form concentric colour bands drawn from a small palette:
    /// a few of the animal's own surface colours plus "extra" colours that exist only inside. Noise scatters
    /// voxels out of their band. Everything comes from the seeded random, so a level always looks the same.
    /// </summary>
    public static class InteriorPainter
    {
        public static Dictionary<VoxelPos, GameColor> Paint(IReadOnlyDictionary<VoxelPos, GameColor> voxels, InteriorStyle style, IRandom random)
        {
            var painted = new Dictionary<VoxelPos, GameColor>(voxels.Count);
            foreach (var pair in voxels) painted.Add(pair.Key, pair.Value);
            if (style.IsUntouched) return painted;

            var depth = DepthFromSurface(voxels);
            var palette = Palette(voxels, depth, style, random);
            if (palette.Count == 0) return painted;

            var interior = depth.Where(p => p.Value > 1).OrderBy(p => p.Value).ThenBy(p => p.Key.X).ThenBy(p => p.Key.Y).ThenBy(p => p.Key.Z).ToList();
            foreach (var pair in interior)
            {
                int band = (pair.Value - 2) / style.ShellThickness;
                bool scattered = random.Next(1000) < style.Noise * 1000f;
                painted[pair.Key] = scattered ? palette[random.Next(palette.Count)] : palette[band % palette.Count];
            }
            EnsureEveryColourAppears(painted, interior, palette, random);
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

        /// <summary>
        /// Reused surface colours (after the dominant one, most common first; the dominant one only if nothing else
        /// exists), then extra colours drawn at random from those the surface does not use.
        /// </summary>
        private static List<GameColor> Palette(IReadOnlyDictionary<VoxelPos, GameColor> voxels, Dictionary<VoxelPos, int> depth, InteriorStyle style, IRandom random)
        {
            var surface = depth.Where(p => p.Value == 1)
                .GroupBy(p => voxels[p.Key])
                .OrderByDescending(g => g.Count())
                .ThenBy(g => g.Key)
                .Select(g => g.Key)
                .ToList();

            var palette = surface.Count <= 1 ? surface.Take(style.ReusedColors).ToList() : surface.Skip(1).Take(style.ReusedColors).ToList();

            var unused = Enum.GetValues(typeof(GameColor)).Cast<GameColor>().Where(c => !surface.Contains(c)).ToList();
            for (int i = 0; i < style.ExtraColors && unused.Count > 0; i++)
            {
                int pick = random.Next(unused.Count);
                palette.Add(unused[pick]);
                unused.RemoveAt(pick);
            }
            return palette;
        }

        /// <summary>
        /// A colour the bands or noise happened to skip still gets one interior voxel, taken from a colour that has
        /// more than one, so the style's colour count is honoured without wiping out another colour.
        /// </summary>
        private static void EnsureEveryColourAppears(Dictionary<VoxelPos, GameColor> painted, List<KeyValuePair<VoxelPos, int>> interior, List<GameColor> palette, IRandom random)
        {
            if (interior.Count < palette.Count) return;
            var counts = new Dictionary<GameColor, int>();
            foreach (var pair in interior)
            {
                counts.TryGetValue(painted[pair.Key], out int count);
                counts[painted[pair.Key]] = count + 1;
            }

            foreach (var color in palette)
            {
                if (counts.ContainsKey(color)) continue;
                var donors = interior.Where(p => counts[painted[p.Key]] > 1).ToList();
                var pos = donors[random.Next(donors.Count)].Key;
                counts[painted[pos]]--;
                painted[pos] = color;
                counts[color] = 1;
            }
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
