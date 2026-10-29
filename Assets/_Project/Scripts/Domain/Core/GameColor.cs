namespace SheepStrike.Domain.Core
{
    /// <summary>
    /// Logical colour shared by voxels and sheep. A sheep can only hit voxels of its own colour.
    /// Concrete RGB values belong to the presentation layer.
    /// </summary>
    public enum GameColor
    {
        White,
        Grey,
        Black,
        Orange,
        Brown,
        Tan,
        Pink,
        Yellow,
        Red,
        Cream
    }
}
