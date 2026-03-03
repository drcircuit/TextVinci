using System.Drawing;
using TextVinci.Core;

namespace TextVinci.Core.Tests;

public class ColorGradientTests
{
    [Fact]
    public void Evaluate_TwoStops_ReturnsEndpoints()
    {
        var g = new ColorGradient(Color.Black, Color.White);
        var atZero = g.Evaluate(0.0);
        var atOne  = g.Evaluate(1.0);
        Assert.Equal(Color.Black.ToArgb(), atZero.ToArgb());
        Assert.Equal(Color.White.ToArgb(), atOne.ToArgb());
    }

    [Fact]
    public void Evaluate_Midpoint_InterpolatesCorrectly()
    {
        var g = new ColorGradient(Color.FromArgb(0, 0, 0), Color.FromArgb(200, 200, 200));
        var mid = g.Evaluate(0.5);
        Assert.Equal(100, mid.R);
        Assert.Equal(100, mid.G);
        Assert.Equal(100, mid.B);
    }

    [Fact]
    public void Evaluate_OutOfRange_Clamps()
    {
        var g = new ColorGradient(Color.Red, Color.Blue);
        Assert.Equal(g.Evaluate(0.0).ToArgb(), g.Evaluate(-1.0).ToArgb());
        Assert.Equal(g.Evaluate(1.0).ToArgb(), g.Evaluate(2.0).ToArgb());
    }

    [Fact]
    public void AddStop_SortsStops()
    {
        var g = new ColorGradient();
        g.AddStop(1.0, Color.White);
        g.AddStop(0.0, Color.Black);
        g.AddStop(0.5, Color.Gray);
        // Evaluate at 0 should return black (R=G=B=0)
        var c = g.Evaluate(0.0);
        Assert.Equal(0, c.R);
        Assert.Equal(0, c.G);
        Assert.Equal(0, c.B);
    }

    [Fact]
    public void PredefinedGradient_Fire_HasStops()
    {
        var fire = ColorGradient.Fire;
        Assert.True(fire.StopCount >= 2);
    }

    [Fact]
    public void EmptyGradient_ReturnsWhite()
    {
        var g = new ColorGradient();
        var c = g.Evaluate(0.5);
        Assert.Equal(Color.White.ToArgb(), c.ToArgb());
    }
}
