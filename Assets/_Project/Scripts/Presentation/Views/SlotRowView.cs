using System;
using SheepStrike.Domain.Core;
using SheepStrike.Presentation.Theme;
using UnityEngine;
using UnityEngine.UI;

namespace SheepStrike.Presentation.Views
{
    /// <summary>The launch pad row: dark sockets with a red rim, plus a "+" socket that triggers the extra-pen booster.</summary>
    public sealed class SlotRowView
    {
        private const float Size = 150f;
        private const float Spacing = 14f;
        private const float UsableWidth = 1040f;
        private const float SheepFill = 0.86f;
        private const float RecoilPunch = 0.18f;
        private const float RetireShrinkPunch = -0.3f;
        private const float SheepBreathe = 0.05f;
        private const float AddSlotBreathe = 0.06f;
        private const float BreatheCycles = 1.4f;

        private sealed class SlotWidget
        {
            public RectTransform Rect;
            public Image Sheep;
            public Text Ammo;
            public ScaleAnimator SheepScale;
        }

        private readonly RectTransform _root;
        private readonly SlotWidget[] _slots;
        private readonly RectTransform _addSlot;
        private int _capacity;

        public SlotRowView(Transform parent, float bottomOffset, ArtLibrary art, int maxCapacity, Action onAddSlot)
        {
            _root = UiFactory.Rect("SlotRow", parent, new Vector2(0.5f, 0f), new Vector2(0f, bottomOffset + Size * 0.5f), new Vector2(1080f, Size));
            _slots = new SlotWidget[maxCapacity];
            for (int i = 0; i < maxCapacity; i++) _slots[i] = CreateSlot(i, art);

            _addSlot = UiFactory.Rect("AddSlot", _root, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(Size, Size));
            UiFactory.AddImage(_addSlot, art.Disc, Palette.SlotLocked, true);
            UiFactory.AddImage(UiFactory.Stretch("Rim", _addSlot), art.Ring, new Color(1f, 1f, 1f, 0.25f));
            var plus = UiFactory.AddLabel(UiFactory.Rect("Plus", _addSlot, new Vector2(0.5f, 0.5f), new Vector2(0f, 14f), new Vector2(Size, 80f)), art.Font, 70, TextAnchor.MiddleCenter);
            plus.text = "+";
            var unlock = UiFactory.AddLabel(UiFactory.Rect("Unlock", _addSlot, new Vector2(0.5f, 0.5f), new Vector2(0f, -40f), new Vector2(Size, 40f)), art.Font, 30, TextAnchor.MiddleCenter);
            unlock.text = Texts.Unlock;
            ScaleAnimator.On(UiFactory.AddButton(_addSlot, onAddSlot)).Breathe(AddSlotBreathe, BreatheCycles * 0.6f, 0f);
        }

        public RectTransform Root => _root;

        public void SetCapacity(int capacity)
        {
            bool grew = capacity > _capacity;
            _capacity = capacity;
            bool canGrow = capacity < _slots.Length;
            int shown = capacity + (canGrow ? 1 : 0);
            float start = -(shown - 1) * (Size + Spacing) * 0.5f;

            float rowWidth = shown * (Size + Spacing) - Spacing;
            _root.localScale = Vector3.one * Mathf.Min(1f, UsableWidth / rowWidth);

            for (int i = 0; i < _slots.Length; i++)
            {
                bool visible = i < capacity;
                _slots[i].Rect.gameObject.SetActive(visible);
                if (visible) _slots[i].Rect.anchoredPosition = new Vector2(start + i * (Size + Spacing), 0f);
            }
            if (grew && capacity > 0) ScaleAnimator.On(_slots[capacity - 1].Rect).PopIn();
            _addSlot.gameObject.SetActive(canGrow);
            _addSlot.anchoredPosition = new Vector2(start + capacity * (Size + Spacing), 0f);
        }

        public void SetSheep(int slot, GameColor color, int ammo)
        {
            var widget = _slots[slot];
            widget.Sheep.gameObject.SetActive(true);
            widget.Sheep.color = Palette.Of(color);
            widget.Ammo.text = ammo.ToString();
            widget.SheepScale.PopIn();
        }

        public void SetAmmo(int slot, int ammo)
        {
            var widget = _slots[slot];
            widget.Ammo.text = ammo > 0 ? ammo.ToString() : string.Empty;
            widget.SheepScale.Punch(RecoilPunch);
        }

        public void ClearSlot(int slot)
        {
            _slots[slot].Sheep.gameObject.SetActive(false);
            _slots[slot].Ammo.text = string.Empty;
            ScaleAnimator.On(_slots[slot].Rect).Punch(RetireShrinkPunch);
        }

        public void ClearAll()
        {
            for (int i = 0; i < _slots.Length; i++)
            {
                _slots[i].Sheep.gameObject.SetActive(false);
                _slots[i].Ammo.text = string.Empty;
            }
        }

        public Vector2 ScreenPositionOf(int slot) => RectTransformUtility.WorldToScreenPoint(null, _slots[Mathf.Clamp(slot, 0, _capacity - 1)].Rect.position);

        public Vector2 SheepSize => Vector2.one * (Size * SheepFill * _root.localScale.x);

        public float TopScreenY()
        {
            var corners = new Vector3[4];
            _root.GetWorldCorners(corners);
            return corners[1].y;
        }

        private SlotWidget CreateSlot(int index, ArtLibrary art)
        {
            var rect = UiFactory.Rect("Slot" + index, _root, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(Size, Size));
            UiFactory.AddImage(rect, art.Disc, Palette.SlotEmpty);
            UiFactory.AddImage(UiFactory.Stretch("Rim", rect), art.Ring, Palette.SlotRing);

            var sheepRect = UiFactory.Rect("Sheep", rect, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(Size * SheepFill, Size * SheepFill));
            var sheep = UiFactory.AddImage(sheepRect, art.SheepIcon, Color.white);
            var ammo = UiFactory.AddLabel(UiFactory.Stretch("Ammo", sheepRect), art.Font, 56, TextAnchor.MiddleCenter);
            sheepRect.gameObject.SetActive(false);

            var sheepScale = ScaleAnimator.On(sheepRect);
            sheepScale.Breathe(SheepBreathe, BreatheCycles, index * 1.3f);
            return new SlotWidget { Rect = rect, Sheep = sheep, Ammo = ammo, SheepScale = sheepScale };
        }
    }
}
