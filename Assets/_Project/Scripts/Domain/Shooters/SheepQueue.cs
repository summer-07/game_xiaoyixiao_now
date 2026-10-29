using System;
using System.Collections.Generic;
using SheepStrike.Domain.Core;

namespace SheepStrike.Domain.Shooters
{
    /// <summary>
    /// The waiting sheep the player can see, arranged in columns of at most <see cref="VisibleRows"/>.
    /// Normally only the front sheep (row 0) of a column can be sent out. Sheep behind them don't exist yet:
    /// the session deals them from the wool reserve as rows free up, so what the player sees never changes.
    /// </summary>
    public sealed class SheepQueue
    {
        public const int VisibleRows = 2;

        private readonly List<List<Sheep>> _columns;

        public SheepQueue(IEnumerable<IEnumerable<Sheep>> columns)
        {
            _columns = new List<List<Sheep>>();
            foreach (var column in columns)
            {
                _columns.Add(new List<Sheep>(column));
            }
        }

        public int ColumnCount => _columns.Count;

        public int RowCount(int column) => _columns[column].Count;

        public bool IsEmpty
        {
            get
            {
                foreach (var column in _columns)
                {
                    if (column.Count > 0) return false;
                }
                return true;
            }
        }

        public Sheep Peek(int column, int row) => IsValid(column, row) ? _columns[column][row] : null;

        public bool IsValid(int column, int row) =>
            column >= 0 && column < _columns.Count && row >= 0 && row < _columns[column].Count;

        public bool HasRoom(int column) => _columns[column].Count < VisibleRows;

        /// <summary>Joins a newly dealt sheep at the back of a column.</summary>
        public void Append(int column, Sheep sheep)
        {
            if (!HasRoom(column)) throw new InvalidOperationException($"Column {column} already shows {VisibleRows} sheep.");
            _columns[column].Add(sheep);
        }

        public Sheep Take(int column, int row)
        {
            if (!IsValid(column, row)) return null;
            var sheep = _columns[column][row];
            _columns[column].RemoveAt(row);
            return sheep;
        }

        /// <summary>Fisher–Yates shuffle across all columns while keeping each column's length.</summary>
        public void Shuffle(IRandom random)
        {
            var all = new List<Sheep>();
            foreach (var column in _columns) all.AddRange(column);

            for (int i = all.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (all[i], all[j]) = (all[j], all[i]);
            }

            int index = 0;
            foreach (var column in _columns)
            {
                for (int row = 0; row < column.Count; row++)
                {
                    column[row] = all[index++];
                }
            }
        }
    }
}
