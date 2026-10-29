using System;
using SheepStrike.UseCases.Ports;

namespace SheepStrike.UseCases.Interactors
{
    /// <summary>Watching a rewarded ad lets the player skip a level they are stuck on.</summary>
    public sealed class SkipLevel
    {
        public const string AdPlacement = "skip_level";

        private readonly IProgressStore _progress;
        private readonly IPlatformService _platform;

        public SkipLevel(IProgressStore progress, IPlatformService platform)
        {
            _progress = progress;
            _platform = platform;
        }

        /// <param name="onDone">The level to play next, or null when the ad was not watched to the end.</param>
        public void Execute(int level, Action<int?> onDone)
        {
            _platform.ShowRewardedAd(AdPlacement, rewarded =>
            {
                if (!rewarded)
                {
                    onDone(null);
                    return;
                }

                int next = level + 1;
                _progress.SaveFailures(level, 0);
                if (next > _progress.HighestUnlockedLevel) _progress.SaveHighestUnlockedLevel(next);
                onDone(next);
            });
        }
    }
}
