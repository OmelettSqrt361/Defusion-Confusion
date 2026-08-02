using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(TMP_Text))]
public class HandDrawnTextRenderer : MonoBehaviour
{
    [TextArea(2, 6)]
    [Tooltip("Type plain text here. Use \\glyphName for special symbols (e.g. \\buttonX). Case doesn't matter.")]
    public string sourceText;

    [Tooltip("Leave empty to use the TMP_Text's assigned sprite asset or the project default.")]
    public TMP_SpriteAsset spriteAssetOverride;

    [Tooltip("Controls which glyph variants get picked. Change this to reroll the look.")]
    public int seed;

    TMP_Text _text;
    TMP_SpriteAsset _cachedAsset;
    Dictionary<string, int> _variantCounts;

    [Header("Appearance")]
    public Color textColor = Color.white;

    void OnEnable()
    {
        _text = GetComponent<TMP_Text>();
        if (seed == 0) RerollSeed(); // give it a real starting value on first add
        Refresh();
    }

    void OnValidate()
    {
        if (_text == null) _text = GetComponent<TMP_Text>();
        Refresh();
    }

    [ContextMenu("Reroll Seed")]
    public void RerollSeed()
    {
        seed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
        Refresh();
    }

    public void Refresh()
    {
        if (_text == null) return;

        TMP_SpriteAsset asset = spriteAssetOverride != null
            ? spriteAssetOverride
            : (_text.spriteAsset != null ? _text.spriteAsset : TMP_Settings.defaultSpriteAsset);

        if (asset == null) return;

        if (_text.spriteAsset != asset)
            _text.spriteAsset = asset;

        _text.color = textColor;

        BuildVariantCountsIfNeeded(asset);
        _text.text = Parse(sourceText ?? string.Empty);
    }

    void BuildVariantCountsIfNeeded(TMP_SpriteAsset asset)
    {
        if (_variantCounts != null && _cachedAsset == asset) return;

        _cachedAsset = asset;
        _variantCounts = new Dictionary<string, int>();

        foreach (var entry in asset.spriteCharacterTable)
        {
            string name = entry.name;
            int lastUnderscore = name.LastIndexOf('_');
            if (lastUnderscore < 0) continue;

            string prefix = name.Substring(0, lastUnderscore);
            string suffix = name.Substring(lastUnderscore + 1);
            if (!int.TryParse(suffix, out int variantIndex)) continue;

            if (!_variantCounts.TryGetValue(prefix, out int currentMax) || variantIndex > currentMax)
                _variantCounts[prefix] = variantIndex;
        }
    }

    string Parse(string input)
    {
        if (string.IsNullOrEmpty(input)) return string.Empty;

        var sb = new StringBuilder();
        var chosenVariant = new Dictionary<string, int>();

        int i = 0;
        while (i < input.Length)
        {
            char c = input[i];

            if (c == '\\')
            {
                int start = i + 1;
                int j = start;
                while (j < input.Length && char.IsLetterOrDigit(input[j]))
                    j++;

                if (j > start)
                {
                    string glyphName = input.Substring(start, j - start);
                    sb.Append($"<sprite name=\"{glyphName}\" tint=\"1\">");
                    i = j;
                    continue;
                }

                i++;
                continue;
            }

            if (c == ' ' || c == '\n' || c == '\t')
            {
                sb.Append(c);
                i++;
                continue;
            }

            char lower = char.ToLowerInvariant(c);
            string prefix = SpriteFontNameOverrides.Map.TryGetValue(lower, out var mapped)
                ? mapped
                : lower.ToString();

            string spriteName;

            if (_variantCounts.TryGetValue(prefix, out int variantCount) && variantCount > 0)
            {
                if (!chosenVariant.TryGetValue(prefix, out int pick))
                {
                    // seed now comes from the manually-set field, not the string —
                    // so the SAME text will render differently once you reroll
                    var rng = new System.Random(seed ^ (prefix.GetHashCode() * 397));
                    pick = rng.Next(1, variantCount + 1);
                    chosenVariant[prefix] = pick;
                }
                spriteName = $"{prefix}_{pick}";
            }
            else
            {
                spriteName = prefix;
            }

            sb.Append($"<sprite name=\"{spriteName}\" tint=\"1\">");
            i++;
        }

        return sb.ToString();
    }
}