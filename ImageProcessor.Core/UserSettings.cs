
namespace ImageProcessor.Core
{
    public class UserSettings
    {
        public string InputFolder { get; set; } = string.Empty;
        public string OutputFolder { get; set; } = string.Empty;
        public bool UseInputFolderAsOutput { get; set; }
        public bool ProcessSubfolders { get; set; }
        public bool ConvertToWebP { get; set; } = true;
        public bool ConvertToAvif { get; set; }
        public bool ApplyUpscale { get; set; } = true;
        public bool DeleteSourceFile { get; set; }
        public bool IncludeWebPFiles { get; set; }
        public bool IncludeAvifFiles { get; set; }
        public string SelectedModel { get; set; } = "realesrgan-x4plus";
        public bool IsDarkMode { get; set; }
        public RealEsrganSettings RealEsrganSettings { get; set; } = new();
    }
}
