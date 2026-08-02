using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore;

public class NudgeSpriteGlyphs : EditorWindow
{
    float nudgeX = 0f;
    float nudgeY = 0f;

    [MenuItem("Tools/Font/Nudge Glyph Offset")]
    static void ShowWindow()
    {
        var window = GetWindow<NudgeSpriteGlyphs>(true, "Nudge Glyph Offset");
        window.minSize = new Vector2(300, 130);
    }

    void OnGUI()
    {
        GUILayout.Label("Select a TMP Sprite Asset in the Project window, then set the amounts to nudge.", EditorStyles.wordWrappedLabel);
        GUILayout.Space(8);

        nudgeX = EditorGUILayout.FloatField("Nudge X (px)", nudgeX);
        nudgeY = EditorGUILayout.FloatField("Nudge Y (px)", nudgeY);

        EditorGUILayout.HelpBox(
            "X: positive moves glyphs RIGHT, negative moves LEFT.\n" +
            "Y: positive moves glyphs UP, negative moves DOWN.\n" +
            "Applies to every glyph in the selected asset. Additive each time you click.",
            MessageType.Info);

        GUILayout.Space(8);

        TMP_SpriteAsset selected = Selection.activeObject as TMP_SpriteAsset;
        using (new EditorGUI.DisabledScope(selected == null))
        {
            if (GUILayout.Button($"Apply Nudge to: {(selected != null ? selected.name : "(none selected)")}"))
            {
                ApplyNudge(selected, nudgeX, nudgeY);
            }
        }
    }

    static void ApplyNudge(TMP_SpriteAsset spriteAsset, float x, float y)
    {
        if (spriteAsset == null || (x == 0f && y == 0f)) return;

        var table = spriteAsset.spriteCharacterTable;

        for (int i = 0; i < table.Count; i++)
        {
            var spriteChar = table[i];
            var glyph = spriteChar.glyph;
            var metrics = glyph.metrics;

            glyph.metrics = new GlyphMetrics(
                metrics.width,
                metrics.height,
                metrics.horizontalBearingX + x,
                metrics.horizontalBearingY + y,
                metrics.horizontalAdvance
            );

            spriteChar.glyph = glyph;
            table[i] = spriteChar;
        }

        EditorUtility.SetDirty(spriteAsset);
        AssetDatabase.SaveAssets();

        Debug.Log($"Nudged all glyphs in {spriteAsset.name} by ({x}, {y})px.");
    }
}