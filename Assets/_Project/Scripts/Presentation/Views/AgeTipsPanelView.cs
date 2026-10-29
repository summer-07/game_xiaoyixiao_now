using System;
using SheepStrike.Presentation.Theme;
using UnityEngine;
using UnityEngine.UI;

namespace SheepStrike.Presentation.Views
{
    /// <summary>The 适龄提示 dialog opened from the age badge on the home screen (minor-protection requirement).</summary>
    public sealed class AgeTipsPanelView
    {
        private static readonly Vector2 BoardSize = new Vector2(940f, 900f);
        private const float TextInset = 60f;

        private readonly NotebookPanel _panel;

        public AgeTipsPanelView(RectTransform parent, ArtLibrary art, Action onClose)
        {
            _panel = new NotebookPanel(parent, art, "AgeTips", BoardSize, Texts.AgeTipsTitle, onClose);
            var body = UiFactory.Stretch("Body", _panel.Content);
            body.offsetMin = new Vector2(TextInset, TextInset);
            body.offsetMax = new Vector2(-TextInset, -20f);
            var text = UiFactory.AddLabel(body, art.Font, 42, TextAnchor.UpperLeft, Palette.WoodText);
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.lineSpacing = 1.25f;
            text.text = string.Format(Texts.AgeTipsBody, ComplianceInfo.MinimumAge);
            foreach (var outline in text.GetComponents<Outline>()) UnityEngine.Object.Destroy(outline);
        }

        public bool IsOpen => _panel.IsOpen;

        public void Show() => _panel.Show();

        public void Hide() => _panel.Hide();
    }
}
