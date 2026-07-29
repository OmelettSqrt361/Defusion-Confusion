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
        // Keeps properties updated live during edit mode
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

            // 2. Set individual property overrides using MaterialPropertyBlock
            target.spriteRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor(OutlineColorID, target.outlineColor);
            propertyBlock.SetInt(OutlineThicknessID, globalThickness);
            target.spriteRenderer.SetPropertyBlock(propertyBlock);
        }
    }
}