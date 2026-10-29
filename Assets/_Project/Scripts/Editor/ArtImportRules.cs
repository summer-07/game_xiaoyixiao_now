using UnityEditor;

namespace SheepStrike.EditorTools
{
    /// <summary>Everything under Resources/Art is imported as a compressed UI sprite sized for mini-game builds.</summary>
    public sealed class ArtImportRules : AssetPostprocessor
    {
        private const string ArtFolder = "Assets/_Project/Resources/Art/";

        private void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith(ArtFolder)) return;

            var importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.maxTextureSize = assetPath.Contains("/Backgrounds/") ? 1024 : 256;
        }
    }
}
