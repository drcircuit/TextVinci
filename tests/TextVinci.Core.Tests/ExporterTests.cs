using System.Drawing;
using TextVinci.Core;

namespace TextVinci.Core.Tests;

public class ExporterTests
{
    private static AsciiCanvas BuildCanvas()
    {
        var canvas = new AsciiCanvas(3, 2);
        canvas.SetCell(0, 0, 'H', Color.Red);
        canvas.SetCell(1, 0, 'i', Color.Green);
        canvas.SetCell(2, 0, '!', Color.Blue);
        canvas.SetCell(0, 1, 'A', Color.White);
        canvas.SetCell(1, 1, 'B', Color.White);
        canvas.SetCell(2, 1, 'C', Color.White);
        return canvas;
    }

    [Fact]
    public void AsciiExporter_ProducesCorrectLineCount()
    {
        var canvas = BuildCanvas();
        string output = AsciiExporter.Export(canvas);
        string[] lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        Assert.Equal(2, lines.Length);
    }

    [Fact]
    public void AsciiExporter_ContainsAllCharacters()
    {
        var canvas = BuildCanvas();
        string output = AsciiExporter.Export(canvas);
        Assert.Contains("Hi!", output);
        Assert.Contains("ABC", output);
    }

    [Fact]
    public void AsciiExporter_NoAnsiEscapes()
    {
        var canvas = BuildCanvas();
        string output = AsciiExporter.Export(canvas);
        // ESC character is code 27
        Assert.DoesNotContain((char)27, output);
    }

    [Fact]
    public void AnsiExporter_ContainsAnsiEscapes()
    {
        var canvas = BuildCanvas();
        string output = AnsiExporter.Export(canvas);
        // ESC character followed by '[' is the start of an ANSI escape sequence
        Assert.Contains((char)27, output);
    }

    [Fact]
    public void AnsiExporter_ContainsAllCharacters()
    {
        var canvas = BuildCanvas();
        string output = AnsiExporter.Export(canvas);
        // Characters must appear somewhere in output (may be surrounded by escape codes)
        Assert.Contains("H", output);
        Assert.Contains("i", output);
        Assert.Contains("!", output);
    }

    [Fact]
    public void AsciiExporter_SaveToFile_CreatesFile()
    {
        var canvas = BuildCanvas();
        string tmpPath = Path.Combine(Path.GetTempPath(), "test_ascii_export.txt");
        try
        {
            AsciiExporter.SaveToFile(canvas, tmpPath);
            Assert.True(File.Exists(tmpPath));
            string content = File.ReadAllText(tmpPath);
            Assert.Contains("Hi!", content);
        }
        finally
        {
            File.Delete(tmpPath);
        }
    }
}
