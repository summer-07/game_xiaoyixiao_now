using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SheepStrike.Domain.Core;
using SheepStrike.Domain.Levels;
using SheepStrike.Domain.Voxels;

namespace SheepStrike.Tests
{
    public class InteriorPainterTests
    {
        /// <summary>A 5x5x5 grey cube with a white top and a black front: the core is three layers deep.</summary>
        private static Dictionary<VoxelPos, GameColor> StripedCube()
        {
            var model = new VoxelModelBuilder()
                .Box(0, 0, 0, 5, 5, 5, GameColor.Grey)
                .Paint(0, 4, 0, 5, 1, 5, GameColor.White)
                .Paint(0, 0, 0, 5, 4, 1, GameColor.Black)
                .Build();
            return model.All.ToDictionary(p => p.Key, p => p.Value);
        }

        [Test]
        public void Surface_KeepsItsColours()
        {
            var voxels = StripedCube();
            var model = new VoxelModel(voxels);

            var painted = InteriorPainter.Paint(voxels, paletteSize: 2, shellThickness: 1);

            foreach (var pair in voxels.Where(p => model.IsExposed(p.Key)))
            {
                Assert.AreEqual(pair.Value, painted[pair.Key]);
            }
        }

        [Test]
        public void Interior_UsesOnlyColoursAlreadyOnTheSurface_AndIsNoLongerOneColour()
        {
            var voxels = StripedCube();
            var model = new VoxelModel(voxels);

            var painted = InteriorPainter.Paint(voxels, paletteSize: 2, shellThickness: 1);

            var interior = painted.Where(p => !model.IsExposed(p.Key)).Select(p => p.Value).Distinct().ToList();
            Assert.Greater(interior.Count, 1);
            CollectionAssert.IsSubsetOf(painted.Values.Distinct(), voxels.Values.Distinct());
            Assert.AreEqual(voxels.Count, painted.Count);
        }

        [Test]
        public void PaletteSizeZero_LeavesTheModelUntouched()
        {
            var voxels = StripedCube();

            var painted = InteriorPainter.Paint(voxels, paletteSize: 0, shellThickness: 1);

            CollectionAssert.AreEquivalent(voxels, painted);
        }
    }
}
