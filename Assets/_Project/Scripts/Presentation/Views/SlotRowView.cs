using System;
using SheepStrike.Domain.Core;
using SheepStrike.Domain.Shooters;
using SheepStrike.Presentation.Theme;
using UnityEngine;
using UnityEngine.UI;

namespace SheepStrike.Presentation.Views
{
    /// <summary>
    /// The pen: dark sockets with a red rim. The open slots form the main row; the slots still to unlock sit above
    /// its ends as locked discs with a video badge, and tapping one unlocks it (the extra-pen booster).
    /// </summary>
    public sealed class SlotRowView
    {
        private const float Size = 150f;
        private const float RowGap = 30f;
        private const float SheepFill = 0.86f;
        private const float RecoilPunch = 0.18f;
        private const float RetireShrinkPunch = -0.3f;
        private const float SheepBreathe = 0.05f;
        private const float LockedBreathe = 0.06f;
        private const float BreatheCycles = 1.4f;

        private sealed class SlotWidget
        {
            public RectTransform Rect;
            public Image Sheep;
            public Text Ammo;
            public ScaleAnimator SheepScale;
            public GameObject Locked;
        }

        private readonly RectTransform _root;
        private readonly SlotWidget[] _slots;
        private int _capacity;

        public SlotRowView(Transform parent, float bottomOffset, ArtLibrary art, int maxCapacity, Action onUnlockSlot)
        {
            _root = UiFactory.Rect("SlotRow", parent, new Vector2(0.5f, 0f), new Vector2(0f, bottomOffset + Size * 0.5f), new Vector2(1080f, Size));
            _slots = new SlotWidget[maxCapacity];
            for (int i = 0; i < maxCapacity; i++) _slots[i] = CreateSlot(i, art, onUnlockSlot);
        }

        public RectTransform Root => _root;

        /// <summary>Places every slot where the pen says it stands and shows the ones not unlocked yet as locked.</summary>
        public void SetPen(int capacity, Func<int, PenPosition> positionOf)
        {
            bool grew = _capacity > 0 && capacity > _capacity;
            _capacity = capacity;

            int rowLength = 0;
            for (int i = 0; i < _slots.Length; i++)
            {
                if (positionOf(i).Row == 0) rowLength++;
            }

            for (int i = 0; i < _slots.Length; i++)
            {
                var position = positionOf(i);
                float x = (position.X - (rowLength - 1) * 0.5f) * SheepQueueView.ColumnStep;
                _slots[i].Rect.anchoredPosition = new Vector2(x, position.Row * (Size + RowGap));
                _slots[i].Locked.SetActive(i >= capacity);
            }
            if (grew) ScaleAnimator.On(_slots[capacity - 1].Rect).PopIn();
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

        public Vector2 SheepSize => Vector2.one * (Size * SheepFill);

        public float TopScreenY()
        {
            var corners = new Vector3[4];
            _root.GetWorldCorners(corners);
            return corners[1].y;
        }

        private SlotWidget CreateSlot(int index, ArtLibrary art, Action onUnlockSlot)
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
            var locked = CreateLockedCover(rect, art, index, onUnlockSlot);
            return new SlotWidget { Rect = rect, Sheep = sheep, Ammo = ammo, SheepScale = sheepScale, Locked = locked };
        }

        /// <summary>Dark disc with a video badge and "解锁"; tapping it asks for the ad that unlocks the slot.</summary>
        private static GameObject CreateLockedCover(RectTransform slot, ArtLibrary art, int index, Action onUnlockSlot)
        {
            var cover = UiFactory.Stretch("Locked", slot);
            UiFactory.AddImage(cover, art.Disc, Palette.SlotLocked, true);
            UiFactory.AddImage(UiFactory.Stretch("Rim", cover), art.Ring, new Color(1f, 1f, 1f, 0.25f));

            var badge = UiFactory.Rect("Video", cover, new Vector2(0.5f, 0.5f), new Vector2(0f, 22f), new Vector2(84f, 46f));
            UiFactory.AddBox(badge, art.FlatBox, Color.white);
            var badgeText = UiFactory.AddLabel(UiFactory.Stretch("Text", badge), art.Font, 28, TextAnchor.MiddleCenter, Palette.PillLightText);
            badgeText.text = Texts.AdBadge;

            var unlock = UiFactory.AddLabel(UiFactory.Rect("Unlock", cover, new Vector2(0.5f, 0.5f), new Vector2(0f, -32f), new Vector2(Size, 44f)), art.Font, 36, TextAnchor.MiddleCenter);
            unlock.text = Texts.Unlock;
            ScaleAnimator.On(UiFactory.AddButton(cover, onUnlockSlot)).Breathe(LockedBreathe, BreatheCycles * 0.6f, index);
            return cover.gameObject;
        }
    }
}
