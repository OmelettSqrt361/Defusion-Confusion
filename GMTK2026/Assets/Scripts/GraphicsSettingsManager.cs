using UnityEngine;
using UnityEngine.SceneManagement;
using System;

/// <summary>
/// Persistent singleton for graphics settings: outline thickness, wobble
/// shader toggle, and fullscreen. Your per-scene OutlineManager and wobble
/// scripts subscribe to the events here so they pick up changes live and
/// on scene load. See OutlineIntegrationExample.cs and
/// WobbleIntegrationExample.cs for the exact snippets to paste in.
/// </summary>
[RequireComponent(typeof(WobbleShaderToggle))]
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

    private WobbleShaderToggle wobbleShaderToggle;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        wobbleShaderToggle = GetComponent<WobbleShaderToggle>();
        if (wobbleShaderToggle == null)
        {
            Debug.LogError(
                "GraphicsSettingsManager: no WobbleShaderToggle component found on this " +
                "GameObject. Add one alongside GraphicsSettingsManager, or wobble toggling " +
                "will be skipped.", this);
        }

        LoadAndApply();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Any material newly loaded with this scene starts at the shader's
        // default (_DoShader = 1), so re-push the current state in case it
        // wasn't already in memory when we last toggled.
        wobbleShaderToggle?.SetEnabled(WobbleEnabled);
    }

    private void LoadAndApply()
    {
        OutlineThickness = PlayerPrefs.GetInt(OutlinePref);
        WobbleEnabled = PlayerPrefs.GetInt(WobblePref, 1) == 1;

        bool fullscreen = PlayerPrefs.GetInt(FullscreenPref, Screen.fullScreen ? 1 : 0) == 1;
        Screen.fullScreenMode = fullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;

        // Make sure materials reflect the saved wobble state right away,
        // not just the WobbleShaderToggle's own PlayerPrefs read in its Awake
        // (the two Awakes can otherwise race depending on script execution order).
        wobbleShaderToggle?.SetEnabled(WobbleEnabled);
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

        if (wobbleShaderToggle == null)
        {
            wobbleShaderToggle = GetComponent<WobbleShaderToggle>();
        }
        wobbleShaderToggle?.SetEnabled(enabled);

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