using System;

namespace SheepStrike.Domain.Shooters
{
    /// <summary>The row of launch slots. Sheep fire automatically while they stand on a slot.</summary>
    public sealed class LaunchPad
    {
        private readonly Sheep[] _slots;

        public LaunchPad(int capacity, int maxCapacity)
        {
            if (capacity <= 0 || capacity > maxCapacity) throw new ArgumentOutOfRangeException(nameof(capacity));
            _slots = new Sheep[maxCapacity];
            Capacity = capacity;
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

        public bool TryPlace(Sheep sheep, out int slot)
        {
            slot = FirstFreeSlot();
            if (slot < 0) return false;
            _slots[slot] = sheep;
            return true;
        }

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
