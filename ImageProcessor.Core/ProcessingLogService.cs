using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace ImageProcessor.Core;

public class ProcessingLogService
{
    private readonly string _logFilePath;

    public ProcessingLogService()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var appFolderPath = Path.Combine(appDataPath, "ImageProcessor");
        Directory.CreateDirectory(appFolderPath);
        _logFilePath = Path.Combine(appFolderPath, "processing_log.txt");
    }

    public void Log(ProcessingLogEntry entry)
    {
        var line = $"{entry.Date:yyyy-MM-dd HH:mm:ss};{entry.InputFile};{entry.OutputFile};{entry.OriginalSize};{entry.ProcessedSize};{entry.InputFolder};{entry.OutputFolder};{entry.OriginalFileName};{entry.ProcessedFileName}";
        Debug.WriteLine($"Logging: {line}");
        File.AppendAllText(_logFilePath, line + Environment.NewLine);
    }

    public IEnumerable<ProcessingLogEntry> GetLogEntries()
    {
        if (!File.Exists(_logFilePath))
        {
            return Enumerable.Empty<ProcessingLogEntry>();
        }

        var lines = File.ReadAllLines(_logFilePath);
        Debug.WriteLine($"Found {lines.Length} log entries.");
        return lines.Select(line =>
        {
            try
            {
                var parts = line.Split(';');
                return new ProcessingLogEntry
                {
                    Date = DateTime.ParseExact(parts[0], "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                    InputFile = parts[1],
                    OutputFile = parts[2],
                    OriginalSize = long.Parse(parts[3]),
                    ProcessedSize = long.Parse(parts[4]),
                    InputFolder = parts[5],
                    OutputFolder = parts[6],
                    OriginalFileName = parts[7],
                    ProcessedFileName = parts[8]
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error parsing log entry: {line} - {ex.Message}");
                return null; // Return null for malformed entries
            }
        }).Where(entry => entry != null)!;
    }
}
