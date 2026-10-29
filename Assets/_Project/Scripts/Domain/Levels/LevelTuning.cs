namespace SheepStrike.Domain.Levels
{
    /// <summary>Difficulty knobs used when generating a level from a voxel animal.</summary>
    public sealed class LevelTuning
    {
        public int Columns = 5;
        public int MinAmmoPerSheep = 10;
        public int MaxAmmoPerSheep = 40;
        public int StartingSlots = 5;
        public int MaxSlots = 7;
        public float TimeLimitSeconds = 240f;
        public float FireIntervalSeconds = 0.09f;

        /// <summary>How the hidden inside of the animal is coloured; harder levels use more colours and more noise.</summary>
        public InteriorStyle Interior = new InteriorStyle(reusedColors: 2, extraColors: 1, shellThickness: 2, noise: 0.1f);

        /// <summary>0..1 chance that an unseen sheep is dealt in the colour the field needs most; lower is harder.</summary>
        public float DealerHelpfulness = 0.6f;
    }
}
