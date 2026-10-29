using System;
using System.Collections.Generic;
using NUnit.Framework;
using SheepStrike.Domain.Levels;
using SheepStrike.Domain.Session;
using SheepStrike.UseCases.Interactors;
using SheepStrike.UseCases.Ports;

namespace SheepStrike.Tests
{
    public class ProgressAssistTests
    {
        private sealed class SilentPlatform : IPlatformService
        {
            public string PlatformName => "Fake";
            public void Initialize() { }
            public void ShowRewardedAd(string placement, Action<bool> onFinished) => onFinished(false);
            public void VibrateShort() { }
            public void Share(string title) { }
            public void ReportLevelResult(int level, bool won, float secondsUsed) { }
            public bool TryShowLeaderboard(int highestLevel) => false;
            public void AddToDesktop(Action<bool> onFinished) => onFinished(false);
        }

        private sealed class FixedCatalog : ILevelCatalog
        {
            public LevelDefinition Load(int levelNumber) => TestLevels.TwoVoxels();
            public string TargetOf(int levelNumber) => "wolf";
        }

        private sealed class MemoryProgress : IProgressStore
        {
            private readonly Dictionary<int, int> _failures = new Dictionary<int, int>();
            public int HighestUnlockedLevel { get; private set; } = 1;
            public void SaveHighestUnlockedLevel(int level) => HighestUnlockedLevel = level;
            public int FailuresAt(int level) => _failures.TryGetValue(level, out int count) ? count : 0;
            public void SaveFailures(int level, int count) => _failures[level] = count;
        }

        private PlayContext _context;
        private MemoryProgress _progress;
        private StartLevel _start;
        private FinishLevel _finish;

        [SetUp]
        public void SetUp()
        {
            _context = new PlayContext();
            _progress = new MemoryProgress();
            _start = new StartLevel(new FixedCatalog(), _progress, _context);
            _finish = new FinishLevel(_context, _progress, new SilentPlatform());
        }

        private void LoseOnce()
        {
            _start.Execute(1).GiveUp();
            _finish.Execute();
        }

        [Test]
        public void AssistThreshold_IsBetweenThreeAndFiveFailures()
        {
            for (int level = 1; level <= 30; level++)
            {
                int threshold = LevelRules.FailuresBeforeAssist(level);
                Assert.That(threshold, Is.InRange(3, 5));
            }
        }

        [Test]
        public void Losing_CountsFailures_UntilTheNextAttemptIsAssisted()
        {
            int threshold = LevelRules.FailuresBeforeAssist(1);
            for (int i = 0; i < threshold - 1; i++) LoseOnce();
            Assert.IsFalse(_start.Execute(1).IsAssisted);

            _finish.Execute(); // the attempt above is abandoned as a loss too
            Assert.AreEqual(threshold, _progress.FailuresAt(1));
            Assert.IsTrue(_start.Execute(1).IsAssisted);
        }

        [Test]
        public void Winning_ResetsTheFailureCount()
        {
            LoseOnce();
            var session = _start.Execute(1);
            session.TryDeploy(0, 0);
            session.TryDeploy(1, 0);
            session.Tick(GameSession.DeployTravelSeconds + 0.016f);
            Assert.AreEqual(SessionState.Won, session.State);

            _finish.Execute();

            Assert.AreEqual(0, _progress.FailuresAt(1));
        }
    }
}
