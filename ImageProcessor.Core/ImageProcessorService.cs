using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ImageProcessor.Core;

public class ImageProcessorService
{
    private readonly string _realesrganExecutablePath;
    private readonly string _cwebpExecutablePath;
    private readonly string _ffmpegExecutablePath; // Changed for ffmpeg conversion
    private readonly string _modelsPath;

    public List<string> DependenciesNotFound { get; } = new();

    public ImageProcessorService()
    {
        string currentDir = AppContext.BaseDirectory;
        _modelsPath = Path.Combine(currentDir, "models");

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            _realesrganExecutablePath = Path.Combine(currentDir, "realesrgan-ncnn-vulkan.exe");
            _cwebpExecutablePath = Path.Combine(currentDir, "cwebp.exe");
            _ffmpegExecutablePath = Path.Combine(currentDir, "ffmpeg.exe");
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            _realesrganExecutablePath = Path.Combine(currentDir, "realesrgan-ncnn-vulkan");
            _cwebpExecutablePath = Path.Combine(currentDir, "cwebp");
            _ffmpegExecutablePath = Path.Combine(currentDir, "ffmpeg");
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            _realesrganExecutablePath = Path.Combine(currentDir, "realesrgan-ncnn-vulkan-mac");
            _cwebpExecutablePath = Path.Combine(currentDir, "cwebp-mac");
            _ffmpegExecutablePath = Path.Combine(currentDir, "ffmpeg");
        }
        else
        {
            throw new PlatformNotSupportedException("This operating system is not supported.");
        }

        if (!File.Exists(_realesrganExecutablePath))
            DependenciesNotFound.Add(Path.GetFileName(_realesrganExecutablePath));

        if (!File.Exists(_cwebpExecutablePath))
            DependenciesNotFound.Add(Path.GetFileName(_cwebpExecutablePath));

