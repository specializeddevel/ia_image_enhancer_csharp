using System;
using System.IO;
using System.Text.Json;
using Avalonia;
using Avalonia.Styling;

namespace ImageProcessor.UI;

public class AppSettings
{
    public string Theme { get; set; } = "Light";
}

public static class ThemeManager
{
    private static readonly string SettingsFilePath = Path.Combine(AppContext.BaseDirectory, "settings.json");

    public static void SetTheme(ThemeVariant themeVariant)
    {
        if (Application.Current != null)
        {
            Application.Current.RequestedThemeVariant = themeVariant;
        }
    }

    public static void SaveTheme(ThemeVariant themeVariant)
    {
        var settings = new AppSettings
        {
            Theme = themeVariant.ToString()
        };
        var json = JsonSerializer.Serialize(settings);
        File.WriteAllText(SettingsFilePath, json);
    }

    public static ThemeVariant LoadTheme()
    {
        if (File.Exists(SettingsFilePath))
        {
            var json = File.ReadAllText(SettingsFilePath);
            var settings = JsonSerializer.Deserialize<AppSettings>(json);
            if (settings != null)
            {
                if (settings.Theme == "Dark")
                {
                    SetTheme(ThemeVariant.Dark);
                    return ThemeVariant.Dark;
                }
            }
        }

        // Default to Light theme
        SetTheme(ThemeVariant.Light);
        return ThemeVariant.Light;
    }
}
