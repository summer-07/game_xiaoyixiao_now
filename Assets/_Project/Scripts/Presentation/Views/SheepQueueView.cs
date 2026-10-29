using System;
using SheepStrike.Domain.Shooters;
using SheepStrike.Presentation.Theme;
using UnityEngine;
using UnityEngine.UI;

namespace SheepStrike.Presentation.Views
{
    /// <summary>
    /// The waiting flock as a grid. Row 0 (top) is the front of each column. Only the first rows are shown,
    /// and the player is deliberately not told how many sheep wait behind them.
    /// </summary>
    public sealed class SheepQueueView
    {
        public const int VisibleRows = SheepQueue.VisibleRows;
        private const float CellWidth = 180f;
        private const float CellHeight = 165f;
        /// <summary>Horizontal distance between queue columns; the pen uses it too so slots sit straight above columns.</summary>
        public const float ColumnStep = 205f;
        private const float RowStep = 180f;
        private const float BackRowScale = 0.92f;
        private const float FrontBreathe = 0.045f;
        private const float BreatheCycles = 1.1f;
        private const float FrontWiggleDegrees = 3f;

        private sealed class Cell
        {
            public RectTransform Rect;
            public Image Sheep;
            public Text Ammo;
            public ScaleAnimator Scale;
            public int SheepId = -1;
        }

        private readonly RectTransform _root;
        private readonly ArtLibrary _art;
        private readonly Action<int, int> _onTap;
        private Cell[,] _cells = new Cell[0, 0];

        public SheepQueueView(Transform parent, float bottomOffset, ArtLibrary art, Action<int, int> onTap)
        {
            _art = art;
            _onTap = onTap;
            float height = RowStep * VisibleRows;
            _root = UiFactory.Rect("SheepQueue", parent, new Vector2(0.5f, 0f), new Vector2(0f, bottomOffset + height * 0.5f), new Vector2(1080f, height));
        }

        public void Build(int columns)
        {
            for (int i = _root.childCount - 1; i >= 0; i--) UnityEngine.Object.Destroy(_root.GetChild(i).gameObject);

            _cells = new Cell[columns, VisibleRows];
            float startX = -(columns - 1) * ColumnStep * 0.5f;
            float topY = _root.sizeDelta.y * 0.5f - CellHeight * 0.5f;

            for (int c = 0; c < columns; c++)
            {
                for (int r = 0; r < VisibleRows; r++)
                {
                    _cells[c, r] = CreateCell(c, r, new Vector2(startX + c * ColumnStep, topY - r * RowStep));
                }
            }
        }

        public void Render(SheepQueue queue, bool anyRowReachable)
        {
            for (int c = 0; c < _cells.GetLength(0); c++)
            {
                for (int r = 0; r < VisibleRows; r++)
                {
                    var cell = _cells[c, r];
                    var sheep = queue.Peek(c, r);
                    cell.Rect.gameObject.SetActive(sheep != null);
                    if (sheep == null) continue;

                    bool reachable = r == 0 || anyRowReachable;
                    cell.Sheep.color = reachable ? Palette.Of(sheep.Color) : Palette.Of(sheep.Color) * Palette.QueueBackRow;
                    cell.Ammo.text = sheep.Ammo.ToString();
                    cell.Scale.BaseScale = r == 0 ? 1f : BackRowScale;
                    cell.Scale.Breathe(r == 0 ? FrontBreathe : 0f, BreatheCycles, c * 0.9f, r == 0 ? FrontWiggleDegrees : 0f);
                    if (cell.SheepId != sheep.Id) cell.Scale.PopIn();
                    cell.SheepId = sheep.Id;
                }
            }
        }

        public RectTransform Root => _root;

        public Vector2 ScreenPositionOf(int column, int row) =>
            RectTransformUtility.WorldToScreenPoint(null, _cells[column, row].Rect.position);

        public Vector2 CellSize => new Vector2(CellWidth, CellHeight);

        private Cell CreateCell(int column, int row, Vector2 position)
        {
            var rect = UiFactory.Rect($"Sheep{column}_{row}", _root, new Vector2(0.5f, 0.5f), position, new Vector2(CellWidth, CellHeight));
            var sheep = UiFactory.AddImage(rect, _art.SheepIcon, Color.white, true);
            var ammo = UiFactory.AddLabel(UiFactory.Stretch("Ammo", rect), _art.Font, 64, TextAnchor.MiddleCenter);
            var button = UiFactory.AddButton(rect, () => _onTap(column, row));
            return new Cell { Rect = rect, Sheep = sheep, Ammo = ammo, Scale = ScaleAnimator.On(button) };
        }
    }
}
