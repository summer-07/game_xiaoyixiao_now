using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SheepStrike.Domain.Core;
using SheepStrike.Domain.Session;
using SheepStrike.Domain.Voxels;

namespace SheepStrike.Tests
{
    public class VisibilityTests
    {
        private static readonly ViewDirection FromFront = new ViewDirection(0f, 0f, -1f);
        private static readonly ViewDirection FromBack = new ViewDirection(0f, 0f, 1f);

        [Test]
        public void VoxelHiddenBehindAnother_CannotBeTargeted()
        {
            var model = new VoxelModelBuilder().Box(0, 0, 0, 1, 1, 2, GameColor.Red).Build();
            model.SetView(FromFront);
            model.RefreshVisibility();

            Assert.IsTrue(model.TryFindVisible(GameColor.Red, new SeededRandom(1), out var target));
            Assert.AreEqual(new VoxelPos(0, 0, 0), target);
        }

        [Test]
        public void ColourOnlyOnTheFarSide_IsNotVisible_UntilTheModelTurns()
        {
            var model = new VoxelModelBuilder()
                .Box(0, 0, 0, 1, 1, 1, GameColor.White)
                .Box(0, 0, 1, 1, 1, 1, GameColor.Black)
                .Build();
            model.SetView(FromFront);
            model.RefreshVisibility();

            Assert.IsTrue(model.IsExposed(new VoxelPos(0, 0, 1)));
            Assert.IsFalse(model.TryFindVisible(GameColor.Black, new SeededRandom(1), out _));

            model.SetView(FromBack);
            model.RefreshVisibility();
            Assert.IsTrue(model.TryFindVisible(GameColor.Black, new SeededRandom(1), out _));
        }

        [Test]
        public void RemovingTheFrontVoxel_RevealsTheOneBehind()
        {
            var model = new VoxelModelBuilder().Box(0, 0, 0, 1, 1, 2, GameColor.Red).Build();
            model.SetView(FromFront);
            model.RefreshVisibility();

            model.Remove(new VoxelPos(0, 0, 0));
            model.RefreshVisibility();

            Assert.IsTrue(model.TryFindVisible(GameColor.Red, new SeededRandom(1), out var target));
            Assert.AreEqual(new VoxelPos(0, 0, 1), target);
        }

        [Test]
        public void WithoutAView_EveryExposedVoxelCounts()
        {
            var model = new VoxelModelBuilder().Box(0, 0, 0, 1, 1, 2, GameColor.Red).Build();

            var picks = new HashSet<VoxelPos>();
            for (int seed = 0; seed < 20; seed++)
            {
                Assert.IsTrue(model.TryFindVisible(GameColor.Red, new SeededRandom(seed), out var target));
                picks.Add(target);
            }

            Assert.AreEqual(2, picks.Count);
        }

        [Test]
        public void Target_IsPickedAtRandom_AmongVisibleVoxels_NotInAFixedOrder()
        {
            var model = new VoxelModelBuilder().Box(0, 0, 0, 5, 5, 1, GameColor.Red).Build();
            model.SetView(FromFront);
            model.RefreshVisibility();

            var picks = new HashSet<VoxelPos>();
            for (int seed = 0; seed < 20; seed++)
            {
                Assert.IsTrue(model.TryFindVisible(GameColor.Red, new SeededRandom(seed), out var target));
                picks.Add(target);
            }

            Assert.Greater(picks.Count, 5);
        }

        [Test]
        public void OuterVoxels_AreHitAboutSevenTimesInTen_InnerOnesTheRest()
        {
            var picks = 0;
            var inner = 0;
            for (int seed = 0; seed < 400; seed++)
            {
                var model = new VoxelModelBuilder().Box(0, 0, 0, 3, 3, 3, GameColor.Red).Build();
                model.Remove(new VoxelPos(1, 1, 0)); // opens the front face onto the core
                model.SetView(FromFront);
                model.RefreshVisibility();

                Assert.IsTrue(model.TryFindVisible(GameColor.Red, new SeededRandom(seed), out var target));
                picks++;
                if (target.Equals(new VoxelPos(1, 1, 1))) inner++;
            }

            float innerShare = (float)inner / picks;
            Assert.That(innerShare, Is.InRange(0.2f, 0.4f));
        }

        [Test]
        public void OnlyInnerVoxelsLeft_TheyAreHitEveryTime()
        {
            var model = new VoxelModelBuilder().Box(0, 0, 0, 3, 3, 3, GameColor.Grey).Paint(1, 1, 1, 1, 1, 1, GameColor.Red).Build();
            model.Remove(new VoxelPos(1, 1, 0));

            Assert.IsTrue(model.TryFindVisible(GameColor.Red, new SeededRandom(3), out var target));
            Assert.AreEqual(new VoxelPos(1, 1, 1), target);
        }

        [Test]
        public void Sheep_HoldsFire_WhileItsColourFacesAway_AndIsNotStuck()
        {
            var session = new GameSession(TestLevels.TwoVoxelsDeep(), new SeededRandom(1));
            session.SetView(FromFront);
            session.TryDeploy(1, 0); // black sheep; the black voxel is behind the white one
            session.DrainEvents();

            session.Tick(GameSession.DeployTravelSeconds + 0.1f);
            Assert.IsFalse(session.DrainEvents().OfType<WoolFired>().Any());
            Assert.AreEqual(SessionState.Playing, session.State);

            session.SetView(FromBack);
            session.Tick(0.016f);
            Assert.AreEqual(1, session.DrainEvents().OfType<WoolFired>().Count());
        }
    }
}
