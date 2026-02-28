using System.Drawing;

namespace TextVinci.Core;

/// <summary>
/// A linear color gradient defined by two or more color stops.
/// </summary>
public sealed class ColorGradient
{
    private readonly List<(double Position, Color Color)> _stops;

    public ColorGradient()
    {
        _stops = new List<(double, Color)>();
    }

    /// <summary>
    /// Creates a gradient from a start color to an end color.
    /// </summary>
    public ColorGradient(Color from, Color to) : this()
    {
        AddStop(0.0, from);
        AddStop(1.0, to);
    }

    /// <summary>
    /// Adds a color stop at the given position in [0..1].
    /// </summary>
    public void AddStop(double position, Color color)
    {
        position = Math.Clamp(position, 0, 1);
        _stops.Add((position, color));
        _stops.Sort((a, b) => a.Position.CompareTo(b.Position));
    }

    /// <summary>
    /// Returns the count of color stops.
    /// </summary>
    public int StopCount => _stops.Count;

    /// <summary>
    /// Evaluates the gradient at position <paramref name="t"/> in [0..1].
    /// </summary>
    public Color Evaluate(double t)
    {
        t = Math.Clamp(t, 0, 1);

        if (_stops.Count == 0) return Color.White;
        if (_stops.Count == 1) return _stops[0].Color;

        // Find the surrounding stops
        for (int i = 0; i < _stops.Count - 1; i++)
        {
            var (p0, c0) = _stops[i];
            var (p1, c1) = _stops[i + 1];

            if (t >= p0 && t <= p1)
            {
                double local = (p1 - p0) < 1e-9 ? 0 : (t - p0) / (p1 - p0);
                return Lerp(c0, c1, local);
            }
        }

        return _stops[^1].Color;
    }

    private static Color Lerp(Color a, Color b, double t)
    {
        return Color.FromArgb(
            (int)Math.Round(a.A + (b.A - a.A) * t),
            (int)Math.Round(a.R + (b.R - a.R) * t),
            (int)Math.Round(a.G + (b.G - a.G) * t),
            (int)Math.Round(a.B + (b.B - a.B) * t)
        );
    }

    // --- Predefined gradients ---

    public static ColorGradient BlackToWhite =>
        new(Color.Black, Color.White);

    public static ColorGradient WhiteToBlack =>
        new(Color.White, Color.Black);

    public static ColorGradient Fire
    {
        get
        {
            var g = new ColorGradient();
            g.AddStop(0.0, Color.Black);
            g.AddStop(0.3, Color.DarkRed);
            g.AddStop(0.6, Color.OrangeRed);
            g.AddStop(0.85, Color.Yellow);
            g.AddStop(1.0, Color.White);
            return g;
        }
    }

    public static ColorGradient Ocean
    {
        get
        {
            var g = new ColorGradient();
            g.AddStop(0.0, Color.Black);
            g.AddStop(0.4, Color.DarkBlue);
            g.AddStop(0.7, Color.DeepSkyBlue);
            g.AddStop(1.0, Color.White);
            return g;
        }
    }

    public static ColorGradient Matrix
    {
        get
        {
            var g = new ColorGradient();
            g.AddStop(0.0, Color.Black);
            g.AddStop(0.5, Color.FromArgb(0, 128, 0));
            g.AddStop(1.0, Color.Lime);
            return g;
        }
    }
}
