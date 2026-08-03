using System;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class SpriteOutlineManager : MonoBehaviour
{
    [Serializable]
    public struct OutlineTarget
    {
        public SpriteMeshGrid meshGrid;
        public Color outlineColor;

        public OutlineTarget(SpriteMeshGrid renderer, Color color)
        {
            this.meshGrid = renderer;
            this.outlineColor = color;
        }
    }

    [Header("Global Settings")]
    [Range(0, 20)]
    [SerializeField] private int globalThickness = 0;
    [SerializeField] private Material outlineMaterial;// Assign a material using Sprites/PixelPerfectOutlineWobble
    [SerializeField] private Material nooutlineMaterial;

    [Header("Wobble Settings")]
    [Tooltip("Sub-pixel offset in texels when a vertex moves. Keep small (0.2-0.6) for a subtle effect.")]
    [Range(0, 4)]
    [SerializeField] private float wobbleAmountPx = 0.4f;
    [Tooltip("How many times per second the wobble steps to a new random state.")]
    [Range(1, 24)]
    [SerializeField] private float wobbleFPS = 3f;
    [Tooltip("Must match the sprites' Pixels Per Unit import setting.")]
    [SerializeField] private float pixelsPerUnit = 16f;
    [Tooltip("Higher = neighboring vertices move less alike (more chaotic). Lower = more coherent drift.")]
    [Range(1, 100)]
    [SerializeField] private float wobbleFrequency = 10f;
    [Tooltip("Fraction of vertices that move on any given step. Lower = subtler, sparser effect.")]
    [Range(0, 1)]
    [SerializeField] private float wobbleProbability = 0.25f;
    [Tooltip("On = whole-pixel jumps (more visible). Off = sub-pixel, relies on Point-filtered textures for hard edges.")]
    [SerializeField] private bool snapWobbleToPixel = false;

    [Header("Individual Object Setup")]
    public List<OutlineTarget> outlineObjects = new List<OutlineTarget>();

    // Property IDs for shader efficiency
    private static readonly int MainTexID = Shader.PropertyToID("_MainTex");
    private static readonly int OutlineColorID = Shader.PropertyToID("_OutlineColor");
    private static readonly int OutlineThicknessID = Shader.PropertyToID("_OutlineThickness");
    private static readonly int WobbleAmountID = Shader.PropertyToID("_WobbleAmountPx");
    private static readonly int WobbleFPSID = Shader.PropertyToID("_WobbleFPS");
    private static readonly int PixelsPerUnitID = Shader.PropertyToID("_PixelsPerUnit");
    private static readonly int WobbleFrequencyID = Shader.PropertyToID("_WobbleFrequency");
    private static readonly int WobbleProbabilityID = Shader.PropertyToID("_WobbleProbability");
    private static readonly int WobbleSnapID = Shader.PropertyToID("_WobbleSnapToPixel");

    private MaterialPropertyBlock propertyBlock;

    private void OnEnable()
    {
        ApplyOutlines();
    }

    private void OnValidate()
    {
        ApplyOutlines();
    }

    private void Update()
    {
        // Keeps properties updated live during edit mode
        if (!Application.isPlaying)
        {
            ApplyOutlines();
        }
    }

    public void ApplyOutlines()
    {
        globalThickness = GraphicsSettingsManager.Instance.OutlineThickness;
        bool hasWidth = GraphicsSettingsManager.Instance.OutlineThickness == 0;
        if (outlineMaterial == null) return;
        if (propertyBlock == null) propertyBlock = new MaterialPropertyBlock();

        if (!GraphicsSettingsManager.Instance.WobbleEnabled)
        {
            wobbleAmountPx = 0;
        }

        foreach (var target in outlineObjects)
        {
            if (target.meshGrid == null) continue;

            MeshRenderer mr = target.meshGrid.RendererComponent;
            if (mr == null) continue;

            // 1. Assign outline material to the MeshRenderer directly
            if (mr.sharedMaterial != outlineMaterial && !hasWidth)
            {
                mr.sharedMaterial = outlineMaterial;
            }
            else if (mr.sharedMaterial != nooutlineMaterial && hasWidth)
            {
                mr.sharedMaterial = nooutlineMaterial;
            }
            else // fallback
            {
                mr.sharedMaterial = outlineMaterial;
            }

            // 2. Fetch block, assign both the sprite texture AND outline parameters together
            mr.GetPropertyBlock(propertyBlock);

            if (target.meshGrid.sprite != null && target.meshGrid.sprite.texture != null)
            {
                propertyBlock.SetTexture(MainTexID, target.meshGrid.sprite.texture);
            }

            propertyBlock.SetColor(OutlineColorID, target.outlineColor);
            propertyBlock.SetInt(OutlineThicknessID, globalThickness);

            propertyBlock.SetFloat(WobbleAmountID, wobbleAmountPx);
            propertyBlock.SetFloat(WobbleFPSID, wobbleFPS);
            propertyBlock.SetFloat(PixelsPerUnitID, pixelsPerUnit);
            propertyBlock.SetFloat(WobbleFrequencyID, wobbleFrequency);
            propertyBlock.SetFloat(WobbleProbabilityID, wobbleProbability);
            propertyBlock.SetFloat(WobbleSnapID, snapWobbleToPixel ? 1f : 0f);

            mr.SetPropertyBlock(propertyBlock);
        }
    }
}