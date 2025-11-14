namespace WinTail.Core.Models;

/// <summary>
/// Represents the state of a file being tailed
/// </summary>
public enum TailState
{
    Idle,
    Running,
    Paused,
    Error,
    FileMissing
}
