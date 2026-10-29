using System;
using System.Collections.Generic;
using SheepStrike.Domain.Core;
using SheepStrike.Domain.Levels;

namespace SheepStrike.Domain.Shooters
{
    /// <summary>
    /// Deals the next unseen sheep. With probability <see cref="DealRules.Helpfulness"/> it picks the colour the field
    /// needs most (targets left minus wool the player already has lined up); otherwise a colour at random, weighted by
    /// how much wool is left. Every sheep is cut from the reserve, so the level always stays winnable.
    /// </summary>
    public sealed class FlockDealer
    {
        private const int PercentScale = 1000;

        private readonly WoolReserve _reserve;
        private readonly DealRules _rules;
        private readonly IRandom _random;
        private readonly List<GameColor> _colors = new List<GameColor>();

        public FlockDealer(WoolReserve reserve, DealRules rules, IRandom random)
        {
            _reserve = reserve;
            _rules = rules;
            _random = random;
        }

        /// <param name="need">Targets of a colour on the field minus wool already committed to it; may be negative.</param>
        public bool TryDeal(Func<GameColor, int> need, out SheepSpec spec)
        {
            spec = default;
            if (_reserve.IsEmpty) return false;

            _colors.Clear();
            _colors.AddRange(_reserve.Colors);
            bool helpful = _random.Next(PercentScale) < _rules.Helpfulness * PercentScale;
            var color = helpful ? MostNeeded(need) : WeightedByReserve();

            int ammo = AmmoFor(color);
            _reserve.Take(color, ammo);
            spec = new SheepSpec(color, ammo);
            return true;
        }

        private GameColor MostNeeded(Func<GameColor, int> need)
        {
            var best = _colors[0];
            int bestNeed = int.MinValue;
            foreach (var color in _colors)
            {
                int value = need(color);
                if (value > bestNeed)
                {
                    best = color;
                    bestNeed = value;
                }
            }
            return best;
        }

        private GameColor WeightedByReserve()
        {
            int total = 0;
            foreach (var color in _colors) total += _reserve.AmmoOf(color);
            int pick = _random.Next(total);
            foreach (var color in _colors)
            {
                pick -= _reserve.AmmoOf(color);
                if (pick < 0) return color;
            }
            return _colors[_colors.Count - 1];
        }

        /// <summary>A random load in the rule's range; the last sheep of a colour takes any leftover too small for another.</summary>
        private int AmmoFor(GameColor color)
        {
            int left = _reserve.AmmoOf(color);
            int ammo = Math.Min(left, _rules.MinAmmo + _random.Next(_rules.MaxAmmo - _rules.MinAmmo + 1));
            if (left - ammo > 0 && left - ammo < _rules.MinAmmo) ammo = left;
            return ammo;
        }
    }
}
