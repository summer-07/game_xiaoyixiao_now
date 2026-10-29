using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SheepStrike.Domain.Levels;
using SheepStrike.UseCases.Interactors;
using SheepStrike.UseCases.Ports;

namespace SheepStrike.Tests
{
    public class MenuUseCaseTests
    {
        private sealed class AdPlatform : IPlatformService
        {
            public bool Rewarded = true;
            public string LastPlacement;
            public string PlatformName => "Fake";
            public void Initialize() { }
            public void ShowRewardedAd(string placement, Action<bool> onFinished) { LastPlacement = placement; onFinished(Rewarded); }
            public void VibrateShort() { }
            public void Share(string title) { }
            public void ReportLevelResult(int level, bool won, float secondsUsed) { }
            public void ShowLeaderboard(int highestLevel) { }
            public void AddToDesktop(Action<bool> onFinished) => onFinished(true);
        }

        private sealed class RosterCatalog : ILevelCatalog
        {
            private static readonly string[] Roster = { "wolf", "fox", "bear" };
            public LevelDefinition Load(int levelNumber) => TestLevels.TwoVoxels();
            public string TargetOf(int levelNumber) => Roster[(levelNumber - 1) % Roster.Length];
        }

        private sealed class MemoryProgress : IProgressStore
        {
            private readonly Dictionary<int, int> _failures = new Dictionary<int, int>();
            public int HighestUnlockedLevel { get; set; } = 1;
            public void SaveHighestUnlockedLevel(int level) => HighestUnlockedLevel = level;
            public int FailuresAt(int level) => _failures.TryGetValue(level, out int count) ? count : 0;
            public void SaveFailures(int level, int count) => _failures[level] = count;
        }

        private sealed class MemorySettings : ISettingsStore
        {
            public GameSettings Saved;
            public GameSettings Load() => Saved ?? new GameSettings();
            public void Save(GameSettings settings) => Saved = settings;
        }

        [Test]
        public void Settings_StartAllOn_AndToggleIsSaved()
        {
            var store = new MemorySettings();
            var settings = new ChangeSettings(store);

            Assert.IsTrue(settings.Current.IsOn(SettingKind.Music));
            settings.Toggle(SettingKind.Vibration);

            Assert.IsFalse(settings.Current.IsOn(SettingKind.Vibration));
            Assert.IsFalse(store.Saved.IsOn(SettingKind.Vibration));
            Assert.IsTrue(store.Saved.IsOn(SettingKind.Sound));
        }

        [Test]
        public void SkipLevel_AfterTheAd_UnlocksTheNextLevel_AndClearsFailures()
        {
            var progress = new MemoryProgress { HighestUnlockedLevel = 4 };
            progress.SaveFailures(4, 2);
            var platform = new AdPlatform();
            int? next = null;

            new SkipLevel(progress, platform).Execute(4, n => next = n);

            Assert.AreEqual(5, next);
            Assert.AreEqual(5, progress.HighestUnlockedLevel);
            Assert.AreEqual(0, progress.FailuresAt(4));
            Assert.AreEqual(SkipLevel.AdPlacement, platform.LastPlacement);
        }

        [Test]
        public void SkipLevel_WithAnAbortedAd_ChangesNothing()
        {
            var progress = new MemoryProgress { HighestUnlockedLevel = 4 };
            int? next = 0;

            new SkipLevel(progress, new AdPlatform { Rewarded = false }).Execute(4, n => next = n);

            Assert.IsNull(next);
            Assert.AreEqual(4, progress.HighestUnlockedLevel);
        }

        [Test]
        public void Collection_ListsBeatenLevels_ThenTheNextOneLocked_NinePerPage()
        {
            var progress = new MemoryProgress { HighestUnlockedLevel = 12 };
            var collection = new BrowseCollection(progress, new RosterCatalog());

            var first = collection.Page(0);
            var second = collection.Page(1);

            Assert.AreEqual(11, collection.UnlockedCount);
            Assert.AreEqual(2, collection.PageCount);
            Assert.AreEqual(BrowseCollection.PageSize, first.Count);
            Assert.AreEqual(1, first[0].Level);
            Assert.AreEqual("wolf", first[0].TargetId);
            Assert.IsTrue(first.All(e => e.IsUnlocked));
            Assert.AreEqual(3, second.Count);
            Assert.IsFalse(second.Last().IsUnlocked);
            Assert.AreEqual(12, second.Last().Level);
        }
    }
}
