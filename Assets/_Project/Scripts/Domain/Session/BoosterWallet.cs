using System.Collections.Generic;

namespace SheepStrike.Domain.Session
{
    /// <summary>Free booster charges for the current level. Once a charge is spent the booster costs a rewarded ad.</summary>
    public sealed class BoosterWallet
    {
        private readonly Dictionary<BoosterType, int> _charges = new Dictionary<BoosterType, int>();

        public BoosterWallet(IReadOnlyDictionary<BoosterType, int> startingCharges)
        {
            foreach (var pair in startingCharges) _charges[pair.Key] = pair.Value;
        }

        public int ChargesOf(BoosterType booster) => _charges.TryGetValue(booster, out int count) ? count : 0;

        public bool TrySpend(BoosterType booster)
        {
            int count = ChargesOf(booster);
            if (count <= 0) return false;
            _charges[booster] = count - 1;
            return true;
        }
    }
}
