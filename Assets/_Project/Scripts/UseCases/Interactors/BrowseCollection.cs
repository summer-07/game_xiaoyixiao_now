using System;
using System.Collections.Generic;
using SheepStrike.UseCases.Ports;

namespace SheepStrike.UseCases.Interactors
{
    /// <summary>One card in the collection (图鉴): a level and the animal beaten there.</summary>
    public readonly struct CollectionEntry
    {
        public CollectionEntry(int level, string targetId, bool isUnlocked)
        {
            Level = level;
            TargetId = targetId;
            IsUnlocked = isUnlocked;
        }

        public int Level { get; }
        public string TargetId { get; }

        /// <summary>Beaten and replayable; the one locked card is the level the player is on now.</summary>
        public bool IsUnlocked { get; }
    }

    /// <summary>Pages through every beaten level, followed by the current level as a locked "?" card.</summary>
    public sealed class BrowseCollection
    {
        public const int PageSize = 9;

        private readonly IProgressStore _progress;
        private readonly ILevelCatalog _catalog;

        public BrowseCollection(IProgressStore progress, ILevelCatalog catalog)
        {
            _progress = progress;
            _catalog = catalog;
        }

        public int UnlockedCount => _progress.HighestUnlockedLevel - 1;

        /// <summary>The level the player is on: the first one not beaten yet.</summary>
        public int CurrentLevel => _progress.HighestUnlockedLevel;

        public int PageCount => (CardCount + PageSize - 1) / PageSize;

        private int CardCount => _progress.HighestUnlockedLevel;

        public IReadOnlyList<CollectionEntry> Page(int page)
        {
            var entries = new List<CollectionEntry>(PageSize);
            int first = Math.Max(0, page) * PageSize + 1;
            for (int level = first; level < first + PageSize && level <= CardCount; level++)
            {
                entries.Add(new CollectionEntry(level, _catalog.TargetOf(level), level <= UnlockedCount));
            }
            return entries;
        }
    }
}
