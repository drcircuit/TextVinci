using System;
using System.IO;
using Avalonia.Controls;
using Avalonia.Interactivity;
using TextVinci.Controls;
using TextVinci.ViewModels;

namespace TextVinci.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        // Wire up canvas paint events after initialization
        Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        if (this.FindControl<AsciiCanvasControl>("AsciiEditor") is { } editor)
        {
            editor.CellPainted += OnCellPainted;
        }
    }

    private void OnCellPainted(object? sender, (int x, int y) cell)
    {
        if (DataContext is MainWindowViewModel vm)
        {
            vm.ApplyTool(cell.x, cell.y);
            // Trigger re-render
            if (sender is AsciiCanvasControl ctrl)
                ctrl.InvalidateVisual();
        }
    }

    private void OnExitClicked(object? sender, RoutedEventArgs e) => Close();

    private void OnZoomIn(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
            vm.Zoom = Math.Min(4.0, vm.Zoom + 0.25);
    }

    private void OnZoomOut(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
            vm.Zoom = Math.Max(0.25, vm.Zoom - 0.25);
    }

    private void OnZoomReset(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
            vm.Zoom = 1.0;
    }

    private void OnCharSetChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel vm) return;
        if (sender is not ComboBox cb) return;
        int idx = cb.SelectedIndex;
        if (idx >= 0 && idx < vm.CharacterSets.Count)
            vm.SelectedCharacterSet = vm.CharacterSets[idx];
    }

    private async void OnPickForeground(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel vm) return;

        // Simple color input dialog — prompt the user to type an HTML color (e.g. #FF8800)
        var dialog = new ColorPickerDialog(vm.ForegroundColor);
        var result = await dialog.ShowDialog<System.Drawing.Color?>(this);
        if (result.HasValue)
            vm.ForegroundColor = result.Value;
    }
}
