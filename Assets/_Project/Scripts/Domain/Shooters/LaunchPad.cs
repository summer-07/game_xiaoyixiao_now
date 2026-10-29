using System;

namespace SheepStrike.Domain.Shooters
{
    /// <summary>
    /// The pen: launch slots where sheep fire automatically. The slots open from the start form the main row;
    /// slots unlocked later sit above the row's ends, alternating left and right. A sheep walks into the free
    /// slot nearest to the queue column it left from.
    /// </summary>
    public sealed class LaunchPad
    {
        /// <summary>The queue waits one row below the pen; this is how far that is in slot spacings.</summary>
        private const double QueueRow = -1.0;

        private readonly Sheep[] _slots;
        private readonly int _rowLength;

        public LaunchPad(int capacity, int maxCapacity)
        {
            if (capacity <= 0 || capacity > maxCapacity) throw new ArgumentOutOfRangeException(nameof(capacity));
            _slots = new Sheep[maxCapacity];
            Capacity = capacity;
            _rowLength = capacity;
        }

        public int Capacity { get; private set; }

        public int MaxCapacity => _slots.Length;

        public bool IsFull => FirstFreeSlot() < 0;

        public bool IsEmpty
        {
            get
            {
                for (int i = 0; i < Capacity; i++)
                {
                    if (_slots[i] != null) return false;
                }
                return true;
            }
        }

        public Sheep At(int slot) => slot >= 0 && slot < Capacity ? _slots[slot] : null;

        public PenPosition PositionOf(int slot)
        {
            if (slot < _rowLength) return new PenPosition(slot, 0);
            int extra = slot - _rowLength;
            return new PenPosition(extra % 2 == 0 ? 0 : _rowLength - 1, 1 + extra / 2);
        }

        /// <summary>Puts the sheep in the free slot closest to its queue column (ties go to the lower slot).</summary>
        public bool TryPlaceNearest(Sheep sheep, int column, int columnCount, out int slot)
        {
            double fromX = ColumnX(column, columnCount);
            slot = -1;
            double best = double.MaxValue;
            for (int i = 0; i < Capacity; i++)
            {
                if (_slots[i] != null) continue;
                var position = PositionOf(i);
                double dx = position.X - fromX, dy = position.Row - QueueRow;
                double distance = dx * dx + dy * dy;
                if (distance < best)
                {
                    best = distance;
                    slot = i;
                }
            }
            if (slot < 0) return false;
            _slots[slot] = sheep;
            return true;
        }

        /// <summary>Queue columns are spread evenly under the main row.</summary>
        private double ColumnX(int column, int columnCount) =>
            columnCount <= 1 ? (_rowLength - 1) * 0.5 : column * (double)(_rowLength - 1) / (columnCount - 1);

        public Sheep Release(int slot)
        {
            var sheep = _slots[slot];
            _slots[slot] = null;
            return sheep;
        }

        public bool TryExpand()
        {
            if (Capacity >= MaxCapacity) return false;
            Capacity++;
            return true;
        }

        private int FirstFreeSlot()
        {
            for (int i = 0; i < Capacity; i++)
            {
                if (_slots[i] == null) return i;
            }
            return -1;
        }
    }
}
