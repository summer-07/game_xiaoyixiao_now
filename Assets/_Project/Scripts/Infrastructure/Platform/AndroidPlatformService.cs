// Plain APK only: Douyin's native scheme forbids any Java/JNI call and scans for it, so mini-game builds exclude this file.
#if UNITY_ANDROID && !DOUYINMINIGAME && !WEIXINMINIGAME
using System;
using SheepStrike.UseCases.Ports;
using UnityEngine;

namespace SheepStrike.Infrastructure.Platform
{
    /// <summary>
    /// Plain Android build (the APK used to try the game on a phone, outside the mini-game hosts).
    /// Rewarded ads are simulated as watched, like the Editor; vibration goes to the system vibrator.
    /// </summary>
    public sealed class AndroidPlatformService : IPlatformService
    {
        private const long ShortVibrationMillis = 30;

        private AndroidJavaObject _vibrator;

        public string PlatformName => "Android";

        public void Initialize()
        {
            try
            {
                using (var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                {
                    var activity = player.GetStatic<AndroidJavaObject>("currentActivity");
                    _vibrator = activity.Call<AndroidJavaObject>("getSystemService", "vibrator");
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("[Android] No vibrator: " + e.Message);
            }
        }

        public void ShowRewardedAd(string placement, Action<bool> onFinished)
        {
            Debug.Log($"[Android] Simulated rewarded ad '{placement}' -> rewarded.");
            onFinished(true);
        }

        /// <remarks>Handheld.Vibrate is only the fallback (it buzzes ~0.5s), but referencing it makes Unity add the VIBRATE permission.</remarks>
        public void VibrateShort()
        {
            if (_vibrator == null)
            {
                Handheld.Vibrate();
                return;
            }
            _vibrator.Call("vibrate", ShortVibrationMillis);
        }

        public void Share(string title) => Debug.Log($"[Android] Share: {title}");

        public void ReportLevelResult(int level, bool won, float secondsUsed) =>
            Debug.Log($"[Android] level={level} won={won} seconds={secondsUsed:0.0}");

        public bool TryShowLeaderboard(int highestLevel) => false;

        public void AddToDesktop(Action<bool> onFinished) => onFinished(false);
    }
}
#endif
