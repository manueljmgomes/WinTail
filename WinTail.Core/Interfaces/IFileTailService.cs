using WinTail.Core.Models;

namespace WinTail.Core.Interfaces;

/// <summary>
/// Service for tailing a single file
/// </summary>
public interface IFileTailService : IDisposable
{
    /// <summary>
    /// Fires when new lines are added to the file
    /// </summary>
    event EventHandler<IEnumerable<TailLine>>? LinesAdded;

    /// <summary>
    /// Fires when the tail state changes
    /// </summary>
    event EventHandler<TailState>? StateChanged;

    /// <summary>
    /// Fires when an error occurs
    /// </summary>
    event EventHandler<string>? ErrorOccurred;

    /// <summary>
    /// The file path being tailed
    /// </summary>
    string FilePath { get; }

    /// <summary>
    /// Current state of the tail operation
    /// </summary>
    TailState State { get; }

    /// <summary>
    /// Total number of lines read
    /// </summary>
    long TotalLinesRead { get; }

    /// <summary>
    /// Start tailing the file
    /// </summary>
    Task StartAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Pause tailing (keep content but stop following)
    /// </summary>
    void Pause();

    /// <summary>
    /// Resume tailing
    /// </summary>
    void Resume();

    /// <summary>
    /// Stop tailing completely
    /// </summary>
    void Stop();
}
