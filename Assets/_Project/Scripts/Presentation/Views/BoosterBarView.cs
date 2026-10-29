using System;
using System.Collections.Generic;
using SheepStrike.Domain.Session;
using SheepStrike.Presentation.Theme;
using UnityEngine;
using UnityEngine.UI;

namespace SheepStrike.Presentation.Views
{
    /// <summary>Bottom bar with the four boosters: round navy buttons, a red badge with free charges, or a "视频" pill when an ad is required.</summary>
    public sealed class BoosterBarView
    {
        public const float Height = 230f;
        private const float ButtonSize = 160f;
        private const float RimWidth = 8f;
        private const float IconFill = 0.72f;
        private const float BarCornerOverlap = 40f;
        private const float HighlightScale = 1.12f;
        private const float BadgeSize = 62f;
        private const float ShadowDrop = 10f;
        private const float ButtonBreathe = 0.03f;
        private const float BadgeBreathe = 0.12f;
        private const float BreatheCycles = 0.8f;
        private const float HighlightHeight = 14f;

        private static readonly BoosterType[] Order =
        {
            BoosterType.RamCharge, BoosterType.ExtraPen, BoosterType.ShepherdDog, BoosterType.Shuffle
        };

        private sealed class Widget
        {
            public Image Icon;
            public RectTransform Count;
            public Text CountText;
            public RectTransform Ad;
            public ScaleAnimator Scale;
        }

        private readonly Dictionary<BoosterType, Widget> _widgets = new Dictionary<BoosterType, Widget>();

        public BoosterBarView(Transform parent, ArtLibrary art, Action<BoosterType> onTap)
        {
            var bar = UiFactory.Rect("BoosterBar", parent, new Vector2(0.5f, 0f), new Vector2(0f, Height * 0.5f - BarCornerOverlap * 0.5f), new Vector2(1080f, Height + BarCornerOverlap));
            UiFactory.AddBox(bar, art.FlatBox, Palette.BoosterBar);
            var highlight = UiFactory.Rect("Highlight", bar, new Vector2(0.5f, 1f), new Vector2(0f, -HighlightHeight * 0.5f - 4f), new Vector2(1040f, HighlightHeight));
            UiFactory.AddBox(highlight, art.FlatBox, new Color(1f, 1f, 1f, 0.25f)).pixelsPerUnitMultiplier = 4f;

            float step = 1080f / Order.Length;
            for (int i = 0; i < Order.Length; i++)
            {
                var booster = Order[i];
                var position = new Vector2(-540f + step * (i + 0.5f), 30f);
                _widgets.Add(booster, CreateWidget(bar, art, booster, position, () => onTap(booster)));
            }
        }

        public void SetCharges(BoosterType booster, int freeCharges)
        {
            var widget = _widgets[booster];
            widget.CountText.text = freeCharges.ToString();
            widget.Count.gameObject.SetActive(freeCharges > 0);
            widget.Ad.gameObject.SetActive(freeCharges <= 0);
        }

        public void SetAvailable(BoosterType booster, bool available)
        {
            var widget = _widgets[booster];
            if (widget.Icon.sprite != null) widget.Icon.color = available ? Color.white : Palette.Disabled;
        }

        public void SetHighlighted(BoosterType booster, bool on) =>
            _widgets[booster].Scale.BaseScale = on ? HighlightScale : 1f;

        private static Widget CreateWidget(RectTransform bar, ArtLibrary art, BoosterType booster, Vector2 position, Action onTap)
        {
            var shadow = UiFactory.Rect(booster + "Shadow", bar, new Vector2(0.5f, 0.5f), position + Vector2.down * ShadowDrop, new Vector2(ButtonSize, ButtonSize));
            UiFactory.AddImage(shadow, art.Disc, new Color(0f, 0f, 0.2f, 0.3f));

            var rect = UiFactory.Rect(booster.ToString(), bar, new Vector2(0.5f, 0.5f), position, new Vector2(ButtonSize, ButtonSize));
            UiFactory.AddImage(rect, art.Disc, Palette.BoosterRing, true);
            var face = UiFactory.Stretch("Face", rect);
            face.offsetMin = Vector2.one * RimWidth;
            face.offsetMax = -Vector2.one * RimWidth;
            UiFactory.AddImage(face, art.Disc, Palette.BoosterButton);
            var gloss = UiFactory.Rect("Gloss", rect, new Vector2(0.5f, 0.5f), new Vector2(0f, ButtonSize * 0.22f), new Vector2(ButtonSize * 0.62f, ButtonSize * 0.3f));
            UiFactory.AddImage(gloss, art.Disc, new Color(1f, 1f, 1f, 0.12f));

            var iconRect = UiFactory.Rect("Icon", rect, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.one * (ButtonSize * IconFill));
            var icon = UiFactory.AddImage(iconRect, art.Booster(booster), Color.white);
            if (icon.sprite == null) icon.color = new Color(1f, 1f, 1f, 0f);

            var label = UiFactory.AddLabel(UiFactory.Rect("Name", rect, new Vector2(0.5f, 0f), new Vector2(0f, -32f), new Vector2(260f, 56f)), art.Font, 40, TextAnchor.MiddleCenter);
            label.text = Texts.Booster(booster);

            var count = UiFactory.Rect("Count", rect, new Vector2(1f, 1f), new Vector2(-14f, -14f), Vector2.one * BadgeSize);
            UiFactory.AddImage(count, art.Disc, Palette.Badge);
            var countText = UiFactory.AddLabel(UiFactory.Stretch("Text", count), art.Font, 38, TextAnchor.MiddleCenter);
            ScaleAnimator.On(count).Breathe(BadgeBreathe, BreatheCycles * 2f, position.x * 0.01f);

            var ad = UiFactory.Rect("Ad", rect, new Vector2(1f, 0f), new Vector2(-10f, 22f), new Vector2(84f, 46f));
            UiFactory.AddBox(ad, art.FlatBox, Color.white);
            var adText = UiFactory.AddLabel(UiFactory.Stretch("Text", ad), art.Font, 28, TextAnchor.MiddleCenter, Palette.PillLightText);
            adText.text = Texts.AdBadge;

            var button = UiFactory.AddButton(rect, onTap);
            ScaleAnimator.On(button).Breathe(ButtonBreathe, BreatheCycles, position.x * 0.01f);
            return new Widget { Icon = icon, Count = count, CountText = countText, Ad = ad, Scale = ScaleAnimator.On(button) };
        }
    }
}
