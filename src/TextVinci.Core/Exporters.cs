using System.Drawing;
using System.Text;

namespace TextVinci.Core;

/// <summary>
/// Exports an <see cref="AsciiCanvas"/> as plain text (no colors).
/// </summary>
public static class AsciiExporter
{
    /// <summary>
    /// Exports the canvas as plain ASCII text lines.
    /// </summary>
    public static string Export(AsciiCanvas canvas)
    {
        var sb = new StringBuilder(canvas.Width * canvas.Height + canvas.Height);
        for (int y = 0; y < canvas.Height; y++)
        {
            for (int x = 0; x < canvas.Width; x++)
                sb.Append(canvas.GetCell(x, y).Character);
            sb.AppendLine();
        }
        return sb.ToString();
    }

    /// <summary>
    /// Saves the canvas as a plain text file.
    /// </summary>
    public static void SaveToFile(AsciiCanvas canvas, string path)
    {
        File.WriteAllText(path, Export(canvas), Encoding.UTF8);
    }
}

/// <summary>
/// Exports an <see cref="AsciiCanvas"/> with ANSI escape codes for 24-bit (true-color) terminals.
/// </summary>
public static class AnsiExporter
{
    private const string Reset = "\u001b[0m";

    /// <summary>
    /// Exports the canvas as an ANSI-colored string using 24-bit color escape sequences.
    /// </summary>
    public static string Export(AsciiCanvas canvas)
    {
        var sb = new StringBuilder(canvas.Width * canvas.Height * 20);

        for (int y = 0; y < canvas.Height; y++)
        {
            Color? lastFg = null;
            Color? lastBg = null;

            for (int x = 0; x < canvas.Width; x++)
            {
                var cell = canvas.GetCell(x, y);
                bool fgChanged = lastFg == null || lastFg.Value != cell.ForegroundColor;
                bool bgChanged = lastBg != cell.BackgroundColor;

                if (fgChanged || bgChanged)
                {
                    sb.Append(BuildEscape(cell.ForegroundColor, cell.BackgroundColor));
                    lastFg = cell.ForegroundColor;
                    lastBg = cell.BackgroundColor;
                }

                sb.Append(cell.Character);
            }

            sb.Append(Reset);
            sb.AppendLine();
        }

        sb.Append(Reset);
        return sb.ToString();
    }

    /// <summary>
    /// Saves the canvas as an ANSI escape-coded text file (.ans).
    /// </summary>
    public static void SaveToFile(AsciiCanvas canvas, string path)
    {
        File.WriteAllText(path, Export(canvas), Encoding.UTF8);
    }

    private static string BuildEscape(Color fg, Color? bg)
    {
        var sb = new StringBuilder();
        // Foreground: ESC[38;2;R;G;Bm
        sb.Append($"\u001b[38;2;{fg.R};{fg.G};{fg.B}m");
        if (bg.HasValue)
            // Background: ESC[48;2;R;G;Bm
            sb.Append($"\u001b[48;2;{bg.Value.R};{bg.Value.G};{bg.Value.B}m");
        return sb.ToString();
    }
}
