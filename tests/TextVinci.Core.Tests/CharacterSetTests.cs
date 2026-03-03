using TextVinci.Core;

namespace TextVinci.Core.Tests;

public class CharacterSetTests
{
    [Theory]
    [InlineData(0,   '@')]   // darkest
    [InlineData(255, ' ')]   // lightest
    public void Map_Standard_ReturnsExpectedEndChars(int brightness, char expected)
    {
        char result = CharacterSet.Map(CharacterSet.Standard, brightness);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Map_EmptyCharset_ReturnsSpace()
    {
        Assert.Equal(' ', CharacterSet.Map("", 128));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(127)]
    [InlineData(255)]
    public void Map_AlwaysReturnsCharInSet(int brightness)
    {
        char result = CharacterSet.Map(CharacterSet.Extended, brightness);
        Assert.Contains(result, CharacterSet.Extended);
    }
}
