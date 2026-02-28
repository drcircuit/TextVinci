using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Avalonia.Platform.Storage;
using TextVinci.Core;
using Color = System.Drawing.Color;

namespace TextVinci.ViewModels;

/// <summary>
/// Available paint tools for the canvas editor.
/// </summary>
public enum PaintTool
{
    Pencil,
    Eraser,
    FloodFill,
    ColorPicker
}

/// <summary>
/// Main application ViewModel — owns the canvas, selected tool, and commands.
/// </summary>
public partial class MainWindowViewModel : ViewModelBase
{
    // ── Canvas state ─────────────────────────────────────────────────

    [ObservableProperty] private AsciiCanvas _canvas;
    [ObservableProperty] private int _canvasWidth = 120;
    [ObservableProperty] private int _canvasHeight = 40;

    // ── Tool state ────────────────────────────────────────────────────

    [ObservableProperty] private PaintTool _selectedTool = PaintTool.Pencil;
    [ObservableProperty] private char _paintChar = '#';
    [ObservableProperty] private Color _foregroundColor = Color.White;
    [ObservableProperty] private Color? _backgroundColor = null;

    // ── Conversion settings ───────────────────────────────────────────

    [ObservableProperty] private string _selectedCharacterSet = CharacterSet.Standard;
    [ObservableProperty] private bool _useColors = true;
    [ObservableProperty] private DitheringMode _selectedDithering = DitheringMode.FloydSteinberg;
    [ObservableProperty] private int _ditheringLevels = 8;
    [ObservableProperty] private bool _invertBrightness = false;
    [ObservableProperty] private bool _useColorGradient = false;
    [ObservableProperty] private string _selectedGradientName = "None";

    // ── Reference image ───────────────────────────────────────────────

    [ObservableProperty] private string? _referenceImagePath;
    [ObservableProperty] private double _referenceImageOpacity = 0.3;
    [ObservableProperty] private bool _showReferenceImage = true;

    // ── View state ────────────────────────────────────────────────────

    [ObservableProperty] private double _zoom = 1.0;
    [ObservableProperty] private int _cursorX = 0;
    [ObservableProperty] private int _cursorY = 0;
    [ObservableProperty] private string _statusMessage = "Ready";
    [ObservableProperty] private int _cellWidth = 10;
    [ObservableProperty] private int _cellHeight = 18;

    // ── Collections ───────────────────────────────────────────────────

    public ObservableCollection<string> CharacterSets { get; } = new()
    {
        CharacterSet.Standard,
        CharacterSet.Extended,
        CharacterSet.Blocks,
        CharacterSet.Simple,
        CharacterSet.Numeric
    };

    public ObservableCollection<string> CharacterSetNames { get; } = new()
    {
        "Standard",
        "Extended",
        "Block",
        "Simple",
        "Numeric"
    };

    public ObservableCollection<DitheringMode> DitheringModes { get; } = new()
    {
        DitheringMode.None,
        DitheringMode.FloydSteinberg,
        DitheringMode.OrderedBayer4x4,
        DitheringMode.Atkinson
    };

    public ObservableCollection<string> GradientNames { get; } = new()
    {
        "None",
        "Black → White",
        "White → Black",
        "Fire",
        "Ocean",
        "Matrix"
    };

    // ── Constructor ───────────────────────────────────────────────────

    public MainWindowViewModel()
    {
        _canvas = new AsciiCanvas(_canvasWidth, _canvasHeight);
    }

    // ── Canvas actions ────────────────────────────────────────────────

    /// <summary>Apply the current paint tool at the given canvas coordinate.</summary>
    public void ApplyTool(int x, int y)
    {
        if (!Canvas.InBounds(x, y)) return;

        switch (SelectedTool)
        {
            case PaintTool.Pencil:
                Canvas.SetCell(x, y, PaintChar, ForegroundColor, BackgroundColor);
                break;

            case PaintTool.Eraser:
                Canvas.SetCell(x, y, ' ', Color.White, null);
                break;

            case PaintTool.FloodFill:
                Canvas.FloodFill(x, y, PaintChar, ForegroundColor, BackgroundColor);
                break;

            case PaintTool.ColorPicker:
                var cell = Canvas.GetCell(x, y);
                PaintChar = cell.Character;
                ForegroundColor = cell.ForegroundColor;
                BackgroundColor = cell.BackgroundColor;
                StatusMessage = $"Picked: '{cell.Character}' FG={cell.ForegroundColor.Name}";
                break;
        }

        CursorX = x;
        CursorY = y;
    }

    // ── Commands ──────────────────────────────────────────────────────

    [RelayCommand]
    private void NewCanvas()
    {
        Canvas = new AsciiCanvas(CanvasWidth, CanvasHeight);
        StatusMessage = $"New canvas {CanvasWidth}×{CanvasHeight}";
    }

