namespace SheepStrike.Domain.Core
{
    /// <summary>Randomness is injected so that every rule stays deterministic under test.</summary>
    public interface IRandom
    {
        /// <returns>A value in [0, maxExclusive).</returns>
        int Next(int maxExclusive);
    }
}
