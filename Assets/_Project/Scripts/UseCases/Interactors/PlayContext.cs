using SheepStrike.Domain.Session;

namespace SheepStrike.UseCases.Interactors
{
    /// <summary>The attempt currently being played. Shared by the interactors of one run.</summary>
    public sealed class PlayContext
    {
        public GameSession Session { get; internal set; }

        public BoosterWallet Wallet { get; internal set; }

        public bool HasSession => Session != null;
    }
}
