using System.Collections.Generic;
using SheepStrike.Domain.Core;
using SheepStrike.Domain.Levels;
using SheepStrike.Domain.Shooters;
using SheepStrike.Domain.Voxels;

namespace SheepStrike.Domain.Session
{
    /// <summary>
    /// One attempt at a level. Pure rules, no rendering and no wall-clock: the caller advances time with <see cref="Tick"/>
    /// and reads what happened through <see cref="DrainEvents"/>.
    /// </summary>
    public sealed class GameSession
    {
        public const float RamChargeDurationSeconds = 10f;
        public const float ComboWindowSeconds = 1.2f;

        /// <summary>A deployed sheep has to walk from the queue into the pen before it can fire.</summary>
        public const float DeployTravelSeconds = 0.4f;

        /// <summary>Extra time on an assisted attempt, on top of always dealing the colour the field needs.</summary>
        public const float AssistTimeFactor = 1.5f;

        private readonly IRandom _random;
        private readonly float[] _cooldowns;
        private readonly List<GameEvent> _events = new List<GameEvent>();
        private readonly FlockDealer _dealer;
        private int _nextSheepId;
        private float _ramChargeLeft;
        private float _sinceLastHit;

        /// <param name="assisted">Set after repeated failures: the dealer always helps and there is more time.</param>
        public GameSession(LevelDefinition level, IRandom random, bool assisted = false)
        {
            Level = level;
            _random = random;
            IsAssisted = assisted;
            Model = new VoxelModel(level.Voxels);
            Queue = new SheepQueue(CreateVisibleFlock(level));
            var reserve = new WoolReserve(UnseenWool(level));
            _dealer = new FlockDealer(reserve, assisted ? level.Dealing.WithHelpfulness(1f) : level.Dealing, random);
            Pad = new LaunchPad(level.StartingSlots, level.MaxSlots);
            _cooldowns = new float[level.MaxSlots];
            TimeLeft = level.TimeLimitSeconds * (assisted ? AssistTimeFactor : 1f);
            State = SessionState.Playing;
            for (int column = 0; column < Queue.ColumnCount; column++) RefillQueue(column);
        }

        public LevelDefinition Level { get; }
        public VoxelModel Model { get; }
        public SheepQueue Queue { get; }
        public LaunchPad Pad { get; }
        public SessionState State { get; private set; }
        public float TimeLeft { get; private set; }
        public int Combo { get; private set; }
        public int BestCombo { get; private set; }
        public bool IsShepherdDogActive { get; private set; }
        public bool IsAssisted { get; }
        public bool IsRamChargeActive => _ramChargeLeft > 0f;

        public IReadOnlyList<GameEvent> DrainEvents()
        {
            var drained = _events.ToArray();
            _events.Clear();
            return drained;
        }

        /// <summary>Where the player is looking from; sheep only shoot voxels visible from here.</summary>
        public void SetView(ViewDirection view) => Model.SetView(view);

        /// <summary>Tap on a sheep. Only the front row is allowed unless the shepherd dog is active.</summary>
        public bool TryDeploy(int column, int row)
        {
            if (State != SessionState.Playing) return false;

            bool reachable = row == 0 || IsShepherdDogActive;
            if (!reachable || !Queue.IsValid(column, row) || Pad.IsFull)
            {
                _events.Add(new DeployRejected(column, row));
                return false;
            }

            var sheep = Queue.Take(column, row);
            Pad.TryPlaceNearest(sheep, column, Queue.ColumnCount, out int slot);
            RefillQueue(column);
            _cooldowns[slot] = DeployTravelSeconds;
            if (row != 0) IsShepherdDogActive = false;
            _events.Add(new SheepDeployed(sheep.Id, column, row, slot));
            return true;
        }

        public bool CanApply(BoosterType booster)
        {
            switch (booster)
            {
                case BoosterType.RamCharge: return State == SessionState.Playing;
                case BoosterType.ExtraPen: return (State == SessionState.Playing || State == SessionState.Stuck) && Pad.Capacity < Pad.MaxCapacity;
                case BoosterType.ShepherdDog: return State == SessionState.Playing && !IsShepherdDogActive && !Queue.IsEmpty;
                case BoosterType.Shuffle: return State == SessionState.Playing && !Queue.IsEmpty;
                default: return false;
            }
        }

        public bool Apply(BoosterType booster)
        {
            if (!CanApply(booster)) return false;

            switch (booster)
            {
                case BoosterType.RamCharge:
                    _ramChargeLeft = RamChargeDurationSeconds;
                    break;
                case BoosterType.ExtraPen:
                    Pad.TryExpand();
                    if (State == SessionState.Stuck) ChangeState(SessionState.Playing);
                    break;
                case BoosterType.ShepherdDog:
                    IsShepherdDogActive = true;
                    break;
                case BoosterType.Shuffle:
                    Queue.Shuffle(_random);
                    break;
            }
            _events.Add(new BoosterApplied(booster));
            return true;
        }

