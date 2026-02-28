using System.Drawing;

namespace TextVinci.Core;

/// <summary>
/// The ASCII art canvas — a 2D grid of <see cref="AsciiCell"/> instances.
/// </summary>
public sealed class AsciiCanvas
{
    private readonly AsciiCell[,] _cells;

    public int Width { get; }
    public int Height { get; }

    public AsciiCanvas(int width, int height)
    {
        if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
        if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height));
        Width = width;
        Height = height;
        _cells = new AsciiCell[width, height];
        Fill(' ', Color.White, null);
    }

    /// <summary>Returns the cell at the given column (x) and row (y).</summary>
    public AsciiCell GetCell(int x, int y)
    {
        ValidateCoords(x, y);
        return _cells[x, y];
    }

    /// <summary>Sets the cell at the given column (x) and row (y).</summary>
    public void SetCell(int x, int y, AsciiCell cell)
    {
        ValidateCoords(x, y);
        _cells[x, y] = cell ?? throw new ArgumentNullException(nameof(cell));
    }

    /// <summary>Sets a character and colors at the given position.</summary>
    public void SetCell(int x, int y, char ch, Color fg, Color? bg = null)
    {
        ValidateCoords(x, y);
        _cells[x, y] = new AsciiCell(ch, fg, bg);
    }

    /// <summary>Fills the entire canvas with the given character and colors.</summary>
    public void Fill(char ch, Color fg, Color? bg = null)
    {
        for (int x = 0; x < Width; x++)
            for (int y = 0; y < Height; y++)
                _cells[x, y] = new AsciiCell(ch, fg, bg);
    }

    /// <summary>Returns true if the coordinates are within the canvas bounds.</summary>
    public bool InBounds(int x, int y) => x >= 0 && x < Width && y >= 0 && y < Height;

    /// <summary>Flood-fills from (startX, startY) with the given cell attributes.</summary>
    public void FloodFill(int startX, int startY, char ch, Color fg, Color? bg = null)
    {
        if (!InBounds(startX, startY)) return;

        char targetChar = _cells[startX, startY].Character;
        Color targetFg = _cells[startX, startY].ForegroundColor;

        if (targetChar == ch && targetFg == fg) return;

        var stack = new Stack<(int x, int y)>();
        stack.Push((startX, startY));

        while (stack.Count > 0)
        {
            var (x, y) = stack.Pop();
            if (!InBounds(x, y)) continue;

            var cell = _cells[x, y];
            if (cell.Character != targetChar || cell.ForegroundColor != targetFg) continue;

            _cells[x, y] = new AsciiCell(ch, fg, bg);

            stack.Push((x - 1, y));
            stack.Push((x + 1, y));
            stack.Push((x, y - 1));
            stack.Push((x, y + 1));
        }
    }

    /// <summary>Creates a deep copy of this canvas.</summary>
    public AsciiCanvas Clone()
    {
        var clone = new AsciiCanvas(Width, Height);
        for (int x = 0; x < Width; x++)
            for (int y = 0; y < Height; y++)
                clone._cells[x, y] = _cells[x, y].Clone();
        return clone;
    }

    /// <summary>Resize the canvas, preserving existing content where possible.</summary>
    public AsciiCanvas Resize(int newWidth, int newHeight)
    {
        var newCanvas = new AsciiCanvas(newWidth, newHeight);
        int copyW = Math.Min(Width, newWidth);
        int copyH = Math.Min(Height, newHeight);
        for (int x = 0; x < copyW; x++)
            for (int y = 0; y < copyH; y++)
                newCanvas._cells[x, y] = _cells[x, y].Clone();
        return newCanvas;
    }

    private void ValidateCoords(int x, int y)
    {
        if (!InBounds(x, y))
            throw new ArgumentOutOfRangeException($"Coordinates ({x},{y}) are out of bounds for canvas {Width}x{Height}.");
    }
}
