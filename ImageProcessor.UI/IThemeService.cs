using Avalonia.Styling;

namespace ImageProcessor.UI
{
    public interface IThemeService
    {
        void InitializeTheme();
        void SetTheme(bool isDarkMode);
        bool IsDarkMode { get; }
    }
}
