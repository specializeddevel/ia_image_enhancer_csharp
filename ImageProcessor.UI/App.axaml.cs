
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using ImageProcessor.UI.ViewModels; // Assuming ThemeManager is in the same namespace or add correct using

namespace ImageProcessor.UI;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Load the theme before creating the main window
            ThemeManager.LoadTheme();

            desktop.MainWindow = new ImageProcessor.UI.Views.MainWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }
}
