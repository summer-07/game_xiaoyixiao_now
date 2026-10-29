using System;
using System.Collections.Generic;
using SheepStrike.Domain.Session;
using SheepStrike.Presentation.Theme;
using UnityEngine;
using UnityEngine.UI;

namespace SheepStrike.Presentation.Views
{
    /// <summary>Bottom bar with the four boosters. The badge shows free charges, or "视频" when an ad is required.</summary>
    public sealed class BoosterBarView
    {
        public const float Height = 240f;
        private const float ButtonSize = 170f;

        private static readonly BoosterType[] Order =
        {
            BoosterType.RamCharge, BoosterType.ExtraPen, BoosterType.ShepherdDog, BoosterType.Shuffle
        };

        private sealed class Widget
        {
            public Image Icon;
            public Image Badge;
            public Text BadgeText;
            public RectTransform Rect;
        }

        private readonly Dictionary<BoosterType, Widget> _widgets = new Dictionary<BoosterType, Widget>();

        public BoosterBarView(Transform parent, ArtLibrary art, Action<BoosterType> onTap)
        {
            var bar = UiFactory.Rect("BoosterBar", parent, new Vector2(0.5f, 0f), new Vector2(0f, Height * 0.5f), new Vector2(1080f, Height));
            UiFactory.AddImage(bar, null, Palette.BoosterBar);

            float step = 1080f / Order.Length;
            for (int i = 0; i < Order.Length; i++)
            {
                var booster = Order[i];
                var rect = UiFactory.Rect(booster.ToString(), bar, new Vector2(0.5f, 0.5f), new Vector2(-540f + step * (i + 0.5f), 25f), new Vector2(ButtonSize, ButtonSize));
                UiFactory.AddImage(rect, art.RoundButton, art.RoundButton != null ? Color.white : new Color(1f, 1f, 1f, 0.2f), true);

                var iconRect = UiFactory.Rect("Icon", rect, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(ButtonSize * 0.75f, ButtonSize * 0.75f));
                var icon = UiFactory.AddImage(iconRect, art.Booster(booster), Color.white);
                if (icon.sprite == null) icon.color = new Color(1f, 1f, 1f, 0f);

                var label = UiFactory.AddLabel(UiFactory.Rect("Name", rect, new Vector2(0.5f, 0f), new Vector2(0f, -30f), new Vector2(240f, 50f)), art.Font, 38, TextAnchor.MiddleCenter);
                label.text = Texts.Booster(booster);

                var badgeRect = UiFactory.Rect("Badge", rect, new Vector2(1f, 1f), new Vector2(-10f, -10f), new Vector2(84f, 56f));
                var badge = UiFactory.AddImage(badgeRect, null, Palette.Badge);
                var badgeText = UiFactory.AddLabel(UiFactory.Stretch("Text", badgeRect), art.Font, 32, TextAnchor.MiddleCenter);

                UiFactory.AddButton(rect, () => onTap(booster));
                _widgets.Add(booster, new Widget { Icon = icon, Badge = badge, BadgeText = badgeText, Rect = rect });
            }
        }

        public void SetCharges(BoosterType booster, int freeCharges) =>
            _widgets[booster].BadgeText.text = freeCharges > 0 ? freeCharges.ToString() : "视频";

        public void SetAvailable(BoosterType booster, bool available)
        {
            var widget = _widgets[booster];
            if (widget.Icon.sprite != null) widget.Icon.color = available ? Color.white : Palette.Disabled;
            widget.Badge.gameObject.SetActive(available);
        }

        public void SetHighlighted(BoosterType booster, bool on) =>
            _widgets[booster].Rect.localScale = Vector3.one * (on ? 1.12f : 1f);
    }
}
