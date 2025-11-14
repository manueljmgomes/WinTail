using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using Microsoft.UI.Xaml;
using WinTail.Core.Models;

namespace WinTail.ViewModels;

/// <summary>
/// ViewModel for the main window, managing tabs, theme, and global settings
/// </summary>
public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<FileTabViewModel> _tabs = new();

    [ObservableProperty]
    private FileTabViewModel? _selectedTab;

    [ObservableProperty]
    private bool _isFollowing = true;

    [ObservableProperty]
    private string _followButtonText = "Pause";

    [ObservableProperty]
    private int _selectedLanguageIndex = 0;

    [ObservableProperty]
    private int _selectedThemeIndex = 2; // Default to System

    [ObservableProperty]
    private string _statusText = "Ready";

    [ObservableProperty]
    private string _totalLinesText = "0 lines";

    [ObservableProperty]
    private string _stateText = "Idle";

    public ObservableCollection<string> AvailableLanguages { get; } = new()
    {
        "Plain Text",
        "Log",
        "JSON",
        "XML",
        "C#"
    };

    public ObservableCollection<string> AvailableThemes { get; } = new()
    {
        "Light",
        "Dark",
        "System"
    };

    private AppSettings _settings = new();
    private Window? _window;

    public MainViewModel()
    {
        // Get the main window reference
        _window = App.MainWindow;
    }

    partial void OnIsFollowingChanged(bool value)
    {
        FollowButtonText = value ? "Pause" : "Resume";
        
        if (SelectedTab != null)
        {
            if (value)
            {
                SelectedTab.Resume();
            }
            else
            {
                SelectedTab.Pause();
            }
        }
    }

    partial void OnSelectedTabChanged(FileTabViewModel? value)
    {
        UpdateStatusBar();
        
        if (value != null)
        {
            // Sync language selector with tab's language
            SelectedLanguageIndex = (int)value.SyntaxLanguage;
        }
    }

    partial void OnSelectedLanguageIndexChanged(int value)
    {
        if (SelectedTab != null && value >= 0 && value < 5)
        {
            SelectedTab.SyntaxLanguage = (SyntaxLanguage)value;
        }
    }

    partial void OnSelectedThemeIndexChanged(int value)
    {
        if (value >= 0 && value < 3)
        {
            var themeMode = (ThemeMode)value;
            ApplyTheme(themeMode);
            _settings.Theme = themeMode;
            _ = SaveSettingsAsync();
        }
    }

    /// <summary>
    /// Load settings from disk
    /// </summary>
    public async Task LoadSettingsAsync()
    {
        _settings = await App.SettingsService.LoadSettingsAsync();
        
        // Apply theme
        SelectedThemeIndex = (int)_settings.Theme;
    }

    /// <summary>
    /// Save settings to disk
    /// </summary>
    public async Task SaveSettingsAsync()
    {
        await App.SettingsService.SaveSettingsAsync(_settings);
    }

    /// <summary>
    /// Open a file in a new tab
    /// </summary>
    public async Task OpenFileAsync(string filePath)
    {
        // Check if file is already open
        var existingTab = Tabs.FirstOrDefault(t => t.FilePath.Equals(filePath, StringComparison.OrdinalIgnoreCase));
        if (existingTab != null)
        {
            SelectedTab = existingTab;
            return;
        }

        // Create new tab
        var tabViewModel = new FileTabViewModel(filePath);
        tabViewModel.StateChanged += OnTabStateChanged;
        
        Tabs.Add(tabViewModel);
        SelectedTab = tabViewModel;

        // Start tailing
        await tabViewModel.StartTailingAsync();
    }

    /// <summary>
    /// Close a tab
    /// </summary>
    public void CloseTab(FileTabViewModel tab)
    {
        tab.StateChanged -= OnTabStateChanged;
        tab.Dispose();
        Tabs.Remove(tab);

        if (Tabs.Count > 0 && SelectedTab == tab)
        {
            SelectedTab = Tabs[^1];
        }
    }

    /// <summary>
    /// Handle tab state changes to update status bar
    /// </summary>
    private void OnTabStateChanged(object? sender, Core.Models.TailState state)
    {
        UpdateStatusBar();
    }

    /// <summary>
    /// Update status bar with current tab information
    /// </summary>
    private void UpdateStatusBar()
    {
        if (SelectedTab == null)
        {
            StatusText = "Ready";
            TotalLinesText = "0 lines";
            StateText = "Idle";
            return;
        }

        StatusText = SelectedTab.FilePath;
        TotalLinesText = $"{SelectedTab.TotalLines} lines";
        StateText = SelectedTab.State.ToString();
    }

    /// <summary>
    /// Apply theme to the window
    /// </summary>
    private void ApplyTheme(ThemeMode themeMode)
    {
        if (_window?.Content is FrameworkElement rootElement)
        {
            rootElement.RequestedTheme = themeMode switch
            {
                ThemeMode.Light => ElementTheme.Light,
                ThemeMode.Dark => ElementTheme.Dark,
                ThemeMode.System => ElementTheme.Default,
                _ => ElementTheme.Default
            };
        }
    }
}
