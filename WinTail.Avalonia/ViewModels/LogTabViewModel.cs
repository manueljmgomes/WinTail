using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WinTail.Avalonia.Models;
using WinTail.Avalonia.Services;

namespace WinTail.Avalonia.ViewModels
{
    public partial class LogTabViewModel : ViewModelBase
    {
        private readonly LogTab _logTab;
        private readonly FileWatcherService _fileWatcher;
        private readonly List<string> _logLines = new();

        [ObservableProperty]
        private string _fileName = string.Empty;

        [ObservableProperty]
        private string _filePath = string.Empty;

        [ObservableProperty]
        private string _logContent = string.Empty;

        [ObservableProperty]
        private string _searchTerm = string.Empty;

        [ObservableProperty]
        private string _filterTerm = string.Empty;

        [ObservableProperty]
        private bool _isFilterActive;

        [ObservableProperty]
        private bool _isSearchVisible;

        [ObservableProperty]
        private bool _isFilterVisible;

        [ObservableProperty]
        private DateTime _lastUpdate = DateTime.Now;

        /// <summary>
        /// Display title with filename and last update time
        /// </summary>
        public string DisplayTitle => $"{FileName} - Updated: {LastUpdate:HH:mm:ss}";

        public LogTabViewModel(LogTab logTab)
        {
            _logTab = logTab;
            FilePath = logTab.FilePath;
            FileName = logTab.FileName;

            _fileWatcher = new FileWatcherService(logTab.FilePath);
            _fileWatcher.NewLinesAdded += OnNewLinesAdded;

            LoadInitialContent();
            _fileWatcher.StartWatching();
        }

        private void LoadInitialContent()
        {
            try
            {
                _logLines.Clear();
                _logLines.AddRange(_fileWatcher.ReadLastLines(1000));
                UpdateLogDisplay();
                UpdateTimestamp();
            }
            catch (Exception ex)
            {
                LogContent = $"Error loading file: {ex.Message}";
            }
        }

        private void OnNewLinesAdded(object? sender, string newContent)
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                var lines = newContent.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                foreach (var line in lines)
                {
                    if (!string.IsNullOrEmpty(line) || lines.Length > 1)
                    {
                        _logLines.Add(line);
                    }
                }

                // Keep only last 10000 lines in memory
                while (_logLines.Count > 10000)
                {
                    _logLines.RemoveAt(0);
                }

                UpdateLogDisplay();
                UpdateTimestamp();
            });
        }

        private void UpdateLogDisplay()
        {
            if (IsFilterActive && !string.IsNullOrWhiteSpace(FilterTerm))
            {
                var filteredLines = _logLines
                    .Where(line => line.Contains(FilterTerm, StringComparison.OrdinalIgnoreCase))
                    .ToList();
                LogContent = string.Join(Environment.NewLine, filteredLines);
            }
            else
            {
                LogContent = string.Join(Environment.NewLine, _logLines);
            }
        }

        private void UpdateTimestamp()
        {
            LastUpdate = DateTime.Now;
            OnPropertyChanged(nameof(DisplayTitle));
        }

        [RelayCommand]
        private void ToggleSearch()
        {
            IsSearchVisible = !IsSearchVisible;
            if (!IsSearchVisible)
            {
                SearchTerm = string.Empty;
            }
        }

        [RelayCommand]
        private void ToggleFilter()
        {
            IsFilterVisible = !IsFilterVisible;
            if (!IsFilterVisible)
            {
                ClearFilter();
            }
        }

        [RelayCommand]
        private void ApplyFilter()
        {
            IsFilterActive = !string.IsNullOrWhiteSpace(FilterTerm);
            UpdateLogDisplay();
        }

        [RelayCommand]
        private void ClearFilter()
        {
            FilterTerm = string.Empty;
            IsFilterActive = false;
            UpdateLogDisplay();
        }

        [RelayCommand]
        private void Refresh()
        {
            LoadInitialContent();
        }

        [RelayCommand]
        private async Task CopyContent()
        {
            // Note: Clipboard access requires TopLevel which is only available from Views
            // This is a simplified version - in production, pass TopLevel reference or use a service
            try
            {
                await Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    var topLevel = TopLevel.GetTopLevel(global::Avalonia.Application.Current?.ApplicationLifetime is global::Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop 
                        ? desktop.MainWindow 
                        : null);
                    
                    if (topLevel?.Clipboard != null)
                    {
                        await topLevel.Clipboard.SetTextAsync(LogContent);
                    }
                });
            }
            catch
            {
                // Clipboard operation failed
            }
        }

        public void Cleanup()
        {
            _fileWatcher?.Dispose();
        }
    }
}
