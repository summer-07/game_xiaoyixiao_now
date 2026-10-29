using NUnit.Framework;
using SheepStrike.Domain.Core;
using SheepStrike.Domain.Voxels;

namespace SheepStrike.Tests
{
    public class VoxelModelTests
    {
        [Test]
        public void CoreOfSolidCube_IsNotExposed()
        {
            var model = new VoxelModelBuilder().Box(0, 0, 0, 3, 3, 3, GameColor.Grey).Build();

            Assert.IsFalse(model.IsExposed(new VoxelPos(1, 1, 1)));
            Assert.IsTrue(model.IsExposed(new VoxelPos(0, 1, 1)));
        }

        [Test]
        public void RemovingNeighbour_ExposesCore()
        {
            var model = new VoxelModelBuilder().Box(0, 0, 0, 3, 3, 3, GameColor.Grey).Build();

            model.Remove(new VoxelPos(1, 2, 1));

            Assert.IsTrue(model.IsExposed(new VoxelPos(1, 1, 1)));
        }

        [Test]
        public void TryFindExposed_PrefersTopmostVoxel()
        {
            var model = new VoxelModelBuilder().Box(0, 0, 0, 1, 3, 1, GameColor.Grey).Build();

            Assert.IsTrue(model.TryFindExposed(GameColor.Grey, out var target));
            Assert.AreEqual(new VoxelPos(0, 2, 0), target);
        }

        [Test]
        public void TryFindExposed_ReturnsFalse_WhenColourOnlyInside()
        {
            var model = new VoxelModelBuilder()
                .Box(0, 0, 0, 3, 3, 3, GameColor.Grey)
                .Paint(1, 1, 1, 1, 1, 1, GameColor.Red)
                .Build();

            Assert.IsFalse(model.TryFindExposed(GameColor.Red, out _));
            Assert.AreEqual(1, model.CountOf(GameColor.Red));
        }
    }
}
