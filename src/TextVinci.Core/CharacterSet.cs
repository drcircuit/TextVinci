namespace TextVinci.Core;

/// <summary>
/// Standard character sets used for ASCII art brightness mapping.
/// Characters are ordered from darkest (most ink) to lightest (least ink).
/// </summary>
public static class CharacterSet
{
    /// <summary>Standard ASCII character set from dense to sparse.</summary>
    public static readonly string Standard = "@%#*+=-:. ";

    /// <summary>Extended character set with more granularity.</summary>
    public static readonly string Extended = "$@B%8&WM#*oahkbdpqwmZO0QLCJUYXzcvunxrjft/\\|()1{}[]?-_+~<>i!lI;:,\"^`'. ";

    /// <summary>Block elements character set for a pixelated look.</summary>
    public static readonly string Blocks = "█▓▒░ ";

    /// <summary>Simple two-character set (on/off).</summary>
    public static readonly string Simple = "@ ";

    /// <summary>Numeric character set.</summary>
    public static readonly string Numeric = "8634210 ";

    /// <summary>Gets a character from the given set mapped to a brightness value [0..255].</summary>
    public static char Map(string charset, int brightness)
    {
        if (string.IsNullOrEmpty(charset))
            return ' ';
        // brightness 0 = darkest → first char; 255 = lightest → last char
        int index = (int)Math.Round((double)brightness / 255 * (charset.Length - 1));
        index = Math.Clamp(index, 0, charset.Length - 1);
        return charset[index];
    }
}
