namespace WinTail.Core.Models;

/// <summary>
/// Represents a new line added to a tailed file
/// </summary>
public class TailLine
{
    public string Content { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.Now;
    public long LineNumber { get; set; }
}
