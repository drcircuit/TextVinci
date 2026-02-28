using System.Drawing;
using SixLabors.ImageSharp.Processing;

namespace TextVinci.Core;

/// <summary>
/// Settings that control how an image is converted to ASCII art.
/// </summary>
public sealed class ConversionSettings
{
    /// <summary>Number of character columns in the output canvas.</summary>
    public int OutputWidth { get; set; } = 120;

    /// <summary>
    /// Number of character rows in the output canvas.
    /// When 0 the converter auto-calculates height preserving aspect ratio
    /// (using <see cref="CharAspectRatio"/>).
    /// </summary>
    public int OutputHeight { get; set; } = 0;

    /// <summary>
    /// The width-to-height ratio of a single character cell in the target font.
    /// Typical monospace fonts have characters that are roughly half as wide as they are tall.
    /// </summary>
    public double CharAspectRatio { get; set; } = 0.5;

    /// <summary>The character set to use for brightness mapping.</summary>
    public string CharacterSet { get; set; } = Core.CharacterSet.Standard;

    /// <summary>Whether to apply ANSI foreground colors from the source image.</summary>
    public bool UseColors { get; set; } = true;

    /// <summary>The dithering algorithm to apply.</summary>
    public DitheringMode Dithering { get; set; } = DitheringMode.FloydSteinberg;

    /// <summary>Number of brightness quantization levels for dithering.</summary>
    public int DitheringLevels { get; set; } = 8;

    /// <summary>
    /// When true, maps colors through a gradient instead of using direct pixel colors.
    /// Useful for stylized, monochromatic output.
    /// </summary>
    public bool UseColorGradient { get; set; } = false;

    /// <summary>The gradient to use when <see cref="UseColorGradient"/> is true.</summary>
    public ColorGradient? ColorGradient { get; set; }

    /// <summary>Whether to invert brightness before mapping characters.</summary>
    public bool InvertBrightness { get; set; } = false;
}

/// <summary>
/// Converts images to ASCII art canvases.
/// </summary>
public static class AsciiConverter
{
    /// <summary>
    /// Converts the image at <paramref name="imagePath"/> to an <see cref="AsciiCanvas"/>
    /// using the supplied settings.
    /// </summary>
    public static AsciiCanvas Convert(string imagePath, ConversionSettings settings)
    {
        if (!File.Exists(imagePath))
            throw new FileNotFoundException("Image file not found.", imagePath);

        using var image = SixLabors.ImageSharp.Image.Load<SixLabors.ImageSharp.PixelFormats.Rgba32>(imagePath);
        return ConvertImage(image, settings);
    }

    /// <summary>
    /// Converts a raw image stream to an <see cref="AsciiCanvas"/>.
    /// </summary>
    public static AsciiCanvas Convert(Stream imageStream, ConversionSettings settings)
    {
        using var image = SixLabors.ImageSharp.Image.Load<SixLabors.ImageSharp.PixelFormats.Rgba32>(imageStream);
        return ConvertImage(image, settings);
    }

    // Internal converter that works on an already-loaded ImageSharp image.
    public static AsciiCanvas ConvertImage(
        SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba32> image,
        ConversionSettings settings)
    {
        int outW = settings.OutputWidth;
        int outH = settings.OutputHeight;

        if (outH <= 0)
        {
            double aspectCorrection = settings.CharAspectRatio;
            outH = Math.Max(1, (int)Math.Round(outW * image.Height / (double)image.Width * aspectCorrection));
        }

        // Resize source image to match output character grid
        var resized = image.Clone(ctx => ctx.Resize(outW, outH));

        // Build brightness array for dithering
        var brightness = new float[outH, outW];
        for (int r = 0; r < outH; r++)
        {
            for (int c = 0; c < outW; c++)
            {
                var px = resized[c, r];
                float lum = 0.2126f * px.R + 0.7152f * px.G + 0.0722f * px.B;
                brightness[r, c] = settings.InvertBrightness ? 255f - lum : lum;
            }
        }

        // Apply dithering
        var dithered = DitheringEngine.Apply(brightness, settings.Dithering, settings.DitheringLevels);

        // Build canvas
        var canvas = new AsciiCanvas(outW, outH);
        for (int r = 0; r < outH; r++)
        {
            for (int c = 0; c < outW; c++)
            {
                char ch = CharacterSet.Map(settings.CharacterSet, (int)Math.Clamp(dithered[r, c], 0, 255));

                Color fg;
                if (settings.UseColorGradient && settings.ColorGradient != null)
                {
                    double t = Math.Clamp(dithered[r, c] / 255.0, 0, 1);
                    fg = settings.ColorGradient.Evaluate(t);
                }
                else if (settings.UseColors)
                {
                    var px = resized[c, r];
                    fg = Color.FromArgb(px.A, px.R, px.G, px.B);
                }
                else
                {
                    fg = Color.White;
                }

                canvas.SetCell(c, r, ch, fg);
            }
        }

        return canvas;
    }
}
