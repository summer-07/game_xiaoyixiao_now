using System.Collections.Generic;
using SheepStrike.Domain.Core;

namespace SheepStrike.Domain.Levels
{
    /// <summary>Immutable description of a level. A fresh <see cref="Session.GameSession"/> is built from it on every attempt.</summary>
    public sealed class LevelDefinition
    {
        public LevelDefinition(
            int number,
            string targetId,
            IReadOnlyDictionary<VoxelPos, GameColor> voxels,
            IReadOnlyList<IReadOnlyList<SheepSpec>> sheepColumns,
            int startingSlots,
            int maxSlots,
            float timeLimitSeconds,
            float fireIntervalSeconds,
            int seed)
        {
            Number = number;
            TargetId = targetId;
            Voxels = voxels;
            SheepColumns = sheepColumns;
            StartingSlots = startingSlots;
            MaxSlots = maxSlots;
            TimeLimitSeconds = timeLimitSeconds;
            FireIntervalSeconds = fireIntervalSeconds;
            Seed = seed;
        }

        public int Number { get; }

        /// <summary>Stable id of the animal (e.g. "wolf"); presentation maps it to names and icons.</summary>
        public string TargetId { get; }

        public IReadOnlyDictionary<VoxelPos, GameColor> Voxels { get; }

        public IReadOnlyList<IReadOnlyList<SheepSpec>> SheepColumns { get; }

        public int StartingSlots { get; }

        public int MaxSlots { get; }

        public float TimeLimitSeconds { get; }

        public float FireIntervalSeconds { get; }

        public int Seed { get; }
    }
}
