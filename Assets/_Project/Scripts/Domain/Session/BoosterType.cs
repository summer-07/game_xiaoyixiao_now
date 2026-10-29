namespace SheepStrike.Domain.Session
{
    public enum BoosterType
    {
        /// <summary>羊角冲锋: double fire rate for a while.</summary>
        RamCharge,
        /// <summary>扩羊圈: one more launch slot.</summary>
        ExtraPen,
        /// <summary>牧羊犬: the next tap may pick any sheep, not only the front one.</summary>
        ShepherdDog,
        /// <summary>赶羊群: shuffle the waiting flock.</summary>
        Shuffle
    }
}
