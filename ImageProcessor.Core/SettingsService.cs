using System;

namespace ImageProcessor.Core
{
    public class SettingsService
    {
        private static readonly Lazy<SettingsService> instance = new(() => new SettingsService());

        public static SettingsService Instance => instance.Value;

        public RealEsrganSettings RealEsrganSettings { get; set; }

        private SettingsService()
        {
            // In the future, load settings from a file here.
            RealEsrganSettings = new RealEsrganSettings();
        }

        public void Save()
        {
            // In the future, save settings to a file here.
        }
    }
}
