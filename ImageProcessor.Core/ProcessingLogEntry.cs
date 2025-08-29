namespace ImageProcessor.Core;

public class ProcessingLogEntry
{
    public DateTime Date { get; set; }
    public required string InputFile { get; set; }
    public required string OutputFile { get; set; }
    public required string InputFolder { get; set; }
    public required string OutputFolder { get; set; }
    public required string OriginalFileName { get; set; }
    public required string ProcessedFileName { get; set; }
    public long OriginalSize { get; set; }
    public long ProcessedSize { get; set; }
    public double ReductionPercentage => OriginalSize > 0 ? (double)(OriginalSize - ProcessedSize) / OriginalSize : 0;
}
