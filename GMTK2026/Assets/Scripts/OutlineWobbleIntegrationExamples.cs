// These are NOT standalone scripts — they're snippets to paste into your
// EXISTING outline manager script and EXISTING wobble shader script.
// Paste over/next to your current OnEnable/OnDisable/Start methods, then
// swap the placeholder shader property names for your real ones.


// ============================================================
// 1) Paste into your existing OUTLINE MANAGER script
// ============================================================
/*
private void OnEnable()
{
    if (GraphicsSettingsManager.Instance != null)
    {
        ApplyThickness(GraphicsSettingsManager.Instance.OutlineThickness);
        GraphicsSettingsManager.Instance.OnOutlineThicknessChanged += ApplyThickness;
    }
}

private void OnDisable()
{
    if (GraphicsSettingsManager.Instance != null)
        GraphicsSettingsManager.Instance.OnOutlineThicknessChanged -= ApplyThickness;
}

private void ApplyThickness(float thickness)
{
    // Replace "_OutlineWidth" with whatever your shader's actual property is named
    outlineMaterial.SetFloat("_OutlineWidth", thickness);

    // If you manage a whole list of outline materials instead of one, loop instead:
    // foreach (var mat in outlineMaterials) mat.SetFloat("_OutlineWidth", thickness);
}
*/


// ============================================================
// 2) Paste into your existing WOBBLE script (the one on each wobbling object)
// ============================================================
/*
private void OnEnable()
{
    if (GraphicsSettingsManager.Instance != null)
    {
        enabled = GraphicsSettingsManager.Instance.WobbleEnabled;
        GraphicsSettingsManager.Instance.OnWobbleToggled += HandleWobbleToggle;
    }
}

private void OnDisable()
{
    if (GraphicsSettingsManager.Instance != null)
        GraphicsSettingsManager.Instance.OnWobbleToggled -= HandleWobbleToggle;
}

private void HandleWobbleToggle(bool isEnabled)
{
    // Setting "enabled = false" stops this component's Update()/shader-driving
    // loop from running at all, which is the cheapest way to turn it off.
    enabled = isEnabled;

    // Optional: if turning wobble off should snap the object back to its
    // resting shape/position rather than freezing mid-wobble, do that here.
    // ResetToRestState();
}
*/

// NOTE: if instead of a per-object script you're driving wobble through a
// single shared material/shader globally (e.g. a global shader property),
// do this once in GraphicsSettingsManager instead:
//
//   Shader.SetGlobalFloat("_WobbleEnabled", enabled ? 1f : 0f);
//
// and read _WobbleEnabled inside the shader to skip the wobble calculation.
// Send me your wobble script/shader and I'll wire the exact version for you.
