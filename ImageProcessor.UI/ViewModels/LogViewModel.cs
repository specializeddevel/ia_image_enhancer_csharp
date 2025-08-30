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
    private ObservableCollection<DailyLog> _dailyLogs = new();

    public LogViewModel(ProcessingLogService logService)
    {
        _logService = logService;
        LoadLogEntries();
    }

    private void LoadLogEntries()
    {
        var entries = _logService.GetLogEntries()
            .GroupBy(e => e.Date.Date)
            .Select(g => new DailyLog
            {
                Date = g.Key,
                Entries = new ObservableCollection<ProcessingLogEntry>(g.ToList()),
                TotalOriginalSize = g.Sum(e => e.OriginalSize),
                TotalProcessedSize = g.Sum(e => e.ProcessedSize)
            })
            .OrderByDescending(d => d.Date);

        DailyLogs = new ObservableCollection<DailyLog>(entries);
        System.Diagnostics.Debug.WriteLine($"Found {DailyLogs.Count} daily logs.");
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

            foreach (var dailyLog in DailyLogs)
            {
                foreach (var entry in dailyLog.Entries)
                {
                    sb.AppendLine($"{entry.Date:HH:mm:ss};{entry.InputFolder};{entry.OutputFolder};{entry.OriginalFileName};{entry.ProcessedFileName};{entry.OriginalSize};{entry.ProcessedSize};{entry.ReductionPercentage:P}");
                }
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
        DailyLogs.Clear();
        IsClearConfirmationVisible = false;
    }

    [RelayCommand]
    private void CancelClearLog()
    {
        IsClearConfirmationVisible = false;
    }
}

public partial class DailyLog : ViewModelBase
{
    [ObservableProperty]
    private DateTime _date;

    [ObservableProperty]
    private ObservableCollection<ProcessingLogEntry> _entries = new();

    [ObservableProperty]
    private long _totalOriginalSize;

    [ObservableProperty]
    private long _totalProcessedSize;

        public double TotalReductionPercentage => TotalOriginalSize > 0 ? (double)(TotalOriginalSize - TotalProcessedSize) / TotalOriginalSize : 0;
}
