using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WinTail.Avalonia.Models;
using WinTail.Avalonia.Services;

namespace WinTail.Avalonia.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly SingleInstanceService _singleInstanceService;

    [ObservableProperty]
    private ObservableCollection<LogTabViewModel> _tabs = new();

    [ObservableProperty]
    private LogTabViewModel? _selectedTab;

    [ObservableProperty]
    private bool _hasNoTabs = true;

    public MainWindowViewModel()
    {
        _singleInstanceService = new SingleInstanceService();
        
        if (_singleInstanceService.IsFirstInstance)
        {
            _singleInstanceService.FilePathReceived += OnFilePathReceived;
            _singleInstanceService.StartListening();
        }

        Tabs.CollectionChanged += (s, e) =>
        {
            HasNoTabs = Tabs.Count == 0;
        };
    }

    [RelayCommand]
    private async Task OpenFile(Window? window)
    {
        if (window == null) return;

        var storage = window.StorageProvider;
        var file = await storage.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open Log File",
            AllowMultiple = false,
            FileTypeFilter = new[]
            {
                new FilePickerFileType("Log Files")
                {
                    Patterns = new[] { "*.log", "*.txt", "*.vlog" }
                },
                new FilePickerFileType("All Files")
                {
                    Patterns = new[] { "*" }
                }
            }
        });

        if (file != null && file.Count > 0)
        {
            var filePath = file[0].Path.LocalPath;
            OpenLogFile(filePath);
        }
    }

    public void OpenLogFile(string filePath)
    {
        // Normalize path
        filePath = Path.GetFullPath(filePath);

        // Check if tab already exists
        var existingTab = Tabs.FirstOrDefault(t => t.FilePath == filePath);
        if (existingTab != null)
        {
            SelectedTab = existingTab;
            return;
        }

        // Create new tab
        var logTab = new LogTab(filePath);
        var tabViewModel = new LogTabViewModel(logTab);
        
        Tabs.Add(tabViewModel);
        SelectedTab = tabViewModel;
    }

    [RelayCommand]
    private void CloseTab(LogTabViewModel? tab)
    {
        if (tab != null)
        {
            tab.Cleanup();
            Tabs.Remove(tab);
        }
    }

    private void OnFilePathReceived(object? sender, string filePath)
    {
        // This will be called from background thread
        // Avalonia will handle the UI thread marshaling when property changes
        OpenLogFile(filePath);
    }

    public void Cleanup()
    {
        foreach (var tab in Tabs)
        {
            tab.Cleanup();
        }
        _singleInstanceService?.Dispose();
    }
}
