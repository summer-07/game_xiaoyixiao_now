using System.Collections.Generic;

namespace SheepStrike.UseCases.Interactors
{
    public enum SettingKind
    {
        Music,
        Sound,
        Vibration,
        ComboDisplay
    }

    /// <summary>The player's on/off preferences. Everything is on until the player turns it off.</summary>
    public sealed class GameSettings
    {
        private readonly HashSet<SettingKind> _off = new HashSet<SettingKind>();

        public bool IsOn(SettingKind kind) => !_off.Contains(kind);

        public void Set(SettingKind kind, bool on)
        {
            if (on) _off.Remove(kind);
            else _off.Add(kind);
        }
    }
}
