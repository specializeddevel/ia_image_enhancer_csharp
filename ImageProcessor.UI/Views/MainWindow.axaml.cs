using Avalonia;
using Avalonia.Controls;
using ImageProcessor.UI.ViewModels;
using System;

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
}