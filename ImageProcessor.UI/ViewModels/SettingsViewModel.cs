using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ImageProcessor.Core;
using System;
using System.Windows.Input;

namespace ImageProcessor.UI.ViewModels
{
    public partial class SettingsViewModel : ObservableObject
    {
        private readonly Action _closeAction;

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
        public SettingsViewModel() : this(() => { }) { }

        public SettingsViewModel(Action closeAction)
        {
            _closeAction = closeAction;
            _realesrganArguments = SettingsService.Instance.RealEsrganSettings.CommandArguments;
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
            SettingsService.Instance.RealEsrganSettings.CommandArguments = RealEsrganArguments;
            SettingsService.Instance.Save();
            _closeAction();
        }

        private void Cancel()
        {
            _closeAction();
        }
    }
}