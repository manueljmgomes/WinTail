using System.Text;
using WinTail.Core.Interfaces;
using WinTail.Core.Models;

namespace WinTail.Core.Services;

/// <summary>
/// Implementation of file tailing service that efficiently monitors and reads new lines from a file.
/// Uses FileSystemWatcher for change detection and async I/O for reading.
/// </summary>
public class FileTailService : IFileTailService
{
    private readonly string _filePath;
    private FileSystemWatcher? _watcher;
    private CancellationTokenSource? _cancellationTokenSource;
    private long _lastPosition;
    private long _currentLineNumber;
    private TailState _state;
    private readonly object _stateLock = new();
    private Task? _tailTask;
    private bool _disposed;

    public event EventHandler<IEnumerable<TailLine>>? LinesAdded;
    public event EventHandler<TailState>? StateChanged;
    public event EventHandler<string>? ErrorOccurred;

    public string FilePath => _filePath;
    public TailState State
    {
        get
        {
            lock (_stateLock)
            {
                return _state;
            }
        }
        private set
        {
            lock (_stateLock)
            {
                if (_state != value)
                {
                    _state = value;
                    StateChanged?.Invoke(this, _state);
                }
            }
        }
    }

    public long TotalLinesRead => _currentLineNumber;

    public FileTailService(string filePath)
    {
        _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
        _state = TailState.Idle;
    }

    /// <summary>
    /// Start tailing the file. Reads from the end and follows new content.
    /// </summary>
    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (State != TailState.Idle && State != TailState.Error && State != TailState.FileMissing)
        {
            return;
        }

        if (!File.Exists(_filePath))
        {
            State = TailState.FileMissing;
            ErrorOccurred?.Invoke(this, $"File not found: {_filePath}");
            return;
        }

        try
        {
            _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            State = TailState.Running;

            // Get initial file size and position at the end
            var fileInfo = new FileInfo(_filePath);
            _lastPosition = Math.Max(0, fileInfo.Length);

            // Read initial lines (last N lines) from the file
            await ReadInitialLinesAsync();

            // Set up file system watcher for changes
            SetupFileWatcher();

            // Start the tailing task
            _tailTask = Task.Run(() => TailLoop(_cancellationTokenSource.Token), _cancellationTokenSource.Token);
        }
        catch (Exception ex)
        {
            State = TailState.Error;
            ErrorOccurred?.Invoke(this, $"Error starting tail: {ex.Message}");
        }
    }

    /// <summary>
    /// Pause tailing - stop following but keep content
    /// </summary>
    public void Pause()
    {
        if (State == TailState.Running)
        {
            State = TailState.Paused;
        }
    }

    /// <summary>
    /// Resume tailing after pause
    /// </summary>
    public void Resume()
    {
        if (State == TailState.Paused)
        {
            State = TailState.Running;
        }
    }

    /// <summary>
    /// Stop tailing completely
    /// </summary>
    public void Stop()
    {
        _cancellationTokenSource?.Cancel();
        _watcher?.Dispose();
        _watcher = null;
        State = TailState.Idle;
    }

    /// <summary>
    /// Read the last 100 lines from the file as initial content
    /// </summary>
    private async Task ReadInitialLinesAsync()
    {
        const int initialLineCount = 100;
        var lines = new List<TailLine>();

        try
        {
            using var fileStream = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            using var reader = new StreamReader(fileStream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);

            // Read all lines first (for small files this is fine, for large files we'll optimize)
            var allLines = new List<string>();
            string? line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                allLines.Add(line);
            }

            // Take last N lines
            var startIndex = Math.Max(0, allLines.Count - initialLineCount);
            _currentLineNumber = startIndex;

            for (int i = startIndex; i < allLines.Count; i++)
            {
                lines.Add(new TailLine
                {
                    Content = allLines[i],
                    LineNumber = ++_currentLineNumber,
                    Timestamp = DateTime.Now
                });
            }

            _lastPosition = fileStream.Position;
        }
        catch (Exception ex)
        {
            ErrorOccurred?.Invoke(this, $"Error reading initial lines: {ex.Message}");
        }

        if (lines.Count > 0)
        {
            LinesAdded?.Invoke(this, lines);
        }
    }

    /// <summary>
    /// Set up FileSystemWatcher to detect file changes
    /// </summary>
    private void SetupFileWatcher()
    {
        var directory = Path.GetDirectoryName(_filePath);
        var fileName = Path.GetFileName(_filePath);

        if (string.IsNullOrEmpty(directory) || string.IsNullOrEmpty(fileName))
        {
            return;
        }

        _watcher = new FileSystemWatcher(directory, fileName)
        {
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.FileName,
            EnableRaisingEvents = true
        };

        _watcher.Changed += OnFileChanged;
        _watcher.Deleted += OnFileDeleted;
        _watcher.Renamed += OnFileRenamed;
    }

    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        // File changed, the tail loop will pick it up
    }

    private void OnFileDeleted(object sender, FileSystemEventArgs e)
    {
        State = TailState.FileMissing;
        ErrorOccurred?.Invoke(this, $"File deleted: {_filePath}");
    }

    private void OnFileRenamed(object sender, RenamedEventArgs e)
    {
        State = TailState.FileMissing;
        ErrorOccurred?.Invoke(this, $"File renamed from {e.OldFullPath} to {e.FullPath}");
    }

    /// <summary>
    /// Main tailing loop that reads new content
    /// </summary>
    private async Task TailLoop(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                if (State == TailState.Running)
                {
                    await ReadNewLinesAsync();
                }

                // Wait a bit before checking again
                await Task.Delay(500, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(this, $"Error in tail loop: {ex.Message}");
                await Task.Delay(1000, cancellationToken);
            }
        }
    }

    /// <summary>
    /// Read new lines that have been added since last read
    /// </summary>
    private async Task ReadNewLinesAsync()
    {
        if (!File.Exists(_filePath))
        {
            if (State != TailState.FileMissing)
            {
                State = TailState.FileMissing;
                ErrorOccurred?.Invoke(this, $"File not found: {_filePath}");
            }
            return;
        }

        try
        {
            using var fileStream = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            
            var fileLength = fileStream.Length;

            // Check if file was truncated
            if (fileLength < _lastPosition)
            {
                _lastPosition = 0;
                ErrorOccurred?.Invoke(this, "File was truncated, restarting from beginning");
            }

            // No new content
            if (fileLength == _lastPosition)
            {
                return;
            }

            // Seek to last read position
            fileStream.Seek(_lastPosition, SeekOrigin.Begin);

            using var reader = new StreamReader(fileStream, Encoding.UTF8, detectEncodingFromByteOrderMarks: false);
            
            var lines = new List<TailLine>();
            string? line;

            while ((line = await reader.ReadLineAsync()) != null)
            {
                lines.Add(new TailLine
                {
                    Content = line,
                    LineNumber = ++_currentLineNumber,
                    Timestamp = DateTime.Now
                });
            }

            _lastPosition = fileStream.Position;

            if (lines.Count > 0)
            {
                LinesAdded?.Invoke(this, lines);
            }
        }
        catch (IOException)
        {
            // File might be locked, try again later
        }
        catch (Exception ex)
        {
            ErrorOccurred?.Invoke(this, $"Error reading new lines: {ex.Message}");
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        Stop();
        _cancellationTokenSource?.Dispose();
        _watcher?.Dispose();
        _disposed = true;

        GC.SuppressFinalize(this);
    }
}
