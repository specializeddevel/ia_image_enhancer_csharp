using Avalonia.Controls;
using ImageProcessor.UI.ViewModels;

namespace ImageProcessor.UI.Views
{
    public partial class SettingsView : Window
    {
        public SettingsView()
        {
            InitializeComponent();
            DataContext = new SettingsViewModel(Close);
        }
    }
}
