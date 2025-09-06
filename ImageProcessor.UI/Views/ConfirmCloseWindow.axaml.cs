using Avalonia.Controls;
using Avalonia.Interactivity;

namespace ImageProcessor.UI.Views;

public partial class ConfirmCloseWindow : Window
{
    public ConfirmCloseWindow()
    {
        InitializeComponent();
    }

    private void YesButton_Click(object? sender, RoutedEventArgs e)
    {
        Close(true);
    }

    private void NoButton_Click(object? sender, RoutedEventArgs e)
    {
        Close(false);
    }
}