        /// <summary>Deals unseen sheep into a column until it shows <see cref="SheepQueue.VisibleRows"/> again or the reserve runs dry.</summary>
        public void RefillQueue(int column)
        {
            while (Queue.HasRoom(column) && _dealer.TryDeal(FieldNeed, out var spec))
            {
                Queue.Append(column, new Sheep(_nextSheepId++, spec.Color, spec.Ammo));
            }
        }

        /// <summary>Exposed voxels of a colour minus the wool already lined up for it on the pad and in the visible queue.</summary>
        private int FieldNeed(GameColor color)
        {
            int need = Model.CountExposed(color);
            for (int slot = 0; slot < Pad.Capacity; slot++)
            {
                var sheep = Pad.At(slot);
                if (sheep != null && sheep.Color == color) need -= sheep.Ammo;
            }
            for (int column = 0; column < Queue.ColumnCount; column++)
            {
                for (int row = 0; row < Queue.RowCount(column); row++)
                {
                    var sheep = Queue.Peek(column, row);
                    if (sheep.Color == color) need -= sheep.Ammo;
                }
            }
            return need;
        }

        public void GiveUp()
        {
            if (State == SessionState.Playing || State == SessionState.Stuck) ChangeState(SessionState.Lost);
        }

        public void Tick(float deltaSeconds)
        {
            if (State != SessionState.Playing) return;

            TimeLeft -= deltaSeconds;
            if (_ramChargeLeft > 0f) _ramChargeLeft -= deltaSeconds;

            Model.RefreshVisibility();
            bool hitThisTick = FireAllSlots(deltaSeconds);
            UpdateCombo(deltaSeconds, hitThisTick);
            ResolveOutcome();
        }

        private bool FireAllSlots(float deltaSeconds)
        {
            float interval = Level.FireIntervalSeconds * (IsRamChargeActive ? 0.5f : 1f);
            bool hit = false;

            for (int slot = 0; slot < Pad.Capacity; slot++)
            {
                var sheep = Pad.At(slot);
                if (sheep == null) continue;

                _cooldowns[slot] -= deltaSeconds;
                while (_cooldowns[slot] <= 0f && !sheep.IsEmpty && Model.TryFindVisible(sheep.Color, _random, out var target))
                {
                    Model.Remove(target);
                    sheep.ConsumeOne();
                    _cooldowns[slot] += interval;
                    hit = true;
                    _events.Add(new WoolFired(sheep.Id, slot, target, sheep.Color, sheep.Ammo));
                }
                if (_cooldowns[slot] < 0f) _cooldowns[slot] = 0f;

                if (sheep.IsEmpty)
                {
                    Pad.Release(slot);
                    _events.Add(new SheepRetired(sheep.Id, slot));
                }
            }
            return hit;
        }

        private void UpdateCombo(float deltaSeconds, bool hitThisTick)
        {
            if (hitThisTick)
            {
                _sinceLastHit = 0f;
                Combo++;
                if (Combo > BestCombo) BestCombo = Combo;
                _events.Add(new ComboChanged(Combo));
                return;
            }

            _sinceLastHit += deltaSeconds;
            if (Combo > 0 && _sinceLastHit > ComboWindowSeconds)
            {
                Combo = 0;
                _events.Add(new ComboChanged(0));
            }
        }

        private void ResolveOutcome()
        {
            if (Model.IsCleared)
            {
                ChangeState(SessionState.Won);
            }
            else if (TimeLeft <= 0f)
            {
                TimeLeft = 0f;
                ChangeState(SessionState.Lost);
            }
            else if (Pad.IsFull && !AnySheepOnPadCanFire())
            {
                ChangeState(SessionState.Stuck);
            }
        }

        private bool AnySheepOnPadCanFire()
        {
            for (int slot = 0; slot < Pad.Capacity; slot++)
            {
                var sheep = Pad.At(slot);
                if (sheep != null && Model.HasExposed(sheep.Color)) return true;
            }
            return false;
        }

        private void ChangeState(SessionState next)
        {
            if (State == next) return;
            State = next;
            _events.Add(new StateChanged(next));
        }

        /// <summary>The first <see cref="SheepQueue.VisibleRows"/> sheep of every generated column, exactly as generated.</summary>
        private IEnumerable<IEnumerable<Sheep>> CreateVisibleFlock(LevelDefinition level)
        {
            var columns = new List<List<Sheep>>();
            foreach (var specColumn in level.SheepColumns)
            {
                var column = new List<Sheep>();
                for (int row = 0; row < specColumn.Count && row < SheepQueue.VisibleRows; row++)
                {
                    column.Add(new Sheep(_nextSheepId++, specColumn[row].Color, specColumn[row].Ammo));
                }
                columns.Add(column);
            }
            return columns;
        }

        /// <summary>Wool of every generated sheep the player can't see yet; it is dealt again during play.</summary>
        private static Dictionary<GameColor, int> UnseenWool(LevelDefinition level)
        {
            var wool = new Dictionary<GameColor, int>();
            foreach (var specColumn in level.SheepColumns)
            {
                for (int row = SheepQueue.VisibleRows; row < specColumn.Count; row++)
                {
                    wool.TryGetValue(specColumn[row].Color, out int ammo);
                    wool[specColumn[row].Color] = ammo + specColumn[row].Ammo;
                }
            }
            return wool;
        }
    }
}
