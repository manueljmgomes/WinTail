using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.UI;
using Microsoft.UI.Xaml.Media;
using Windows.UI;
using WinTail.Core.Models;

namespace WinTail.Helpers;

/// <summary>
/// Helper class for applying syntax highlighting to text based on language
/// Note: This is a simplified implementation. For production, consider using
/// a dedicated syntax highlighting library like ColorCode or RichTextKit.
/// </summary>
public static class SyntaxHighlighter
{
    // Color schemes for light and dark themes
    private static readonly Dictionary<string, (Color Light, Color Dark)> ColorSchemes = new()
    {
        ["keyword"] = (Color.FromArgb(255, 0, 0, 255), Color.FromArgb(255, 86, 156, 214)),
        ["string"] = (Color.FromArgb(255, 163, 21, 21), Color.FromArgb(255, 206, 145, 120)),
        ["comment"] = (Color.FromArgb(255, 0, 128, 0), Color.FromArgb(255, 87, 166, 74)),
        ["number"] = (Color.FromArgb(255, 9, 134, 88), Color.FromArgb(255, 181, 206, 168)),
        ["type"] = (Color.FromArgb(255, 43, 145, 175), Color.FromArgb(255, 78, 201, 176)),
        ["error"] = (Color.FromArgb(255, 255, 0, 0), Color.FromArgb(255, 244, 71, 71)),
        ["warning"] = (Color.FromArgb(255, 255, 140, 0), Color.FromArgb(255, 255, 200, 0)),
        ["info"] = (Color.FromArgb(255, 0, 122, 204), Color.FromArgb(255, 70, 150, 230)),
        ["timestamp"] = (Color.FromArgb(255, 128, 128, 128), Color.FromArgb(255, 156, 156, 156))
    };

    /// <summary>
    /// Get patterns for a specific language
    /// </summary>
    public static List<HighlightPattern> GetPatternsForLanguage(SyntaxLanguage language)
    {
        return language switch
        {
            SyntaxLanguage.Log => GetLogPatterns(),
            SyntaxLanguage.Json => GetJsonPatterns(),
            SyntaxLanguage.Xml => GetXmlPatterns(),
            SyntaxLanguage.CSharp => GetCSharpPatterns(),
            _ => new List<HighlightPattern>()
        };
    }

    /// <summary>
    /// Log file highlighting patterns
    /// </summary>
    private static List<HighlightPattern> GetLogPatterns()
    {
        return new List<HighlightPattern>
        {
            // Timestamp patterns
            new(@"\d{4}-\d{2}-\d{2}[\sT]\d{2}:\d{2}:\d{2}", "timestamp"),
            new(@"\d{2}:\d{2}:\d{2}", "timestamp"),
            
            // Log levels
            new(@"\b(ERROR|FATAL|CRITICAL)\b", "error"),
            new(@"\b(WARN|WARNING)\b", "warning"),
            new(@"\b(INFO|INFORMATION|DEBUG|TRACE)\b", "info"),
            
            // Numbers
            new(@"\b\d+\b", "number"),
        };
    }

    /// <summary>
    /// JSON highlighting patterns
    /// </summary>
    private static List<HighlightPattern> GetJsonPatterns()
    {
        return new List<HighlightPattern>
        {
            // String values
            new(@"""(?:[^""\\]|\\.)*""", "string"),
            
            // Property names (keys)
            new(@"""([^""]+)""\s*:", "keyword"),
            
            // Numbers
            new(@"\b-?\d+\.?\d*\b", "number"),
            
            // Keywords
            new(@"\b(true|false|null)\b", "keyword"),
        };
    }

    /// <summary>
    /// XML highlighting patterns
    /// </summary>
    private static List<HighlightPattern> GetXmlPatterns()
    {
        return new List<HighlightPattern>
        {
            // Comments
            new(@"<!--.*?-->", "comment"),
            
            // Tags
            new(@"</?[\w:]+", "keyword"),
            new(@"/>|>", "keyword"),
            
            // Attributes
            new(@"[\w:]+(?==)", "type"),
            
            // String values
            new(@"""[^""]*""", "string"),
            new(@"'[^']*'", "string"),
        };
    }

    /// <summary>
    /// C# highlighting patterns (simplified)
    /// </summary>
    private static List<HighlightPattern> GetCSharpPatterns()
    {
        return new List<HighlightPattern>
        {
            // Comments
            new(@"//.*$", "comment"),
            new(@"/\*.*?\*/", "comment"),
            
            // Strings
            new(@"""(?:[^""\\]|\\.)*""", "string"),
            new(@"@""(?:""""|[^""])*""", "string"),
            
            // Keywords
            new(@"\b(public|private|protected|internal|static|class|interface|namespace|using|void|string|int|bool|var|new|return|if|else|for|foreach|while|switch|case)\b", "keyword"),
            
            // Types
            new(@"\b[A-Z]\w+\b", "type"),
            
            // Numbers
            new(@"\b\d+\.?\d*[fFdDmM]?\b", "number"),
        };
    }

    /// <summary>
    /// Get color for a highlight type based on current theme
    /// </summary>
    public static SolidColorBrush GetBrush(string highlightType, bool isDarkTheme)
    {
        if (ColorSchemes.TryGetValue(highlightType, out var colors))
        {
            var color = isDarkTheme ? colors.Dark : colors.Light;
            return new SolidColorBrush(color);
        }

        return new SolidColorBrush(isDarkTheme ? Colors.White : Colors.Black);
    }
}

/// <summary>
/// Represents a syntax highlighting pattern
/// </summary>
public class HighlightPattern
{
    public Regex Pattern { get; }
    public string HighlightType { get; }

    public HighlightPattern(string pattern, string highlightType)
    {
        Pattern = new Regex(pattern, RegexOptions.Compiled | RegexOptions.Multiline);
        HighlightType = highlightType;
    }
}
