using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Persistent singleton that stores rebindable key bindings and replaces
/// direct Input.GetKey(KeyCode.X) / Input.GetAxisRaw("Horizontal") calls.
///
/// Put this on an empty GameObject in your very first/boot scene
/// (it survives scene loads via DontDestroyOnLoad, so you only need one).
/// </summary>
public class KeyBindingManager : MonoBehaviour
{
    public static KeyBindingManager Instance { get; private set; }

    public enum GameAction
    {
        Interact,
        AltInteract,
        MoveUp,
        MoveDown,
        MoveLeft,
        MoveRight
    }

    // Default rebindable key for each action
    private readonly Dictionary<GameAction, KeyCode> defaultBindings = new Dictionary<GameAction, KeyCode>
    {
        { GameAction.Interact,    KeyCode.X },
        { GameAction.AltInteract, KeyCode.E },
        { GameAction.MoveUp,      KeyCode.W },
        { GameAction.MoveDown,    KeyCode.S },
        { GameAction.MoveLeft,    KeyCode.A },
        { GameAction.MoveRight,   KeyCode.D }
    };

    // Fixed arrow-key fallback for movement (always active, not rebindable,
    // so arrows keep working no matter what WASD gets rebound to)
    private readonly Dictionary<GameAction, KeyCode> arrowFallback = new Dictionary<GameAction, KeyCode>
    {
        { GameAction.MoveUp,    KeyCode.UpArrow },
        { GameAction.MoveDown,  KeyCode.DownArrow },
        { GameAction.MoveLeft,  KeyCode.LeftArrow },
        { GameAction.MoveRight, KeyCode.RightArrow }
    };

    private readonly Dictionary<GameAction, KeyCode> bindings = new Dictionary<GameAction, KeyCode>();

    public event Action OnBindingsChanged;

    private const string PrefPrefix = "KeyBind_";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadBindings();
    }

    private void LoadBindings()
    {
        foreach (var pair in defaultBindings)
        {
            int saved = PlayerPrefs.GetInt(PrefPrefix + pair.Key, (int)pair.Value);
            bindings[pair.Key] = (KeyCode)saved;
        }
    }

    private void SaveBindings()
    {
        foreach (var pair in bindings)
        {
            PlayerPrefs.SetInt(PrefPrefix + pair.Key, (int)pair.Value);
        }
        PlayerPrefs.Save();
    }

    public KeyCode GetKeyCode(GameAction action) => bindings[action];

    public bool GetKey(GameAction action)
    {
        if (Input.GetKey(bindings[action])) return true;
        if (arrowFallback.TryGetValue(action, out KeyCode fallback) && Input.GetKey(fallback)) return true;
        return false;
    }

    public bool GetKeyDown(GameAction action)
    {
        if (Input.GetKeyDown(bindings[action])) return true;
        if (arrowFallback.TryGetValue(action, out KeyCode fallback) && Input.GetKeyDown(fallback)) return true;
        return false;
    }

    // Drop-in replacements for Input.GetAxisRaw("Horizontal") / ("Vertical")
    public float GetHorizontal()
    {
        float value = 0f;
        if (GetKey(GameAction.MoveLeft)) value -= 1f;
        if (GetKey(GameAction.MoveRight)) value += 1f;
        return value;
    }

    public float GetVertical()
    {
        float value = 0f;
        if (GetKey(GameAction.MoveDown)) value -= 1f;
        if (GetKey(GameAction.MoveUp)) value += 1f;
        return value;
    }

    // Drop-in replacements for Input.GetKey(KeyCode.X) || Input.GetKey(KeyCode.E)
    public bool IsInteractPressed() => GetKeyDown(GameAction.Interact) || GetKeyDown(GameAction.AltInteract);
    public bool IsInteractHeld() => GetKey(GameAction.Interact) || GetKey(GameAction.AltInteract);

    public void Rebind(GameAction action, KeyCode newKey)
    {
        bindings[action] = newKey;
        SaveBindings();
        OnBindingsChanged?.Invoke();
    }

    public void ResetToDefaults()
    {
        foreach (var pair in defaultBindings)
            bindings[pair.Key] = pair.Value;
        SaveBindings();
        OnBindingsChanged?.Invoke();
    }
}
