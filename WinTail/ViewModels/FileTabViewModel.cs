using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls;
using WinTail.Core.Interfaces;
using WinTail.Core.Models;
using WinTail.Core.Services;

namespace WinTail.ViewModels;

/// <summary>
/// ViewModel for a single file tab, managing the tailing operation and content display
/// </summary>
public partial class FileTabViewModel : ObservableObject, IDisposable
{
    private readonly IFileTailService _tailService;
    private readonly StringBuilder _contentBuilder = new();
    private readonly Queue<TailLine> _lineBuffer = new();
    private const int MaxLines = 10000;
    private bool _disposed;

    [ObservableProperty]
    private string _fileName = string.Empty;

    [ObservableProperty]
    private string _filePath = string.Empty;

    [ObservableProperty]
    private string _content = string.Empty;

    [ObservableProperty]
    private long _totalLines = 0;

    [ObservableProperty]
    private TailState _state = TailState.Idle;

    [ObservableProperty]
    private SyntaxLanguage _syntaxLanguage = SyntaxLanguage.PlainText;

    [ObservableProperty]
    private bool _showStatusInfo = false;

    [ObservableProperty]
    private InfoBarSeverity _statusSeverity = InfoBarSeverity.Informational;

    [ObservableProperty]
    private string _statusTitle = string.Empty;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private IconSource? _tabIcon;

    public event EventHandler<TailState>? StateChanged;

    public FileTabViewModel(string filePath)
    {
        FilePath = filePath;
        FileName = Path.GetFileName(filePath);
        
        _tailService = new FileTailService(filePath);
        _tailService.LinesAdded += OnLinesAdded;
        _tailService.StateChanged += OnStateChanged;
        _tailService.ErrorOccurred += OnErrorOccurred;

        // Infer syntax language from file extension
        InferSyntaxLanguage();
        
        UpdateTabIcon();
    }

    /// <summary>
    /// Start tailing the file
    /// </summary>
    public async Task StartTailingAsync()
    {
        try
        {
            await _tailService.StartAsync();
        }
        catch (Exception ex)
        {
            ShowError($"Failed to start tailing: {ex.Message}");
        }
    }

    /// <summary>
    /// Pause tailing
    /// </summary>
    public void Pause()
    {
        _tailService.Pause();
    }

    /// <summary>
    /// Resume tailing
    /// </summary>
    public void Resume()
    {
        _tailService.Resume();
    }

    /// <summary>
    /// Stop tailing
    /// </summary>
    public void Stop()
    {
        _tailService.Stop();
    }

    /// <summary>
    /// Handle new lines from the tail service
    /// </summary>
    private void OnLinesAdded(object? sender, IEnumerable<TailLine> lines)
    {
        // Add lines to buffer
        foreach (var line in lines)
        {
            _lineBuffer.Enqueue(line);
            TotalLines++;
        }

        // Maintain max line limit
        while (_lineBuffer.Count > MaxLines)
        {
            _lineBuffer.Dequeue();
        }

        // Rebuild content from buffer
        UpdateContent();
    }

    /// <summary>
    /// Update the displayed content
    /// </summary>
    private void UpdateContent()
    {
        _contentBuilder.Clear();
        
        foreach (var line in _lineBuffer)
        {
            _contentBuilder.AppendLine(line.Content);
        }

        Content = _contentBuilder.ToString();
    }

    /// <summary>
    /// Handle state changes from the tail service
    /// </summary>
    private void OnStateChanged(object? sender, TailState newState)
    {
        State = newState;
        StateChanged?.Invoke(this, newState);
        UpdateTabIcon();
        UpdateStatusInfo();
    }

    /// <summary>
    /// Handle errors from the tail service
    /// </summary>
    private void OnErrorOccurred(object? sender, string errorMessage)
    {
        ShowError(errorMessage);
    }

    /// <summary>
    /// Update status info bar based on current state
    /// </summary>
    private void UpdateStatusInfo()
    {
        switch (State)
        {
            case TailState.Paused:
                ShowStatusInfo = true;
                StatusSeverity = InfoBarSeverity.Warning;
                StatusTitle = "Paused";
                StatusMessage = "File tailing is paused. New content will not be displayed.";
                break;

            case TailState.Error:
                ShowStatusInfo = true;
                StatusSeverity = InfoBarSeverity.Error;
                StatusTitle = "Error";
                StatusMessage = "An error occurred while tailing the file.";
                break;

            case TailState.FileMissing:
                ShowStatusInfo = true;
                StatusSeverity = InfoBarSeverity.Error;
                StatusTitle = "File Missing";
                StatusMessage = "The file is no longer available or has been deleted.";
                break;

            default:
                ShowStatusInfo = false;
                break;
        }
    }

    /// <summary>
    /// Show an error message
    /// </summary>
    private void ShowError(string message)
    {
        ShowStatusInfo = true;
        StatusSeverity = InfoBarSeverity.Error;
        StatusTitle = "Error";
        StatusMessage = message;
    }

    /// <summary>
    /// Update tab icon based on state
    /// </summary>
    private void UpdateTabIcon()
    {
        var symbol = State switch
        {
            TailState.Running => Symbol.Play,
            TailState.Paused => Symbol.Pause,
            TailState.Error => Symbol.ReportHacked,
            TailState.FileMissing => Symbol.Delete,
            _ => Symbol.Document
        };

        TabIcon = new SymbolIconSource { Symbol = symbol };
    }

    /// <summary>
    /// Infer syntax language from file extension
    /// </summary>
    private void InferSyntaxLanguage()
    {
        var extension = Path.GetExtension(FilePath).ToLowerInvariant();

        SyntaxLanguage = extension switch
        {
            ".log" or ".txt" => SyntaxLanguage.Log,
            ".json" => SyntaxLanguage.Json,
            ".xml" or ".xaml" or ".config" => SyntaxLanguage.Xml,
            ".cs" => SyntaxLanguage.CSharp,
            _ => SyntaxLanguage.PlainText
        };
    }

    partial void OnSyntaxLanguageChanged(SyntaxLanguage value)
    {
        // In a real implementation, this would trigger syntax highlighting update
        // For now, we're just using plain text display
        // TODO: Integrate with a syntax highlighting library when adding step 9
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        Stop();
        
        if (_tailService != null)
        {
            _tailService.LinesAdded -= OnLinesAdded;
            _tailService.StateChanged -= OnStateChanged;
            _tailService.ErrorOccurred -= OnErrorOccurred;
            _tailService.Dispose();
        }

        _lineBuffer.Clear();
        _contentBuilder.Clear();
        
        _disposed = true;
    }
}
