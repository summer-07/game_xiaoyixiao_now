using System;
using SheepStrike.UseCases.Ports;
using UnityEngine;

namespace SheepStrike.Infrastructure.Platform
{
    /// <summary>Used in the Editor and in plain WebGL builds: rewarded ads always succeed, everything else just logs.</summary>
    public sealed class EditorPlatformService : IPlatformService
    {
        public string PlatformName => "Editor";

        public void Initialize() => Debug.Log("[Platform] Editor platform ready.");

        public void ShowRewardedAd(string placement, Action<bool> onFinished)
        {
            Debug.Log($"[Platform] Simulated rewarded ad '{placement}' -> rewarded.");
            onFinished(true);
        }

        public void VibrateShort()
        {
        }

        public void Share(string title) => Debug.Log($"[Platform] Share: {title}");

        public void ReportLevelResult(int level, bool won, float secondsUsed) =>
            Debug.Log($"[Platform] Level {level} {(won ? "won" : "lost")} in {secondsUsed:0.0}s");

        public bool TryShowLeaderboard(int highestLevel) => false;

        public void AddToDesktop(Action<bool> onFinished)
        {
            Debug.Log("[Platform] Add to desktop is only available inside the mini-game hosts.");
            onFinished(false);
        }
    }
}
