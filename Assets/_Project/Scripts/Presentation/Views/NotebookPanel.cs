using System;
using SheepStrike.Presentation.Theme;
using UnityEngine;
using UnityEngine.UI;

namespace SheepStrike.Presentation.Views
{
    /// <summary>
    /// The wooden notebook dialog used by settings and the collection: a dimmed backdrop, a wood board with a
    /// paper sheet, two metal clips at the top, a title and an optional red close cross.
    /// </summary>
    public sealed class NotebookPanel
    {
        private const float BoardInset = 26f;
        private const float TitleHeight = 120f;
        private const float ClipInset = 150f;
        private static readonly Vector2 ClipSize = new Vector2(46f, 110f);
        private const float CloseSize = 96f;

        public NotebookPanel(RectTransform parent, ArtLibrary art, string name, Vector2 size, string title, Action onClose)
        {
            Root = UiFactory.Stretch(name, parent);
            UiFactory.AddImage(Root, null, Palette.Dim, true);

            Board = UiFactory.Rect("Board", Root, new Vector2(0.5f, 0.5f), Vector2.zero, size);
            UiFactory.AddBox(Board, art.BevelBox, Palette.Wood);
            var paper = UiFactory.Stretch("Paper", Board);
            paper.offsetMin = new Vector2(BoardInset, BoardInset + 10f);
            paper.offsetMax = new Vector2(-BoardInset, -BoardInset);
            UiFactory.AddBox(paper, art.FlatBox, Palette.Paper);

            AddClip(art, -size.x * 0.5f + ClipInset, size.y * 0.5f);
            AddClip(art, size.x * 0.5f - ClipInset, size.y * 0.5f);

            var titleRect = UiFactory.Rect("Title", Board, new Vector2(0.5f, 1f), new Vector2(0f, -TitleHeight * 0.5f - BoardInset), new Vector2(size.x, TitleHeight));
            Title = UiFactory.AddLabel(titleRect, art.Font, 78, TextAnchor.MiddleCenter, Palette.WoodText);
            Title.text = title;
            foreach (var outline in Title.GetComponents<Outline>()) outline.effectColor = Color.white;

            Content = UiFactory.Stretch("Content", paper);
            Content.offsetMax = new Vector2(0f, -TitleHeight);

            if (onClose != null) AddCloseButton(art, size, onClose);
            Root.gameObject.SetActive(false);
            _backdrop = UiTransition.On(Root, UiTransition.Motion.Fade);
            _board = UiTransition.On(Board, UiTransition.Motion.Pop);
        }

        private readonly UiTransition _backdrop;
        private readonly UiTransition _board;

        /// <summary>Full-screen backdrop; toggle this to show or hide the panel.</summary>
        public RectTransform Root { get; }

        /// <summary>The wood board (centre-anchored); add buttons that sit outside the paper here or on <see cref="Root"/>.</summary>
        public RectTransform Board { get; }

        /// <summary>The paper area below the title, stretched; lay rows out from its top.</summary>
        public RectTransform Content { get; }

        public Text Title { get; }

        public bool IsOpen => _backdrop.IsShown;

        /// <summary>The backdrop fades in and the board pops; closing fades both out.</summary>
        public void Show()
        {
            Root.SetAsLastSibling();
            _backdrop.Show();
            _board.Show();
        }

        public void Hide() => _backdrop.Hide();

        private void AddClip(ArtLibrary art, float x, float topY)
        {
            var clip = UiFactory.Rect("Clip", Board, new Vector2(0.5f, 0.5f), new Vector2(x, topY), ClipSize);
            UiFactory.AddBox(clip, art.BevelBox, Palette.Clip);
        }

        private void AddCloseButton(ArtLibrary art, Vector2 size, Action onClose)
        {
            var rect = UiFactory.Rect("Close", Board, new Vector2(0.5f, 0.5f), new Vector2(size.x * 0.5f - 80f, size.y * 0.5f - 80f), Vector2.one * CloseSize);
            var cross = UiFactory.AddLabel(rect, art.Font, 110, TextAnchor.MiddleCenter, Palette.CloseRed);
            cross.text = Texts.Close;
            cross.raycastTarget = true;
            UiFactory.AddButton(rect, onClose);
        }
    }
}
