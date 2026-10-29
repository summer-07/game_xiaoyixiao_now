namespace SheepStrike.Domain.Session
{
    public enum SessionState
    {
        Playing,
        /// <summary>Every slot is taken and no sheep on the pad can reach its colour. A booster can rescue the run.</summary>
        Stuck,
        Won,
        Lost
    }
}