        if (!File.Exists(_ffmpegExecutablePath))
            DependenciesNotFound.Add(Path.GetFileName(_ffmpegExecutablePath));
    }

    public async Task<List<ProcessingLogEntry>> ProcessImagesAsync(ProcessingOptions options, IProgress<ProcessingUpdate> progress, CancellationToken cancellationToken)
    {
        if (DependenciesNotFound.Any())
        {
            string missingFiles = string.Join(", ", DependenciesNotFound);
            string errorMessage = $"One or more required dependencies were not found: {missingFiles}. Please make sure they are in the application's directory.";
            progress.Report(new ProcessingUpdate { Message = errorMessage, IsError = true, ErrorMessage = errorMessage });
            return new List<ProcessingLogEntry>();
        }

        var logEntries = new List<ProcessingLogEntry>();
        int skippedFilesCount = 0;
        try
        {
            var imageFiles = FindImageFiles(options);
            if (imageFiles.Count == 0)
            {
                progress.Report(new ProcessingUpdate { Message = "No images found to process.", IsComplete = true });
                return logEntries;
            }

            var filesByDirectory = imageFiles.GroupBy(f => f.DirectoryName!)
                                             .ToDictionary(g => g.Key, g => g.ToList());

            var folderSizes = new Dictionary<string, (long originalSize, long convertedSize)>();
            folderSizes["total"] = (0, 0);

            int totalFiles = imageFiles.Count;
            int processedFiles = 0;
            string? lastDirectory = null;
            int processedFilesInFolder = 0;
            var totalSize = folderSizes["total"];            

            progress.Report(new ProcessingUpdate { Message = $"Found {totalFiles} images in {filesByDirectory.Count} folders." });
            await Task.Delay(1000, cancellationToken); // Give user time to read the message

            foreach (var file in imageFiles)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var directoryName = file.DirectoryName!;

                // Report new folder entry
                if (directoryName != lastDirectory)
                {
                    lastDirectory = directoryName;
                    processedFilesInFolder = 0; // Reset folder progress counter
                    if (!folderSizes.ContainsKey(directoryName))
                    {
                        folderSizes[directoryName] = (0, 0);
                    }

                    var filesInFolder = filesByDirectory[lastDirectory].Count;
                    progress.Report(new ProcessingUpdate
                    {
                        Message = "Processing folder...",
                        CurrentFolderName = new DirectoryInfo(lastDirectory).Name,
                        FilesInCurrentFolder = filesInFolder,
                        OverallProgress = (double)processedFiles / totalFiles, // Keep overall progress updated
                        FolderSpaceSaving = null
                    });
                    await Task.Delay(500, cancellationToken); // Give user time to read folder name
                }

                if (file.Length == 0)
                {
                    skippedFilesCount++;
                    Debug.WriteLine($"Skipping empty or corrupt file: {file.FullName}");
                    progress.Report(new ProcessingUpdate { Message = $"Skipping corrupt file: {file.Name}" });
                    continue;
                }

                processedFiles++;
                processedFilesInFolder++;
                var filesInCurrentFolder = filesByDirectory[directoryName].Count;

                // First, report that we are starting to process the file.
                var startingUpdate = new ProcessingUpdate
                {
                    Message = $"Processing file {processedFiles} of {totalFiles}...",
                    CurrentFile = file.Name,
                    OverallProgress = (double)processedFiles / totalFiles,
                    FolderProgress = (double)processedFilesInFolder / filesInCurrentFolder,
                    CurrentFilePath = file.FullName,
                    CurrentFolderName = new DirectoryInfo(directoryName).Name,
                    FilesInCurrentFolder = filesInCurrentFolder,
                    TotalOriginalSize = totalSize.originalSize,
                    TotalConvertedSize = totalSize.convertedSize,
                    TotalSpaceSaving = (totalSize.originalSize > 0) ? (1.0 - ((double)totalSize.convertedSize / totalSize.originalSize)) : null,
                    FolderOriginalSize = folderSizes[directoryName].originalSize,
                    FolderConvertedSize = folderSizes[directoryName].convertedSize,
                    FolderSpaceSaving = (folderSizes[directoryName].originalSize > 0) ? (1.0 - ((double)folderSizes[directoryName].convertedSize / folderSizes[directoryName].originalSize)) : null
                };
                progress.Report(startingUpdate);

                var (convertedFileSize, finalPath) = await ProcessSingleFileAsync(file, options, cancellationToken);

                var sourceDirectory = file.DirectoryName;
                if (sourceDirectory is null)
                {
                    throw new InvalidOperationException($"Could not determine the directory of the file: {file.FullName}");
                }

                string relativePath = Path.GetRelativePath(options.InputFolder, sourceDirectory);
                string targetOutputFolder = Path.Combine(options.OutputFolder, relativePath);

                logEntries.Add(new ProcessingLogEntry
                {
                    Date = DateTime.Now,
                    InputFile = file.FullName,
                    OutputFile = finalPath,
                    OriginalSize = file.Length,
                    ProcessedSize = convertedFileSize,
                    InputFolder = file.DirectoryName!,
                    OutputFolder = targetOutputFolder,
                    OriginalFileName = file.Name,
                    ProcessedFileName = Path.GetFileName(finalPath)
                });

                // Update folder size info
                var sizes = folderSizes[directoryName];
                sizes.originalSize += file.Length;
                sizes.convertedSize += convertedFileSize;
                folderSizes[directoryName] = sizes;

                // Update total size correctly by adding only the current file's sizes
                totalSize.originalSize += file.Length;
                totalSize.convertedSize += convertedFileSize;
                folderSizes["total"] = totalSize; // Update the dictionary entry for total

                Debug.WriteLine($"Folder: original: {sizes.originalSize} / converted: {sizes.convertedSize}");
                Debug.WriteLine($"Totales: original: {totalSize.originalSize} / converted: {totalSize.convertedSize}");

                // Calculate space saving for the folder
                double? spaceSaving = null;
                if (sizes.originalSize > 0)
                {
                    spaceSaving = 1.0 - ((double)sizes.convertedSize / sizes.originalSize);
                }

                // Calculate total space saving
                double? totalSpaceSaving = null;
                if (totalSize.originalSize > 0)
                {
                    totalSpaceSaving = 1.0 - ((double)totalSize.convertedSize / totalSize.originalSize);
                }

                // Send a final update for the file with the new space saving info
                var finalUpdate = new ProcessingUpdate
                {
                    Message = $"File {processedFiles} of {totalFiles} processed.",
                    CurrentFile = file.Name,
                    OverallProgress = (double)processedFiles / totalFiles,
                    FolderProgress = (double)processedFilesInFolder / filesInCurrentFolder,
                    CurrentFilePath = file.FullName,
                    CurrentFolderName = new DirectoryInfo(directoryName).Name,
                    FilesInCurrentFolder = filesInCurrentFolder,
                    FolderSpaceSaving = spaceSaving,
                    FolderOriginalSize = sizes.originalSize,
                    FolderConvertedSize = sizes.convertedSize,
                    TotalSpaceSaving = totalSpaceSaving,
                    TotalOriginalSize = totalSize.originalSize,
                    TotalConvertedSize = totalSize.convertedSize
                };
                progress.Report(finalUpdate);
            }

            if (options.DeleteSourceFile)
            {
                progress.Report(new ProcessingUpdate { Message = "Cleaning up empty source directories..." });
                DeleteEmptySourceDirectories(options.InputFolder, options.ProcessSubfolders);
            }

            string finalMessage = skippedFilesCount > 0
                ? $"Process completed. Skipped {skippedFilesCount} files because they were corrupt or empty."
                : "Process completed!";
            progress.Report(new ProcessingUpdate { Message = finalMessage, IsComplete = true, OverallProgress = 1.0 });
        }
        catch (OperationCanceledException)
        {
            progress.Report(new ProcessingUpdate { Message = "The process was canceled.", IsError = true, ErrorMessage = "Canceled" });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"An error occurred during image processing: {ex}");
            string errorMessage = $"An unexpected error occurred: {ex.Message}. The process has been stopped.";
            progress.Report(new ProcessingUpdate { Message = errorMessage, IsError = true, ErrorMessage = ex.Message });
        }
        return logEntries;
    }

    private async Task<(long finalSize, string finalPath)> ProcessSingleFileAsync(FileInfo file, ProcessingOptions options, CancellationToken cancellationToken)
    {
        var sourceDirectory = file.DirectoryName;
        if (sourceDirectory is null)
        {
            throw new InvalidOperationException($"Could not determine the directory of the file: {file.FullName}");
        }

        string relativePath = Path.GetRelativePath(options.InputFolder, sourceDirectory);
        string targetOutputFolder = Path.Combine(options.OutputFolder, relativePath);
        Directory.CreateDirectory(targetOutputFolder);

        string outputFileNameBase = Path.GetFileNameWithoutExtension(file.Name);
        string improvedPngPath = Path.Combine(targetOutputFolder, $"{outputFileNameBase}_improved.png");
        string finalWebPPath = Path.Combine(targetOutputFolder, $"{outputFileNameBase}_final.webp");
        string finalAvifPath = Path.Combine(targetOutputFolder, $"{outputFileNameBase}_final.avif");
        long finalSize = 0;
        string finalPath = string.Empty;

        if (options.ApplyUpscale)
        {
            await RunProcessAsync(_realesrganExecutablePath, $"-i \"{file.FullName}\" -o \"{improvedPngPath}\" -n {options.Model} -f png -m \"{_modelsPath}\"", cancellationToken);
        }

        string sourceForConversion = options.ApplyUpscale && File.Exists(improvedPngPath) ? improvedPngPath : file.FullName;

        if (options.ConvertToWebP)
        {
            await RunProcessAsync(_cwebpExecutablePath, $"-q 80 \"{sourceForConversion}\" -o \"{finalWebPPath}\"", cancellationToken);
            if (File.Exists(finalWebPPath))
            {
                finalSize = new FileInfo(finalWebPPath).Length;
                finalPath = finalWebPPath;
            }
        }
        else if (options.ConvertToAvif)
        {
            // Using ffmpeg arguments as requested, with -y to overwrite without asking.
            await RunProcessAsync(_ffmpegExecutablePath, $"-y -i \"{sourceForConversion}\" -c:v libaom-av1 -still-picture 1 -crf 35 -b:v 0 -cpu-used 4 -threads 8 \"{finalAvifPath}\"", cancellationToken);
            if (File.Exists(finalAvifPath))
            {
                finalSize = new FileInfo(finalAvifPath).Length;
                finalPath = finalAvifPath;
            }
        }
        else if (options.ApplyUpscale && File.Exists(improvedPngPath))
        {
            finalSize = new FileInfo(improvedPngPath).Length;
            finalPath = improvedPngPath;
        }

        // Delete the intermediate upscaled file if it exists and a conversion was made
        if (options.ApplyUpscale && (options.ConvertToWebP || options.ConvertToAvif) && File.Exists(improvedPngPath))
        {
            File.Delete(improvedPngPath);
        }

        if (options.DeleteSourceFile)
        {
            File.Delete(file.FullName);
        }

        return (finalSize, finalPath);
    }

    private void DeleteEmptySourceDirectories(string rootFolder, bool processSubfolders)
    {
        if (!processSubfolders)
        {
            if (!Directory.EnumerateFileSystemEntries(rootFolder).Any())
            {
                Directory.Delete(rootFolder);
            }
            return;
        }

        var allDirs = Directory.GetDirectories(rootFolder, "*", SearchOption.AllDirectories)
                               .OrderByDescending(d => d.Length)
                               .ToList();

        foreach (var dir in allDirs)
        {
            if (!Directory.EnumerateFileSystemEntries(dir).Any())
            {
                try { Directory.Delete(dir); } 
                catch (Exception ex) { Debug.WriteLine($"Could not delete directory {dir}: {ex.Message}"); }
            }
        }

        if (!Directory.EnumerateFileSystemEntries(rootFolder).Any())
        {
            try { Directory.Delete(rootFolder); } 
            catch (Exception ex) { Debug.WriteLine($"Could not delete root directory {rootFolder}: {ex.Message}"); }
        }
    }

    private List<FileInfo> FindImageFiles(ProcessingOptions options)
    {
        var directory = new DirectoryInfo(options.InputFolder);
        var searchOption = options.ProcessSubfolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
        
        var extensions = new List<string> { ".jpg", ".jpeg", ".png" };
        if (options.IncludeWebPFiles)
        {
            extensions.Add(".webp");
        }
        if (options.IncludeAvifFiles)
        {
            extensions.Add(".avif");
        }

        return directory.GetFiles("*.*", searchOption)
                        .Where(f => extensions.Contains(f.Extension.ToLower()))
                        .ToList();
    }

    private async Task RunProcessAsync(string executablePath, string arguments, CancellationToken cancellationToken)
    {
        Debug.WriteLine($"Executing command: {executablePath} {arguments}"); // Print command to IDE's debug output

        var processStartInfo = new ProcessStartInfo
        {
            FileName = executablePath,
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        using var process = new Process { StartInfo = processStartInfo };

        process.Start();

        // Asynchronously read the output and error streams to prevent deadlocks.
        var outputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var errorTask = process.StandardError.ReadToEndAsync(cancellationToken);

        // Now wait for the process to exit, and for the stream readers to finish.
        await process.WaitForExitAsync(cancellationToken);
        string output = await outputTask;
        string error = await errorTask;

        if (process.ExitCode != 0)
        {
            Debug.WriteLine($"Process output: {output}");
            throw new InvalidOperationException($"The process {Path.GetFileName(executablePath)} failed with exit code {process.ExitCode}. Error: {error}");
        }
    }
}