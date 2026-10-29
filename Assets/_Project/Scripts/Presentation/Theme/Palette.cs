using SheepStrike.Domain.Core;
using UnityEngine;

namespace SheepStrike.Presentation.Theme
{
    /// <summary>
    /// Toy-like, saturated colours: flat purple sky, dark rounded pills, chunky bevelled buttons.
    /// Game colours are pushed apart so every sheep reads at a glance; "Cream" is drawn as green for that reason.
    /// </summary>
    public static class Palette
    {
        public static readonly Color Background = new Color32(0x6B, 0x63, 0x98, 0xFF);
        public static readonly Color Cloud = new Color(1f, 1f, 1f, 0.07f);
        public static readonly Color Panel = new Color32(0x4B, 0x42, 0x7A, 0xFF);
        public static readonly Color PanelDark = new Color32(0x2E, 0x28, 0x4D, 0xE6);
        public static readonly Color PillLight = new Color32(0xD8, 0xEC, 0xFF, 0xFF);
        public static readonly Color PillLightText = new Color32(0x24, 0x3A, 0x6E, 0xFF);
        public static readonly Color SlotEmpty = new Color32(0x45, 0x3D, 0x6E, 0xFF);
        public static readonly Color SlotRing = new Color32(0xE8, 0x4A, 0x43, 0xFF);
        public static readonly Color SlotLocked = new Color32(0x3A, 0x34, 0x5E, 0xFF);
        public static readonly Color TextLight = Color.white;
        public static readonly Color TextOutline = new Color32(0x22, 0x1C, 0x3A, 0xFF);
        public static readonly Color Combo = new Color32(0xFF, 0xD2, 0x3D, 0xFF);
        public static readonly Color BoosterBar = new Color32(0x3E, 0x7B, 0xE0, 0xFF);
        public static readonly Color BoosterButton = new Color32(0x24, 0x4C, 0xA8, 0xFF);
        public static readonly Color BoosterRing = new Color32(0x8F, 0xC2, 0xFF, 0xFF);
        public static readonly Color Badge = new Color32(0xF0, 0x4A, 0x3A, 0xFF);
        public static readonly Color ButtonGreen = new Color32(0x4C, 0xC9, 0x5A, 0xFF);
        public static readonly Color ButtonOrange = new Color32(0xFF, 0xA9, 0x2E, 0xFF);
        public static readonly Color ButtonBlue = new Color32(0x3E, 0x8B, 0xF0, 0xFF);
        public static readonly Color ButtonGrey = new Color32(0x8A, 0x86, 0xA6, 0xFF);
        public static readonly Color Dim = new Color(0f, 0f, 0f, 0.6f);
        public static readonly Color Disabled = new Color(1f, 1f, 1f, 0.35f);
        public static readonly Color QueueBackRow = new Color(0.78f, 0.78f, 0.82f, 1f);
        public static readonly Color Wood = new Color32(0xC9, 0x7E, 0x45, 0xFF);
        public static readonly Color WoodLight = new Color32(0xF6, 0xDF, 0xBE, 0xFF);
        public static readonly Color Paper = new Color32(0xFF, 0xF4, 0xE2, 0xFF);
        public static readonly Color PaperLine = new Color32(0xEE, 0xD6, 0xB4, 0xFF);
        public static readonly Color WoodText = new Color32(0xC0, 0x68, 0x3A, 0xFF);
        public static readonly Color Clip = new Color32(0xD8, 0xDD, 0xE6, 0xFF);
        public static readonly Color CheckGreen = new Color32(0x3C, 0xC0, 0x3C, 0xFF);
        public static readonly Color CloseRed = new Color32(0xF0, 0x3C, 0x32, 0xFF);
        public static readonly Color MenuIcon = new Color32(0xFF, 0xFF, 0xFF, 0xF2);

        public static Color Of(GameColor color)
        {
            switch (color)
            {
                case GameColor.White: return new Color32(0xF7, 0xF7, 0xF4, 0xFF);
                case GameColor.Grey: return new Color32(0x8E, 0xA6, 0xE8, 0xFF);
                case GameColor.Black: return new Color32(0x2B, 0x28, 0x33, 0xFF);
                case GameColor.Orange: return new Color32(0xFF, 0x8A, 0x1F, 0xFF);
                case GameColor.Brown: return new Color32(0x8A, 0x52, 0x2E, 0xFF);
                case GameColor.Tan: return new Color32(0xFF, 0xB0, 0xA0, 0xFF);
                case GameColor.Pink: return new Color32(0xF0, 0x3C, 0xD2, 0xFF);
                case GameColor.Yellow: return new Color32(0xFF, 0xD8, 0x1F, 0xFF);
                case GameColor.Red: return new Color32(0xE8, 0x2A, 0x3A, 0xFF);
                case GameColor.Cream: return new Color32(0x6C, 0xD6, 0x6B, 0xFF);
                default: return Color.magenta;
            }
        }
    }
}
