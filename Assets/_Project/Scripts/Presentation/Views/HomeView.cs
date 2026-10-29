using System;
using SheepStrike.Presentation.Theme;
using UnityEngine;
using UnityEngine.UI;

namespace SheepStrike.Presentation.Views
{
    /// <summary>
    /// The start screen. A transparent overlay: the next level's animal is already spinning behind it.
    /// Gear (settings) top-left, a big "开始游戏 / 第 N 关" button at the bottom, and a column of icon
    /// buttons on the right for the collection, the leaderboard and add-to-desktop.
    /// </summary>
    public sealed class HomeView
    {
        private static readonly Vector2 StartSize = new Vector2(640f, 220f);
        private const float StartY = 260f;
        private const float StartBreathe = 0.035f;
        private const float SideX = -110f;
        private const float SideBottom = 560f;
        private const float SideStep = 210f;
        private const float IconSize = 130f;

        private readonly RectTransform _root;
        private readonly Text _level;
        private readonly RectTransform _start;

        public Action StartTapped;
        public Action SettingsTapped;
        public Action CollectionTapped;
        public Action LeaderboardTapped;
        public Action AddToDesktopTapped;

        public HomeView(RectTransform parent, ArtLibrary art)
        {
            _root = UiFactory.Stretch("Home", parent);
            BuildGear(art);
            _start = BuildStartButton(art, out _level);
            BuildCollectionButton(art, SideBottom + SideStep * 2f);
            BuildLeaderboardButton(art, SideBottom + SideStep);
            BuildDesktopButton(art, SideBottom);
            _root.gameObject.SetActive(false);
        }

        public bool IsVisible => _root.gameObject.activeSelf;

        public void Show(int levelNumber)
        {
            _level.text = Texts.Level(levelNumber);
            _root.gameObject.SetActive(true);
            ScaleAnimator.On(_start).PopIn();
        }

        public void Hide() => _root.gameObject.SetActive(false);

        private void BuildGear(ArtLibrary art)
        {
            var rect = UiFactory.Rect("Settings", _root, new Vector2(0f, 1f), HudView.GearPosition, Vector2.one * HudView.GearSize);
            UiFactory.AddImage(rect, art.Disc, Palette.PanelDark, true);
            UiFactory.AddImage(UiFactory.Rect("Gear", rect, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.one * (HudView.GearSize * 0.78f)), art.Gear, Palette.MenuIcon);
            UiFactory.AddButton(rect, () => SettingsTapped?.Invoke());
        }

        private RectTransform BuildStartButton(ArtLibrary art, out Text level)
        {
            var button = UiFactory.TextButton(_root, "Start", Vector2.zero, StartSize, art, Texts.StartGame, Palette.ButtonBlue, () => StartTapped?.Invoke());
            var rect = (RectTransform)button.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0f);
            rect.anchoredPosition = new Vector2(0f, StartY);

            var title = rect.Find("Label");
            ((RectTransform)title).offsetMin = new Vector2(0f, StartSize.y * 0.38f);
            title.GetComponent<Text>().fontSize = 84;
            level = UiFactory.AddLabel(UiFactory.Rect("Level", rect, new Vector2(0.5f, 0f), new Vector2(0f, StartSize.y * 0.3f), new Vector2(StartSize.x, 70f)), art.Font, 56, TextAnchor.MiddleCenter);
            ScaleAnimator.On(rect).Breathe(StartBreathe, 0.9f, 0f);
            return rect;
        }

