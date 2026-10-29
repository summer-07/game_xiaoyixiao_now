using SheepStrike.UseCases.Interactors;

namespace SheepStrike.UseCases.Ports
{
    public interface ISettingsStore
    {
        GameSettings Load();

        void Save(GameSettings settings);
    }
}
