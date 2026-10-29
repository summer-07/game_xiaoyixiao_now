using System;
using System.Collections.Generic;
using SheepStrike.Presentation.Theme;
using SheepStrike.UseCases.Interactors;
using UnityEngine;
using UnityEngine.UI;

namespace SheepStrike.Presentation.Views
{
    /// <summary>
    /// Settings / pause dialog: four on/off rows on the notebook, and under it the in-game actions
    /// (home, restart, skip level for an ad, continue). On the home screen only the toggles and continue show.
    /// </summary>
    public sealed class SettingsPanelView
    {
        private static readonly Vector2 BoardSize = new Vector2(900f, 720f);
        private const float RowTop = -70f;
        private const float RowStep = 118f;
        private const float BoxSize = 96f;
        private const float BoardY = 260f;
        private const float SquareButton = 180f;

        private static readonly (SettingKind Kind, string Label, Color Icon)[] Rows =
        {
            (SettingKind.Music, Texts.Music, new Color32(0xF2, 0x8C, 0x4C, 0xFF)),
            (SettingKind.Sound, Texts.Sound, new Color32(0x4C, 0xA6, 0xF2, 0xFF)),
            (SettingKind.Vibration, Texts.Vibration, new Color32(0x9B, 0x6C, 0xE8, 0xFF)),
            (SettingKind.ComboDisplay, Texts.ComboDisplay, new Color32(0xF2, 0xC2, 0x2E, 0xFF))
        };

        private readonly NotebookPanel _panel;
        private readonly Dictionary<SettingKind, GameObject> _checks = new Dictionary<SettingKind, GameObject>();
        private readonly GameObject _gameActions;

        public Action<SettingKind> ToggleTapped;
        public Action HomeTapped;
        public Action RestartTapped;
        public Action SkipTapped;
        public Action ContinueTapped;

        public SettingsPanelView(RectTransform parent, ArtLibrary art, string version)
        {
            _panel = new NotebookPanel(parent, art, "Settings", BoardSize, Texts.Settings, null);
            _panel.Board.anchoredPosition = new Vector2(0f, BoardY);
            for (int i = 0; i < Rows.Length; i++) BuildRow(art, i);

            _gameActions = BuildGameActions(art);
            UiFactory.TextButton(_panel.Root, "Continue", new Vector2(0f, -440f), new Vector2(380f, 130f), art, Texts.Continue, Palette.ButtonGreen, () => ContinueTapped?.Invoke());

            var versionLabel = UiFactory.AddLabel(UiFactory.Rect("Version", _panel.Root, new Vector2(1f, 0f), new Vector2(-110f, 70f), new Vector2(240f, 60f)), art.Font, 40, TextAnchor.MiddleCenter);
            versionLabel.text = Texts.Version(version);
        }

        public bool IsOpen => _panel.IsOpen;

        /// <param name="inGame">Show home / restart / skip, which only make sense during a level.</param>
        public void Show(bool inGame)
        {
            _gameActions.SetActive(inGame);
            _panel.Show();
        }

        public void Hide() => _panel.Hide();

        public void SetToggle(SettingKind kind, bool on)
        {
            _checks[kind].SetActive(on);
            if (on) ScaleAnimator.On(_checks[kind].transform).PopIn();
        }

        private void BuildRow(ArtLibrary art, int index)
        {
            var (kind, label, iconColor) = Rows[index];
            var row = UiFactory.Rect(kind.ToString(), _panel.Content, new Vector2(0.5f, 1f), new Vector2(0f, RowTop - index * RowStep), new Vector2(760f, RowStep));

            var icon = UiFactory.Rect("Icon", row, new Vector2(0.5f, 0.5f), new Vector2(-300f, 0f), Vector2.one * 70f);
            UiFactory.AddImage(icon, art.Disc, iconColor);

            var text = UiFactory.AddLabel(UiFactory.Rect("Label", row, new Vector2(0.5f, 0.5f), new Vector2(-30f, 0f), new Vector2(360f, 90f)), art.Font, 62, TextAnchor.MiddleCenter, Palette.WoodText);
            text.text = label;
            foreach (var outline in text.GetComponents<Outline>()) outline.effectColor = Color.white;

            var box = UiFactory.Rect("Box", row, new Vector2(0.5f, 0.5f), new Vector2(290f, 0f), Vector2.one * BoxSize);
            UiFactory.AddBox(box, art.BevelBox, Palette.WoodLight, true);
            _checks[kind] = BuildCheckMark(art, box);
            UiFactory.AddButton(box, () => ToggleTapped?.Invoke(kind));
        }

        /// <summary>A green tick made of two rounded bars, since the font has no check glyph.</summary>
        private static GameObject BuildCheckMark(ArtLibrary art, RectTransform box)
        {
            var check = UiFactory.Rect("Check", box, new Vector2(0.5f, 0.5f), new Vector2(0f, 6f), Vector2.one * BoxSize);
            var shortStroke = UiFactory.Rect("Short", check, new Vector2(0.5f, 0.5f), new Vector2(-16f, -8f), new Vector2(18f, 40f));
            shortStroke.localRotation = Quaternion.Euler(0f, 0f, 40f);
            UiFactory.AddBox(shortStroke, art.FlatBox, Palette.CheckGreen).pixelsPerUnitMultiplier = 3f;
            var longStroke = UiFactory.Rect("Long", check, new Vector2(0.5f, 0.5f), new Vector2(10f, 4f), new Vector2(18f, 70f));
            longStroke.localRotation = Quaternion.Euler(0f, 0f, -38f);
            UiFactory.AddBox(longStroke, art.FlatBox, Palette.CheckGreen).pixelsPerUnitMultiplier = 3f;
            return check.gameObject;
        }

        private GameObject BuildGameActions(ArtLibrary art)
        {
            var actions = UiFactory.Rect("GameActions", _panel.Root, new Vector2(0.5f, 0.5f), new Vector2(0f, -250f), new Vector2(1000f, 220f));
            UiFactory.TextButton(actions, "Home", new Vector2(-330f, 0f), Vector2.one * SquareButton, art, Texts.Home, Palette.ButtonOrange, () => HomeTapped?.Invoke());
            UiFactory.TextButton(actions, "Restart", new Vector2(0f, 0f), new Vector2(380f, 130f), art, Texts.Restart, Palette.ButtonBlue, () => RestartTapped?.Invoke());
            var skip = UiFactory.TextButton(actions, "Skip", new Vector2(330f, 0f), Vector2.one * SquareButton, art, Texts.SkipLevel, new Color32(0xF0, 0x6A, 0x2E, 0xFF), () => SkipTapped?.Invoke());

            var ad = UiFactory.Rect("Ad", skip.transform, new Vector2(1f, 1f), new Vector2(-6f, -6f), new Vector2(84f, 46f));
            UiFactory.AddBox(ad, art.FlatBox, Color.white);
            var adText = UiFactory.AddLabel(UiFactory.Stretch("Text", ad), art.Font, 28, TextAnchor.MiddleCenter, Palette.PillLightText);
            adText.text = Texts.AdBadge;
            return actions.gameObject;
        }
    }
}
