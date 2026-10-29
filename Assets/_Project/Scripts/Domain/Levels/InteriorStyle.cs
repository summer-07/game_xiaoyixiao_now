namespace SheepStrike.Domain.Levels
{
    /// <summary>How a level paints the hidden inside of its animal. Harder levels add more colours and more noise.</summary>
    public sealed class InteriorStyle
    {
        public static readonly InteriorStyle Untouched = new InteriorStyle(0, 0, 1, 0f);

        /// <param name="reusedColors">Surface colours (after the dominant one) reused inside.</param>
        /// <param name="extraColors">Colours that never appear on the surface, only inside.</param>
        /// <param name="shellThickness">Voxel layers per colour band.</param>
        /// <param name="noise">0..1 chance that a voxel ignores its band and takes a random interior colour.</param>
        public InteriorStyle(int reusedColors, int extraColors, int shellThickness, float noise)
        {
            ReusedColors = reusedColors;
            ExtraColors = extraColors;
            ShellThickness = shellThickness < 1 ? 1 : shellThickness;
            Noise = noise < 0f ? 0f : noise > 1f ? 1f : noise;
        }

        public int ReusedColors { get; }
        public int ExtraColors { get; }
        public int ShellThickness { get; }
        public float Noise { get; }

        public bool IsUntouched => ReusedColors <= 0 && ExtraColors <= 0;
    }
}
