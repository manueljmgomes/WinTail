using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        // Partial method to intercept LogContent changes
        partial void OnLogContentChanged(string value)
        {
            Debug.WriteLine($"[LogTabViewModel] ***** LogContent CHANGED ***** Length: {value?.Length ?? 0}");
            if (value != null && value.Length > 0)
            {
                Debug.WriteLine($"[LogTabViewModel] ***** LogContent first 200 chars: {value.Substring(0, Math.Min(200, value.Length))}");
            }
        }

        public LogTabViewModel(LogTab logTab)
        {
            Debug.WriteLine($"[LogTabViewModel] Constructor called for: {logTab.FilePath}");
            
            _logTab = logTab;
            FilePath = logTab.FilePath;
            FileName = logTab.FileName;

            Debug.WriteLine($"[LogTabViewModel] FilePath={FilePath}, FileName={FileName}");

            _fileWatcher = new FileWatcherService(logTab.FilePath);
            _fileWatcher.NewLinesAdded += OnNewLinesAdded;

            Debug.WriteLine($"[LogTabViewModel] Calling LoadInitialContent...");
            LoadInitialContent();
            
            Debug.WriteLine($"[LogTabViewModel] Calling StartWatching...");
            _fileWatcher.StartWatching();
            
            Debug.WriteLine($"[LogTabViewModel] Constructor complete. LogContent length: {LogContent?.Length ?? 0}");
        }

        private void LoadInitialContent()
        {
            Debug.WriteLine($"[LogTabViewModel] LoadInitialContent: Starting...");
            
            try
            {
                _logLines.Clear();
                var lines = _fileWatcher.ReadLastLines(1000);
                
                Debug.WriteLine($"[LogTabViewModel] LoadInitialContent: Got {lines.Count} lines from FileWatcher");
                
                _logLines.AddRange(lines);
                
                Debug.WriteLine($"[LogTabViewModel] LoadInitialContent: Total lines in _logLines: {_logLines.Count}");
                
                UpdateLogDisplay();
                UpdateTimestamp();
                
                Debug.WriteLine($"[LogTabViewModel] LoadInitialContent: Complete. LogContent length: {LogContent?.Length ?? 0}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[LogTabViewModel] LoadInitialContent ERROR: {ex.Message}");
                Debug.WriteLine($"[LogTabViewModel] LoadInitialContent Stack Trace: {ex.StackTrace}");
                LogContent = $"Error loading file: {ex.Message}";
            }
        }

        private void OnNewLinesAdded(object? sender, string newContent)
        {
            Debug.WriteLine($"[LogTabViewModel] OnNewLinesAdded: Received {newContent?.Length ?? 0} characters");
            
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                var lines = newContent.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                Debug.WriteLine($"[LogTabViewModel] OnNewLinesAdded: Split into {lines.Length} lines");
                
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

                Debug.WriteLine($"[LogTabViewModel] OnNewLinesAdded: Total lines now: {_logLines.Count}");

                UpdateLogDisplay();
                UpdateTimestamp();
            });
        }

        private void UpdateLogDisplay()
        {
            Debug.WriteLine($"[LogTabViewModel] UpdateLogDisplay: Starting with {_logLines.Count} lines");
            
            string newContent;
            if (IsFilterActive && !string.IsNullOrWhiteSpace(FilterTerm))
            {
                var filteredLines = _logLines
                    .Where(line => line.Contains(FilterTerm, StringComparison.OrdinalIgnoreCase))
                    .ToList();
                newContent = string.Join(Environment.NewLine, filteredLines);
                Debug.WriteLine($"[LogTabViewModel] UpdateLogDisplay: Filtered to {filteredLines.Count} lines");
            }
            else
            {
                newContent = string.Join(Environment.NewLine, _logLines);
                Debug.WriteLine($"[LogTabViewModel] UpdateLogDisplay: All lines included");
            }
            
            Debug.WriteLine($"[LogTabViewModel] UpdateLogDisplay: About to set LogContent to {newContent?.Length ?? 0} characters");
            LogContent = newContent;
            Debug.WriteLine($"[LogTabViewModel] UpdateLogDisplay: LogContent property now has {LogContent?.Length ?? 0} characters");
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
            Debug.WriteLine($"[LogTabViewModel] Refresh: Called");
            LoadInitialContent();
        }

        [RelayCommand]
        private async Task CopyContent()
        {
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
                        Debug.WriteLine($"[LogTabViewModel] CopyContent: Copied {LogContent?.Length ?? 0} characters to clipboard");
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[LogTabViewModel] CopyContent ERROR: {ex.Message}");
            }
        }

        public void Cleanup()
        {
            Debug.WriteLine($"[LogTabViewModel] Cleanup: Called for {FilePath}");
            _fileWatcher?.Dispose();
        }
    }
}
