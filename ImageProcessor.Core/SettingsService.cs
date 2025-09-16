using System;
using System.IO;
using System.Text.Json;

namespace ImageProcessor.Core
{
    public class SettingsService
    {
        private static readonly Lazy<SettingsService> instance = new(() => new SettingsService());
        private readonly string _settingsFilePath;

        public static SettingsService Instance => instance.Value;

        public UserSettings UserSettings { get; set; }

        private SettingsService()
        {
            string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string appFolder = Path.Combine(appDataFolder, "ImageProcessor");
            Directory.CreateDirectory(appFolder);
            _settingsFilePath = Path.Combine(appFolder, "usersettings.json");

            Load();
        }

        public void Load()
        {
            if (File.Exists(_settingsFilePath))
            {
                try
                {
                    string json = File.ReadAllText(_settingsFilePath);
                    System.Diagnostics.Debug.WriteLine($"Loading settings: {json}");
                    UserSettings = JsonSerializer.Deserialize<UserSettings>(json) ?? new UserSettings();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error loading settings: {ex.Message}");
                    // If deserialization fails, create new settings
                    UserSettings = new UserSettings();
                }
            }
            else
            {
                UserSettings = new UserSettings();
            }
        }

        public void Save()
        {
            try
            {
                string json = JsonSerializer.Serialize(UserSettings, new JsonSerializerOptions { WriteIndented = true });
                System.Diagnostics.Debug.WriteLine($"Saving settings: {json}");
                File.WriteAllText(_settingsFilePath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving settings: {ex.Message}");
                // Handle exceptions during save
            }
        }
    }
}