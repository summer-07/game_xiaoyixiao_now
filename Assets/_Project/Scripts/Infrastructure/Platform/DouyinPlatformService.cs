#if DOUYINMINIGAME
using System;
using SheepStrike.UseCases.Ports;
using UnityEngine;

namespace SheepStrike.Infrastructure.Platform
{
    /// <summary>
    /// Douyin (抖音) mini-game adapter. Compiled only when DOUYINMINIGAME is defined (add it in
    /// Player Settings > Scripting Define Symbols after importing the Douyin Unity SDK).
    /// TODO: wire the TTSDK rewarded-video, vibrate and share calls for the SDK version in use.
    /// Until then rewarded ads report "not completed" so no booster is granted for free.
    /// </summary>
    public sealed class DouyinPlatformService : IPlatformService
    {
        public string PlatformName => "Douyin";

        public void Initialize() => Debug.Log("[Douyin] platform adapter initialised (SDK calls pending).");

        public void ShowRewardedAd(string placement, Action<bool> onFinished)
        {
            Debug.LogWarning($"[Douyin] Rewarded ad '{placement}' not wired yet ({PlatformIds.DouyinRewardedAdUnit}).");
            onFinished(false);
        }

        public void VibrateShort()
        {
        }

        public void Share(string title) => Debug.Log($"[Douyin] Share: {title}");

        public void ReportLevelResult(int level, bool won, float secondsUsed) =>
            Debug.Log($"[Douyin] level={level} won={won} seconds={secondsUsed:0.0}");
    }
}
#endif
