using System;
using System.Collections.Generic;
using SheepStrike.Domain.Core;

namespace SheepStrike.Domain.Shooters
{
    /// <summary>Wool not yet given to any sheep, per colour. Unseen sheep are dealt from it on demand.</summary>
    public sealed class WoolReserve
    {
        private readonly SortedDictionary<GameColor, int> _ammo = new SortedDictionary<GameColor, int>();

        public WoolReserve(IReadOnlyDictionary<GameColor, int> ammo)
        {
            foreach (var pair in ammo)
            {
                if (pair.Value > 0) _ammo[pair.Key] = pair.Value;
            }
        }

        public bool IsEmpty => _ammo.Count == 0;

        public IEnumerable<GameColor> Colors => _ammo.Keys;

        public int AmmoOf(GameColor color) => _ammo.TryGetValue(color, out int ammo) ? ammo : 0;

        public void Take(GameColor color, int amount)
        {
            int left = AmmoOf(color) - amount;
            if (amount <= 0 || left < 0) throw new InvalidOperationException($"Cannot take {amount} {color} wool from the reserve.");
            if (left == 0) _ammo.Remove(color);
            else _ammo[color] = left;
        }
    }
}
