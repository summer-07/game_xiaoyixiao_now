using System.Linq;
using NUnit.Framework;
using SheepStrike.Domain.Core;
using SheepStrike.Domain.Session;

namespace SheepStrike.Tests
{
    public class GameSessionTests
    {
        private const float AfterTravel = GameSession.DeployTravelSeconds + 0.016f;

        [Test]
        public void DeployedSheep_HoldsFire_UntilItReachesThePen()
        {
            var session = new GameSession(TestLevels.TwoVoxels(), new SeededRandom(1));
            session.TryDeploy(0, 0);
            session.DrainEvents();

            session.Tick(GameSession.DeployTravelSeconds * 0.5f);
            Assert.IsFalse(session.DrainEvents().OfType<WoolFired>().Any());

            session.Tick(GameSession.DeployTravelSeconds * 0.5f + 0.016f);
            Assert.AreEqual(1, session.DrainEvents().OfType<WoolFired>().Count());
        }

        [Test]
        public void DeployingBothSheep_ClearsModel_AndWins()
        {
            var session = new GameSession(TestLevels.TwoVoxels(), new SeededRandom(1));

            Assert.IsTrue(session.TryDeploy(0, 0));
            Assert.IsTrue(session.TryDeploy(1, 0));
            session.Tick(AfterTravel);

            Assert.AreEqual(SessionState.Won, session.State);
            Assert.AreEqual(2, session.DrainEvents().OfType<WoolFired>().Count());
        }

        [Test]
        public void SheepWithNoAmmo_LeavesItsSlot()
        {
            var session = new GameSession(TestLevels.TwoVoxels(), new SeededRandom(1));

            session.TryDeploy(0, 0);
            session.Tick(AfterTravel);

            Assert.IsNull(session.Pad.At(0));
            Assert.IsTrue(session.DrainEvents().OfType<SheepRetired>().Any());
        }

        [Test]
        public void BackRowSheep_CannotBeDeployed_WithoutShepherdDog()
        {
            var session = new GameSession(TestLevels.TwoVoxels(), new SeededRandom(1));

            Assert.IsFalse(session.TryDeploy(0, 1));
            Assert.IsTrue(session.DrainEvents().OfType<DeployRejected>().Any());
        }

        [Test]
        public void FullPad_WithNoReachableTarget_BecomesStuck_AndExtraPenRescues()
        {
            var session = new GameSession(TestLevels.HiddenCore(slots: 1), new SeededRandom(1));

            session.TryDeploy(0, 0); // black sheep, but the black voxel is buried
            session.Tick(0.016f);
            Assert.AreEqual(SessionState.Stuck, session.State);

            Assert.IsTrue(session.Apply(BoosterType.ExtraPen));
            Assert.AreEqual(SessionState.Playing, session.State);
            Assert.AreEqual(2, session.Pad.Capacity);
        }

        [Test]
        public void HiddenCore_IsReachable_OnceShellIsCleared()
        {
            var session = new GameSession(TestLevels.HiddenCore(slots: 2), new SeededRandom(1));

            session.TryDeploy(0, 0);
            session.TryDeploy(1, 0);
            for (int i = 0; i < 400 && session.State == SessionState.Playing; i++) session.Tick(0.05f);

            Assert.AreEqual(SessionState.Won, session.State);
        }

        [Test]
        public void RunningOutOfTime_Loses()
        {
            var session = new GameSession(TestLevels.TwoVoxels(timeLimit: 1f), new SeededRandom(1));

            session.Tick(1.5f);

            Assert.AreEqual(SessionState.Lost, session.State);
        }

        [Test]
        public void RamCharge_HalvesFireInterval()
        {
            var session = new GameSession(TestLevels.HiddenCore(slots: 2), new SeededRandom(1));
            session.TryDeploy(1, 0); // 26 white wool
            session.Apply(BoosterType.RamCharge);
            session.DrainEvents();

            session.Tick(GameSession.DeployTravelSeconds);
            session.Tick(0.18f);

            // first shot as the sheep lands, then one every 0.05s instead of 0.1s
            Assert.AreEqual(4, session.DrainEvents().OfType<WoolFired>().Count());
        }
    }
}
