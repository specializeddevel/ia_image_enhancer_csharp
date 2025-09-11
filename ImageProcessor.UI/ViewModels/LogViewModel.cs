using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ImageProcessor.Core;

namespace ImageProcessor.UI.ViewModels;

public partial class LogViewModel : ViewModelBase
{
    private readonly ProcessingLogService _logService;

    [ObservableProperty]
    private ObservableCollection<ProcessingLogEntry> _logEntries = new();

    // Constructor for design-time
    public LogViewModel()
    {
        _logService = new ProcessingLogService();
        LoadLogEntries();
    }

    public LogViewModel(ProcessingLogService logService)
    {
        _logService = logService;
        LoadLogEntries();
    }

    private void LoadLogEntries()
    {
        var entries = _logService.GetLogEntries().OrderByDescending(e => e.Date);
        LogEntries = new ObservableCollection<ProcessingLogEntry>(entries);
    }

    [RelayCommand]
    private async Task ExportToCsv(Window owner)
    {
        var file = await owner.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Save Log As CSV",
            SuggestedFileName = $"processing_log_{DateTime.Now:yyyyMMdd}.csv",
            DefaultExtension = "csv",
            ShowOverwritePrompt = true,
            FileTypeChoices = new[]
            {
                new FilePickerFileType("CSV File") { Patterns = new[] { "*.csv" } }
            }
        });

        if (file is not null)
        {
            try
            {
                await using var stream = await file.OpenWriteAsync();
                await using var writer = new StreamWriter(stream, Encoding.UTF8);
                
                await writer.WriteLineAsync("Time;Input Folder;Output Folder;Original File Name;Processed File Name;Original Size;Processed Size;Reduction");

                foreach (var entry in LogEntries)
                {
                    await writer.WriteLineAsync($"{entry.Date:HH:mm:ss};{entry.InputFolder};{entry.OutputFolder};{entry.OriginalFileName};{entry.ProcessedFileName};{entry.OriginalSize};{entry.ProcessedSize};{entry.ReductionPercentage:P}");
                }
            }
            catch (Exception ex)
            {
                // In a real app, you'd want to show this error to the user.
                Console.WriteLine($"Error saving file: {ex.Message}");
            }
        }
    }

    [RelayCommand]
    private void CloseWindow(Window window)
    {
        window.Close();
    }

    [ObservableProperty]
    private bool _isClearConfirmationVisible;

    [RelayCommand]
    private void RequestClearLog()
    {
        IsClearConfirmationVisible = true;
    }

    [RelayCommand]
    private void ConfirmClearLog()
    {
        _logService.ClearLog();
        LogEntries.Clear();
        IsClearConfirmationVisible = false;
    }

    [RelayCommand]
    private void CancelClearLog()
    {
        IsClearConfirmationVisible = false;
    }
}
