using System;
using SheepStrike.Domain.Levels;
using SheepStrike.UseCases.Ports;

namespace SheepStrike.Infrastructure.Levels
{
    /// <summary>
    /// Endless level catalogue: cycles through the animal roster and tightens the tuning every lap.
    /// Higher levels hide more colours inside the animal, scatter them more, and deal unseen sheep less helpfully.
    /// </summary>
    public sealed class ProceduralLevelCatalog : ILevelCatalog
    {
        private const int MaxExtraInteriorColors = 4;
        private const float BaseInteriorNoise = 0.1f;
        private const float InteriorNoisePerLevel = 0.04f;
        private const float MaxInteriorNoise = 0.6f;
        private const float BaseHelpfulness = 0.7f;
        private const float HelpfulnessLossPerLevel = 0.03f;
        private const float MinHelpfulness = 0.2f;

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
                FireIntervalSeconds = 0.09f,
                Interior = InteriorFor(index),
                DealerHelpfulness = Math.Max(MinHelpfulness, BaseHelpfulness - index * HelpfulnessLossPerLevel)
            };

            return LevelGenerator.Generate(levelNumber, animal, AnimalLibrary.Build(animal), tuning, Seed(levelNumber));
        }

        /// <summary>One hidden colour on the first levels, a new one every few levels after that, up to four.</summary>
        private static InteriorStyle InteriorFor(int index) => new InteriorStyle(
            reusedColors: 2,
            extraColors: Math.Min(MaxExtraInteriorColors, 1 + index / 3),
            shellThickness: index < 5 ? 2 : 1,
            noise: Math.Min(MaxInteriorNoise, BaseInteriorNoise + index * InteriorNoisePerLevel));

        private static int Seed(int levelNumber) => unchecked(levelNumber * 7919 + 17);
    }
}
