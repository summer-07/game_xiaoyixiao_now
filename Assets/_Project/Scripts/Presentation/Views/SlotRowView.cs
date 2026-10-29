using System;
using SheepStrike.Domain.Core;
using SheepStrike.Presentation.Theme;
using UnityEngine;
using UnityEngine.UI;

namespace SheepStrike.Presentation.Views
{
    /// <summary>The launch pad row: one ring per slot plus a "+" ghost that triggers the extra-pen booster.</summary>
    public sealed class SlotRowView
    {
        private const float Size = 150f;
        private const float Spacing = 10f;

        private sealed class SlotWidget
        {
            public RectTransform Rect;
            public Image Sheep;
            public Text Ammo;
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
            UiFactory.AddImage(_addSlot, art.SlotRing, new Color(1f, 1f, 1f, 0.35f), true);
            var plus = UiFactory.AddLabel(UiFactory.Stretch("Plus", _addSlot), art.Font, 80, TextAnchor.MiddleCenter);
            plus.text = "+";
            UiFactory.AddButton(_addSlot, onAddSlot);
        }

        public RectTransform Root => _root;

        public void SetCapacity(int capacity)
        {
            _capacity = capacity;
            bool canGrow = capacity < _slots.Length;
            int shown = capacity + (canGrow ? 1 : 0);
            float start = -(shown - 1) * (Size + Spacing) * 0.5f;

            for (int i = 0; i < _slots.Length; i++)
            {
                bool visible = i < capacity;
                _slots[i].Rect.gameObject.SetActive(visible);
                if (visible) _slots[i].Rect.anchoredPosition = new Vector2(start + i * (Size + Spacing), 0f);
            }
            _addSlot.gameObject.SetActive(canGrow);
            _addSlot.anchoredPosition = new Vector2(start + capacity * (Size + Spacing), 0f);
        }

        public void SetSheep(int slot, GameColor color, int ammo)
        {
            var widget = _slots[slot];
            widget.Sheep.gameObject.SetActive(true);
            widget.Sheep.color = Palette.Of(color);
            widget.Ammo.text = ammo.ToString();
        }

        public void SetAmmo(int slot, int ammo) => _slots[slot].Ammo.text = ammo > 0 ? ammo.ToString() : string.Empty;

        public void ClearSlot(int slot)
        {
            _slots[slot].Sheep.gameObject.SetActive(false);
            _slots[slot].Ammo.text = string.Empty;
        }

        public void ClearAll()
        {
            for (int i = 0; i < _slots.Length; i++) ClearSlot(i);
        }

        public Vector2 ScreenPositionOf(int slot) => RectTransformUtility.WorldToScreenPoint(null, _slots[Mathf.Clamp(slot, 0, _capacity - 1)].Rect.position);

        public float TopScreenY()
        {
            var corners = new Vector3[4];
            _root.GetWorldCorners(corners);
            return corners[1].y;
        }

        private SlotWidget CreateSlot(int index, ArtLibrary art)
        {
            var rect = UiFactory.Rect("Slot" + index, _root, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(Size, Size));
            UiFactory.AddImage(rect, art.SlotRing, art.SlotRing != null ? Color.white : Palette.SlotEmpty);

            var sheepRect = UiFactory.Rect("Sheep", rect, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(Size * 0.8f, Size * 0.8f));
            var sheep = UiFactory.AddImage(sheepRect, art.SheepIcon, Color.white);
            var ammo = UiFactory.AddLabel(UiFactory.Stretch("Ammo", sheepRect), art.Font, 54, TextAnchor.MiddleCenter);
            sheepRect.gameObject.SetActive(false);

            return new SlotWidget { Rect = rect, Sheep = sheep, Ammo = ammo };
        }
    }
}
