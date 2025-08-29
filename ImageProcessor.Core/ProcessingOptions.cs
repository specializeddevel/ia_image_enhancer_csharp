
// Namespace for the core business logic of the ImageProcessor application.
namespace ImageProcessor.Core;

/// <summary>
/// Defines the settings for an image processing job.
/// This is an immutable record, which is a good practice for transferring data.
/// </summary>
/// <param name="InputFolder">The source folder containing images to process.</param>
/// <param name="OutputFolder">The destination folder for processed images.</param>
/// <param name="Model">The name of the Real-ESRGAN model to use for upscaling.</param>
/// <param name="ProcessSubfolders">Whether to search for images in subdirectories.</param>
/// <param name="ConvertToWebP">Whether to convert the final image to WebP format.</param>
/// <param name="ConvertToAvif">Whether to convert the final image to AVIF format.</param>
/// <param name="ApplyUpscale">Whether to apply the AI upscaling process.</param>
/// <param name="DeleteSourceFile">Whether to delete the original source file after successful processing.</param>
/// <param name="IncludeWebPFiles">Whether to include existing .webp files from the source folder in the processing queue.</param>
public record ProcessingOptions(
    string InputFolder,
    string OutputFolder,
    string Model,
    bool ProcessSubfolders,
    bool ConvertToWebP,
    bool ConvertToAvif,
    bool ApplyUpscale,
    bool DeleteSourceFile,
    bool IncludeWebPFiles,
    bool IncludeAvifFiles
);
