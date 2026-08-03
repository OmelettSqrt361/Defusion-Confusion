#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

// Select one or more AnimationClip assets that swap sprites on a
// SpriteRenderer (i.e. they have an object-reference curve bound to
// SpriteRenderer.sprite) and run this to create copies that drive
// SpriteMeshGrid.sprite instead. New clips are saved alongside the
// originals as "<clipname>_MeshGrid.anim" — originals are untouched.
public static class SpriteMeshGridAnimationConverter
{
    [MenuItem("Tools/Sprite Mesh Grid/Convert Selected Clips To Mesh Grid")]
    private static void ConvertSelectedClips()
    {
        var clips = Selection.GetFiltered<AnimationClip>(SelectionMode.Assets);
        if (clips.Length == 0)
        {
            Debug.LogWarning("Select one or more AnimationClip assets in the Project window first.");
            return;
        }

        int converted = 0;
        foreach (var clip in clips)
        {
            if (ConvertClip(clip)) converted++;
        }

        if (converted > 0)
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        Debug.Log($"Converted {converted}/{clips.Length} clip(s).");
    }

    private static bool ConvertClip(AnimationClip sourceClip)
    {
        var objectBindings = AnimationUtility.GetObjectReferenceCurveBindings(sourceClip);
        bool hasSpriteCurve = false;
        foreach (var b in objectBindings)
        {
            if (b.type == typeof(SpriteRenderer) && b.propertyName == "m_Sprite")
            {
                hasSpriteCurve = true;
                break;
            }
        }

        if (!hasSpriteCurve)
        {
            Debug.Log($"'{sourceClip.name}' has no SpriteRenderer.sprite curve — skipped.");
            return false;
        }

        var newClip = Object.Instantiate(sourceClip);
        newClip.name = sourceClip.name + "_MeshGrid";

        foreach (var binding in objectBindings)
        {
            if (binding.type != typeof(SpriteRenderer) || binding.propertyName != "m_Sprite")
                continue;

            var keyframes = AnimationUtility.GetObjectReferenceCurve(sourceClip, binding);

            // Remove the copy Instantiate carried over under the old binding,
            // then add it back bound to SpriteMeshGrid.sprite instead.
            AnimationUtility.SetObjectReferenceCurve(newClip, binding, null);

            var newBinding = binding;
            newBinding.type = typeof(SpriteMeshGrid);
            newBinding.propertyName = "sprite";
            AnimationUtility.SetObjectReferenceCurve(newClip, newBinding, keyframes);
        }

        string sourcePath = AssetDatabase.GetAssetPath(sourceClip);
        string dir = Path.GetDirectoryName(sourcePath);
        string newPath = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(dir, newClip.name + ".anim"));

        AssetDatabase.CreateAsset(newClip, newPath);
        Debug.Log($"Created '{newPath}' from '{sourceClip.name}'.");
        return true;
    }
}
#endif