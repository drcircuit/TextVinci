using TextVinci.Core;

namespace TextVinci.Core.Tests;

public class DitheringEngineTests
{
    private static float[,] UniformBrightness(int rows, int cols, float value)
    {
        var arr = new float[rows, cols];
        for (int r = 0; r < rows; r++)
            for (int c = 0; c < cols; c++)
                arr[r, c] = value;
        return arr;
    }

    [Theory]
    [InlineData(DitheringMode.None)]
    [InlineData(DitheringMode.FloydSteinberg)]
    [InlineData(DitheringMode.OrderedBayer4x4)]
    [InlineData(DitheringMode.Atkinson)]
    public void Apply_PreservesSize(DitheringMode mode)
    {
        var input = UniformBrightness(10, 20, 128f);
        var result = DitheringEngine.Apply(input, mode, 4);
        Assert.Equal(10, result.GetLength(0));
        Assert.Equal(20, result.GetLength(1));
    }

    [Theory]
    [InlineData(DitheringMode.None)]
    [InlineData(DitheringMode.FloydSteinberg)]
    [InlineData(DitheringMode.OrderedBayer4x4)]
    [InlineData(DitheringMode.Atkinson)]
    public void Apply_OutputValuesInRange(DitheringMode mode)
    {
        var rng = new Random(42);
        var input = new float[8, 8];
        for (int r = 0; r < 8; r++)
            for (int c = 0; c < 8; c++)
                input[r, c] = (float)(rng.NextDouble() * 255);

        var result = DitheringEngine.Apply(input, mode, 4);

        for (int r = 0; r < 8; r++)
            for (int c = 0; c < 8; c++)
                Assert.InRange(result[r, c], 0f, 255f);
    }

    [Fact]
    public void Apply_None_QuantizesUniform()
    {
        // With 2 levels, all pixels should be 0 or 255
        var input = UniformBrightness(4, 4, 64f);
        var result = DitheringEngine.Apply(input, DitheringMode.None, 2);
        for (int r = 0; r < 4; r++)
            for (int c = 0; c < 4; c++)
                Assert.True(result[r, c] == 0f || result[r, c] == 255f);
    }
}
