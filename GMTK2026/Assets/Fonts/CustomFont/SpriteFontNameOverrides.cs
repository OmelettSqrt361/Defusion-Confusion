using System.Collections.Generic;

public static class SpriteFontNameOverrides
{
    // Maps characters that can't be used literally as sprite names
    // to the prefix you used when exporting (slash_1, slash_2, etc.)
    public static readonly Dictionary<char, string> Map = new Dictionary<char, string>
    {
        { '/', "slash" },
        { '?', "question" },
        { '!', "exclamation" },
        { '.', "period" },
        { ',', "comma" },
        { ':', "colon" },
        { '\'', "\'" },
        { '"', "quotes" },
        { '#', "hashtag" },
        // add more as needed — key = the literal character, value = your glyph prefix
    };
}