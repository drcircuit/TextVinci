using System.Drawing;

namespace TextVinci.Core;

/// <summary>
/// Represents a single cell in the ASCII art canvas.
/// Each cell has a character, a foreground color, and an optional background color.
/// </summary>
public sealed class AsciiCell
{
    public static readonly AsciiCell Empty = new(' ', Color.White, null);

    public char Character { get; set; }
    public Color ForegroundColor { get; set; }
    public Color? BackgroundColor { get; set; }

    public AsciiCell(char character, Color foregroundColor, Color? backgroundColor = null)
    {
        Character = character;
        ForegroundColor = foregroundColor;
        BackgroundColor = backgroundColor;
    }

    public AsciiCell Clone() => new(Character, ForegroundColor, BackgroundColor);

    public override string ToString() => Character.ToString();
}
