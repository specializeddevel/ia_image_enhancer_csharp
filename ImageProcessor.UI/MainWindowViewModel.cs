using Avalonia;
using Avalonia.Controls;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ImageProcessor.Core;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using System.Linq;
using ImageProcessor.UI.Views;
using ImageProcessor.UI.ViewModels;

namespace ImageProcessor.UI.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly ImageProcessorService _processorService;
    private readonly ProcessingLogService _logService;

    [ObservableProperty]
    private string _inputFolder = string.Empty;

    [ObservableProperty]
    private string _outputFolder = string.Empty;

    [ObservableProperty]
    private bool _processSubfolders;

    [ObservableProperty]
    private bool _convertToWebP = true;

    [ObservableProperty]
    private bool _convertToAvif;

    [ObservableProperty]
    private bool _applyUpscale = true;

    [ObservableProperty]
    private bool _deleteSourceFile;

    [ObservableProperty]
    private bool _includeWebPFiles = false;

    [ObservableProperty]
    private bool _includeAvifFiles = false;

    [ObservableProperty]
    private bool _isProcessing;

    public bool IsUiEnabled => !IsProcessing;

    [ObservableProperty]
    private double _progressBarValue;

    [ObservableProperty]
    private double _folderProgressBarValue;

    [ObservableProperty]
    private string _statusMessage = "Ready";

    [ObservableProperty]
    private string _currentFolderName = string.Empty;

    [ObservableProperty]
    private int _filesInCurrentFolder;

    [ObservableProperty]
    private double? _folderSpaceSaving;

    [ObservableProperty]
    private long _folderOriginalSize;

    [ObservableProperty]
    private long _folderConvertedSize;

    [ObservableProperty]
    private double? _totalSpaceSaving;

    [ObservableProperty]
    private long _totalOriginalSize;

    [ObservableProperty]
    private long _totalConvertedSize;

    [ObservableProperty]
    private string _selectedModel = "realesrgan-x4plus";

    [ObservableProperty]
    private Bitmap? _imagePreview;

    [ObservableProperty]
    private bool _showPreview = true;

    [ObservableProperty]
    private bool _isDeleteConfirmationVisible;

    [ObservableProperty]
    private bool _isDarkMode;

    public ObservableCollection<string> Models { get; }

    private CancellationTokenSource? _cancellationTokenSource;
    private string _currentInputSubFolder = string.Empty;
    private string _currentOutputSubFolder = string.Empty;

    [ObservableProperty]
    private string? _errorMessage;

    public MainWindowViewModel()
    {
        _processorService = new ImageProcessorService();
        _logService = new ProcessingLogService();
        Models = new ObservableCollection<string>
        {
            "realesrgan-x4plus",
            "realesrnet-x4plus",
            "realesrgan-x4plus-anime",
            "realesr-animevideov3",
            "realesr-animevideov3-x2",
            "realesr-animevideov3-x4"
        };

        if (Application.Current != null)
        {
            IsDarkMode = Application.Current.RequestedThemeVariant == ThemeVariant.Dark;
        }

        CheckForMissingDependencies();
    }

    private void CheckForMissingDependencies()
    {
        if (_processorService.DependenciesNotFound.Any())
        {
            ErrorMessage = $"One or more required dependencies were not found: {string.Join(", ", _processorService.DependenciesNotFound)}. Please make sure they are in the application's directory.";
        }
    }

    partial void OnIsDarkModeChanged(bool value)
    {
        var theme = value ? ThemeVariant.Dark : ThemeVariant.Light;
        ThemeManager.SetTheme(theme);
        ThemeManager.SaveTheme(theme);
    }

    [RelayCommand]
    private async Task BrowseInputFolder(Window parent)
    {
        var result = await new OpenFolderDialog { Title = "Select Input Folder" }.ShowAsync(parent);
        if (!string.IsNullOrEmpty(result))
        {
            InputFolder = result;
        }
    }

    [RelayCommand]
    private async Task BrowseOutputFolder(Window parent)
    {
        var result = await new OpenFolderDialog { Title = "Select Output Folder" }.ShowAsync(parent);
        if (!string.IsNullOrEmpty(result))
        {
            OutputFolder = result;
        }
    }

    [RelayCommand(CanExecute = nameof(CanStartProcessing))]
    private async Task StartProcessing()
    {
        if (DeleteSourceFile)
        {
            IsDeleteConfirmationVisible = true;
        }
        else
        {
            await ExecuteProcessingAsync();
        }
    }

    [RelayCommand]
    private async Task ConfirmDeleteAndStart()
    {
        IsDeleteConfirmationVisible = false;
        await ExecuteProcessingAsync();
    }

    [RelayCommand]
    private void CancelDelete()
    {
        IsDeleteConfirmationVisible = false;
    }

    private async Task ExecuteProcessingAsync()
    {
        IsProcessing = true;
        StatusMessage = "Starting...";
        CurrentFolderName = string.Empty;
        FilesInCurrentFolder = 0;
        ProgressBarValue = 0;
        FolderProgressBarValue = 0;
        FolderSpaceSaving = null;
        FolderOriginalSize = 0;
        FolderConvertedSize = 0;
        TotalSpaceSaving = 0;
        TotalConvertedSize = 0;
        TotalOriginalSize = 0;
        _cancellationTokenSource = new CancellationTokenSource();

        var options = new ProcessingOptions(
            InputFolder,
            OutputFolder,
            SelectedModel,
            ProcessSubfolders,
            ConvertToWebP,
            ConvertToAvif,
            ApplyUpscale,
            DeleteSourceFile,
            IncludeWebPFiles,
            IncludeAvifFiles
        );

        var progress = new Progress<ProcessingUpdate>(update =>
        {
            StatusMessage = update.Message;
            ProgressBarValue = update.OverallProgress * 100;
            FolderProgressBarValue = update.FolderProgress * 100;

            FolderSpaceSaving = update.FolderSpaceSaving;
            FolderOriginalSize = update.FolderOriginalSize;
            FolderConvertedSize = update.FolderConvertedSize;
            TotalSpaceSaving = update.TotalSpaceSaving;
            TotalOriginalSize = update.TotalOriginalSize;
            TotalConvertedSize = update.TotalConvertedSize;

            if (update.CurrentFolderName is not null)
            {
                CurrentFolderName = update.CurrentFolderName;
            }
            if (update.FilesInCurrentFolder is not null)
            {
                FilesInCurrentFolder = update.FilesInCurrentFolder.Value;
            }

            if (!string.IsNullOrEmpty(update.CurrentFilePath))
            {
                _currentInputSubFolder = Path.GetDirectoryName(update.CurrentFilePath) ?? string.Empty;
                if (!string.IsNullOrEmpty(_currentInputSubFolder))
                {
                    string relativePath = Path.GetRelativePath(InputFolder, _currentInputSubFolder);
                    _currentOutputSubFolder = Path.Combine(OutputFolder, relativePath);
                }
                else
                {
                    _currentOutputSubFolder = string.Empty;
                }
            }

            if (ShowPreview && !string.IsNullOrEmpty(update.CurrentFilePath) && File.Exists(update.CurrentFilePath))
            {
                // Load the image preview on a background thread to avoid blocking the UI
                _ = LoadImagePreviewAsync(update.CurrentFilePath);
            }

            if (update.IsComplete || update.IsError)
            {
                IsProcessing = false;
                ImagePreview = null;
                CurrentFolderName = string.Empty;
                FilesInCurrentFolder = 0;
                FolderProgressBarValue = 0;
                FolderSpaceSaving = null;
                FolderOriginalSize = 0;
                FolderConvertedSize = 0;

                TotalSpaceSaving = TotalSpaceSaving; // Keep the last calculated value
                TotalConvertedSize = 0; 
                TotalOriginalSize = 0;

                _currentInputSubFolder = string.Empty;
                _currentOutputSubFolder = string.Empty;
                CancelProcessingCommand.NotifyCanExecuteChanged();
                StartProcessingCommand.NotifyCanExecuteChanged();
            }
        });

        try
        {
            var logEntries = await _processorService.ProcessImagesAsync(options, progress, _cancellationTokenSource.Token);
            foreach (var entry in logEntries)
            {
                _logService.Log(entry);
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
            IsProcessing = false;
        }
        finally
        {
            StartProcessingCommand.NotifyCanExecuteChanged();
            CancelProcessingCommand.NotifyCanExecuteChanged();
        }
    }

    private async Task LoadImagePreviewAsync(string imagePath)
    {
        try
        {
            var bitmap = await Task.Run(() =>
            {
                using var fileStream = new FileStream(imagePath, FileMode.Open, FileAccess.Read);
                using var memoryStream = new MemoryStream();
                fileStream.CopyTo(memoryStream);
                memoryStream.Position = 0;
                return new Bitmap(memoryStream);
            });
            ImagePreview = bitmap;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to load preview image: {ex.Message}");
            ImagePreview = null;
        }
    }

    private bool CanStartProcessing()
    {
        return !IsProcessing && 
               !string.IsNullOrEmpty(InputFolder) && 
               !string.IsNullOrEmpty(OutputFolder) &&
               (ApplyUpscale || ConvertToWebP || ConvertToAvif) &&
               ErrorMessage is null;
    }

    [RelayCommand(CanExecute = nameof(CanCancelProcessing))]
    private void CancelProcessing()
    {
        _cancellationTokenSource?.Cancel();
    }

    private bool CanCancelProcessing()
    {
        return IsProcessing;
    }

    partial void OnInputFolderChanged(string value) => StartProcessingCommand.NotifyCanExecuteChanged();
    partial void OnOutputFolderChanged(string value) => StartProcessingCommand.NotifyCanExecuteChanged();
    partial void OnIsProcessingChanged(bool value)
    {
        OnPropertyChanged(nameof(IsUiEnabled)); // Notify the UI that the enabled state has changed
        StartProcessingCommand.NotifyCanExecuteChanged();
        CancelProcessingCommand.NotifyCanExecuteChanged();
    }

    partial void OnApplyUpscaleChanged(bool value) => StartProcessingCommand.NotifyCanExecuteChanged();
    partial void OnConvertToWebPChanged(bool value)
    {
        if (value)
        {
            ConvertToAvif = false;
        }
        StartProcessingCommand.NotifyCanExecuteChanged();
    }

    partial void OnConvertToAvifChanged(bool value)
    {
        if (value)
        {
            ConvertToWebP = false;
        }
        StartProcessingCommand.NotifyCanExecuteChanged();
    }

 
    partial void OnShowPreviewChanged(bool value)
    {
        if (!value)
        {
            ImagePreview = null;
        }
    }

    public string FolderSpaceSavingInMB
    {
        get
        {
            if (FolderOriginalSize == 0)
                return string.Empty;

            double savedBytes = (double)FolderOriginalSize - FolderConvertedSize;
            double savedMB = savedBytes / (1024.0 * 1024.0);

            if (savedMB >= 0)
            {
                return $"({savedMB:F2} MB) saved";
            }
            else
            {
                return $"({-savedMB:F2} MB) ADDED";
            }
        }
    }

    public string TotalSpaceSavingInMB
    {
        get
        {
            if (TotalOriginalSize == 0)
                return string.Empty;

            double savedBytes = (double)TotalOriginalSize - TotalConvertedSize;
            double savedMB = savedBytes / (1024.0 * 1024.0);

            if (savedMB >= 0)
            {
                return $"({savedMB:F2} MB) saved";
            }
            else
            {
                return $"({-savedMB:F2} MB) ADDED)";
            }
        }
    }

    public string TotalSpaceSavingSummary
    {
        get
        {
            if (!TotalSpaceSaving.HasValue || TotalSpaceSaving.Value <= 0)
                return string.Empty;

            string percentage = $"{TotalSpaceSaving.Value:P2}";
            string mbSaved = TotalSpaceSavingInMB;

            if (string.IsNullOrEmpty(mbSaved))
                return percentage;

            return $"{percentage} {mbSaved}";
        }
    }

    partial void OnFolderOriginalSizeChanged(long value)
    {
        OnPropertyChanged(nameof(FolderSpaceSavingInMB));
    }

    partial void OnFolderConvertedSizeChanged(long value)
    {
        OnPropertyChanged(nameof(FolderSpaceSavingInMB));
    }

    partial void OnTotalOriginalSizeChanged(long value)
    {
        OnPropertyChanged(nameof(TotalSpaceSavingInMB));
        OnPropertyChanged(nameof(TotalSpaceSavingSummary));
    }

    partial void OnTotalConvertedSizeChanged(long value)
    {
        OnPropertyChanged(nameof(TotalSpaceSavingInMB));
        OnPropertyChanged(nameof(TotalSpaceSavingSummary));
    }

    partial void OnTotalSpaceSavingChanged(double? value)
    {
        OnPropertyChanged(nameof(TotalSpaceSavingSummary));
    }

        [RelayCommand]
    private async Task ViewLog(Window parentWindow)
    {
        var logView = new Views.LogView
        {
            DataContext = new LogViewModel(_logService)
        };
        await logView.ShowDialog(parentWindow);
    }

    [RelayCommand]
    private void OpenInputFolder()
    {
        OpenFolder(!string.IsNullOrEmpty(_currentInputSubFolder) ? _currentInputSubFolder : InputFolder);
    }

    [RelayCommand]
    private void OpenOutputFolder()
    {
        OpenFolder(!string.IsNullOrEmpty(_currentOutputSubFolder) ? _currentOutputSubFolder : OutputFolder);
    }

    private void OpenFolder(string folderPath)
    {
        if (string.IsNullOrEmpty(folderPath) || !Directory.Exists(folderPath))
        {
            Debug.WriteLine($"Folder not found or invalid: {folderPath}");
            return;
        }

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = folderPath,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error opening folder: {ex.Message}");
            // Consider notifying the user in a more visible way
        }
    }
}