using System;
using System.Collections.Generic;
using SheepStrike.Presentation.Theme;
using SheepStrike.UseCases.Interactors;
using UnityEngine;
using UnityEngine.UI;

namespace SheepStrike.Presentation.Views
{
    /// <summary>
    /// The collection (图鉴): a 3x3 page of hanging picture frames, one per beaten level with its animal and a replay
    /// button, the current level as a "?" frame, an unlocked counter and page controls.
    /// </summary>
    public sealed class CollectionPanelView
    {
        private const int Columns = 3;
        private static readonly Vector2 BoardSize = new Vector2(980f, 1420f);
        private static readonly Vector2 FrameSize = new Vector2(230f, 230f);
        private static readonly Vector2 ReplaySize = new Vector2(200f, 76f);
        private const float ReplayGap = 14f;
        private const float CellWidth = 290f;
        private const float CellHeight = 340f;
        private const float PagerY = 55f;
        private const float GridTop = -130f;
        private const float FrameInset = 22f;
        private const float BadgeSize = 64f;

        private sealed class Card
        {
            public RectTransform Root;
            public Text Number;
            public Image Animal;
            public Text Locked;
            public GameObject Replay;
            public int Level;
        }

        private readonly ArtLibrary _art;
        private readonly NotebookPanel _panel;
        private readonly Text _unlocked;
        private readonly Text _pageLabel;
        private readonly Button _previous;
        private readonly Button _next;
        private readonly List<Card> _cards = new List<Card>();
        private int _page;

        public CollectionPanelView(RectTransform parent, ArtLibrary art, Action<int> onReplay, Action<int> onPageRequested, Action onClose)
        {
            _art = art;
            _panel = new NotebookPanel(parent, art, "Collection", BoardSize, Texts.Collection, onClose);

            var pill = UiFactory.Rect("UnlockedPill", _panel.Content, new Vector2(0.5f, 1f), new Vector2(0f, -40f), new Vector2(420f, 70f));
            UiFactory.AddBox(pill, art.FlatBox, Palette.PaperLine);
            _unlocked = UiFactory.AddLabel(UiFactory.Stretch("Text", pill), art.Font, 40, TextAnchor.MiddleCenter, Palette.WoodText);
            KeepWhiteRim(_unlocked);

            for (int i = 0; i < BrowseCollection.PageSize; i++) _cards.Add(CreateCard(art, i, onReplay));

            var pager = UiFactory.Rect("Pager", _panel.Content, new Vector2(0.5f, 0f), new Vector2(0f, PagerY), new Vector2(860f, 90f));
            _previous = PagerButton(art, pager, -280f, Texts.PreviousPage, () => onPageRequested(_page - 1));
            _next = PagerButton(art, pager, 280f, Texts.NextPage, () => onPageRequested(_page + 1));
            _pageLabel = UiFactory.AddLabel(UiFactory.Rect("Page", pager, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(300f, 90f)), art.Font, 48, TextAnchor.MiddleCenter, Palette.WoodText);
            KeepWhiteRim(_pageLabel);
        }

        public bool IsOpen => _panel.IsOpen;

        public void Show() => _panel.Show();

        public void Hide() => _panel.Hide();

        /// <param name="page">0-based page being shown.</param>
        public void Render(int unlockedCount, int page, int pageCount, IReadOnlyList<CollectionEntry> entries)
        {
            _page = page;
            _unlocked.text = Texts.Unlocked(unlockedCount);
            _pageLabel.text = Texts.PageNumber(page + 1);
            SetPagerEnabled(_previous, page > 0);
            SetPagerEnabled(_next, page < pageCount - 1);

            for (int i = 0; i < _cards.Count; i++)
            {
                bool used = i < entries.Count;
                _cards[i].Root.gameObject.SetActive(used);
                if (used) Fill(_cards[i], entries[i]);
            }
        }

