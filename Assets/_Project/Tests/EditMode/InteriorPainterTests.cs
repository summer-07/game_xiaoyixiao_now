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

            var painted = InteriorPainter.Paint(voxels, new InteriorStyle(reusedColors: 2, extraColors: 0, shellThickness: 1, noise: 0f), new SeededRandom(1));

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

            var painted = InteriorPainter.Paint(voxels, new InteriorStyle(reusedColors: 2, extraColors: 0, shellThickness: 1, noise: 0f), new SeededRandom(1));

            var interior = painted.Where(p => !model.IsExposed(p.Key)).Select(p => p.Value).Distinct().ToList();
            Assert.Greater(interior.Count, 1);
            CollectionAssert.IsSubsetOf(painted.Values.Distinct(), voxels.Values.Distinct());
            Assert.AreEqual(voxels.Count, painted.Count);
        }

        [Test]
        public void ExtraColours_AddNewColoursInside_ButNeverOnTheSurface()
        {
            var voxels = StripedCube();
            var model = new VoxelModel(voxels);

            var painted = InteriorPainter.Paint(voxels, new InteriorStyle(reusedColors: 1, extraColors: 2, shellThickness: 1, noise: 0.5f), new SeededRandom(4));

            var surfaceColours = voxels.Values.Distinct().ToList();
            var newColours = painted.Values.Distinct().Except(surfaceColours).ToList();
            Assert.AreEqual(2, newColours.Count);
            foreach (var pair in painted.Where(p => newColours.Contains(p.Value)))
            {
                Assert.IsFalse(model.IsExposed(pair.Key));
            }
        }

        [Test]
        public void SameSeed_PaintsTheSameInterior()
        {
            var style = new InteriorStyle(reusedColors: 2, extraColors: 3, shellThickness: 1, noise: 0.6f);

            var a = InteriorPainter.Paint(StripedCube(), style, new SeededRandom(11));
            var b = InteriorPainter.Paint(StripedCube(), style, new SeededRandom(11));

            CollectionAssert.AreEquivalent(a, b);
        }

        [Test]
        public void PaletteSizeZero_LeavesTheModelUntouched()
        {
            var voxels = StripedCube();

            var painted = InteriorPainter.Paint(voxels, InteriorStyle.Untouched, new SeededRandom(1));

            CollectionAssert.AreEquivalent(voxels, painted);
        }
    }
}
