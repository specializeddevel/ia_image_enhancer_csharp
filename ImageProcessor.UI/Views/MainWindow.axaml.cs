using Avalonia;
using Avalonia.Controls;
using ImageProcessor.UI.ViewModels;
using System;
using System.ComponentModel;
using Avalonia.Interactivity;

namespace ImageProcessor.UI.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        DataContext = new MainWindowViewModel();

        var exitButton = this.FindControl<Button>("ExitButton");
        if (exitButton != null)
        {
            exitButton.Click += (sender, e) => Close();
        }

        // Handle the Loaded event to manually set the top position
        Loaded += OnLoaded;
        Closing += OnClosing;
    }

    private void OnLoaded(object? sender, EventArgs e)
    {
        // We get the screen where the window is located.
        var screen = Screens.ScreenFromVisual(this);
        if (screen != null)
        {
            // Calculate the new Y position with a 40px margin from the top.
            var newY = screen.WorkingArea.Y + 40;

            // The window is already centered horizontally by WindowStartupLocation="CenterScreen"
            // We just need to adjust the vertical position.
            Position = new PixelPoint(Position.X, newY);
        }
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
