namespace SheepStrike.UseCases.Ports
{
    public interface IProgressStore
    {
        int HighestUnlockedLevel { get; }

        void SaveHighestUnlockedLevel(int level);

        /// <summary>Losses on a level since it was last won.</summary>
        int FailuresAt(int level);

        void SaveFailures(int level, int count);
    }
}
