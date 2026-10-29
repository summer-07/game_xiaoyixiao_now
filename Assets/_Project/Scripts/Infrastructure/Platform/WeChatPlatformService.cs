#if WEIXINMINIGAME
using System;
using SheepStrike.UseCases.Ports;
using UnityEngine;
using WeChatWASM;

namespace SheepStrike.Infrastructure.Platform
{
    /// <summary>
    /// WeChat mini-game adapter. Compiled only when the WeChat Unity SDK (minigame-unity-webgl-transform)
    /// is installed, which defines WEIXINMINIGAME. Verify signatures against the SDK version you import.
    /// </summary>
    public sealed class WeChatPlatformService : IPlatformService
    {
        private WXRewardedVideoAd _rewardedAd;
        private Action<bool> _pendingReward;

        public string PlatformName => "WeChat";

        public void Initialize()
        {
            WX.InitSDK(_ =>
            {
                _rewardedAd = WX.CreateRewardedVideoAd(new WXCreateRewardedVideoAdParam
                {
                    adUnitId = PlatformIds.WeChatRewardedAdUnit
                });
                _rewardedAd.OnClose(response => CompleteReward(response != null && response.isEnded));
                _rewardedAd.OnError(_ => CompleteReward(false));
            });
        }

        public void ShowRewardedAd(string placement, Action<bool> onFinished)
        {
            if (_rewardedAd == null)
            {
                onFinished(false);
                return;
            }
            _pendingReward = onFinished;
            _rewardedAd.Show(null, _ => _rewardedAd.Load(__ => _rewardedAd.Show(), ___ => CompleteReward(false)));
        }

        public void VibrateShort() => WX.VibrateShort(new VibrateShortOption { type = "light" });

        public void Share(string title) => WX.ShareAppMessage(new ShareAppMessageOption { title = title });

        public void ReportLevelResult(int level, bool won, float secondsUsed) =>
            Debug.Log($"[WeChat] level={level} won={won} seconds={secondsUsed:0.0}");

        // TODO: open the host's ranking with the SDK version in use (friends + global, keyed by highest level).
        public void ShowLeaderboard(int highestLevel) =>
            Debug.LogWarning($"[WeChat] Leaderboard not wired yet (highest level {highestLevel}).");

        // TODO: call the host's add-to-desktop / shortcut API for the SDK version in use.
        public void AddToDesktop(Action<bool> onFinished)
        {
            Debug.LogWarning("[WeChat] Add to desktop not wired yet.");
            onFinished(false);
        }

        private void CompleteReward(bool rewarded)
        {
            var callback = _pendingReward;
            _pendingReward = null;
            callback?.Invoke(rewarded);
        }
    }
}
#endif
