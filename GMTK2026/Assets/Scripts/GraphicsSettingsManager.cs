using UnityEngine;
using System;

/// <summary>
/// Persistent singleton for graphics settings: outline thickness, wobble
/// shader toggle, and fullscreen. Your per-scene OutlineManager and wobble
/// scripts subscribe to the events here so they pick up changes live and
/// on scene load. See OutlineIntegrationExample.cs and
/// WobbleIntegrationExample.cs for the exact snippets to paste in.
/// </summary>
public class GraphicsSettingsManager : MonoBehaviour
{
    public static GraphicsSettingsManager Instance { get; private set; }

    public event Action<float> OnOutlineThicknessChanged;
    public event Action<bool> OnWobbleToggled;

    private const string OutlinePref = "Graphics_OutlineThickness";
    private const string WobblePref = "Graphics_WobbleEnabled";
    private const string FullscreenPref = "Graphics_Fullscreen";

    public int OutlineThickness { get; private set; } = 2;
    public bool WobbleEnabled { get; private set; } = true;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadAndApply();
    }

    private void LoadAndApply()
    {
        OutlineThickness = PlayerPrefs.GetInt(OutlinePref);
        WobbleEnabled = PlayerPrefs.GetInt(WobblePref, 1) == 1;

        bool fullscreen = PlayerPrefs.GetInt(FullscreenPref, Screen.fullScreen ? 1 : 0) == 1;
        Screen.fullScreenMode = fullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
    }

    // Hook to your outline thickness slider (pick a min/max that suits your shader, e.g. 0-10)
    public void SetOutlineThickness(int value)
    {
        OutlineThickness = value;
        PlayerPrefs.SetInt(OutlinePref, value);
        OnOutlineThicknessChanged?.Invoke(value);
    }

    // Hook to your wobble toggle
    public void SetWobbleEnabled(bool enabled)
    {
        WobbleEnabled = enabled;
        PlayerPrefs.SetInt(WobblePref, enabled ? 1 : 0);
        OnWobbleToggled?.Invoke(enabled);
    }

    // Hook to your fullscreen toggle
    public void SetFullscreen(bool fullscreen)
    {
        Screen.fullScreenMode = fullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
        PlayerPrefs.SetInt(FullscreenPref, fullscreen ? 1 : 0);
    }

    public bool IsFullscreen() => Screen.fullScreenMode == FullScreenMode.FullScreenWindow;
}
