namespace SheepStrike.UseCases.Ports
{
    public interface IProgressStore
    {
        int HighestUnlockedLevel { get; }

        void SaveHighestUnlockedLevel(int level);
    }
}
