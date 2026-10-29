using System;

namespace SheepStrike.UseCases.Ports
{
    /// <summary>
    /// Everything the game needs from the host mini-game platform (WeChat, Douyin, Editor...).
    /// Implementations live in Infrastructure; use cases never touch a platform SDK directly.
    /// </summary>
    public interface IPlatformService
    {
        string PlatformName { get; }

        void Initialize();

        /// <param name="placement">Logical ad placement, mapped to a platform ad-unit id by the implementation.</param>
        /// <param name="onFinished">true when the player watched the ad to the end and earned the reward.</param>
        void ShowRewardedAd(string placement, Action<bool> onFinished);

        void VibrateShort();

        void Share(string title);

        void ReportLevelResult(int level, bool won, float secondsUsed);
    }
}
