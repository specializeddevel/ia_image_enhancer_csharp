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
using Avalonia.Platform.Storage;
using System.Linq;
using ImageProcessor.UI.Views;
using ImageProcessor.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace ImageProcessor.UI.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly ImageProcessorService _processorService;
    private readonly ProcessingLogService _logService;
    private readonly SettingsService _settingsService;
    private readonly ProfileService _profileService;
    private readonly IThemeService _themeService;
    private readonly IServiceProvider _serviceProvider;
    private bool _isLoading;

    [ObservableProperty]
    private string _inputFolder = string.Empty;

    [ObservableProperty]
    private string _outputFolder = string.Empty;

    [ObservableProperty]
    private bool _useInputFolderAsOutput;

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
    private string _totalQueueSummary = string.Empty;

    [ObservableProperty]
    private string _currentFolderSummary = string.Empty;

    

    [ObservableProperty]
    private string _currentFile = string.Empty;

    [ObservableProperty]
    private long _currentFileSize;

    [ObservableProperty]
    private int _filesInCurrentFolder;

    [ObservableProperty]
    private int _processedFilesInCurrentFolder; // Make it observable

    public string HumanReadableCurrentFileSize
    {
        get
        {
            if (CurrentFileSize == 0)
                return string.Empty;
            return $"({FormatBytes(CurrentFileSize)})";
        }
    }

    public string HumanReadableFolderOriginalSize // New property
    {
        get
        {
            if (FolderOriginalSize == 0)
                return string.Empty;
            return $"({FormatBytes(FolderOriginalSize)} => ";
        }
    }

    public string HumanReadableFolderConvertedSize // New property
    {
        get
        {
            if (FolderConvertedSize == 0)
                return string.Empty;
            return $"{FormatBytes(FolderConvertedSize)})";
        }
    }

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
    private string _selectedModel = "Photo-Small-W2xEX";

    [ObservableProperty]
    private Bitmap? _imagePreview;

    [ObservableProperty]
    private bool _showPreview = true;

    [ObservableProperty]
    private bool _isDeleteConfirmationVisible;

    [ObservableProperty]
    private bool _isDarkMode;

    public ObservableCollection<string> Models { get; }

    [ObservableProperty]
    private string _newProfileName = "New Profile";

    private CancellationTokenSource? _cancellationTokenSource;
    private string _currentInputSubFolder = string.Empty;
    private string _currentOutputSubFolder = string.Empty;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private string? _previewErrorMessage;

    [ObservableProperty]
    private bool _canProcess = true; // Default to true, assume dependencies are fine until checked

    public MainWindowViewModel(ImageProcessorService processorService, ProcessingLogService logService, SettingsService settingsService, ProfileService profileService, IThemeService themeService, IServiceProvider serviceProvider)
    {
        _isLoading = true;
        _processorService = processorService;
        _logService = logService;
        _settingsService = settingsService;
        _profileService = profileService;
        _themeService = themeService;
        _serviceProvider = serviceProvider;

        // Populate Models from ImageProcessorService
        Models = new ObservableCollection<string>(_processorService.GetAvailableModels());

        LoadSettings();

        // Ensure SelectedModel is valid after loading settings
        if (!Models.Contains(SelectedModel) && Models.Any())
        {
            SelectedModel = Models.First();
        }
        else if (!Models.Any())
        {
            SelectedModel = string.Empty; // No models available
        }

        CheckForMissingDependencies();

        // Load last used profile if it exists
        var lastProfileName = _settingsService.UserSettings.LastUsedProfileName;
        if (!string.IsNullOrEmpty(lastProfileName))
        {
            var profileOptions = _profileService.LoadProfile(lastProfileName);
            if (profileOptions != null)
            {
                ApplyProfileOptions(profileOptions);
                NewProfileName = lastProfileName;
                StatusMessage = $"Loaded last used profile: '{lastProfileName}'.";
                Task.Delay(3000).ContinueWith(t => StatusMessage = "Ready");
            }
        }

        _isLoading = false;
    }

    private void LoadSettings()
    {
        var userSettings = _settingsService.UserSettings;
        InputFolder = userSettings.InputFolder;
        OutputFolder = userSettings.OutputFolder;
        UseInputFolderAsOutput = userSettings.UseInputFolderAsOutput;
        ProcessSubfolders = userSettings.ProcessSubfolders;
        ConvertToWebP = userSettings.ConvertToWebP;
        ConvertToAvif = userSettings.ConvertToAvif;
        ApplyUpscale = userSettings.ApplyUpscale;
        DeleteSourceFile = userSettings.DeleteSourceFile;
        IncludeWebPFiles = userSettings.IncludeWebPFiles;
        IncludeAvifFiles = userSettings.IncludeAvifFiles;
        SelectedModel = userSettings.SelectedModel;
        IsDarkMode = userSettings.IsDarkMode;
    }

    public void SaveSettings()
    {
        if (_isLoading) return;
        System.Diagnostics.Debug.WriteLine("SaveSettings called");
        var userSettings = _settingsService.UserSettings;
        userSettings.InputFolder = InputFolder;
        userSettings.OutputFolder = OutputFolder;
        userSettings.UseInputFolderAsOutput = UseInputFolderAsOutput;
        userSettings.ProcessSubfolders = ProcessSubfolders;
        userSettings.ConvertToWebP = ConvertToWebP;
        userSettings.ConvertToAvif = ConvertToAvif;
        userSettings.ApplyUpscale = ApplyUpscale;
        userSettings.DeleteSourceFile = DeleteSourceFile;
        userSettings.IncludeWebPFiles = IncludeWebPFiles;
        userSettings.IncludeAvifFiles = IncludeAvifFiles;
        userSettings.SelectedModel = SelectedModel;
        userSettings.IsDarkMode = IsDarkMode;

        _settingsService.Save();
    }

    private void ApplyProfileOptions(ProcessingOptions options)
    {
        // Stop listening to changes while we update properties
        bool wasLoading = _isLoading;
        _isLoading = true;

        InputFolder = options.InputFolder;
        OutputFolder = options.OutputFolder;
        ProcessSubfolders = options.ProcessSubfolders;
        ConvertToWebP = options.ConvertToWebP;
        ConvertToAvif = options.ConvertToAvif;
        ApplyUpscale = options.ApplyUpscale;
        DeleteSourceFile = options.DeleteSourceFile;
        IncludeWebPFiles = options.IncludeWebPFiles;
        IncludeAvifFiles = options.IncludeAvifFiles;

        if (Models.Contains(options.Model))
        {
            SelectedModel = options.Model;
        }
        
        _isLoading = wasLoading;
    }

    private void CheckForMissingDependencies()
    {
        if (_processorService.DependenciesNotFound.Any())
        {
            ErrorMessage = $"One or more required dependencies were not found: {string.Join(", ", _processorService.DependenciesNotFound)}. Please make sure they are in the application's directory.";
            CanProcess = false;
        }
        else
        {
            ErrorMessage = null;
            CanProcess = true;
        }
    }

    [RelayCommand]
    private async Task OpenProfilesDialog(object parameter)
    {
        if (parameter is not Window owner) return;

        var profilesView = _serviceProvider.GetRequiredService<ProfilesView>();
        var result = await profilesView.ShowDialog<(string ProfileName, ProcessingOptions Options)?>(owner);

        if (result.HasValue)
        {
            var (profileName, options) = result.Value;
            ApplyProfileOptions(options);
            NewProfileName = profileName; // Update the profile name text box

            // Save as last used profile
            _settingsService.UserSettings.LastUsedProfileName = profileName;
            _settingsService.Save();
        }
    }

    [RelayCommand(CanExecute = nameof(CanSaveProfile))]
    private void SaveProfile()
    {
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
            IncludeAvifFiles,
            _settingsService.UserSettings.WebPQuality,
            _settingsService.UserSettings.AvifQuality
        );

        _profileService.SaveProfile(NewProfileName, options);

        // Save as last used profile
        _settingsService.UserSettings.LastUsedProfileName = NewProfileName;
        _settingsService.Save();

        StatusMessage = $"Profile '{NewProfileName}' saved!";
        // Use the UI thread to reset the message after a delay to prevent cross-thread exceptions.
        Task.Delay(3000).ContinueWith((Task task) => 
        {
            Avalonia.Threading.Dispatcher.UIThread.Post(() => StatusMessage = "Ready");
        });
    }

    private bool CanSaveProfile() => !string.IsNullOrWhiteSpace(NewProfileName);

    partial void OnNewProfileNameChanged(string value)
    {
        SaveProfileCommand.NotifyCanExecuteChanged();
    }

    partial void OnIsDarkModeChanged(bool value)
    {
        _themeService.SetTheme(value);
    }

    [RelayCommand]
    private async Task BrowseInputFolder(Window parent)
    {
        var result = await parent.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Select Input Folder",
            AllowMultiple = false
        });

        if (result.Count > 0 && result[0].TryGetLocalPath() is string path)
        {
            InputFolder = path;
        }
    }

    [RelayCommand]
    private async Task BrowseOutputFolder(Window parent)
    {
        var result = await parent.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Select Output Folder",
            AllowMultiple = false
        });

        if (result.Count > 0 && result[0].TryGetLocalPath() is string path)
        {
            OutputFolder = path;
        }
    }

    [RelayCommand]
    private async Task OpenSettings(object parameter)
    {
        if (parameter is Window owner)
        {
            var settingsWindow = _serviceProvider.GetRequiredService<SettingsView>();
            await settingsWindow.ShowDialog(owner);
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
        TotalQueueSummary = string.Empty;
        CurrentFolderSummary = string.Empty;
        FilesInCurrentFolder = 0;
        ProgressBarValue = 0;
        FolderProgressBarValue = 0;
        FolderSpaceSaving = null;
        FolderOriginalSize = 0;
        FolderConvertedSize = 0;
        TotalSpaceSaving = 0;
        TotalConvertedSize = 0;
        TotalOriginalSize = 0;
        ErrorMessage = null; // Clear any previous general error message
        PreviewErrorMessage = null; // Clear any previous preview error message
        _cancellationTokenSource = new CancellationTokenSource();

        var userSettings = _settingsService.UserSettings;
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
            IncludeAvifFiles,
            userSettings.WebPQuality,
            userSettings.AvifQuality
        );

        var progress = new Progress<ProcessingUpdate>(update =>
        {
            StatusMessage = update.Message;
            ProgressBarValue = update.OverallProgress * 100;
            FolderProgressBarValue = update.FolderProgress * 100;

            if (update.TotalQueueFileCount.HasValue && update.TotalQueueSizeInBytes.HasValue)
            {
                TotalQueueSummary = $"{update.TotalQueueFileCount.Value} files to process ({FormatBytes(update.TotalQueueSizeInBytes.Value)})";
            }

            if (update.CurrentFolderName is not null && update.CurrentFolderTotalSizeInBytes.HasValue)
            {
                CurrentFolderSummary = $"{update.CurrentFolderName} - ({FormatBytes(update.CurrentFolderTotalSizeInBytes.Value)})";
            }

            FolderSpaceSaving = update.FolderSpaceSaving;
            FolderOriginalSize = update.FolderOriginalSize;
            FolderConvertedSize = update.FolderConvertedSize;
            TotalSpaceSaving = update.TotalSpaceSaving;
            TotalOriginalSize = update.TotalOriginalSize;
            TotalConvertedSize = update.TotalConvertedSize;

            if (update.FilesInCurrentFolder is not null)
            {
                FilesInCurrentFolder = update.FilesInCurrentFolder.Value;
            }

            if (update.ProcessedFilesInCurrentFolder.HasValue)
            {
                ProcessedFilesInCurrentFolder = update.ProcessedFilesInCurrentFolder.Value;
            }

            Debug.WriteLine($"Processed {ProcessedFilesInCurrentFolder} of {FilesInCurrentFolder} files in current folder.");

            CurrentFile = update.CurrentFile;
            CurrentFileSize = update.CurrentFileSize;
            OnPropertyChanged(nameof(HumanReadableCurrentFileSize));

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

                if (update.IsError)
                {
                    StatusMessage = "An error occurred. Please check the error message.";
                    ErrorMessage = update.ErrorMessage;
                }
                else // isComplete
                {
                    StatusMessage = update.Message; // "Process completed!"
                    ErrorMessage = null;
                }

                ImagePreview = null;
                CurrentFolderSummary = string.Empty;
                CurrentFile = string.Empty;
                CurrentFileSize = 0;
                FilesInCurrentFolder = 0;
                FolderProgressBarValue = 0;
                FolderSpaceSaving = null;
                FolderOriginalSize = 0;
                FolderConvertedSize = 0;
                TotalQueueSummary = string.Empty;

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
        // Clear any previous preview error message
        PreviewErrorMessage = null; // Use the new property
        ErrorMessage = null; // Clear general error message too, in case it was set by a previous preview attempt

        // Do not attempt to load AVIF files for preview
        if (imagePath.EndsWith(".avif", StringComparison.OrdinalIgnoreCase))
        {
            ImagePreview = null;
            PreviewErrorMessage = "Preview not available for AVIF files."; // Set new property
            return;
        }

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
            PreviewErrorMessage = $"No se pudo cargar la vista previa para '{Path.GetFileName(imagePath)}': {ex.Message}"; // Set new property
        }
    }

    private bool CanStartProcessing()
    {
        return !IsProcessing && 
               !string.IsNullOrEmpty(InputFolder) && 
               !string.IsNullOrEmpty(OutputFolder) &&
               (ApplyUpscale || ConvertToWebP || ConvertToAvif) &&
               CanProcess;
    }

    [RelayCommand(CanExecute = nameof(CanCancelProcessing))]
    private async Task CancelProcessing(Window owner)
    {
        var dialog = new ConfirmCloseWindow();
        var result = await dialog.ShowDialog<bool>(owner);

        if (result)
        {
            _cancellationTokenSource?.Cancel();
        }
    }

    private bool CanCancelProcessing()
    {
        return IsProcessing;
    }

    public bool CanSelectSameOutputFolder => !string.IsNullOrEmpty(InputFolder);

    partial void OnUseInputFolderAsOutputChanged(bool value)
    {
        if (value)
        {
            OutputFolder = InputFolder;
        }
        SaveSettings();
    }

    partial void OnInputFolderChanged(string value)
    {
        if (UseInputFolderAsOutput)
        {
            OutputFolder = value;
        }
        OnPropertyChanged(nameof(CanSelectSameOutputFolder));
        StartProcessingCommand.NotifyCanExecuteChanged();
        SaveSettings();
    }
    partial void OnOutputFolderChanged(string value)
    {
        StartProcessingCommand.NotifyCanExecuteChanged();
        SaveSettings();
    }
    partial void OnIsProcessingChanged(bool value)
    {
        OnPropertyChanged(nameof(IsUiEnabled)); // Notify the UI that the enabled state has changed
        StartProcessingCommand.NotifyCanExecuteChanged();
        CancelProcessingCommand.NotifyCanExecuteChanged();
    }

    partial void OnApplyUpscaleChanged(bool value)
    {
        StartProcessingCommand.NotifyCanExecuteChanged();
        SaveSettings();
    }
    partial void OnConvertToWebPChanged(bool value)
    {
        StartProcessingCommand.NotifyCanExecuteChanged();
        SaveSettings();
    }

    partial void OnConvertToAvifChanged(bool value)
    {
        StartProcessingCommand.NotifyCanExecuteChanged();
        SaveSettings();
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
                return $"({-savedMB:F2} MB) +Added+";
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
                return $"({-savedMB:F2} MB) +Added+";
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
        OnPropertyChanged(nameof(HumanReadableFolderOriginalSize)); // Add this
    }

    partial void OnFolderConvertedSizeChanged(long value)
    {
        OnPropertyChanged(nameof(FolderSpaceSavingInMB));
        OnPropertyChanged(nameof(HumanReadableFolderConvertedSize)); // Add this
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
        var logView = _serviceProvider.GetRequiredService<Views.LogView>();
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

    partial void OnProcessSubfoldersChanged(bool value) { SaveSettings(); }
    partial void OnDeleteSourceFileChanged(bool value) { SaveSettings(); }
    partial void OnIncludeWebPFilesChanged(bool value) { SaveSettings(); }
    partial void OnIncludeAvifFilesChanged(bool value) { SaveSettings(); }
    partial void OnSelectedModelChanged(string value) { SaveSettings(); }

    private static string FormatBytes(long bytes)
    {
        if (bytes == 0) return "0 B";
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        int order = 0;
        double size = bytes;
        while (size >= 1024 && order < sizes.Length - 1)
        {
            order++;
            size /= 1024;
        }
        return $"{size:0.##} {sizes[order]}";
    }
}