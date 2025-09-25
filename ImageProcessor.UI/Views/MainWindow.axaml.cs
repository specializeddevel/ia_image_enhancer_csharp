using Avalonia;
using Avalonia.Controls;
using ImageProcessor.UI.ViewModels;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform.Storage;

namespace ImageProcessor.UI.Views;

public partial class MainWindow : Window
{
    // Parameterless constructor for designer support
    public MainWindow() 
    {
        InitializeComponent();
    }

    public MainWindow(MainWindowViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;

        var border = this.FindControl<Border>("DropTargetBorder");
        if (border != null)
        {
            DragDrop.SetAllowDrop(border, true);
        }

        var exitButton = this.FindControl<Button>("ExitButton");
        if (exitButton != null)
        {
            exitButton.Click += (sender, e) => Close();
        }

        // Handle the Loaded event to manually set the top position
        Loaded += OnLoaded;
        Closing += OnClosing;
    }

    private void OnDragOver(object? sender, DragEventArgs e)
    {
        // Check if the dragged data contains files or folders
        if (e.Data.Contains(DataFormats.Files))
        {
            e.DragEffects = DragDropEffects.Copy;
            DropTargetBorder.Background = new SolidColorBrush(Colors.DodgerBlue, 0.2); // Visual feedback
        }
        else
        {
            e.DragEffects = DragDropEffects.None;
        }
    }

    private void OnDragLeave(object? sender, DragEventArgs e)
    {
        DropTargetBorder.Background = Brushes.Transparent; // Reset visual feedback
    }

    private void OnDrop(object? sender, DragEventArgs e)
    {
        DropTargetBorder.Background = Brushes.Transparent; // Reset visual feedback

        if (e.Data.GetFiles() is { } files && files.Any())
        {
            var path = files.First().TryGetLocalPath();
            if (path != null)
            {
                if (DataContext is MainWindowViewModel vm)
                {
                    // If the dropped item is a file, use its parent directory.
                    // If it's a directory, use the directory itself.
                    if (File.Exists(path))
                    {
                        vm.InputFolder = Path.GetDirectoryName(path) ?? string.Empty;
                    }
                    else if (Directory.Exists(path))
                    {
                        vm.InputFolder = path;
                    }
                }
            }
        }
    }

    private void OnLoaded(object? sender, EventArgs e)
    {
  
    }

    private async void OnClosing(object? sender, WindowClosingEventArgs e)
    {
        if (DataContext is not MainWindowViewModel vm) return;

        if (vm.IsProcessing)
        {
            // We need to cancel the default closing event to show our dialog
            e.Cancel = true;

            var dialog = new ConfirmCloseWindow();
            var result = await dialog.ShowDialog<bool>(this);

            if (result)
            {
                vm.CancelProcessingCommand.Execute(null);
                // Now that the process is canceled, we can close the window for real
                Closing -= OnClosing; // Unsubscribe to avoid re-triggering this logic
                Close();
            }
        }
    }
}
