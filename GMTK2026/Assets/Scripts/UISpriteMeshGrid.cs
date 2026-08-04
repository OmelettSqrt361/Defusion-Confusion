using UnityEngine;
using UnityEngine.UI;

// UI equivalent of SpriteMeshGrid. Instead of building a MeshFilter/MeshRenderer
// mesh in world space, this subclasses Image and overrides OnPopulateMesh so the
// subdivided grid becomes the UI mesh that the CanvasRenderer draws. This keeps
// all the normal Image behavior (sprite assignment, color, raycasting, material,
// masking) and just replaces the single quad with a subdivided grid.

[AddComponentMenu("UI/Sprite Mesh Grid (Wobble)")]
public class UISpriteMeshGrid : Image
{
    [Range(2, 64)] public int gridColumns = 8;
    [Range(2, 64)] public int gridRows = 8;

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        if (sprite == null)
        {
            // Fall back to Image's normal quad/sliced/tiled/filled behavior
            base.OnPopulateMesh(vh);
            return;
        }

        vh.Clear();

        int cols = Mathf.Max(2, gridColumns);
        int rows = Mathf.Max(2, gridRows);

        // Local rect (already accounts for RectTransform size, pivot, and
        // pixel-perfect adjustment) — this replaces the sprite-PPU sizing math
        // from the world-space version, since UI sizing comes from the
        // RectTransform, not the sprite's pixels-per-unit.
        Rect rect = GetPixelAdjustedRect();

        Vector2[] spriteUVs = sprite.uv;
        Vector2 uvMin = spriteUVs[0];
        Vector2 uvMax = spriteUVs[0];
        foreach (var uv in spriteUVs)
        {
            uvMin = Vector2.Min(uvMin, uv);
            uvMax = Vector2.Max(uvMax, uv);
        }

        Color32 color32 = color;

        for (int y = 0; y < rows; y++)
        {
            float v = (float)y / (rows - 1);
            float py = Mathf.Lerp(rect.yMin, rect.yMax, v);

            for (int x = 0; x < cols; x++)
            {
                float u = (float)x / (cols - 1);
                float px = Mathf.Lerp(rect.xMin, rect.xMax, u);

                UIVertex vert = UIVertex.simpleVert;
                vert.color = color32;
                vert.position = new Vector3(px, py, 0f);
                vert.uv0 = new Vector2(
                    Mathf.Lerp(uvMin.x, uvMax.x, u),
                    Mathf.Lerp(uvMin.y, uvMax.y, v));

                vh.AddVert(vert);
            }
        }

        for (int y = 0; y < rows - 1; y++)
        {
            for (int x = 0; x < cols - 1; x++)
            {
                int i0 = y * cols + x;
                int i1 = i0 + 1;
                int i2 = i0 + cols;
                int i3 = i2 + 1;

                vh.AddTriangle(i0, i2, i1);
                vh.AddTriangle(i1, i2, i3);
            }
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        SetVerticesDirty();
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        SetVerticesDirty();
    }

    // Delay component swap by 1 frame so Unity finishes adding this script
    protected override void Reset()
    {
        base.Reset();
        UnityEditor.EditorApplication.delayCall += ConvertFromImage;
    }

    private void ConvertFromImage()
    {
        if (this == null || gameObject == null) return;

        // Look for a plain Image component on the same object to convert
        // from. GetComponents<Image>() also returns this component (since
        // UISpriteMeshGrid is itself an Image), so filter for the exact base
        // type and skip ourselves.
        Image source = null;
        foreach (var img in GetComponents<Image>())
        {
            if (img != this && img.GetType() == typeof(Image))
            {
                source = img;
                break;
            }
        }

        if (source == null) return;

        UnityEditor.Undo.RegisterCompleteObjectUndo(gameObject, "Auto Convert Image");

        // 1. Copy settings across
        sprite = source.sprite;
        color = source.color;
        material = source.material;
        raycastTarget = source.raycastTarget;
        maskable = source.maskable;

        // 2. Safely destroy the plain Image now that its settings are copied
        UnityEditor.Undo.DestroyObjectImmediate(source);

        SetVerticesDirty();
        UnityEditor.EditorUtility.SetDirty(gameObject);
    }
#endif
}