using SheepStrike.Domain.Core;

namespace SheepStrike.Domain.Session
{
    /// <summary>Facts produced by <see cref="GameSession"/>; the presentation layer drains and animates them.</summary>
    public abstract class GameEvent
    {
    }

    public sealed class SheepDeployed : GameEvent
    {
        public SheepDeployed(int sheepId, int column, int row, int slot)
        {
            SheepId = sheepId;
            Column = column;
            Row = row;
            Slot = slot;
        }

        public int SheepId { get; }
        public int Column { get; }
        public int Row { get; }
        public int Slot { get; }
    }

    public sealed class WoolFired : GameEvent
    {
        public WoolFired(int sheepId, int slot, VoxelPos target, GameColor color, int ammoLeft)
        {
            SheepId = sheepId;
            Slot = slot;
            Target = target;
            Color = color;
            AmmoLeft = ammoLeft;
        }

        public int SheepId { get; }
        public int Slot { get; }
        public VoxelPos Target { get; }
        public GameColor Color { get; }
        public int AmmoLeft { get; }
    }

    public sealed class SheepRetired : GameEvent
    {
        public SheepRetired(int sheepId, int slot)
        {
            SheepId = sheepId;
            Slot = slot;
        }

        public int SheepId { get; }
        public int Slot { get; }
    }

    public sealed class DeployRejected : GameEvent
    {
        public DeployRejected(int column, int row)
        {
            Column = column;
            Row = row;
        }

        public int Column { get; }
        public int Row { get; }
    }

    public sealed class ComboChanged : GameEvent
    {
        public ComboChanged(int combo) => Combo = combo;

        public int Combo { get; }
    }

    public sealed class BoosterApplied : GameEvent
    {
        public BoosterApplied(BoosterType booster) => Booster = booster;

        public BoosterType Booster { get; }
    }

    public sealed class StateChanged : GameEvent
    {
        public StateChanged(SessionState state) => State = state;

        public SessionState State { get; }
    }
}
