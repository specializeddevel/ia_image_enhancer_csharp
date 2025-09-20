using Avalonia;
using ImageProcessor.Core;
using ImageProcessor.UI.ViewModels;
using ImageProcessor.UI.Views;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace ImageProcessor.UI;

class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        var services = new ServiceCollection();
        ConfigureServices(services);
        var serviceProvider = services.BuildServiceProvider();

        BuildAvaloniaApp(serviceProvider)
            .StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp(IServiceProvider serviceProvider)
        => AppBuilder.Configure(() => new App(serviceProvider))
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();

    private static void ConfigureServices(IServiceCollection services)
    {
        // Core Services
        services.AddSingleton<ImageProcessorService>();
        services.AddSingleton<ProcessingLogService>();
        services.AddSingleton<SettingsService>();

        // ViewModels
        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<SettingsViewModel>(); 
        services.AddTransient<LogViewModel>();

        // Views
        services.AddTransient<MainWindow>();
        services.AddTransient<SettingsView>();
        services.AddTransient<LogView>();
    }
}
