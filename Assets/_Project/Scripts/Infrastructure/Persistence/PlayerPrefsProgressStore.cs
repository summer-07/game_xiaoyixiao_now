using SheepStrike.UseCases.Ports;
using UnityEngine;

namespace SheepStrike.Infrastructure.Persistence
{
    /// <summary>
    /// PlayerPrefs is mapped to the host storage by both the WeChat and Douyin Unity converters,
    /// so a single implementation works on every target.
    /// </summary>
    public sealed class PlayerPrefsProgressStore : IProgressStore
    {
        private const string HighestLevelKey = "sheepstrike.highest_level";

        public int HighestUnlockedLevel => Mathf.Max(1, PlayerPrefs.GetInt(HighestLevelKey, 1));

        public void SaveHighestUnlockedLevel(int level)
        {
            PlayerPrefs.SetInt(HighestLevelKey, level);
            PlayerPrefs.Save();
        }
    }
}
