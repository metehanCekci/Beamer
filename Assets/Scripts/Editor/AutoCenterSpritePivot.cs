using UnityEngine;
using UnityEditor;

/// <summary>
/// Tüm sprite'ları import edildiğinde otomatik olarak pivot noktasını center yapar.
/// Bu script Editor klasöründe olmalıdır.
/// </summary>
public class AutoCenterSpritePivot : AssetPostprocessor
{
    void OnPostprocessTexture(Texture2D texture)
    {
        // Sadece sprite'lar için çalış
        TextureImporter importer = assetImporter as TextureImporter;
        
        if (importer != null && importer.textureType == TextureImporterType.Sprite)
        {
            // Sprite Mode kontrolü
            if (importer.spriteImportMode == SpriteImportMode.Single)
            {
                // Tek sprite için pivot'u center yap
                TextureImporterSettings settings = new TextureImporterSettings();
                importer.ReadTextureSettings(settings);
                settings.spriteAlignment = (int)SpriteAlignment.Center;
                importer.SetTextureSettings(settings);
            }
            else if (importer.spriteImportMode == SpriteImportMode.Multiple)
            {
                // Multiple sprite (sprite sheet) için her bir sprite'ın pivot'unu center yap
                TextureImporterSettings settings = new TextureImporterSettings();
                importer.ReadTextureSettings(settings);
                settings.spriteAlignment = (int)SpriteAlignment.Center;
                importer.SetTextureSettings(settings);
                
                // Her bir sub-sprite için de pivot'u center yap
                SpriteMetaData[] spritesheet = importer.spritesheet;
                for (int i = 0; i < spritesheet.Length; i++)
                {
                    spritesheet[i].alignment = (int)SpriteAlignment.Center;
                    spritesheet[i].pivot = new Vector2(0.5f, 0.5f);
                }
                importer.spritesheet = spritesheet;
            }
            
            // Pixel Perfect ayarları (opsiyonel ama önerilen)
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
        }
    }
}
