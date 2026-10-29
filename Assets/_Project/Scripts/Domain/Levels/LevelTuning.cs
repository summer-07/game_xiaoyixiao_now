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
    }
}
