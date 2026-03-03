using System;
using System.Threading.Tasks;
using System.Drawing;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Color = System.Drawing.Color;

namespace TextVinci.Views;

/// <summary>
/// A minimal color picker dialog that lets the user enter an HTML hex color.
/// </summary>
public class ColorPickerDialog : Window
{
    private readonly TextBox _hexInput;
    private readonly Border _preview;

    public ColorPickerDialog(Color initialColor)
    {
        Title = "Pick Color";
        Width = 300;
        Height = 180;
        CanResize = false;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;

        _hexInput = new TextBox
        {
            Text = $"#{initialColor.R:X2}{initialColor.G:X2}{initialColor.B:X2}",
            Watermark = "#RRGGBB",
            Margin = new Avalonia.Thickness(8)
        };

        _preview = new Border
        {
            Height = 30,
            Margin = new Avalonia.Thickness(8, 0),
            CornerRadius = new Avalonia.CornerRadius(4),
            BorderThickness = new Avalonia.Thickness(1),
            BorderBrush = Brushes.Gray,
            Background = new SolidColorBrush(new Avalonia.Media.Color(
                initialColor.A, initialColor.R, initialColor.G, initialColor.B))
        };

        _hexInput.TextChanged += (_, _) => UpdatePreview();

        var okBtn = new Button { Content = "OK", Width = 80, HorizontalAlignment = HorizontalAlignment.Center };
        okBtn.Click += OkClicked;

        var cancelBtn = new Button { Content = "Cancel", Width = 80, HorizontalAlignment = HorizontalAlignment.Center };
        cancelBtn.Click += (_, _) => Close(null);

        Content = new StackPanel
        {
            Spacing = 4,
            Margin = new Avalonia.Thickness(0, 8),
            Children =
            {
                new TextBlock { Text = "Enter hex color:", Margin = new Avalonia.Thickness(8, 0) },
                _hexInput,
                _preview,
                new StackPanel
                {
                    Orientation = Avalonia.Layout.Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Spacing = 8,
                    Children = { okBtn, cancelBtn }
                }
            }
        };
    }

    private void UpdatePreview()
    {
        var color = ParseHex(_hexInput.Text);
        if (color.HasValue)
            _preview.Background = new SolidColorBrush(
                new Avalonia.Media.Color(color.Value.A, color.Value.R, color.Value.G, color.Value.B));
    }

    private void OkClicked(object? sender, RoutedEventArgs e)
    {
        Close(ParseHex(_hexInput.Text));
    }

    private static Color? ParseHex(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        var s = text.Trim().TrimStart('#');
        if (s.Length == 6 && int.TryParse(s, System.Globalization.NumberStyles.HexNumber, null, out int rgb))
        {
            byte r = (byte)((rgb >> 16) & 0xFF);
            byte g = (byte)((rgb >> 8) & 0xFF);
            byte b = (byte)(rgb & 0xFF);
            return Color.FromArgb(255, r, g, b);
        }
        return null;
    }
}