    [RelayCommand]
    private async Task OpenImage(object? ownerWindow)
    {
        var window = ownerWindow as Avalonia.Controls.Window;
        if (window == null) return;
        var topLevel = Avalonia.Controls.TopLevel.GetTopLevel(window);
        if (topLevel == null) return;
        var files = await topLevel.StorageProvider.OpenFilePickerAsync(
            new Avalonia.Platform.Storage.FilePickerOpenOptions
            {
                Title = "Open Image",
                AllowMultiple = false,
                FileTypeFilter = new[]
                {
                    new Avalonia.Platform.Storage.FilePickerFileType("Images")
                    {
                        Patterns = new[] { "*.png", "*.jpg", "*.jpeg", "*.bmp", "*.gif", "*.tiff", "*.webp" }
                    }
                }
            });

        if (files is { Count: > 0 })
        {
            var path = files[0].TryGetLocalPath();
            if (path != null)
                await ConvertImageToAsciiAsync(path);
        }
    }

    [RelayCommand]
    private async Task OpenReferenceImage(object? ownerWindow)
    {
        var window = ownerWindow as Avalonia.Controls.Window;
        if (window == null) return;
        var topLevel = Avalonia.Controls.TopLevel.GetTopLevel(window);
        if (topLevel == null) return;
        var files = await topLevel.StorageProvider.OpenFilePickerAsync(
            new Avalonia.Platform.Storage.FilePickerOpenOptions
            {
                Title = "Open Reference Image",
                AllowMultiple = false,
                FileTypeFilter = new[]
                {
                    new Avalonia.Platform.Storage.FilePickerFileType("Images")
                    {
                        Patterns = new[] { "*.png", "*.jpg", "*.jpeg", "*.bmp", "*.gif", "*.tiff" }
                    }
                }
            });

        if (files is { Count: > 0 })
        {
            ReferenceImagePath = files[0].TryGetLocalPath();
            ShowReferenceImage = true;
            StatusMessage = $"Reference image: {System.IO.Path.GetFileName(ReferenceImagePath)}";
        }
    }

    [RelayCommand]
    private async Task ExportAscii(object? ownerWindow)
    {
        var window = ownerWindow as Avalonia.Controls.Window;
        if (window == null) return;
        var topLevel = Avalonia.Controls.TopLevel.GetTopLevel(window);
        if (topLevel == null) return;
        var file = await topLevel.StorageProvider.SaveFilePickerAsync(
            new Avalonia.Platform.Storage.FilePickerSaveOptions
            {
                Title = "Export ASCII Art",
                SuggestedFileName = "art.txt",
                FileTypeChoices = new[]
                {
                    new Avalonia.Platform.Storage.FilePickerFileType("Text file") { Patterns = new[] { "*.txt" } }
                }
            });

        if (file != null)
        {
            var path = file.TryGetLocalPath();
            if (path != null)
            {
                AsciiExporter.SaveToFile(Canvas, path);
                StatusMessage = $"Exported ASCII to {System.IO.Path.GetFileName(path)}";
            }
        }
    }

    [RelayCommand]
    private async Task ExportAnsi(object? ownerWindow)
    {
        var window = ownerWindow as Avalonia.Controls.Window;
        if (window == null) return;
        var topLevel = Avalonia.Controls.TopLevel.GetTopLevel(window);
        if (topLevel == null) return;
        var file = await topLevel.StorageProvider.SaveFilePickerAsync(
            new Avalonia.Platform.Storage.FilePickerSaveOptions
            {
                Title = "Export ANSI Art",
                SuggestedFileName = "art.ans",
                FileTypeChoices = new[]
                {
                    new Avalonia.Platform.Storage.FilePickerFileType("ANSI file") { Patterns = new[] { "*.ans", "*.txt" } }
                }
            });

        if (file != null)
        {
            var path = file.TryGetLocalPath();
            if (path != null)
            {
                AnsiExporter.SaveToFile(Canvas, path);
                StatusMessage = $"Exported ANSI to {System.IO.Path.GetFileName(path)}";
            }
        }
    }

    private async Task ConvertImageToAsciiAsync(string imagePath)
    {
        try
        {
            StatusMessage = "Converting...";
            var gradient = SelectedGradientName switch
            {
                "Black → White" => Core.ColorGradient.BlackToWhite,
                "White → Black" => Core.ColorGradient.WhiteToBlack,
                "Fire"          => Core.ColorGradient.Fire,
                "Ocean"         => Core.ColorGradient.Ocean,
                "Matrix"        => Core.ColorGradient.Matrix,
                _               => null
            };

            var settings = new ConversionSettings
            {
                OutputWidth      = CanvasWidth,
                OutputHeight     = 0, // auto-calculate from aspect ratio
                CharacterSet     = SelectedCharacterSet,
                UseColors        = UseColors,
                Dithering        = SelectedDithering,
                DitheringLevels  = DitheringLevels,
                InvertBrightness = InvertBrightness,
                UseColorGradient = UseColorGradient && gradient != null,
                ColorGradient    = gradient
            };

            var converted = await Task.Run(() => AsciiConverter.Convert(imagePath, settings));
            Canvas       = converted;
            CanvasWidth  = converted.Width;
            CanvasHeight = converted.Height;
            StatusMessage = $"Converted: {System.IO.Path.GetFileName(imagePath)} → {converted.Width}×{converted.Height}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
    }

    partial void OnCanvasWidthChanged(int value) { }
    partial void OnCanvasHeightChanged(int value) { }
    partial void OnZoomChanged(double value)
    {
        StatusMessage = $"Zoom: {value:P0}";
    }
}
