using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SheepStrike.Domain.Core;
using SheepStrike.Domain.Levels;
using SheepStrike.Domain.Session;
using SheepStrike.Domain.Shooters;
using SheepStrike.Domain.Voxels;

namespace SheepStrike.Tests
{
    public class AdaptiveFlockTests
    {
        /// <summary>One column of six sheep over a red and yellow bar: two are visible, four wait unseen.</summary>
        private static LevelDefinition DeepColumn(float helpfulness)
        {
            var voxels = new VoxelModelBuilder()
                .Box(0, 0, 0, 10, 1, 1, GameColor.Red)
                .Box(0, 1, 0, 10, 1, 1, GameColor.Yellow)
                .Build()
                .All.ToDictionary(p => p.Key, p => p.Value);
            var column = new[]
            {
                new SheepSpec(GameColor.Red, 4), new SheepSpec(GameColor.Yellow, 4),
                new SheepSpec(GameColor.Red, 3), new SheepSpec(GameColor.Yellow, 3),
                new SheepSpec(GameColor.Red, 3), new SheepSpec(GameColor.Yellow, 3)
            };
            return new LevelDefinition(1, "wolf", voxels, new[] { column }, 7, 7, 60f, 0.1f, 1,
                new DealRules(minAmmo: 2, maxAmmo: 4, helpfulness: helpfulness));
        }

        [Test]
        public void VisibleRows_AreTheGeneratedSheep_AndDoNotChange()
        {
            var session = new GameSession(DeepColumn(0.5f), new SeededRandom(3));

            Assert.AreEqual(SheepQueue.VisibleRows, session.Queue.RowCount(0));
            Assert.AreEqual(GameColor.Red, session.Queue.Peek(0, 0).Color);
            Assert.AreEqual(4, session.Queue.Peek(0, 0).Ammo);
            Assert.AreEqual(GameColor.Yellow, session.Queue.Peek(0, 1).Color);
        }

        [Test]
        public void TakingASheep_ShiftsTheColumn_AndDealsAnUnseenOneBehind()
        {
            var session = new GameSession(DeepColumn(0.5f), new SeededRandom(3));
            var second = session.Queue.Peek(0, 1);

            session.TryDeploy(0, 0);

            Assert.AreSame(second, session.Queue.Peek(0, 0));
            Assert.IsNotNull(session.Queue.Peek(0, 1));
        }

        [Test]
        public void AllWoolEverDealt_MatchesTheVoxels_SoTheLevelStaysWinnable()
        {
            var level = DeepColumn(0.5f);
            var session = new GameSession(level, new SeededRandom(9));
            var dealt = new Dictionary<GameColor, int>();

            while (!session.Queue.IsEmpty)
            {
                var sheep = session.Queue.Take(0, 0);
                dealt.TryGetValue(sheep.Color, out int sum);
                dealt[sheep.Color] = sum + sheep.Ammo;
                session.RefillQueue(0);
            }

            Assert.AreEqual(10, dealt[GameColor.Red]);
            Assert.AreEqual(10, dealt[GameColor.Yellow]);
        }

        [Test]
        public void HelpfulDealer_DealsTheColourTheFieldNeedsMost()
        {
            var reserve = new WoolReserve(new Dictionary<GameColor, int> { { GameColor.Red, 10 }, { GameColor.Yellow, 10 } });
            var dealer = new FlockDealer(reserve, new DealRules(minAmmo: 2, maxAmmo: 4, helpfulness: 1f), new SeededRandom(5));
            var need = new Dictionary<GameColor, int> { { GameColor.Red, 1 }, { GameColor.Yellow, 8 } };

            Assert.IsTrue(dealer.TryDeal(color => need[color], out var spec));

            Assert.AreEqual(GameColor.Yellow, spec.Color);
            Assert.AreEqual(10 - spec.Ammo, reserve.AmmoOf(GameColor.Yellow));
        }

        [Test]
        public void EmptyReserve_DealsNothing()
        {
            var dealer = new FlockDealer(new WoolReserve(new Dictionary<GameColor, int>()), new DealRules(2, 4, 0.5f), new SeededRandom(1));

            Assert.IsFalse(dealer.TryDeal(_ => 0, out _));
        }

        [Test]
        public void AssistedSession_DealsHelpfully_AndGetsExtraTime()
        {
            var level = DeepColumn(0f);

            var normal = new GameSession(level, new SeededRandom(1));
            var assisted = new GameSession(level, new SeededRandom(1), assisted: true);

            Assert.IsTrue(assisted.IsAssisted);
            Assert.Greater(assisted.TimeLeft, normal.TimeLeft);
        }
    }
}
