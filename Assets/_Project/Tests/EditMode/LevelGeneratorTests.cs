using NUnit.Framework;
using SheepStrike.Domain.Core;
using SheepStrike.Domain.Levels;
using SheepStrike.Domain.Voxels;
using System.Collections.Generic;

namespace SheepStrike.Tests
{
    public class LevelGeneratorTests
    {
        [Test]
        public void GeneratedLevel_IsBalanced_AndRespectsAmmoRange()
        {
            var model = new VoxelModelBuilder()
                .Box(0, 0, 0, 10, 10, 10, GameColor.Grey)
                .Paint(0, 0, 0, 10, 2, 10, GameColor.Black)
                .Build();
            var cells = new Dictionary<VoxelPos, GameColor>();
            foreach (var pair in model.All) cells.Add(pair.Key, pair.Value);
            var tuning = new LevelTuning { Columns = 5, MinAmmoPerSheep = 10, MaxAmmoPerSheep = 40 };

            var level = LevelGenerator.Generate(1, "wolf", cells, tuning, seed: 42);

            Assert.IsTrue(LevelRules.IsBalanced(level));
            Assert.AreEqual(5, level.SheepColumns.Count);
            foreach (var column in level.SheepColumns)
            foreach (var sheep in column)
            {
                Assert.GreaterOrEqual(sheep.Ammo, 10);
                Assert.LessOrEqual(sheep.Ammo, 40 + 10);
            }
        }

        [Test]
        public void SameSeed_GivesSameLevel()
        {
            var cells = new Dictionary<VoxelPos, GameColor>();
            foreach (var pair in new VoxelModelBuilder().Box(0, 0, 0, 6, 6, 6, GameColor.Brown).Build().All) cells.Add(pair.Key, pair.Value);
            var tuning = new LevelTuning();

            var a = LevelGenerator.Generate(1, "bear", cells, tuning, 7);
            var b = LevelGenerator.Generate(1, "bear", cells, tuning, 7);

            Assert.AreEqual(a.SheepColumns[0][0].Ammo, b.SheepColumns[0][0].Ammo);
            Assert.AreEqual(a.SheepColumns[0][0].Color, b.SheepColumns[0][0].Color);
        }
    }
}
