using System.Text.Json;
using WinTail.Core.Interfaces;
using WinTail.Core.Models;

namespace WinTail.Core.Services;

/// <summary>
/// Service for persisting and loading application settings to/from JSON file
/// </summary>
public class SettingsService : ISettingsService
{
    private readonly string _settingsFilePath;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public SettingsService()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var appFolder = Path.Combine(appDataPath, "WinTail");
        Directory.CreateDirectory(appFolder);
        _settingsFilePath = Path.Combine(appFolder, "settings.json");
    }

    /// <summary>
    /// Load settings from disk, or return defaults if file doesn't exist
    /// </summary>
    public async Task<AppSettings> LoadSettingsAsync()
    {
        if (!File.Exists(_settingsFilePath))
        {
            return new AppSettings();
        }

        try
        {
            using var fileStream = File.OpenRead(_settingsFilePath);
            var settings = await JsonSerializer.DeserializeAsync<AppSettings>(fileStream, JsonOptions);
            return settings ?? new AppSettings();
        }
        catch (Exception)
        {
            // If deserialization fails, return default settings
            return new AppSettings();
        }
    }

    /// <summary>
    /// Save settings to disk
    /// </summary>
    public async Task SaveSettingsAsync(AppSettings settings)
    {
        try
        {
            using var fileStream = File.Create(_settingsFilePath);
            await JsonSerializer.SerializeAsync(fileStream, settings, JsonOptions);
        }
        catch (Exception)
        {
            // Silently fail - settings are not critical
        }
    }

    /// <summary>
    /// Get the full path to the settings file
    /// </summary>
    public string GetSettingsFilePath()
    {
        return _settingsFilePath;
    }
}
