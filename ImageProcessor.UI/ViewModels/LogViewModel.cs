using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
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
        var saveFileDialog = new SaveFileDialog
        {
            Title = "Save Log As CSV",
            DefaultExtension = "csv",
            InitialFileName = $"processing_log_{DateTime.Now:yyyyMMdd}.csv"
        };

        var result = await saveFileDialog.ShowAsync(owner);

        if (result is not null)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Time;Input Folder;Output Folder;Original File Name;Processed File Name;Original Size;Processed Size;Reduction");

            foreach (var entry in LogEntries)
            {
                sb.AppendLine($"{entry.Date:HH:mm:ss};{entry.InputFolder};{entry.OutputFolder};{entry.OriginalFileName};{entry.ProcessedFileName};{entry.OriginalSize};{entry.ProcessedSize};{entry.ReductionPercentage:P}");
            }

            await File.WriteAllTextAsync(result, sb.ToString());
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
