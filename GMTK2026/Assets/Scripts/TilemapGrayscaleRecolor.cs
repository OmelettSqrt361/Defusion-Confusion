using UnityEngine;
using UnityEngine.Tilemaps;

[ExecuteAlways]
[RequireComponent(typeof(TilemapRenderer))]
public class TilemapGrayscaleRecolor : MonoBehaviour
{
    [System.Serializable]
    public struct ColorPair
    {
        public Color sourceColor;
        public Color targetColor;
        [Range(0f, 0.5f)] public float tolerance; // Useful for slight compression/color differences
    }

    [Header("Color Mappings (Max 16)")]
    public ColorPair[] colorMappings = new ColorPair[]
    {
        new ColorPair { sourceColor = Color.red, targetColor = Color.blue, tolerance = 0.05f }
    };

    private MaterialPropertyBlock mpb;
    private TilemapRenderer tr;

    // Fixed array buffers matching shader capacity
    private Vector4[] sourceColorsBuffer = new Vector4[16];
    private Vector4[] targetColorsBuffer = new Vector4[16];
    private float[] tolerancesBuffer = new float[16];

    void OnEnable()
    {
        Apply();
    }

    void OnValidate()
    {
        Apply();
    }

    void Apply()
    {
        if (tr == null) tr = GetComponent<TilemapRenderer>();
        if (tr == null || colorMappings == null) return;

        if (mpb == null) mpb = new MaterialPropertyBlock();

        int count = Mathf.Min(colorMappings.Length, 16);

        for (int i = 0; i < count; i++)
        {
            sourceColorsBuffer[i] = colorMappings[i].sourceColor;
            targetColorsBuffer[i] = colorMappings[i].targetColor;
            tolerancesBuffer[i] = colorMappings[i].tolerance;
        }

        tr.GetPropertyBlock(mpb);
        mpb.SetInt("_MappingCount", count);
        mpb.SetVectorArray("_SourceColors", sourceColorsBuffer);
        mpb.SetVectorArray("_TargetColors", targetColorsBuffer);
        mpb.SetFloatArray("_Tolerances", tolerancesBuffer);
        tr.SetPropertyBlock(mpb);
    }
}