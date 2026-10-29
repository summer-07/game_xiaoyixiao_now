namespace SheepStrike.Presentation.Theme
{
    /// <summary>
    /// Purely cosmetic combo milestones. Each tier makes the combo counter, debris and sound louder;
    /// crossing a threshold shows a banner and a confetti burst.
    /// </summary>
    public static class ComboTier
    {
        private static readonly int[] Thresholds = { 10, 30, 60, 100 };

        public static int Count => Thresholds.Length;

        /// <returns>0 below the first threshold, otherwise 1..Count.</returns>
        public static int Of(int combo)
        {
            int tier = 0;
            while (tier < Thresholds.Length && combo >= Thresholds[tier]) tier++;
            return tier;
        }
    }
}
