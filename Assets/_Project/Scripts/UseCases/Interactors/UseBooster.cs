using System;
using SheepStrike.Domain.Session;
using SheepStrike.UseCases.Ports;

namespace SheepStrike.UseCases.Interactors
{
    public enum BoosterOutcome
    {
        Applied,
        NotApplicable,
        AdNotCompleted
    }

    /// <summary>Spends a free charge if one is left, otherwise asks the platform for a rewarded ad first.</summary>
    public sealed class UseBooster
    {
        public const string AdPlacementPrefix = "booster_";

        private readonly PlayContext _context;
        private readonly IPlatformService _platform;

        public UseBooster(PlayContext context, IPlatformService platform)
        {
            _context = context;
            _platform = platform;
        }

        public void Execute(BoosterType booster, Action<BoosterOutcome> onDone)
        {
            if (!_context.HasSession || !_context.Session.CanApply(booster))
            {
                onDone(BoosterOutcome.NotApplicable);
                return;
            }

            if (_context.Wallet.TrySpend(booster))
            {
                onDone(Apply(booster));
                return;
            }

            _platform.ShowRewardedAd(AdPlacementPrefix + booster, rewarded =>
                onDone(rewarded ? Apply(booster) : BoosterOutcome.AdNotCompleted));
        }

        private BoosterOutcome Apply(BoosterType booster) =>
            _context.Session.Apply(booster) ? BoosterOutcome.Applied : BoosterOutcome.NotApplicable;
    }
}