        /// <summary>An orange book with a gold star-disc on its cover.</summary>
        private void BuildCollectionButton(ArtLibrary art, float y)
        {
            var icon = SideButton(art, y, Texts.Collection, () => CollectionTapped?.Invoke());
            var book = UiFactory.Rect("Book", icon, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(IconSize * 0.8f, IconSize * 0.9f));
            book.localRotation = Quaternion.Euler(0f, 0f, 12f);
            UiFactory.AddBox(book, art.BevelBox, Palette.ButtonOrange);
            var pages = UiFactory.Rect("Pages", book, new Vector2(1f, 0.5f), new Vector2(-10f, 0f), new Vector2(14f, IconSize * 0.75f));
            UiFactory.AddBox(pages, art.FlatBox, Palette.Paper).pixelsPerUnitMultiplier = 4f;
            UiFactory.AddImage(UiFactory.Rect("Star", book, new Vector2(0.5f, 0.5f), new Vector2(-6f, 6f), Vector2.one * (IconSize * 0.36f)), art.Disc, Palette.Combo);
        }

        /// <summary>A three-step podium: 2, 1, 3.</summary>
        private void BuildLeaderboardButton(ArtLibrary art, float y)
        {
            var icon = SideButton(art, y, Texts.Leaderboard, () => LeaderboardTapped?.Invoke());
            AddPodiumStep(art, icon, -38f, 0.55f, "2", new Color32(0x4C, 0xA6, 0xF2, 0xFF));
            AddPodiumStep(art, icon, 0f, 0.8f, "1", Palette.ButtonGreen);
            AddPodiumStep(art, icon, 38f, 0.42f, "3", Palette.ButtonOrange);
        }

        private static void AddPodiumStep(ArtLibrary art, RectTransform icon, float x, float heightFraction, string place, Color color)
        {
            float height = IconSize * heightFraction;
            var step = UiFactory.Rect("Step" + place, icon, new Vector2(0.5f, 0f), new Vector2(x, IconSize * 0.1f + height * 0.5f), new Vector2(38f, height));
            UiFactory.AddBox(step, art.BevelBox, color).pixelsPerUnitMultiplier = 2f;
            var label = UiFactory.AddLabel(UiFactory.Rect("Place", step, new Vector2(0.5f, 1f), new Vector2(0f, -20f), new Vector2(38f, 36f)), art.Font, 30, TextAnchor.MiddleCenter);
            label.text = place;
        }

        /// <summary>Four little app tiles with a green plus, the usual "add to home screen" picture.</summary>
        private void BuildDesktopButton(ArtLibrary art, float y)
        {
            var icon = SideButton(art, y, Texts.AddToDesktop, () => AddToDesktopTapped?.Invoke());
            float tile = IconSize * 0.34f;
            var colors = new Color[] { Palette.ButtonOrange, Palette.ButtonOrange, Palette.ButtonOrange, Palette.ButtonGreen };
            for (int i = 0; i < 4; i++)
            {
                var position = new Vector2((i % 2 - 0.5f) * tile * 1.15f, (0.5f - i / 2) * tile * 1.15f);
                UiFactory.AddBox(UiFactory.Rect("Tile" + i, icon, new Vector2(0.5f, 0.5f), position, Vector2.one * tile), art.BevelBox, colors[i]).pixelsPerUnitMultiplier = 2.5f;
            }
            var plus = UiFactory.AddLabel(UiFactory.Rect("Plus", icon, new Vector2(0.5f, 0.5f), new Vector2(tile * 0.57f, -tile * 0.5f), Vector2.one * tile), art.Font, 52, TextAnchor.MiddleCenter);
            plus.text = "+";
        }

        /// <returns>The icon area to draw the picture into; the caption sits underneath.</returns>
        private RectTransform SideButton(ArtLibrary art, float y, string caption, Action onTap)
        {
            var rect = UiFactory.Rect(caption, _root, new Vector2(1f, 0f), new Vector2(SideX, y), Vector2.one * IconSize);
            var label = UiFactory.AddLabel(UiFactory.Rect("Caption", rect, new Vector2(0.5f, 0f), new Vector2(0f, -28f), new Vector2(220f, 50f)), art.Font, 40, TextAnchor.MiddleCenter);
            label.text = caption;
            ScaleAnimator.On(UiFactory.AddButton(rect, onTap)).Breathe(0.03f, 0.7f, y * 0.01f);
            return rect;
        }
    }
}
