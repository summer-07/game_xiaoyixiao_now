using UnityEngine;

namespace SheepStrike.Presentation.Theme
{
    /// <summary>
    /// White sprites drawn in code and tinted at runtime: a 9-sliced rounded box with a darker bottom lip
    /// (the chunky "toy button" look), a flat rounded box, a disc and a ring. No texture assets needed.
    /// </summary>
    public static class ShapeSprites
    {
        private const int BoxSize = 96;
        private const int BoxRadius = 30;
        private const int LipHeight = 10;
        private const float LipShade = 0.72f;
        private const float GlossShade = 1f;
        private const float BodyShade = 0.94f;
        private const int DiscSize = 128;
        private const float RingThickness = 0.16f;

        public static Sprite BevelBox() => CreateBox(true);

        public static Sprite FlatBox() => CreateBox(false);

        public static Sprite Disc() => CreateRound(0f);

        public static Sprite Ring() => CreateRound(1f - RingThickness * 2f);

        private static Sprite CreateBox(bool bevel)
        {
            var texture = NewTexture(BoxSize);
            for (int y = 0; y < BoxSize; y++)
            {
                for (int x = 0; x < BoxSize; x++)
                {
                    float alpha = RoundedBoxCoverage(x + 0.5f, y + 0.5f);
                    float shade = bevel ? BevelShade(y) : 1f;
                    texture.SetPixel(x, y, new Color(shade, shade, shade, alpha));
                }
            }
            texture.Apply();
            int border = BoxRadius + (bevel ? LipHeight : 0);
            return Sprite.Create(texture, new Rect(0, 0, BoxSize, BoxSize), new Vector2(0.5f, 0.5f), 100f, 0,
                SpriteMeshType.FullRect, new Vector4(BoxRadius, border, BoxRadius, BoxRadius));
        }

        private static float BevelShade(int y)
        {
            if (y < LipHeight) return LipShade;
            return y > BoxSize - BoxRadius ? GlossShade : BodyShade;
        }

        private static float RoundedBoxCoverage(float x, float y)
        {
            float cx = Mathf.Clamp(x, BoxRadius, BoxSize - BoxRadius);
            float cy = Mathf.Clamp(y, BoxRadius, BoxSize - BoxRadius);
            float distance = Vector2.Distance(new Vector2(x, y), new Vector2(cx, cy));
            return Mathf.Clamp01(BoxRadius - distance + 0.5f);
        }

        /// <param name="innerFraction">0 for a filled disc, otherwise the hole's radius as a fraction of the outer radius.</param>
        private static Sprite CreateRound(float innerFraction)
        {
            var texture = NewTexture(DiscSize);
            float outer = DiscSize * 0.5f;
            float inner = outer * innerFraction;
            for (int y = 0; y < DiscSize; y++)
            {
                for (int x = 0; x < DiscSize; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(outer, outer));
                    float alpha = Mathf.Clamp01(outer - distance) * (innerFraction > 0f ? Mathf.Clamp01(distance - inner) : 1f);
                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, DiscSize, DiscSize), new Vector2(0.5f, 0.5f));
        }

        private static Texture2D NewTexture(int size) =>
            new Texture2D(size, size, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
    }
}
