using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Persistent singleton controlling Ticking/SFX/Music volume via an AudioMixer.
///
/// SETUP REQUIRED IN UNITY:
/// 1. Create an AudioMixer asset (right click in Project > Create > Audio Mixer).
/// 2. Inside it, create 3 child groups under Master: "Ticking", "SFX", "Music".
/// 3. Route your ticking sound's AudioSource, your SFX AudioSources, and your
///    music AudioSource to their matching group (set the AudioSource's
///    "Output" field to that group).
/// 4. Select each group, right-click its "Volume" slider in the inspector,
///    choose "Expose to script". Then in the Mixer window's top-left dropdown
///    ("Exposed Parameters"), rename each one to exactly:
///    TickingVolume, SFXVolume, MusicVolume  (must match the constants below).
/// 5. Put this script on an empty GameObject in your boot scene and drag the
///    AudioMixer asset into the "Mixer" field in the inspector.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private AudioMixer mixer;

    private const string TickingParam = "TickingVolume";
    private const string SFXParam = "SFXVolume";
    private const string MusicParam = "MusicVolume";

    private const string TickingPref = "Volume_Ticking";
    private const string SFXPref = "Volume_SFX";
    private const string MusicPref = "Volume_Music";

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
        SetTickingVolume(PlayerPrefs.GetFloat(TickingPref, 0.75f));
        SetSFXVolume(PlayerPrefs.GetFloat(SFXPref, 0.75f));
        SetMusicVolume(PlayerPrefs.GetFloat(MusicPref, 0.75f));
    }

    // Hook these directly to your sliders' onValueChanged (range 0-1)
    public void SetTickingVolume(float linear01)
    {
        ApplyVolume(TickingParam, linear01);
        PlayerPrefs.SetFloat(TickingPref, linear01);
    }

    public void SetSFXVolume(float linear01)
    {
        ApplyVolume(SFXParam, linear01);
        PlayerPrefs.SetFloat(SFXPref, linear01);
    }

    public void SetMusicVolume(float linear01)
    {
        ApplyVolume(MusicParam, linear01);
        PlayerPrefs.SetFloat(MusicPref, linear01);
    }

    private void ApplyVolume(string param, float linear01)
    {
        // Mixer volume is in dB, sliders are linear 0-1, so convert with log10.
        // Clamp away from 0 since log10(0) is -infinity.
        linear01 = Mathf.Clamp(linear01, 0.0001f, 1f);
        float dB = Mathf.Log10(linear01) * 20f;
        mixer.SetFloat(param, dB);
    }

    // Used by the settings UI to initialize slider positions on open
    public float GetSavedTicking() => PlayerPrefs.GetFloat(TickingPref, 0.75f);
    public float GetSavedSFX() => PlayerPrefs.GetFloat(SFXPref, 0.75f);
    public float GetSavedMusic() => PlayerPrefs.GetFloat(MusicPref, 0.75f);
}
