using SheepStrike.Domain.Levels;

namespace SheepStrike.UseCases.Ports
{
    public interface ILevelCatalog
    {
        /// <summary>Levels are endless; numbers beyond the hand-tuned set are generated with rising difficulty.</summary>
        LevelDefinition Load(int levelNumber);
    }
}
