using SheepStrike.Presentation.Theme;
using UnityEngine;

namespace SheepStrike.Presentation.Views
{
    /// <summary>Flat purple sky with faint, blocky clouds that drift sideways and wrap around.</summary>
    public sealed class SkyView : MonoBehaviour
    {
        private const float CanvasWidth = 1080f;
        private const float Block = 46f;
        private const float WrapMargin = 400f;

        private static readonly Vector3[] Clouds =
        {
            // x, y (canvas units from centre), drift speed
            new Vector3(-320f, 700f, 14f), new Vector3(300f, 380f, 9f), new Vector3(-200f, 40f, 11f),
            new Vector3(380f, -300f, 7f), new Vector3(-420f, -620f, 12f)
        };

        /// <summary>Each cloud is a little pixel-art shape: rows of blocks, widest in the middle.</summary>
        private static readonly int[] CloudRows = { 3, 6, 8, 5 };

        private RectTransform[] _clouds;
        private float[] _speeds;

        public void Build(ArtLibrary art)
        {
            var root = (RectTransform)transform;
            _clouds = new RectTransform[Clouds.Length];
            _speeds = new float[Clouds.Length];
            for (int i = 0; i < Clouds.Length; i++)
            {
                _clouds[i] = BuildCloud(root, art, new Vector2(Clouds[i].x, Clouds[i].y), i);
                _speeds[i] = Clouds[i].z;
            }
        }

        private void Update()
        {
            if (_clouds == null) return;
            for (int i = 0; i < _clouds.Length; i++)
            {
                var position = _clouds[i].anchoredPosition;
                position.x += _speeds[i] * Time.deltaTime;
                if (position.x > CanvasWidth * 0.5f + WrapMargin) position.x = -CanvasWidth * 0.5f - WrapMargin;
                _clouds[i].anchoredPosition = position;
            }
        }

        private static RectTransform BuildCloud(RectTransform root, ArtLibrary art, Vector2 position, int seed)
        {
            var cloud = UiFactory.Rect("Cloud" + seed, root, new Vector2(0.5f, 0.5f), position, Vector2.zero);
            for (int row = 0; row < CloudRows.Length; row++)
            {
                int width = CloudRows[(row + seed) % CloudRows.Length];
                var strip = UiFactory.Rect("Row" + row, cloud, new Vector2(0.5f, 0.5f), new Vector2((seed % 2 == 0 ? 1 : -1) * row * Block * 0.5f, -row * Block), new Vector2(width * Block, Block));
                UiFactory.AddImage(strip, null, Palette.Cloud);
            }
            return cloud;
        }
    }
}
