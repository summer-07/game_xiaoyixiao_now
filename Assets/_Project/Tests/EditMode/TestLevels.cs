using System.Collections.Generic;
using SheepStrike.Domain.Core;
using SheepStrike.Domain.Levels;
using SheepStrike.Domain.Voxels;

namespace SheepStrike.Tests
{
    internal static class TestLevels
    {
        /// <summary>A 2x1x1 bar: one white voxel, one black voxel, one sheep of each colour.</summary>
        public static LevelDefinition TwoVoxels(int slots = 2, float timeLimit = 60f)
        {
            var voxels = new VoxelModelBuilder()
                .Box(0, 0, 0, 1, 1, 1, GameColor.White)
                .Box(1, 0, 0, 1, 1, 1, GameColor.Black)
                .Build();
            return Wrap(voxels, new[]
            {
                new[] { new SheepSpec(GameColor.White, 1) },
                new[] { new SheepSpec(GameColor.Black, 1) }
            }, slots, timeLimit);
        }

        /// <summary>A white voxel in front of a black one (front = lowest Z), one sheep of each colour.</summary>
        public static LevelDefinition TwoVoxelsDeep()
        {
            var voxels = new VoxelModelBuilder()
                .Box(0, 0, 0, 1, 1, 1, GameColor.White)
                .Box(0, 0, 1, 1, 1, 1, GameColor.Black)
                .Build();
            return Wrap(voxels, new[]
            {
                new[] { new SheepSpec(GameColor.White, 1) },
                new[] { new SheepSpec(GameColor.Black, 1) }
            }, 2, 60f);
        }

        /// <summary>A 3x3x3 white cube with a black core: the core is hidden until the shell is gone.</summary>
        public static LevelDefinition HiddenCore(int slots)
        {
            var voxels = new VoxelModelBuilder()
                .Box(0, 0, 0, 3, 3, 3, GameColor.White)
                .Paint(1, 1, 1, 1, 1, 1, GameColor.Black)
                .Build();
            return Wrap(voxels, new[]
            {
                new[] { new SheepSpec(GameColor.Black, 1) },
                new[] { new SheepSpec(GameColor.White, 26) }
            }, slots, 60f);
        }

        private static LevelDefinition Wrap(VoxelModel model, SheepSpec[][] columns, int slots, float timeLimit)
        {
            var cells = new Dictionary<VoxelPos, GameColor>();
            foreach (var pair in model.All) cells.Add(pair.Key, pair.Value);
            return new LevelDefinition(1, "wolf", cells, columns, slots, 7, timeLimit, 0.1f, 1);
        }
    }
}
