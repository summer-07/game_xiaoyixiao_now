using SheepStrike.UseCases.Ports;

namespace SheepStrike.UseCases.Interactors
{
    /// <summary>Reads the saved preferences once and persists every change immediately.</summary>
    public sealed class ChangeSettings
    {
        private readonly ISettingsStore _store;

        public ChangeSettings(ISettingsStore store)
        {
            _store = store;
            Current = store.Load();
        }

        public GameSettings Current { get; }

        public void Toggle(SettingKind kind)
        {
            Current.Set(kind, !Current.IsOn(kind));
            _store.Save(Current);
        }
    }
}
