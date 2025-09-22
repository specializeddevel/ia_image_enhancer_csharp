using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ImageProcessor.Core;
using System;
using System.Windows.Input;

namespace ImageProcessor.UI.ViewModels
{
    public partial class SettingsViewModel : ObservableObject
    {
        private readonly SettingsService _settingsService;
        public event EventHandler? CloseRequested;

        [ObservableProperty]
        private int _webPQuality;

        [ObservableProperty]
        private int _avifQuality;

        private string _realesrganArguments;

        [ObservableProperty]
        private string _commandPreview;

        public string RealEsrganArguments
        {
            get => _realesrganArguments;
            set
            {
                if (SetProperty(ref _realesrganArguments, value))
                {
                    UpdatePreview();
                }
            }
        }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        // Design-time constructor
        public SettingsViewModel()
        {
            _settingsService = new SettingsService(); // For designer
            _webPQuality = 80;
            _avifQuality = 44;
            _commandPreview = string.Empty;
            _realesrganArguments = new UserSettings().RealEsrganSettings.CommandArguments;
            UpdatePreview();
            SaveCommand = new RelayCommand(Save);
            CancelCommand = new RelayCommand(Cancel);
        }

        public SettingsViewModel(SettingsService settingsService)
        {
            _settingsService = settingsService;
            _webPQuality = _settingsService.UserSettings.WebPQuality;
            _avifQuality = _settingsService.UserSettings.AvifQuality;
            _commandPreview = string.Empty; // Initialize to satisfy CS8618
            _realesrganArguments = _settingsService.UserSettings.RealEsrganSettings.CommandArguments;
            UpdatePreview();

            SaveCommand = new RelayCommand(Save);
            CancelCommand = new RelayCommand(Cancel);
        }

        private void UpdatePreview()
        {
            CommandPreview = _realesrganArguments
                .Replace("{inputFile}", "C:\\path\\to\\input.jpg")
                .Replace("{outputFile}", "C:\\path\\to\\output.png")
                .Replace("{modelName}", "realesrgan-x4plus")
                .Replace("{scale}", "4")
                .Replace("{modelsPath}", "C:\\path\\to\\models");
        }

        private void Save()
        {
            _settingsService.UserSettings.WebPQuality = WebPQuality;
            _settingsService.UserSettings.AvifQuality = AvifQuality;
            _settingsService.UserSettings.RealEsrganSettings.CommandArguments = RealEsrganArguments;
            _settingsService.Save();
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }

        private void Cancel()
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}