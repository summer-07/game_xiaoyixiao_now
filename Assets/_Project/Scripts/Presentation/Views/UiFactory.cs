using System;
using SheepStrike.Presentation.Theme;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SheepStrike.Presentation.Views
{
    /// <summary>Builds uGUI elements in code so the HUD needs no prefab or scene wiring.</summary>
    public static class UiFactory
    {
        public static readonly Vector2 ReferenceResolution = new Vector2(1080f, 1920f);
        private const float TextOutlineRatio = 0.07f;
        private const float ButtonTextRatio = 0.42f;
        private const float ButtonTextWidthFill = 0.8f;
        private const float ButtonLipOffset = 10f;

        public static Canvas CreateCanvas(string name, int sortingOrder)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;
            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = ReferenceResolution;
            scaler.matchWidthOrHeight = 0f;
            go.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        public static void EnsureEventSystem()
        {
            if (EventSystem.current != null) return;
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }

        public static RectTransform Rect(string name, Transform parent, Vector2 anchor, Vector2 position, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            return rect;
        }

        public static RectTransform Stretch(string name, Transform parent)
        {
            var rect = Rect(name, parent, Vector2.zero, Vector2.zero, Vector2.zero);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            return rect;
        }

        /// <summary>A 9-sliced rounded panel or pill.</summary>
        public static Image AddBox(RectTransform rect, Sprite box, Color color, bool raycast = false)
        {
            var image = AddImage(rect, box, color, raycast);
            image.type = Image.Type.Sliced;
            image.preserveAspect = false;
            return image;
        }

        public static Image AddImage(RectTransform rect, Sprite sprite, Color color, bool raycast = false)
        {
            var image = rect.gameObject.AddComponent<Image>();
            image.sprite = sprite;
            image.color = color;
            image.preserveAspect = sprite != null;
            image.raycastTarget = raycast;
            return image;
        }

        public static Text AddLabel(RectTransform rect, Font font, int size, TextAnchor alignment, Color? color = null)
        {
            var text = rect.gameObject.AddComponent<Text>();
            text.font = font;
            text.fontSize = size;
            text.alignment = alignment;
            text.color = color ?? Palette.TextLight;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            AddOutline(rect.gameObject, Mathf.Max(2f, size * TextOutlineRatio));
            return text;
        }

        public static Button AddButton(RectTransform rect, Action onClick)
        {
            if (rect.GetComponent<Graphic>() == null) AddImage(rect, null, new Color(1f, 1f, 1f, 0f));
            rect.GetComponent<Graphic>().raycastTarget = true;
            var button = rect.gameObject.AddComponent<Button>();
            button.transition = Selectable.Transition.None;
            button.onClick.AddListener(() => onClick());
            rect.gameObject.AddComponent<PressFeedback>();
            return button;
        }

        /// <summary>Chunky rounded button with a darker bottom lip, like a toy key.</summary>
        public static Button TextButton(Transform parent, string name, Vector2 position, Vector2 size, ArtLibrary art, string label, Color color, Action onClick)
        {
            var rect = Rect(name, parent, new Vector2(0.5f, 0.5f), position, size);
            AddBox(rect, art.BevelBox, color, true);
            var labelRect = Stretch("Label", rect);
            labelRect.offsetMin = new Vector2(0f, ButtonLipOffset);
            int fontSize = Mathf.RoundToInt(Mathf.Min(size.y * ButtonTextRatio, size.x * ButtonTextWidthFill / Mathf.Max(1, label.Length)));
            var text = AddLabel(labelRect, art.Font, fontSize, TextAnchor.MiddleCenter);
            text.text = label;
            return AddButton(rect, onClick);
        }

        /// <summary>Two offset outlines make the thick dark rim cartoon games put around white text.</summary>
        private static void AddOutline(GameObject go, float distance)
        {
            var outer = go.AddComponent<Outline>();
            outer.effectColor = Palette.TextOutline;
            outer.effectDistance = new Vector2(distance, -distance);
            var inner = go.AddComponent<Outline>();
            inner.effectColor = Palette.TextOutline;
            inner.effectDistance = new Vector2(distance * 0.5f, distance * 0.5f);
        }
    }
}
