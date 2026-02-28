using System.Drawing;
using TextVinci.Core;

namespace TextVinci.Core.Tests;

public class AsciiCanvasTests
{
    [Fact]
    public void Constructor_SetsWidthAndHeight()
    {
        var canvas = new AsciiCanvas(80, 24);
        Assert.Equal(80, canvas.Width);
        Assert.Equal(24, canvas.Height);
    }

    [Fact]
    public void Constructor_FillsWithSpaces()
    {
        var canvas = new AsciiCanvas(5, 3);
        for (int x = 0; x < 5; x++)
            for (int y = 0; y < 3; y++)
                Assert.Equal(' ', canvas.GetCell(x, y).Character);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_ThrowsOnInvalidWidth(int w)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new AsciiCanvas(w, 5));
    }

    [Fact]
    public void SetCell_CharAndColors_Roundtrip()
    {
        var canvas = new AsciiCanvas(10, 10);
        canvas.SetCell(3, 4, 'X', Color.Red, Color.Blue);
        var cell = canvas.GetCell(3, 4);
        Assert.Equal('X', cell.Character);
        Assert.Equal(Color.Red, cell.ForegroundColor);
        Assert.Equal(Color.Blue, cell.BackgroundColor);
    }

    [Fact]
    public void SetCell_ThrowsOnOutOfBounds()
    {
        var canvas = new AsciiCanvas(5, 5);
        Assert.Throws<ArgumentOutOfRangeException>(() => canvas.SetCell(5, 0, 'A', Color.White));
    }

    [Fact]
    public void FloodFill_FillsConnectedRegion()
    {
        var canvas = new AsciiCanvas(5, 5);
        canvas.FloodFill(0, 0, '#', Color.Red);
        // All cells were ' ' + White, so all should be filled
        for (int x = 0; x < 5; x++)
            for (int y = 0; y < 5; y++)
            {
                Assert.Equal('#', canvas.GetCell(x, y).Character);
                Assert.Equal(Color.Red, canvas.GetCell(x, y).ForegroundColor);
            }
    }

    [Fact]
    public void FloodFill_DoesNotFillSameChar()
    {
        var canvas = new AsciiCanvas(3, 3);
        canvas.Fill('#', Color.Red);
        canvas.FloodFill(1, 1, '#', Color.Red); // same char + same color → no-op
        Assert.Equal('#', canvas.GetCell(0, 0).Character);
    }

    [Fact]
    public void Clone_ProducesIndependentCopy()
    {
        var canvas = new AsciiCanvas(3, 3);
        canvas.SetCell(0, 0, 'Z', Color.Green);
        var clone = canvas.Clone();
        clone.SetCell(0, 0, 'A', Color.Red);
        Assert.Equal('Z', canvas.GetCell(0, 0).Character);
    }

    [Fact]
    public void Resize_PreservesExistingContent()
    {
        var canvas = new AsciiCanvas(3, 3);
        canvas.SetCell(1, 1, 'Q', Color.Yellow);
        var resized = canvas.Resize(5, 5);
        Assert.Equal('Q', resized.GetCell(1, 1).Character);
        Assert.Equal(5, resized.Width);
    }
}
