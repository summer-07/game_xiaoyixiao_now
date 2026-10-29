using System;
using SheepStrike.Domain.Levels;
using SheepStrike.UseCases.Ports;

namespace SheepStrike.Infrastructure.Levels
{
    /// <summary>Endless level catalogue: cycles through the animal roster and tightens the tuning every lap.</summary>
    public sealed class ProceduralLevelCatalog : ILevelCatalog
    {
        public LevelDefinition Load(int levelNumber)
        {
            if (levelNumber < 1) throw new ArgumentOutOfRangeException(nameof(levelNumber));

            int index = levelNumber - 1;
            string animal = AnimalLibrary.Rotation[index % AnimalLibrary.Rotation.Count];
            int lap = index / AnimalLibrary.Rotation.Count;

            var tuning = new LevelTuning
            {
                Columns = levelNumber <= 2 ? 4 : 5,
                MinAmmoPerSheep = 10 + Math.Min(lap * 2, 10),
                MaxAmmoPerSheep = 40 + Math.Min(lap * 5, 40),
                StartingSlots = lap == 0 ? 5 : 4,
                MaxSlots = 7,
                TimeLimitSeconds = Math.Max(150f, 300f - index * 8f),
                FireIntervalSeconds = 0.09f
            };

            return LevelGenerator.Generate(levelNumber, animal, AnimalLibrary.Build(animal), tuning, Seed(levelNumber));
        }

        private static int Seed(int levelNumber) => unchecked(levelNumber * 7919 + 17);
    }
}
