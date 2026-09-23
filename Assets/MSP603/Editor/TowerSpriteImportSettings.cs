using UnityEditor;
using UnityEngine;

namespace MSP603.TowerDefense.Editor
{
    public sealed class TowerSpriteImportSettings : AssetPostprocessor
    {
        private void OnPreprocessTexture()
        {
            if (assetPath.StartsWith("Assets/MSP603/Resources/FantasyUI/"))
            {
                TextureImporter ui = (TextureImporter)assetImporter;
                ui.textureType = TextureImporterType.Sprite;
                ui.spriteImportMode = SpriteImportMode.Single;
                ui.spritePixelsPerUnit = 100f;
                ui.alphaIsTransparency = true;
                ui.mipmapEnabled = false;
                ui.filterMode = FilterMode.Bilinear;
                ui.maxTextureSize = 2048;
                ui.textureCompression = TextureImporterCompression.Compressed;
                return;
            }
            if (!assetPath.StartsWith("Assets/MSP603/Resources/Towers/"))
            {
                return;
            }

            TextureImporter importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 256f;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Bilinear;
            importer.maxTextureSize = 512;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
        }
    }
}
