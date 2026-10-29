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
        private const int GearTeeth = 8;
        private const float GearBody = 0.34f;
        private const float GearToothLength = 0.12f;
        private const float GearHole = 0.13f;
        private const int SheepSize = 256;
        private const float SheepRim = 0.07f;
        private const float SheepRimShade = 0.5f;

        /// <summary>The sheep token as circles in a -1..1 box: (x, y, radius). Woolly top, round body, ears, a curl of tail.</summary>
        private static readonly Vector3[] SheepBlobs =
        {
            new Vector3(0f, -0.02f, 0.62f),
            new Vector3(-0.42f, 0.36f, 0.3f), new Vector3(0f, 0.48f, 0.32f), new Vector3(0.42f, 0.36f, 0.3f),
            new Vector3(-0.58f, 0.02f, 0.28f), new Vector3(0.58f, 0.02f, 0.28f),
            new Vector3(-0.34f, -0.44f, 0.3f), new Vector3(0.34f, -0.44f, 0.3f),
            new Vector3(-0.74f, 0.42f, 0.13f), new Vector3(0.74f, 0.42f, 0.13f),
            new Vector3(0.5f, -0.74f, 0.13f)
        };

        public static Sprite BevelBox() => CreateBox(true);

        public static Sprite FlatBox() => CreateBox(false);

        public static Sprite Disc() => CreateRound(0f);

        public static Sprite Ring() => CreateRound(1f - RingThickness * 2f);

        /// <summary>
        /// Flat, chunky sheep token tinted per colour at runtime, like the reference's sheep. The body is near-white
        /// with a soft top highlight; a built-in rim at half brightness becomes a darker outline of the same hue,
        /// so every colour reads clearly even on the purple background.
        /// </summary>
        public static Sprite SheepToken()
        {
            var texture = NewTexture(SheepSize);
            for (int y = 0; y < SheepSize; y++)
            {
                for (int x = 0; x < SheepSize; x++)
                {
                    float px = (x + 0.5f) / SheepSize * 2f - 1f, py = (y + 0.5f) / SheepSize * 2f - 1f;
                    float inside = SheepCoverage(px, py);
                    float alpha = Mathf.Clamp01(inside * SheepSize * 0.5f + 0.5f);
                    float rim = Mathf.Clamp01((SheepRim - inside) * SheepSize * 0.5f);
                    float body = Mathf.Lerp(0.86f, 1f, (py + 1f) * 0.5f);
                    float shade = Mathf.Lerp(body, SheepRimShade, rim);
                    texture.SetPixel(x, y, new Color(shade, shade, shade, alpha));
                }
            }
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, SheepSize, SheepSize), new Vector2(0.5f, 0.5f));
        }

        /// <summary>How far inside the nearest blob a point is (negative outside).</summary>
        private static float SheepCoverage(float x, float y)
        {
            float best = float.MinValue;
            foreach (var blob in SheepBlobs)
            {
                float distance = Mathf.Sqrt((x - blob.x) * (x - blob.x) + (y - blob.y) * (y - blob.y));
                best = Mathf.Max(best, blob.z - distance);
            }
            return best * 0.92f;
        }

        /// <summary>A cog for the settings button: a disc with square-ish teeth and a hole.</summary>
        public static Sprite Gear()
        {
            var texture = NewTexture(DiscSize);
            float half = DiscSize * 0.5f;
            for (int y = 0; y < DiscSize; y++)
            {
                for (int x = 0; x < DiscSize; x++)
                {
                    var offset = new Vector2(x + 0.5f - half, y + 0.5f - half) / DiscSize;
                    float angle = Mathf.Atan2(offset.y, offset.x);
                    float tooth = Mathf.Clamp01(Mathf.Cos(angle * GearTeeth) * 3f + 0.5f);
                    float outer = GearBody + GearToothLength * tooth;
                    float distance = offset.magnitude;
                    float alpha = Mathf.Clamp01((outer - distance) * DiscSize) * Mathf.Clamp01((distance - GearHole) * DiscSize);
                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, DiscSize, DiscSize), new Vector2(0.5f, 0.5f));
        }

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
