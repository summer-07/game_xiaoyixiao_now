using NUnit.Framework;
using SheepStrike.Domain.Core;
using SheepStrike.Domain.Shooters;

namespace SheepStrike.Tests
{
    public class LaunchPadTests
    {
        private const int Columns = 5;
        private static Sheep NewSheep(int id) => new Sheep(id, GameColor.Red, 10);

        [Test]
        public void Sheep_EntersThePenStraightAboveItsColumn()
        {
            var pad = new LaunchPad(5, 7);

            Assert.IsTrue(pad.TryPlaceNearest(NewSheep(1), 4, Columns, out int slot));

            Assert.AreEqual(4, slot);
        }

        [Test]
        public void WhenThatPenIsTaken_TheNextClosestOneIsUsed()
        {
            var pad = new LaunchPad(5, 7);
            pad.TryPlaceNearest(NewSheep(1), 0, Columns, out _);

            pad.TryPlaceNearest(NewSheep(2), 0, Columns, out int slot);

            Assert.AreEqual(1, slot);
        }

        [Test]
        public void ExtraPens_SitAboveTheRowEnds_AndOnlyCountOnceUnlocked()
        {
            var pad = new LaunchPad(5, 7);
            Assert.AreEqual(new PenPosition(0, 1), pad.PositionOf(5));
            Assert.AreEqual(new PenPosition(4, 1), pad.PositionOf(6));

            pad.TryPlaceNearest(NewSheep(1), 4, Columns, out _);
            pad.TryPlaceNearest(NewSheep(2), 4, Columns, out _);
            pad.TryPlaceNearest(NewSheep(3), 4, Columns, out int lockedChoice);
            Assert.AreEqual(2, lockedChoice);

            pad.TryExpand();
            pad.TryExpand();
            pad.Release(2);
            pad.TryPlaceNearest(NewSheep(4), 4, Columns, out int unlockedChoice);
            Assert.AreEqual(6, unlockedChoice);
        }

        [Test]
        public void FullPad_RefusesTheSheep()
        {
            var pad = new LaunchPad(1, 1);
            pad.TryPlaceNearest(NewSheep(1), 0, Columns, out _);

            Assert.IsFalse(pad.TryPlaceNearest(NewSheep(2), 0, Columns, out _));
        }
    }
}
