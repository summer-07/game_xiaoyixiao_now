using System;
using NUnit.Framework;
using SheepStrike.Domain.Levels;
using SheepStrike.Domain.Session;
using SheepStrike.UseCases.Interactors;
using SheepStrike.UseCases.Ports;

namespace SheepStrike.Tests
{
    public class UseBoosterTests
    {
        private sealed class FakePlatform : IPlatformService
        {
            public bool RewardAds;
            public int AdsShown;
            public string PlatformName => "Fake";
            public void Initialize() { }
            public void ShowRewardedAd(string placement, Action<bool> onFinished) { AdsShown++; onFinished(RewardAds); }
            public void VibrateShort() { }
            public void Share(string title) { }
            public void ReportLevelResult(int level, bool won, float secondsUsed) { }
        }

        private sealed class FixedCatalog : ILevelCatalog
        {
            public LevelDefinition Load(int levelNumber) => TestLevels.HiddenCore(slots: 2);
        }

        private sealed class MemoryProgress : IProgressStore
        {
            public int HighestUnlockedLevel { get; private set; } = 1;
            public void SaveHighestUnlockedLevel(int level) => HighestUnlockedLevel = level;
        }

        private PlayContext _context;
        private FakePlatform _platform;

        [SetUp]
        public void SetUp()
        {
            _context = new PlayContext();
            _platform = new FakePlatform();
            new StartLevel(new FixedCatalog(), new MemoryProgress(), _context).Execute(1);
        }

        [Test]
        public void FreeCharge_IsSpentBeforeAskingForAnAd()
        {
            BoosterOutcome? outcome = null;

            new UseBooster(_context, _platform).Execute(BoosterType.RamCharge, o => outcome = o);

            Assert.AreEqual(BoosterOutcome.Applied, outcome);
            Assert.AreEqual(0, _platform.AdsShown);
        }

        [Test]
        public void WithoutCharges_AbortedAd_GivesNoBooster()
        {
            BoosterOutcome? outcome = null;
            _platform.RewardAds = false;

            new UseBooster(_context, _platform).Execute(BoosterType.Shuffle, o => outcome = o);

            Assert.AreEqual(BoosterOutcome.AdNotCompleted, outcome);
            Assert.AreEqual(1, _platform.AdsShown);
        }

        [Test]
        public void WinningLevel_UnlocksNext()
        {
            var progress = new MemoryProgress();
            new StartLevel(new FixedCatalog(), progress, _context).Execute(1);
            _context.Session.TryDeploy(0, 0);
            _context.Session.TryDeploy(1, 0);
            for (int i = 0; i < 400 && _context.Session.State == SessionState.Playing; i++) _context.Session.Tick(0.05f);

            int next = new FinishLevel(_context, progress, _platform).Execute();

            Assert.AreEqual(2, next);
            Assert.AreEqual(2, progress.HighestUnlockedLevel);
        }
    }
}
