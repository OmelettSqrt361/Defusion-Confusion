using System;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class SpriteOutlineManager : MonoBehaviour
{
    [System.Serializable]
    public struct OutlineTarget
    {
        public SpriteRenderer spriteRenderer;
        public Color outlineColor;

        // Constructor for quick instantiation
        public OutlineTarget(SpriteRenderer renderer, Color color)
        {
            this.spriteRenderer = renderer;
            this.outlineColor = color;
        }
    }

    [Header("Global Settings")]
    [Range(1, 10)]
    [SerializeField] private int globalThickness = 1;
    [SerializeField] private Material outlineMaterial;

    [Tooltip("Amount to expand sprite bounds so camera culling doesn't clip the outline when moving.")]
    [SerializeField] private Vector3 boundsPadding = new Vector3(0.5f, 0.5f, 0f);

    [Header("Individual Object Setup")]
    public List<OutlineTarget> outlineObjects = new List<OutlineTarget>();

    // Property IDs for shader efficiency
    private static readonly int OutlineColorID = Shader.PropertyToID("_OutlineColor");
    private static readonly int OutlineThicknessID = Shader.PropertyToID("_OutlineThickness");

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
        // Keeps properties and bounds updated live during edit mode or runtime
        if (!Application.isPlaying)
        {
            ApplyOutlines();
        }
    }

    public void ApplyOutlines()
    {
        if (outlineMaterial == null) return;
        if (propertyBlock == null) propertyBlock = new MaterialPropertyBlock();

        foreach (var target in outlineObjects)
        {
            if (target.spriteRenderer == null) continue;

            // 1. Assign outline material if not already set
            if (target.spriteRenderer.sharedMaterial != outlineMaterial)
            {
                target.spriteRenderer.sharedMaterial = outlineMaterial;
            }

            // 2. Prevent camera frustum culling flickering by expanding renderer bounds
            if (target.spriteRenderer.sprite != null)
            {
                Bounds currentBounds = target.spriteRenderer.bounds;
                currentBounds.Expand(boundsPadding);
            }

            // 3. Set individual property overrides using MaterialPropertyBlock
            target.spriteRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor(OutlineColorID, target.outlineColor);
            propertyBlock.SetInt(OutlineThicknessID, globalThickness);
            target.spriteRenderer.SetPropertyBlock(propertyBlock);
        }
    }
}