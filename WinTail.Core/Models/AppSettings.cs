namespace WinTail.Core.Models;

/// <summary>
/// Application settings
/// </summary>
public class AppSettings
{
    public ThemeMode Theme { get; set; } = ThemeMode.System;
    public Dictionary<string, SyntaxLanguage> FileTypeLanguageMap { get; set; } = new();
    public string DefaultFontFamily { get; set; } = "Cascadia Code,Consolas,Courier New";
    public int DefaultFontSize { get; set; } = 12;
    public int MaxLinesPerFile { get; set; } = 10000;
}
