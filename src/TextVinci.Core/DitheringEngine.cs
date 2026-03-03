namespace TextVinci.Core;

/// <summary>
/// Dithering algorithm to use when converting an image to ASCII art.
/// </summary>
public enum DitheringMode
{
    None,
    FloydSteinberg,
    OrderedBayer4x4,
    Atkinson
}

/// <summary>
/// Applies dithering to a 2-D brightness array and returns dithered values.
/// All brightness values are in the range [0, 255].
/// </summary>
public static class DitheringEngine
{
    /// <summary>
    /// Applies the selected dithering algorithm and returns the quantized brightness map.
    /// </summary>
    /// <param name="pixels">Input brightness values [row, col] in range [0,255].</param>
    /// <param name="mode">The dithering algorithm to apply.</param>
    /// <param name="levels">Number of distinct brightness levels to quantize to (2..256).</param>
    /// <returns>Quantized brightness values [row, col] in range [0,255].</returns>
    public static float[,] Apply(float[,] pixels, DitheringMode mode, int levels = 8)
    {
        levels = Math.Clamp(levels, 2, 256);
        float step = 255f / (levels - 1);

        return mode switch
        {
            DitheringMode.None => Quantize(pixels, step),
            DitheringMode.FloydSteinberg => FloydSteinberg(pixels, step),
            DitheringMode.OrderedBayer4x4 => Bayer4x4(pixels, step),
            DitheringMode.Atkinson => Atkinson(pixels, step),
            _ => Quantize(pixels, step)
        };
    }

    private static float Quantize(float value, float step) =>
        (float)(Math.Round(value / step) * step);

    private static float[,] Quantize(float[,] pixels, float step)
    {
        int rows = pixels.GetLength(0), cols = pixels.GetLength(1);
        var result = new float[rows, cols];
        for (int r = 0; r < rows; r++)
            for (int c = 0; c < cols; c++)
                result[r, c] = Quantize(pixels[r, c], step);
        return result;
    }

    private static float[,] FloydSteinberg(float[,] pixels, float step)
    {
        int rows = pixels.GetLength(0), cols = pixels.GetLength(1);
        var buf = (float[,])pixels.Clone();

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                float old = Math.Clamp(buf[r, c], 0, 255);
                float @new = Quantize(old, step);
                buf[r, c] = @new;
                float err = old - @new;

                if (c + 1 < cols)             buf[r, c + 1]     += err * 7 / 16f;
                if (r + 1 < rows)
                {
                    if (c - 1 >= 0)           buf[r + 1, c - 1] += err * 3 / 16f;
                                              buf[r + 1, c]     += err * 5 / 16f;
                    if (c + 1 < cols)         buf[r + 1, c + 1] += err * 1 / 16f;
                }
            }
        }

        return buf;
    }

    private static readonly float[,] BayerMatrix4x4 =
    {
        {  0, 8, 2, 10 },
        { 12, 4, 14, 6 },
        {  3, 11, 1, 9 },
        { 15, 7, 13, 5 }
    };

    private static float[,] Bayer4x4(float[,] pixels, float step)
    {
        int rows = pixels.GetLength(0), cols = pixels.GetLength(1);
        var result = new float[rows, cols];

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                float threshold = (BayerMatrix4x4[r % 4, c % 4] / 16f - 0.5f) * step;
                result[r, c] = Quantize(Math.Clamp(pixels[r, c] + threshold, 0, 255), step);
            }
        }

        return result;
    }

    private static float[,] Atkinson(float[,] pixels, float step)
    {
        int rows = pixels.GetLength(0), cols = pixels.GetLength(1);
        var buf = (float[,])pixels.Clone();

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                float old = Math.Clamp(buf[r, c], 0, 255);
                float @new = Quantize(old, step);
                buf[r, c] = @new;
                float err = (old - @new) / 8f;

                if (c + 1 < cols)         buf[r, c + 1]     += err;
                if (c + 2 < cols)         buf[r, c + 2]     += err;
                if (r + 1 < rows)
                {
                    if (c - 1 >= 0)       buf[r + 1, c - 1] += err;
                                          buf[r + 1, c]     += err;
                    if (c + 1 < cols)     buf[r + 1, c + 1] += err;
                }
                if (r + 2 < rows)         buf[r + 2, c]     += err;
            }
        }

        return buf;
    }
}
