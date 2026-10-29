namespace SheepStrike.Domain.Levels
{
    /// <summary>How unseen sheep are dealt from the wool reserve while the level is played.</summary>
    public sealed class DealRules
    {
        public static readonly DealRules Default = new DealRules(10, 40, 0.5f);

        /// <param name="helpfulness">0..1 chance that a dealt sheep has the colour the field needs most; otherwise it is random.</param>
        public DealRules(int minAmmo, int maxAmmo, float helpfulness)
        {
            MinAmmo = minAmmo < 1 ? 1 : minAmmo;
            MaxAmmo = maxAmmo < MinAmmo ? MinAmmo : maxAmmo;
            Helpfulness = helpfulness < 0f ? 0f : helpfulness > 1f ? 1f : helpfulness;
        }

        public int MinAmmo { get; }
        public int MaxAmmo { get; }
        public float Helpfulness { get; }

        public DealRules WithHelpfulness(float helpfulness) => new DealRules(MinAmmo, MaxAmmo, helpfulness);
    }
}