        private void Fill(Card card, CollectionEntry entry)
        {
            card.Level = entry.Level;
            card.Number.text = entry.Level.ToString();
            card.Animal.sprite = entry.IsUnlocked ? _art.Target(entry.TargetId) : null;
            card.Animal.enabled = card.Animal.sprite != null;
            card.Locked.gameObject.SetActive(!entry.IsUnlocked);
            card.Replay.SetActive(entry.IsUnlocked);
            ScaleAnimator.On(card.Root).PopIn();
        }

        private Card CreateCard(ArtLibrary art, int index, Action<int> onReplay)
        {
            int column = index % Columns, row = index / Columns;
            var position = new Vector2((column - 1) * CellWidth, GridTop - row * CellHeight - FrameSize.y * 0.5f);
            var root = UiFactory.Rect("Card" + index, _panel.Content, new Vector2(0.5f, 1f), position, FrameSize);

            var hook = UiFactory.Rect("Hook", root, new Vector2(0.5f, 1f), new Vector2(0f, 14f), Vector2.one * 26f);
            UiFactory.AddImage(hook, art.Disc, Palette.Clip);
            UiFactory.AddBox(root, art.BevelBox, Palette.Wood);
            var inner = UiFactory.Stretch("Picture", root);
            inner.offsetMin = new Vector2(FrameInset, FrameInset + 8f);
            inner.offsetMax = new Vector2(-FrameInset, -FrameInset);
            UiFactory.AddBox(inner, art.FlatBox, Palette.Paper);

            var animal = UiFactory.AddImage(UiFactory.Rect("Animal", inner, new Vector2(0.5f, 0.5f), Vector2.zero, FrameSize * 0.68f), null, Color.white);
            var locked = UiFactory.AddLabel(UiFactory.Stretch("Locked", inner), art.Font, 140, TextAnchor.MiddleCenter);
            locked.text = Texts.Locked;

            var badge = UiFactory.Rect("Badge", root, new Vector2(0f, 1f), new Vector2(26f, -18f), new Vector2(BadgeSize * 1.3f, BadgeSize));
            UiFactory.AddBox(badge, art.FlatBox, Palette.WoodLight);
            var number = UiFactory.AddLabel(UiFactory.Stretch("Number", badge), art.Font, 38, TextAnchor.MiddleCenter, Palette.WoodText);
            KeepWhiteRim(number);

            var card = new Card { Root = root, Number = number, Animal = animal, Locked = locked };
            var replay = UiFactory.TextButton(root, "Replay", new Vector2(0f, -(FrameSize.y + ReplaySize.y) * 0.5f - ReplayGap), ReplaySize, art, Texts.Replay, Palette.WoodLight, () => onReplay(card.Level));
            foreach (var label in replay.GetComponentsInChildren<Text>()) label.color = Palette.WoodText;
            card.Replay = replay.gameObject;
            return card;
        }

        private static Button PagerButton(ArtLibrary art, RectTransform pager, float x, string label, Action onClick)
        {
            var rect = UiFactory.Rect(label, pager, new Vector2(0.5f, 0.5f), new Vector2(x, 0f), new Vector2(220f, 90f));
            var text = UiFactory.AddLabel(rect, art.Font, 46, TextAnchor.MiddleCenter, Palette.WoodText);
            text.text = label;
            text.raycastTarget = true;
            KeepWhiteRim(text);
            var underline = UiFactory.Rect("Underline", rect, new Vector2(0.5f, 0f), new Vector2(0f, 10f), new Vector2(170f, 6f));
            UiFactory.AddImage(underline, null, Palette.WoodText);
            return UiFactory.AddButton(rect, onClick);
        }

        private static void SetPagerEnabled(Button button, bool enabled)
        {
            button.interactable = enabled;
            foreach (var graphic in button.GetComponentsInChildren<Graphic>()) graphic.color = WithAlpha(graphic.color, enabled ? 1f : 0.35f);
        }

        private static Color WithAlpha(Color color, float alpha) => new Color(color.r, color.g, color.b, alpha);

        private static void KeepWhiteRim(Text text)
        {
            foreach (var outline in text.GetComponents<Outline>()) outline.effectColor = Color.white;
        }
    }
}
