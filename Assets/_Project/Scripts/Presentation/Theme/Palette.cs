using SheepStrike.Domain.Core;
using UnityEngine;

namespace SheepStrike.Presentation.Theme
{
    public static class Palette
    {
        public static readonly Color Background = new Color32(0x6E, 0x63, 0x9C, 0xFF);
        public static readonly Color Panel = new Color32(0x3E, 0x36, 0x63, 0xE6);
        public static readonly Color SlotEmpty = new Color32(0x4A, 0x40, 0x72, 0xFF);
        public static readonly Color SlotRing = new Color32(0xE0, 0x5A, 0x4F, 0xFF);
        public static readonly Color TextLight = Color.white;
        public static readonly Color TextOutline = new Color32(0x2A, 0x22, 0x44, 0xFF);
        public static readonly Color Combo = new Color32(0xFF, 0xC8, 0x3D, 0xFF);
        public static readonly Color BoosterBar = new Color32(0x3B, 0x7B, 0xD9, 0xFF);
        public static readonly Color Badge = new Color32(0xF0, 0x4E, 0x3A, 0xFF);
        public static readonly Color Disabled = new Color(1f, 1f, 1f, 0.35f);

        public static Color Of(GameColor color)
        {
            switch (color)
            {
                case GameColor.White: return new Color32(0xF4, 0xF4, 0xF0, 0xFF);
                case GameColor.Grey: return new Color32(0x9A, 0xA0, 0xAE, 0xFF);
                case GameColor.Black: return new Color32(0x2B, 0x2B, 0x33, 0xFF);
                case GameColor.Orange: return new Color32(0xF3, 0x8A, 0x2E, 0xFF);
                case GameColor.Brown: return new Color32(0x8A, 0x55, 0x33, 0xFF);
                case GameColor.Tan: return new Color32(0xE2, 0xB4, 0x86, 0xFF);
                case GameColor.Pink: return new Color32(0xF4, 0x9C, 0xB7, 0xFF);
                case GameColor.Yellow: return new Color32(0xFF, 0xD9, 0x3B, 0xFF);
                case GameColor.Red: return new Color32(0xE2, 0x3A, 0x3A, 0xFF);
                case GameColor.Cream: return new Color32(0xF7, 0xE7, 0xC6, 0xFF);
                default: return Color.magenta;
            }
        }
    }
}
