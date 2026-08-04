using UnityEngine;

/// <summary>
/// Turns the wobble/outline effect on or off across every material that uses
/// one of the three wobble shaders, by driving each material's "_DoShader"
/// float property (added via [Toggle] _DoShader in the shader Properties).
///
/// Works across scene loads: rather than hunting down Renderer/Graphic
/// components (which only finds what's currently in the active scene, and
/// goes stale the moment you unload it), this sets the property directly
/// on every loaded Material *asset* using Resources.FindObjectsOfTypeAll.
/// Renderers/Graphics that later call .material (instantiating a per-object
/// copy) inherit whatever value was last set on the shared asset, so newly
/// loaded scenes pick up the current state automatically -- no re-scan
/// needed on scene load.
///
/// Wire SetEnabled(bool) up to your settings toggle's onValueChanged event.
/// </summary>
public class WobbleShaderToggle : MonoBehaviour
{
    private static readonly int DoShaderId = Shader.PropertyToID("_DoShader");

    private static readonly string[] WobbleShaderNames =
    {
        "Sprites/Wobble",
        "UI/Wobble",
        "Sprites/PixelPerfectOutlineWobble"
    };

    /// <summary>
    /// Applies the given state to every wobble material currently loaded in
    /// memory, across all scenes. Persistence lives in GraphicsSettingsManager,
    /// which calls this -- this component only touches materials, it doesn't
    /// read or write PlayerPrefs itself.
    /// </summary>
    public void SetEnabled(bool isOn)
    {
        float value = isOn ? 1f : 0f;

        foreach (Material mat in Resources.FindObjectsOfTypeAll<Material>())
        {
            if (mat.shader == null || !IsWobbleShader(mat.shader.name))
            {
                continue;
            }

            if (mat.HasProperty(DoShaderId))
            {
                mat.SetFloat(DoShaderId, value);
            }
        }
    }

    private static bool IsWobbleShader(string shaderName)
    {
        foreach (string wobbleName in WobbleShaderNames)
        {
            if (shaderName == wobbleName)
            {
                return true;
            }
        }
        return false;
    }
}