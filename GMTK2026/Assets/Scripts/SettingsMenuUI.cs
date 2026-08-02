using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Put this on your Settings Canvas/Panel and drag in the UI references
/// in the inspector. Handles initializing UI to saved values, wiring
/// listeners, and the "press any key" rebinding flow.
///
/// Outline thickness is now a TMP_Dropdown of presets instead of a slider.
/// Set up the dropdown's Options list in the inspector to match, in order,
/// whatever you put in outlineThicknessPresets below (e.g. "Thin", "Medium",
/// "Thick", "Extra Thick").
/// </summary>
public class SettingsMenuUI : MonoBehaviour
{
    [Header("Audio")]
    public Slider tickingSlider;
    public Slider sfxSlider;
    public Slider musicSlider;

    [Header("Graphics")]
    public TMP_Dropdown outlineThicknessDropdown;
    [Tooltip("Order must match the dropdown's Options list exactly")]
    public float[] outlineThicknessPresets = { 1f, 2f, 4f, 6f }; // e.g. Thin, Medium, Thick, Extra Thick
    public Toggle wobbleToggle;
    public Toggle fullscreenToggle;

    [Header("Controls - Rebind Buttons + Labels")]
    public Button interactBindButton;
    public HandDrawnTextRenderer interactBindLabel;
    public Button altInteractBindButton;
    public HandDrawnTextRenderer altInteractBindLabel;
    public Button moveUpBindButton;
    public HandDrawnTextRenderer moveUpBindLabel;
    public Button moveDownBindButton;
    public HandDrawnTextRenderer moveDownBindLabel;
    public Button moveLeftBindButton;
    public HandDrawnTextRenderer moveLeftBindLabel;
    public Button moveRightBindButton;
    public HandDrawnTextRenderer moveRightBindLabel;

    private static readonly KeyCode[] AllKeyCodes = (KeyCode[])System.Enum.GetValues(typeof(KeyCode));
    private KeyBindingManager.GameAction? actionBeingRebound = null;

    private void Start()
    {
        // --- Audio ---
        tickingSlider.value = AudioManager.Instance.GetSavedTicking();
        sfxSlider.value = AudioManager.Instance.GetSavedSFX();
        musicSlider.value = AudioManager.Instance.GetSavedMusic();

        tickingSlider.onValueChanged.AddListener(AudioManager.Instance.SetTickingVolume);
        sfxSlider.onValueChanged.AddListener(AudioManager.Instance.SetSFXVolume);
        musicSlider.onValueChanged.AddListener(AudioManager.Instance.SetMusicVolume);

        // --- Graphics ---
        outlineThicknessDropdown.value = ClosestPresetIndex(GraphicsSettingsManager.Instance.OutlineThickness);
        outlineThicknessDropdown.RefreshShownValue();
        wobbleToggle.isOn = GraphicsSettingsManager.Instance.WobbleEnabled;
        fullscreenToggle.isOn = GraphicsSettingsManager.Instance.IsFullscreen();

        outlineThicknessDropdown.onValueChanged.AddListener(OnOutlineThicknessDropdownChanged);
        wobbleToggle.onValueChanged.AddListener(GraphicsSettingsManager.Instance.SetWobbleEnabled);
        fullscreenToggle.onValueChanged.AddListener(GraphicsSettingsManager.Instance.SetFullscreen);

        // --- Controls ---
        RefreshBindingLabels();

        interactBindButton.onClick.AddListener(() => BeginRebind(KeyBindingManager.GameAction.Interact));
        altInteractBindButton.onClick.AddListener(() => BeginRebind(KeyBindingManager.GameAction.AltInteract));
        moveUpBindButton.onClick.AddListener(() => BeginRebind(KeyBindingManager.GameAction.MoveUp));
        moveDownBindButton.onClick.AddListener(() => BeginRebind(KeyBindingManager.GameAction.MoveDown));
        moveLeftBindButton.onClick.AddListener(() => BeginRebind(KeyBindingManager.GameAction.MoveLeft));
        moveRightBindButton.onClick.AddListener(() => BeginRebind(KeyBindingManager.GameAction.MoveRight));
    }

    private void OnOutlineThicknessDropdownChanged(int index)
    {
        if (index < 0 || index >= outlineThicknessPresets.Length) return;
        GraphicsSettingsManager.Instance.SetOutlineThickness(outlineThicknessPresets[index]);
    }

    // Maps a saved float thickness back to whichever preset is closest,
    // so the dropdown shows the right option on menu open.
    private int ClosestPresetIndex(float value)
    {
        int closest = 0;
        float closestDiff = Mathf.Abs(outlineThicknessPresets[0] - value);
        for (int i = 1; i < outlineThicknessPresets.Length; i++)
        {
            float diff = Mathf.Abs(outlineThicknessPresets[i] - value);
            if (diff < closestDiff)
            {
                closestDiff = diff;
                closest = i;
            }
        }
        return closest;
    }

    private void BeginRebind(KeyBindingManager.GameAction action)
    {
        actionBeingRebound = action;
        SetLabel(action, "Press any key \\dots");
    }

    private void Update()
    {
        if (actionBeingRebound == null) return;

        foreach (KeyCode key in AllKeyCodes)
        {
            if (!Input.GetKeyDown(key)) continue;

            if (key == KeyCode.Escape)
            {
                // Cancel rebind, restore old label
                actionBeingRebound = null;
                RefreshBindingLabels();
                return;
            }

            KeyBindingManager.Instance.Rebind(actionBeingRebound.Value, key);
            actionBeingRebound = null;
            RefreshBindingLabels();
            return;
        }
    }

    private void RefreshBindingLabels()
    {
        foreach (KeyBindingManager.GameAction action in System.Enum.GetValues(typeof(KeyBindingManager.GameAction)))
        {
            SetLabel(action, KeyBindingManager.Instance.GetKeyCode(action).ToString());
        }
    }

    private void SetLabel(KeyBindingManager.GameAction action, string text)
    {
        switch (action)
        {
            case KeyBindingManager.GameAction.Interact: SetHandDrawnText(interactBindLabel, text); break;
            case KeyBindingManager.GameAction.AltInteract: SetHandDrawnText(altInteractBindLabel, text); break;
            case KeyBindingManager.GameAction.MoveUp: SetHandDrawnText(moveUpBindLabel, text); break;
            case KeyBindingManager.GameAction.MoveDown: SetHandDrawnText(moveDownBindLabel, text); break;
            case KeyBindingManager.GameAction.MoveLeft: SetHandDrawnText(moveLeftBindLabel, text); break;
            case KeyBindingManager.GameAction.MoveRight: SetHandDrawnText(moveRightBindLabel, text); break;
        }
    }

    // Goes through HandDrawnTextRenderer instead of TMP_Text.text directly,
    // since HandDrawnTextRenderer owns the underlying TMP_Text.text and
    // rebuilds it (sprite tags, glyph variants, etc.) from sourceText on Refresh().
    private void SetHandDrawnText(HandDrawnTextRenderer renderer, string text)
    {
        renderer.sourceText = text;
        renderer.Refresh();
    }
}