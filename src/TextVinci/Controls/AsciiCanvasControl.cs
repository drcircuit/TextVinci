using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using TextVinci.Core;
using DrawingColor = System.Drawing.Color;

namespace TextVinci.Controls;

/// <summary>
/// A custom Avalonia control that renders an <see cref="AsciiCanvas"/> as colored
/// monospace characters and handles mouse-based paint interactions.
/// </summary>
public class AsciiCanvasControl : Control
{
    // ── Styled properties ─────────────────────────────────────────────

    public static readonly StyledProperty<AsciiCanvas?> CanvasSourceProperty =
        AvaloniaProperty.Register<AsciiCanvasControl, AsciiCanvas?>(nameof(CanvasSource));

    public static readonly StyledProperty<double> ZoomProperty =
        AvaloniaProperty.Register<AsciiCanvasControl, double>(nameof(Zoom), 1.0);

    public static readonly StyledProperty<int> CellWidthProperty =
        AvaloniaProperty.Register<AsciiCanvasControl, int>(nameof(CellWidth), 10);

    public static readonly StyledProperty<int> CellHeightProperty =
        AvaloniaProperty.Register<AsciiCanvasControl, int>(nameof(CellHeight), 18);

    public static readonly StyledProperty<string?> ReferenceImagePathProperty =
        AvaloniaProperty.Register<AsciiCanvasControl, string?>(nameof(ReferenceImagePath));

    public static readonly StyledProperty<double> ReferenceImageOpacityProperty =
        AvaloniaProperty.Register<AsciiCanvasControl, double>(nameof(ReferenceImageOpacity), 0.3);

    public static readonly StyledProperty<bool> ShowReferenceImageProperty =
        AvaloniaProperty.Register<AsciiCanvasControl, bool>(nameof(ShowReferenceImage), true);

    // ── CLR properties ────────────────────────────────────────────────

    public AsciiCanvas? CanvasSource
    {
        get => GetValue(CanvasSourceProperty);
        set => SetValue(CanvasSourceProperty, value);
    }

    public double Zoom
    {
        get => GetValue(ZoomProperty);
        set => SetValue(ZoomProperty, value);
    }

    public int CellWidth
    {
        get => GetValue(CellWidthProperty);
        set => SetValue(CellWidthProperty, value);
    }

    public int CellHeight
    {
        get => GetValue(CellHeightProperty);
        set => SetValue(CellHeightProperty, value);
    }

    public string? ReferenceImagePath
    {
        get => GetValue(ReferenceImagePathProperty);
        set => SetValue(ReferenceImagePathProperty, value);
    }

    public double ReferenceImageOpacity
    {
        get => GetValue(ReferenceImageOpacityProperty);
        set => SetValue(ReferenceImageOpacityProperty, value);
    }

    public bool ShowReferenceImage
    {
        get => GetValue(ShowReferenceImageProperty);
        set => SetValue(ShowReferenceImageProperty, value);
    }

    // ── Events ────────────────────────────────────────────────────────

    /// <summary>Raised when the user clicks or drags on a cell.</summary>
    public event EventHandler<(int x, int y)>? CellPainted;

    // ── Internal fields ───────────────────────────────────────────────

    private Bitmap? _referenceImage;
    private string? _loadedImagePath;
    private Typeface _typeface = new Typeface("Cascadia Mono, Consolas, Courier New, monospace");
    private bool _isPainting;

    // ── Property change wiring ────────────────────────────────────────

    static AsciiCanvasControl()
    {
        AffectsRender<AsciiCanvasControl>(
            CanvasSourceProperty, ZoomProperty, CellWidthProperty, CellHeightProperty,
            ReferenceImagePathProperty, ReferenceImageOpacityProperty, ShowReferenceImageProperty);

        AffectsMeasure<AsciiCanvasControl>(
            CanvasSourceProperty, ZoomProperty, CellWidthProperty, CellHeightProperty);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ReferenceImagePathProperty)
            LoadReferenceImage(change.GetNewValue<string?>());
    }

    // ── Measure/Arrange ───────────────────────────────────────────────

    protected override Size MeasureOverride(Size availableSize)
    {
        var canvas = CanvasSource;
        if (canvas == null) return new Size(100, 50);
        double cw = CellWidth * Zoom;
        double ch = CellHeight * Zoom;
        return new Size(canvas.Width * cw, canvas.Height * ch);
    }

    // ── Rendering ─────────────────────────────────────────────────────

    public override void Render(DrawingContext ctx)
    {
        var canvas = CanvasSource;
        if (canvas == null) return;

        double cw = CellWidth * Zoom;
        double ch = CellHeight * Zoom;
        double totalW = canvas.Width * cw;
        double totalH = canvas.Height * ch;

        // Background
        ctx.FillRectangle(Brushes.Black, new Rect(0, 0, totalW, totalH));

        // Reference image layer
        if (ShowReferenceImage && _referenceImage != null)
        {
            using var _ = ctx.PushOpacity(ReferenceImageOpacity);
            ctx.DrawImage(_referenceImage, new Rect(0, 0, totalW, totalH));
        }

        // ASCII characters
        double fontSize = Math.Max(4, ch * 0.85);
        for (int y = 0; y < canvas.Height; y++)
        {
            for (int x = 0; x < canvas.Width; x++)
            {
                var cell = canvas.GetCell(x, y);
                if (cell.Character == ' ') continue;

                // Background fill
                if (cell.BackgroundColor.HasValue)
                {
                    var bg = cell.BackgroundColor.Value;
                    ctx.FillRectangle(
                        new SolidColorBrush(new Avalonia.Media.Color(bg.A, bg.R, bg.G, bg.B)),
                        new Rect(x * cw, y * ch, cw, ch));
                }

                // Foreground character
                var fg = cell.ForegroundColor;
                var brush = new SolidColorBrush(new Avalonia.Media.Color(fg.A, fg.R, fg.G, fg.B));
                var ft = new FormattedText(
                    cell.Character.ToString(),
                    System.Globalization.CultureInfo.InvariantCulture,
                    FlowDirection.LeftToRight,
                    _typeface,
                    fontSize,
                    brush);

                ctx.DrawText(ft, new Point(x * cw, y * ch));
            }
        }
    }

    // ── Mouse input ───────────────────────────────────────────────────

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        _isPainting = true;
        HitTestCell(e.GetPosition(this));
        e.Handled = true;
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        if (_isPainting)
            HitTestCell(e.GetPosition(this));
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        _isPainting = false;
    }

    private void HitTestCell(Point pos)
    {
        double cw = CellWidth * Zoom;
        double ch = CellHeight * Zoom;
        int cx = (int)(pos.X / cw);
        int cy = (int)(pos.Y / ch);
        CellPainted?.Invoke(this, (cx, cy));
        InvalidateVisual();
    }

    // ── Reference image loading ───────────────────────────────────────

    private void LoadReferenceImage(string? path)
    {
        if (path == _loadedImagePath) return;
        _referenceImage?.Dispose();
        _referenceImage = null;
        _loadedImagePath = null;

        if (!string.IsNullOrEmpty(path) && System.IO.File.Exists(path))
        {
            try
            {
                _referenceImage = new Bitmap(path);
                _loadedImagePath = path;
            }
            catch
            {
                // silently ignore load errors
            }
        }
    }
}
