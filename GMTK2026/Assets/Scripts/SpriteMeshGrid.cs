using UnityEngine;
using UnityEngine.Serialization;

// Replaces a SpriteRenderer with a MeshFilter/MeshRenderer showing the same
// sprite, but built as a subdivided grid instead of a single quad.

[ExecuteAlways]
public class SpriteMeshGrid : MonoBehaviour
{
    // NOTE: keep this field named "sprite" — that's the serialized name
    // every existing scene/prefab already stores its reference under.
    // FormerlySerializedAs covers anything that got saved as "_sprite"
    // during a previous version of this script.
    [FormerlySerializedAs("_sprite")]
    [Tooltip("Sprite to build the grid mesh from")]
    public Sprite sprite;

    [Tooltip("Material using Sprites/Wobble or Sprites/PixelPerfectOutlineWobble")]
    public Material material;

    [Range(2, 64)] public int gridColumns = 8;
    [Range(2, 64)] public int gridRows = 8;

    [Tooltip("Matched to how a SpriteRenderer would sort against other sprites")]
    public string sortingLayerName = "Default";
    public int sortingOrder = 0;

    private Mesh _mesh;
    private Sprite _builtFor;
    private int _builtCols = -1;
    private int _builtRows = -1;
    private MeshRenderer _meshRenderer;

    public MeshRenderer RendererComponent
    {
        get
        {
            if (_meshRenderer == null)
            {
                _meshRenderer = GetComponent<MeshRenderer>();
            }
            return _meshRenderer;
        }
    }

    private void OnEnable()
    {
        EnsureMeshComponentsExist();
        Build();
    }

    private void OnValidate()
    {
    #if UNITY_EDITOR
        // EnsureMeshComponentsExist()/Build() can end up assignings
        // MeshFilter.sharedMesh, which triggers SendMessage under the hood.
        // That's illegal inside OnValidate, so push it to the next editor tick.
        UnityEditor.EditorApplication.delayCall += DeferredValidate;
    #else
        EnsureMeshComponentsExist();
        Build();
    #endif
    }

    #if UNITY_EDITOR
    private void DeferredValidate()
    {
        if (this == null) return; // object may have been destroyed/reloaded by then
        EnsureMeshComponentsExist();
        Build();
    }
    #endif

    private void LateUpdate()
    {
        // An Animator/Animation clip driving a Sprite curve writes straight
        // into the serialized `sprite` field via reflection — that bypasses
        // any C# property setter entirely, so polling is the only reliable
        // way to catch it. LateUpdate runs after the Animator has applied
        // this frame's curves. Build() itself is a cheap no-op when nothing
        // changed, but we check first to skip the call entirely most frames.
        if (sprite != null &&
            (sprite != _builtFor || _builtCols != gridColumns || _builtRows != gridRows))
        {
            Build();
        }
    }

    private void EnsureMeshComponentsExist()
    {
        // Add components dynamically if they don't exist yet
        if (GetComponent<MeshFilter>() == null)
        {
            gameObject.AddComponent<MeshFilter>();
        }

        if (_meshRenderer == null && GetComponent<MeshRenderer>() == null)
        {
            _meshRenderer = gameObject.AddComponent<MeshRenderer>();
        }
    }

    public void Build()
    {
        if (sprite == null) return;

        EnsureMeshComponentsExist();

        MeshFilter mf = GetComponent<MeshFilter>();
        if (_mesh == null)
        {
            _mesh = new Mesh { name = "SpriteGridMesh" };
            mf.sharedMesh = _mesh;
        }

        if (_builtFor == sprite && _builtCols == gridColumns && _builtRows == gridRows)
        {
            ApplyRendererSettings();
            return;
        }

        int cols = Mathf.Max(2, gridColumns);
        int rows = Mathf.Max(2, gridRows);

        float ppu = sprite.pixelsPerUnit;
        Rect rect = sprite.rect;
        Vector2 pivotPx = sprite.pivot;

        float width = rect.width / ppu;
        float height = rect.height / ppu;
        float pivotX = pivotPx.x / rect.width;
        float pivotY = pivotPx.y / rect.height;

        Vector2[] spriteUVs = sprite.uv;
        Vector2 uvMin = spriteUVs[0];
        Vector2 uvMax = spriteUVs[0];
        foreach (var uv in spriteUVs)
        {
            uvMin = Vector2.Min(uvMin, uv);
            uvMax = Vector2.Max(uvMax, uv);
        }

        var vertices = new Vector3[cols * rows];
        var uvs = new Vector2[cols * rows];

        for (int y = 0; y < rows; y++)
        {
            float v = (float)y / (rows - 1);
            for (int x = 0; x < cols; x++)
            {
                float u = (float)x / (cols - 1);
                int i = y * cols + x;

                vertices[i] = new Vector3((u - pivotX) * width, (v - pivotY) * height, 0f);
                uvs[i] = new Vector2(Mathf.Lerp(uvMin.x, uvMax.x, u), Mathf.Lerp(uvMin.y, uvMax.y, v));
            }
        }

        int quadCount = (cols - 1) * (rows - 1);
        var triangles = new int[quadCount * 6];
        int t = 0;
        for (int y = 0; y < rows - 1; y++)
        {
            for (int x = 0; x < cols - 1; x++)
            {
                int i0 = y * cols + x;
                int i1 = i0 + 1;
                int i2 = i0 + cols;
                int i3 = i2 + 1;

                triangles[t++] = i0; triangles[t++] = i2; triangles[t++] = i1;
                triangles[t++] = i1; triangles[t++] = i2; triangles[t++] = i3;
            }
        }

        _mesh.Clear();
        _mesh.vertices = vertices;
        _mesh.uv = uvs;
        _mesh.triangles = triangles;
        _mesh.RecalculateBounds();

        _builtFor = sprite;
        _builtCols = cols;
        _builtRows = rows;

        ApplyRendererSettings();
    }

    public void ApplyRendererSettings()
    {
        MeshRenderer mr = RendererComponent;
        if (mr == null) return;

        if (material != null && mr.sharedMaterial != material)
        {
            mr.sharedMaterial = material;
        }

        if (sprite != null && sprite.texture != null)
        {
            var mpb = new MaterialPropertyBlock();
            mr.GetPropertyBlock(mpb);
            mpb.SetTexture("_MainTex", sprite.texture);
            mr.SetPropertyBlock(mpb);
        }

        mr.sortingLayerName = sortingLayerName;
        mr.sortingOrder = sortingOrder;
    }

#if UNITY_EDITOR
    private void Reset()
    {
        // Delay component swap by 1 frame so Unity finishes adding this script
        UnityEditor.EditorApplication.delayCall += ConvertFromSpriteRenderer;
    }

    private void ConvertFromSpriteRenderer()
    {
        if (this == null || gameObject == null) return;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            UnityEditor.Undo.RegisterCompleteObjectUndo(gameObject, "Auto Convert SpriteRenderer");

            // 1. Store settings
            sprite = sr.sprite;
            material = sr.sharedMaterial;
            sortingLayerName = sr.sortingLayerName;
            sortingOrder = sr.sortingOrder;

            // 2. Safely destroy SpriteRenderer first
            UnityEditor.Undo.DestroyObjectImmediate(sr);

            // 3. Add Mesh components now that SpriteRenderer is gone
            EnsureMeshComponentsExist();

            // 4. Build grid
            Build();

            UnityEditor.EditorUtility.SetDirty(gameObject);
        }
    }
#endif
}