using Avalonia;
using Avalonia.Styling;
using ImageProcessor.Core;

namespace ImageProcessor.UI
{
    public class ThemeService : IThemeService
    {
        private readonly SettingsService _settingsService;

        public ThemeService(SettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        public bool IsDarkMode => _settingsService.UserSettings.IsDarkMode;

        public void InitializeTheme()
        {
            ApplyTheme(IsDarkMode);
        }

        public void SetTheme(bool isDarkMode)
        {
            ApplyTheme(isDarkMode);

            _settingsService.UserSettings.IsDarkMode = isDarkMode;
            _settingsService.Save();
        }

        private void ApplyTheme(bool isDarkMode)
        {
            if (Application.Current != null)
            {
                Application.Current.RequestedThemeVariant = isDarkMode ? ThemeVariant.Dark : ThemeVariant.Light;
            }
        }
    }
}
