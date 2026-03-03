using System.Drawing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using TextVinci.Core;

namespace TextVinci.Core.Tests;

public class AsciiConverterTests
{
    /// <summary>Creates a solid-color 16x16 image for testing.</summary>
    private static Image<Rgba32> SolidImage(int w, int h, byte r, byte g, byte b)
    {
        var img = new Image<Rgba32>(w, h);
        img.ProcessPixelRows(accessor =>
        {
            for (int y = 0; y < h; y++)
            {
                var row = accessor.GetRowSpan(y);
                for (int x = 0; x < w; x++)
                    row[x] = new Rgba32(r, g, b, 255);
            }
        });
        return img;
    }

    [Fact]
    public void ConvertImage_OutputSizeMatchesSettings()
    {
        using var img = SolidImage(100, 50, 128, 128, 128);
        var settings = new ConversionSettings { OutputWidth = 40, OutputHeight = 20, UseColors = false };
        var canvas = AsciiConverter.ConvertImage(img, settings);
        Assert.Equal(40, canvas.Width);
        Assert.Equal(20, canvas.Height);
    }

    [Fact]
    public void ConvertImage_AutoHeight_IsCalculated()
    {
        using var img = SolidImage(100, 100, 128, 128, 128);
        var settings = new ConversionSettings { OutputWidth = 40, OutputHeight = 0, CharAspectRatio = 0.5 };
        var canvas = AsciiConverter.ConvertImage(img, settings);
        Assert.Equal(40, canvas.Width);
        // With square image and 0.5 aspect ratio, height = 40 * 0.5 = 20
        Assert.Equal(20, canvas.Height);
    }

    [Fact]
    public void ConvertImage_NoColors_AllCellsAreWhite()
    {
        using var img = SolidImage(10, 10, 200, 100, 50);
        var settings = new ConversionSettings
        {
            OutputWidth = 5,
            OutputHeight = 5,
            UseColors = false,
            Dithering = DitheringMode.None
        };
        var canvas = AsciiConverter.ConvertImage(img, settings);
        for (int x = 0; x < 5; x++)
            for (int y = 0; y < 5; y++)
                Assert.Equal(System.Drawing.Color.White, canvas.GetCell(x, y).ForegroundColor);
    }

    [Fact]
    public void ConvertImage_WithGradient_UsesDerivedColor()
    {
        using var img = SolidImage(10, 10, 255, 255, 255); // all white → brightness=255
        var settings = new ConversionSettings
        {
            OutputWidth = 5,
            OutputHeight = 5,
            UseColorGradient = true,
            ColorGradient = new ColorGradient(System.Drawing.Color.Black, System.Drawing.Color.Red),
            Dithering = DitheringMode.None
        };
        var canvas = AsciiConverter.ConvertImage(img, settings);
        // brightness = 255 → t = 1.0 → should be Red
        for (int x = 0; x < 5; x++)
            for (int y = 0; y < 5; y++)
            {
                var fg = canvas.GetCell(x, y).ForegroundColor;
                Assert.Equal(255, fg.R);
                Assert.Equal(0, fg.G);
                Assert.Equal(0, fg.B);
            }
    }

    [Fact]
    public void ConvertImage_InvertBrightness_ChangesCharacters()
    {
        using var img = SolidImage(10, 10, 255, 255, 255); // fully white image
        var settingsNormal = new ConversionSettings
        {
            OutputWidth = 5, OutputHeight = 5, Dithering = DitheringMode.None,
            UseColors = false, InvertBrightness = false
        };
        var settingsInverted = new ConversionSettings
        {
            OutputWidth = 5, OutputHeight = 5, Dithering = DitheringMode.None,
            UseColors = false, InvertBrightness = true
        };
        var canvasNormal   = AsciiConverter.ConvertImage(img, settingsNormal);
        var canvasInverted = AsciiConverter.ConvertImage(img, settingsInverted);
        // Characters should differ when brightness is inverted
        Assert.NotEqual(canvasNormal.GetCell(0, 0).Character, canvasInverted.GetCell(0, 0).Character);
    }

    [Fact]
    public void Convert_FileNotFound_Throws()
    {
        Assert.Throws<FileNotFoundException>(() =>
            AsciiConverter.Convert("/nonexistent/path/image.png", new ConversionSettings()));
    }
}
