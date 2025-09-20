using Avalonia.Controls;
using ImageProcessor.UI.ViewModels;

namespace ImageProcessor.UI.Views
{
    public partial class SettingsView : Window
    {
        // For designer
        public SettingsView()
        {
            InitializeComponent();
        }

        public SettingsView(SettingsViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            viewModel.CloseRequested += (sender, e) => Close();
        }
    }
}
