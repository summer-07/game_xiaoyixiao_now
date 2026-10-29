using System;
using SheepStrike.Domain.Core;

namespace SheepStrike.Domain.Shooters
{
    /// <summary>A sheep carrying wool balls of a single colour. Each wool ball destroys one voxel.</summary>
    public sealed class Sheep
    {
        public Sheep(int id, GameColor color, int ammo)
        {
            if (ammo <= 0) throw new ArgumentOutOfRangeException(nameof(ammo), "A sheep needs at least one wool ball.");
            Id = id;
            Color = color;
            Ammo = ammo;
        }

        public int Id { get; }

        public GameColor Color { get; }

        public int Ammo { get; private set; }

        public bool IsEmpty => Ammo == 0;

        public void ConsumeOne()
        {
            if (IsEmpty) throw new InvalidOperationException($"Sheep {Id} has no wool left.");
            Ammo--;
        }
    }
}
