using Avalonia.Controls;
using ImageProcessor.UI.ViewModels;

namespace ImageProcessor.UI.Views;

public partial class LogView : Window
{
    // Parameterless constructor for the designer
    public LogView()
    {
        InitializeComponent();
    }

    public LogView(LogViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
