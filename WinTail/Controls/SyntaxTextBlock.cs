using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Media;
using WinTail.Core.Models;
using WinTail.Helpers;

namespace WinTail.Controls;

/// <summary>
/// Custom control for displaying syntax-highlighted text
/// </summary>
public sealed class SyntaxTextBlock : Control
{
    private RichTextBlock? _richTextBlock;
    private bool _isLoaded;

    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(
            nameof(Text),
            typeof(string),
            typeof(SyntaxTextBlock),
            new PropertyMetadata(string.Empty, OnTextChanged));

    public static readonly DependencyProperty SyntaxLanguageProperty =
        DependencyProperty.Register(
            nameof(SyntaxLanguage),
            typeof(SyntaxLanguage),
            typeof(SyntaxTextBlock),
            new PropertyMetadata(SyntaxLanguage.PlainText, OnSyntaxLanguageChanged));

    public static readonly new DependencyProperty FontFamilyProperty =
        DependencyProperty.Register(
            nameof(FontFamily),
            typeof(FontFamily),
            typeof(SyntaxTextBlock),
            new PropertyMetadata(new FontFamily("Cascadia Code,Consolas,Courier New")));

    public static readonly new DependencyProperty FontSizeProperty =
        DependencyProperty.Register(
            nameof(FontSize),
            typeof(double),
            typeof(SyntaxTextBlock),
            new PropertyMetadata(12.0));

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public SyntaxLanguage SyntaxLanguage
    {
        get => (SyntaxLanguage)GetValue(SyntaxLanguageProperty);
        set => SetValue(SyntaxLanguageProperty, value);
    }

    public new FontFamily FontFamily
    {
        get => (FontFamily)GetValue(FontFamilyProperty);
        set => SetValue(FontFamilyProperty, value);
    }

    public new double FontSize
    {
        get => (double)GetValue(FontSizeProperty);
        set => SetValue(FontSizeProperty, value);
    }

    public SyntaxTextBlock()
    {
        this.DefaultStyleKey = typeof(SyntaxTextBlock);
        this.Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _isLoaded = true;
        UpdateContent();
    }

    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        _richTextBlock = GetTemplateChild("PART_RichTextBlock") as RichTextBlock;
        UpdateContent();
    }

    private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SyntaxTextBlock control)
        {
            control.UpdateContent();
        }
    }

    private static void OnSyntaxLanguageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SyntaxTextBlock control)
        {
            control.UpdateContent();
        }
    }

    /// <summary>
    /// Update the displayed content with syntax highlighting
    /// </summary>
    private void UpdateContent()
    {
        if (_richTextBlock == null || !_isLoaded)
        {
            return;
        }

        _richTextBlock.Blocks.Clear();

        if (string.IsNullOrEmpty(Text))
        {
            return;
        }

        // For plain text or if syntax highlighting is not needed, just display as-is
        if (SyntaxLanguage == SyntaxLanguage.PlainText)
        {
            AddPlainText(Text);
            return;
        }

        // Apply syntax highlighting
        ApplySyntaxHighlighting(Text);
    }

    /// <summary>
    /// Add plain text without highlighting
    /// </summary>
    private void AddPlainText(string text)
    {
        if (_richTextBlock == null) return;

        var paragraph = new Paragraph();
        var run = new Run { Text = text };
        paragraph.Inlines.Add(run);
        _richTextBlock.Blocks.Add(paragraph);
    }

    /// <summary>
    /// Apply syntax highlighting to text
    /// </summary>
    private void ApplySyntaxHighlighting(string text)
    {
        if (_richTextBlock == null) return;

        var isDarkTheme = ActualTheme == ElementTheme.Dark;
        var patterns = SyntaxHighlighter.GetPatternsForLanguage(SyntaxLanguage);

        if (patterns.Count == 0)
        {
            AddPlainText(text);
            return;
        }

        // Split text into lines for better performance
        var lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

        foreach (var line in lines)
        {
            var paragraph = new Paragraph();
            
            if (string.IsNullOrWhiteSpace(line))
            {
                paragraph.Inlines.Add(new Run { Text = line });
                _richTextBlock.Blocks.Add(paragraph);
                continue;
            }

            // Find all matches for all patterns in this line
            var matches = new List<(int Start, int Length, string Type)>();

            foreach (var pattern in patterns)
            {
                var patternMatches = pattern.Pattern.Matches(line);
                foreach (System.Text.RegularExpressions.Match match in patternMatches)
                {
                    matches.Add((match.Index, match.Length, pattern.HighlightType));
                }
            }

            // Sort matches by start position
            matches = matches.OrderBy(m => m.Start).ToList();

            // Build runs from matches
            int currentPos = 0;

            foreach (var match in matches)
            {
                // Add text before match
                if (match.Start > currentPos)
                {
                    var beforeText = line.Substring(currentPos, match.Start - currentPos);
                    paragraph.Inlines.Add(new Run { Text = beforeText });
                }

                // Add highlighted text
                var highlightedText = line.Substring(match.Start, match.Length);
                var highlightedRun = new Run
                {
                    Text = highlightedText,
                    Foreground = SyntaxHighlighter.GetBrush(match.Type, isDarkTheme)
                };
                paragraph.Inlines.Add(highlightedRun);

                currentPos = match.Start + match.Length;
            }

            // Add remaining text
            if (currentPos < line.Length)
            {
                var remainingText = line.Substring(currentPos);
                paragraph.Inlines.Add(new Run { Text = remainingText });
            }

            _richTextBlock.Blocks.Add(paragraph);
        }
    }
}
