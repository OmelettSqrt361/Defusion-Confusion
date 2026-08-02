using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore;

public class SetTightAdvance
{
    [MenuItem("Tools/Font/Set Advance to Tight Width")]
    static void SetAdvance()
    {
        TMP_SpriteAsset spriteAsset = Selection.activeObject as TMP_SpriteAsset;
        if (spriteAsset == null)
        {
            Debug.LogError("Select a TMP Sprite Asset in the Project window first.");
            return;
        }

        Texture2D atlas = spriteAsset.spriteSheet as Texture2D;
        string path = AssetDatabase.GetAssetPath(atlas);
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        bool wasReadable = importer.isReadable;
        if (!wasReadable) { importer.isReadable = true; importer.SaveAndReimport(); }

        Color32[] pixels = atlas.GetPixels32();
        int atlasWidth = atlas.width;
        const byte alphaThreshold = 8;
        const float horizontalPadding = 2f; // extra px of space after each letter, tweak to taste

        var table = spriteAsset.spriteCharacterTable;

        for (int i = 0; i < table.Count; i++)
        {
            var spriteChar = table[i];
            var glyph = spriteChar.glyph;
            GlyphRect rect = glyph.glyphRect; // untouched — just reading pixels from it

            int minX = rect.width, maxX = -1;

            for (int y = 0; y < rect.height; y++)
            {
                for (int x = 0; x < rect.width; x++)
                {
                    int index = (rect.y + y) * atlasWidth + (rect.x + x);
                    if (index < 0 || index >= pixels.Length) continue;
                    if (pixels[index].a > alphaThreshold)
                    {
                        if (x < minX) minX = x;
                        if (x > maxX) maxX = x;
                    }
                }
            }

            if (maxX < minX)
            {
                // fully transparent glyph (space, etc) — give it a sane default advance
                continue;
            }

            float tightWidth = (maxX - minX + 1) + horizontalPadding;

            var metrics = glyph.metrics;
            glyph.metrics = new GlyphMetrics(
                metrics.width,           // unchanged — glyphRect/quad size stays as-is
                metrics.height,          // unchanged
                metrics.horizontalBearingX,
                metrics.horizontalBearingY,
                tightWidth               // <-- this is the only thing actually changing
            );

            spriteChar.glyph = glyph;
            table[i] = spriteChar;
        }

        EditorUtility.SetDirty(spriteAsset);
        AssetDatabase.SaveAssets();

        if (!wasReadable) { importer.isReadable = false; importer.SaveAndReimport(); }
        Debug.Log("Set tight advance values for " + spriteAsset.name);
    }
}