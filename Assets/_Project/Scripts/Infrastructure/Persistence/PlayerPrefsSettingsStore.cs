using System;
using SheepStrike.UseCases.Interactors;
using SheepStrike.UseCases.Ports;
using UnityEngine;

namespace SheepStrike.Infrastructure.Persistence
{
    /// <summary>One PlayerPrefs int per setting (1 = on), so new settings default to on for existing players.</summary>
    public sealed class PlayerPrefsSettingsStore : ISettingsStore
    {
        private const string KeyPrefix = "sheepstrike.setting.";

        public GameSettings Load()
        {
            var settings = new GameSettings();
            foreach (SettingKind kind in Enum.GetValues(typeof(SettingKind)))
            {
                settings.Set(kind, PlayerPrefs.GetInt(KeyPrefix + kind, 1) == 1);
            }
            return settings;
        }

        public void Save(GameSettings settings)
        {
            foreach (SettingKind kind in Enum.GetValues(typeof(SettingKind)))
            {
                PlayerPrefs.SetInt(KeyPrefix + kind, settings.IsOn(kind) ? 1 : 0);
            }
            PlayerPrefs.Save();
        }
    }
}
