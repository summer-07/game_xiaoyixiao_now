using System.Collections.Generic;
using SheepStrike.Domain.Session;
using UnityEngine;

namespace SheepStrike.Presentation.Theme
{
    /// <summary>
    /// Loads sprites and fonts from Resources. Every lookup may return null; views fall back to flat shapes,
    /// so the game stays playable before the art is imported.
    /// </summary>
    public sealed class ArtLibrary
    {
        private const string Root = "Art/";
        private readonly Dictionary<string, Sprite> _cache = new Dictionary<string, Sprite>();

        public ArtLibrary()
        {
            // Put a TTF with Chinese glyphs at Resources/Fonts/GameFont.ttf; WebGL has no system font fallback.
            Font = Resources.Load<Font>("Fonts/GameFont") ?? Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            BevelBox = ShapeSprites.BevelBox();
            FlatBox = ShapeSprites.FlatBox();
            Disc = ShapeSprites.Disc();
            Ring = ShapeSprites.Ring();
            Gear = ShapeSprites.Gear();
            SheepIcon = ShapeSprites.SheepToken();
        }

        public Font Font { get; }

        /// <summary>9-sliced rounded box with a darker bottom lip, for chunky buttons.</summary>
        public Sprite BevelBox { get; }

        /// <summary>9-sliced rounded box, for pills and panels.</summary>
        public Sprite FlatBox { get; }

        public Sprite Disc { get; }

        public Sprite Ring { get; }

        public Sprite Gear { get; }

        /// <summary>The flat sheep token drawn in code; the painted sheep_shooter.png was too busy to read once tinted.</summary>
        public Sprite SheepIcon { get; }
        public Sprite SlotRing => Load("UI/slot_ring");
        public Sprite RoundButton => Load("UI/button_round");
        public Sprite Background => Load("Backgrounds/bg_meadow_dusk");
        public Sprite Splash => Load("Backgrounds/splash");

        public Sprite Target(string targetId) => Load("Icons/target_" + (targetId == "alpha_wolf" ? "wolf" : targetId));

        public Sprite Booster(BoosterType booster)
        {
            switch (booster)
            {
                case BoosterType.RamCharge: return Load("Icons/booster_ram_charge");
                case BoosterType.ExtraPen: return Load("Icons/booster_extra_pen");
                case BoosterType.ShepherdDog: return Load("Icons/booster_shepherd_dog");
                case BoosterType.Shuffle: return Load("Icons/booster_shuffle");
                default: return null;
            }
        }

        private Sprite Load(string path)
        {
            if (_cache.TryGetValue(path, out var sprite)) return sprite;
            sprite = Resources.Load<Sprite>(Root + path);
            _cache[path] = sprite;
            return sprite;
        }
    }
}
