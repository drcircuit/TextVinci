using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using TextVinci.ViewModels;
using DrawingColor = System.Drawing.Color;

namespace TextVinci.ViewModels;

/// <summary>
/// Converts a <see cref="PaintTool"/> value to a boolean indicating whether the
/// given tool (via ConverterParameter) is selected.  Used for ToggleButton.IsChecked.
/// </summary>
public sealed class ToolConverter : IValueConverter
{
    public static readonly ToolConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is PaintTool tool && parameter is string name &&
            Enum.TryParse<PaintTool>(name, out var target))
            return tool == target;
        return false;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is true && parameter is string name &&
            Enum.TryParse<PaintTool>(name, out var tool))
            return tool;
        return Avalonia.Data.BindingOperations.DoNothing;
    }
}

/// <summary>Converts a <see cref="System.Drawing.Color"/> to an Avalonia <see cref="IBrush"/>.</summary>
public sealed class DrawingColorConverter : IValueConverter
{
    public static readonly DrawingColorConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is DrawingColor dc)
            return new SolidColorBrush(new Color(dc.A, dc.R, dc.G, dc.B));
        return Brushes.White;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}

/// <summary>Converts a char to/from a single-character string.</summary>
public sealed class CharConverter : IValueConverter
{
    public static readonly CharConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is char c ? c.ToString() : "#";

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string s && s.Length > 0) return s[0];
        return '#';
    }
}

/// <summary>Returns true when value is not null or empty string.</summary>
public sealed class NullToBoolConverter : IValueConverter
{
    public static readonly NullToBoolConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is string s ? !string.IsNullOrEmpty(s) : value != null;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}
