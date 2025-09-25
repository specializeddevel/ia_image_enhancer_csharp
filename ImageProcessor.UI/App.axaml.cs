
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using ImageProcessor.UI.Views;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace ImageProcessor.UI;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;

    private readonly IServiceProvider _serviceProvider;

    // This constructor is used by the designer.
    public App()
    {
        _serviceProvider = new ServiceCollection().BuildServiceProvider(); // Empty provider for designer
    }

    public App(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        Services = _serviceProvider;
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var themeService = Services.GetRequiredService<IThemeService>();
            themeService.InitializeTheme();

            desktop.MainWindow = Services.GetRequiredService<MainWindow>();
        }

        base.OnFrameworkInitializationCompleted();
    }
}
