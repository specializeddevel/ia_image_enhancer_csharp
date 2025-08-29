
namespace ImageProcessor.Core;

/// <summary>
/// Represents a progress update from the image processing service.
/// This allows the core logic to send structured progress information to any consumer (UI, API logger, etc.)
/// without being directly coupled to it.
/// </summary>
public class ProcessingUpdate
{
    /// <summary>
    /// A general message describing the current status (e.g., "Processing folder X...").
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// The current file being processed.
    /// </summary>
    public string CurrentFile { get; set; } = string.Empty;

    /// <summary>
    /// The overall progress of the entire batch, from 0.0 to 1.0.
    /// </summary>
    public double OverallProgress { get; set; }

    /// <summary>
    /// The progress of the current folder being processed, from 0.0 to 1.0.
    /// </summary>
    public double FolderProgress { get; set; }

    /// <summary>
    /// Indicates whether the process has completed.
    /// </summary>
    public bool IsComplete { get; set; }

    /// <summary>
    /// Indicates whether an error occurred during processing.
    /// </summary>
    public bool IsError { get; set; }

    /// <summary>
    /// The error message, if an error occurred.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// The absolute path of the file currently being processed.
    /// </summary>
    public string? CurrentFilePath { get; set; }

    /// <summary>
    /// The name of the folder currently being processed.
    /// </summary>
    public string? CurrentFolderName { get; set; }

    /// <summary>
    /// The total number of files in the folder currently being processed.
    /// </summary>
    public int? FilesInCurrentFolder { get; set; }

    /// <summary>
    /// The percentage of disk space saved in the current folder, from 0.0 to 1.0.
    /// </summary>
    public double? FolderSpaceSaving { get; set; }

    /// <summary>
    /// The total original size of files in the current folder, in bytes.
    /// </summary>
    public long FolderOriginalSize { get; set; }

    /// <summary>
    /// The total converted size of files in the current folder, in bytes.
    /// </summary>
    public long FolderConvertedSize { get; set; }
    
    /// <summary>
    /// The percentage of disk space saved, from 0.0 to 1.0.
    /// </summary>
    public double? TotalSpaceSaving { get; set; }
    
    /// <summary>
    /// The total original size of all files, in bytes.
    /// </summary>
    public long TotalOriginalSize { get; set; }

    /// <summary>
    /// The total converted size of all files, in bytes.
    /// </summary>
    public long TotalConvertedSize { get; set; }
}
