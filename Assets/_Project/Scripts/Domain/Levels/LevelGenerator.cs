using System;
using System.Collections.Generic;
using SheepStrike.Domain.Core;

namespace SheepStrike.Domain.Levels
{
    /// <summary>
    /// Turns a voxel animal into a playable level: the wool of every colour is split across sheep so that
    /// total ammo per colour equals the number of voxels of that colour (see <see cref="LevelRules.IsBalanced"/>).
    /// </summary>
    public static class LevelGenerator
    {
        public static LevelDefinition Generate(
            int number,
            string targetId,
            IReadOnlyDictionary<VoxelPos, GameColor> voxels,
            LevelTuning tuning,
            int seed)
        {
            if (tuning.MinAmmoPerSheep <= 0 || tuning.MaxAmmoPerSheep < tuning.MinAmmoPerSheep)
            {
                throw new ArgumentException("Invalid ammo range in tuning.", nameof(tuning));
            }

            var random = new SeededRandom(seed);
            var flock = SplitIntoSheep(CountColors(voxels), tuning, random);
            Shuffle(flock, random);

            return new LevelDefinition(
                number,
                targetId,
                voxels,
                DealIntoColumns(flock, tuning.Columns),
                tuning.StartingSlots,
                tuning.MaxSlots,
                tuning.TimeLimitSeconds,
                tuning.FireIntervalSeconds,
                seed);
        }

        private static SortedDictionary<GameColor, int> CountColors(IReadOnlyDictionary<VoxelPos, GameColor> voxels)
        {
            var counts = new SortedDictionary<GameColor, int>();
            foreach (var color in voxels.Values)
            {
                counts.TryGetValue(color, out int count);
                counts[color] = count + 1;
            }
            return counts;
        }

        private static List<SheepSpec> SplitIntoSheep(SortedDictionary<GameColor, int> counts, LevelTuning tuning, IRandom random)
        {
            var flock = new List<SheepSpec>();
            foreach (var pair in counts)
            {
                int remaining = pair.Value;
                while (remaining > 0)
                {
                    int span = tuning.MaxAmmoPerSheep - tuning.MinAmmoPerSheep + 1;
                    int ammo = Math.Min(remaining, tuning.MinAmmoPerSheep + random.Next(span));
                    bool leftoverTooSmall = remaining - ammo > 0 && remaining - ammo < tuning.MinAmmoPerSheep;
                    if (leftoverTooSmall) ammo = remaining;
                    flock.Add(new SheepSpec(pair.Key, ammo));
                    remaining -= ammo;
                }
            }
            return flock;
        }

        private static void Shuffle(List<SheepSpec> flock, IRandom random)
        {
            for (int i = flock.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (flock[i], flock[j]) = (flock[j], flock[i]);
            }
        }

        private static IReadOnlyList<IReadOnlyList<SheepSpec>> DealIntoColumns(List<SheepSpec> flock, int columnCount)
        {
            var columns = new List<List<SheepSpec>>();
            for (int c = 0; c < columnCount; c++) columns.Add(new List<SheepSpec>());
            for (int i = 0; i < flock.Count; i++) columns[i % columnCount].Add(flock[i]);
            return columns.ConvertAll(column => (IReadOnlyList<SheepSpec>)column);
        }
    }
}
