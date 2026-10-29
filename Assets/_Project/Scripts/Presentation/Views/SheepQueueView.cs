using System;
using SheepStrike.Domain.Shooters;
using SheepStrike.Presentation.Theme;
using UnityEngine;
using UnityEngine.UI;

namespace SheepStrike.Presentation.Views
{
    /// <summary>The waiting flock as a grid. Row 0 (top) is the front of each column.</summary>
    public sealed class SheepQueueView
    {
        public const int VisibleRows = 3;
        private const float CellWidth = 180f;
        private const float CellHeight = 165f;
        private const float ColumnStep = 205f;
        private const float RowStep = 180f;
        private const float BackRowAlpha = 0.55f;

        private sealed class Cell
        {
            public RectTransform Rect;
            public Image Sheep;
            public Text Ammo;
        }

        private readonly RectTransform _root;
        private readonly ArtLibrary _art;
        private readonly Action<int, int> _onTap;
        private Cell[,] _cells = new Cell[0, 0];
        private Text[] _hiddenCounts = new Text[0];

        public SheepQueueView(Transform parent, float bottomOffset, ArtLibrary art, Action<int, int> onTap)
        {
            _art = art;
            _onTap = onTap;
            float height = RowStep * VisibleRows + 40f;
            _root = UiFactory.Rect("SheepQueue", parent, new Vector2(0.5f, 0f), new Vector2(0f, bottomOffset + height * 0.5f), new Vector2(1080f, height));
        }

        public void Build(int columns)
        {
            for (int i = _root.childCount - 1; i >= 0; i--) UnityEngine.Object.Destroy(_root.GetChild(i).gameObject);

            _cells = new Cell[columns, VisibleRows];
            _hiddenCounts = new Text[columns];
            float startX = -(columns - 1) * ColumnStep * 0.5f;
            float topY = _root.sizeDelta.y * 0.5f - CellHeight * 0.5f;

            for (int c = 0; c < columns; c++)
            {
                for (int r = 0; r < VisibleRows; r++)
                {
                    _cells[c, r] = CreateCell(c, r, new Vector2(startX + c * ColumnStep, topY - r * RowStep));
                }
                var countRect = UiFactory.Rect("Hidden" + c, _root, new Vector2(0.5f, 0.5f), new Vector2(startX + c * ColumnStep, topY - VisibleRows * RowStep + 70f), new Vector2(CellWidth, 40f));
                _hiddenCounts[c] = UiFactory.AddLabel(countRect, _art.Font, 30, TextAnchor.MiddleCenter);
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

                    var color = Palette.Of(sheep.Color);
                    bool reachable = r == 0 || anyRowReachable;
                    color.a = reachable ? 1f : BackRowAlpha;
                    cell.Sheep.color = color;
                    cell.Ammo.text = sheep.Ammo.ToString();
                    cell.Rect.localScale = Vector3.one * (r == 0 ? 1f : 0.92f);
                }

                int hidden = queue.RowCount(c) - VisibleRows;
                _hiddenCounts[c].text = hidden > 0 ? "+" + hidden : string.Empty;
            }
        }

        private Cell CreateCell(int column, int row, Vector2 position)
        {
            var rect = UiFactory.Rect($"Sheep{column}_{row}", _root, new Vector2(0.5f, 0.5f), position, new Vector2(CellWidth, CellHeight));
            var sheep = UiFactory.AddImage(rect, _art.SheepIcon, Color.white, true);
            var ammo = UiFactory.AddLabel(UiFactory.Stretch("Ammo", rect), _art.Font, 58, TextAnchor.MiddleCenter);
            UiFactory.AddButton(rect, () => _onTap(column, row));
            return new Cell { Rect = rect, Sheep = sheep, Ammo = ammo };
        }
    }
}
