
using System.Text.Json;

namespace ImageProcessor.Core;

/// <summary>
/// Manages saving, loading, and deleting processing configuration profiles.
/// </summary>
public class ProfileService
{
    private readonly string _profilesDirectory;

    public ProfileService()
    {
        // Store profiles in a user-specific, application-dedicated folder.
        string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        _profilesDirectory = Path.Combine(appDataPath, "ImageProcessor", "Profiles");

        // Ensure the directory exists every time the service is instantiated.
        Directory.CreateDirectory(_profilesDirectory);
    }

    /// <summary>
    /// Gets the full path for a given profile name.
    /// </summary>
    private string GetProfilePath(string profileName)
    {
        // Sanitize file name to prevent directory traversal attacks, although unlikely in this context.
        string safeFileName = Path.GetFileNameWithoutExtension(profileName);
        return Path.Combine(_profilesDirectory, $"{safeFileName}.json");
    }

    /// <summary>
    /// Retrieves a list of all available profile names.
    /// </summary>
    public List<string> GetProfileNames()
    {
        var profileFiles = Directory.GetFiles(_profilesDirectory, "*.json");
        return profileFiles
            .Select(Path.GetFileNameWithoutExtension)
            .Where(name => name != null)
            .ToList()!;
    }

    /// <summary>
    /// Saves a ProcessingOptions object to a JSON file.
    /// </summary>
    public void SaveProfile(string profileName, ProcessingOptions options)
    {
        if (string.IsNullOrWhiteSpace(profileName))
        {
            throw new ArgumentException("Profile name cannot be empty.", nameof(profileName));
        }

        string filePath = GetProfilePath(profileName);
        var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
        string jsonString = JsonSerializer.Serialize(options, jsonOptions);
        File.WriteAllText(filePath, jsonString);
    }

    /// <summary>
    /// Loads ProcessingOptions from a JSON file.
    /// </summary>
    public ProcessingOptions? LoadProfile(string profileName)
    {
        if (string.IsNullOrWhiteSpace(profileName))
        {
            return null;
        }

        string filePath = GetProfilePath(profileName);

        if (!File.Exists(filePath))
        {
            return null;
        }

        string jsonString = File.ReadAllText(filePath);
        return JsonSerializer.Deserialize<ProcessingOptions>(jsonString);
    }

    /// <summary>
    /// Deletes a profile file.
    /// </summary>
    public void DeleteProfile(string profileName)
    {
        if (string.IsNullOrWhiteSpace(profileName))
        {
            return;
        }

        string filePath = GetProfilePath(profileName);

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }
}
