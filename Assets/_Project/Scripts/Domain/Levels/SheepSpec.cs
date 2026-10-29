using SheepStrike.Domain.Core;

namespace SheepStrike.Domain.Levels
{
    public readonly struct SheepSpec
    {
        public readonly GameColor Color;
        public readonly int Ammo;

        public SheepSpec(GameColor color, int ammo)
        {
            Color = color;
            Ammo = ammo;
        }
    }
}
