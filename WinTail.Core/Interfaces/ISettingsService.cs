using WinTail.Core.Models;

namespace WinTail.Core.Interfaces;

/// <summary>
/// Service for managing application settings
/// </summary>
public interface ISettingsService
{
    /// <summary>
    /// Load settings from storage
    /// </summary>
    Task<AppSettings> LoadSettingsAsync();

    /// <summary>
    /// Save settings to storage
    /// </summary>
    Task SaveSettingsAsync(AppSettings settings);

    /// <summary>
    /// Get the settings file path
    /// </summary>
    string GetSettingsFilePath();
}
