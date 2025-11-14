using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using System;
using System.Text.RegularExpressions;

namespace LogTailPlus.Controls
{
    /// <summary>
    /// Custom control for displaying log files with syntax highlighting using TextBlock Inlines
    /// </summary>
    public class SyntaxHighlightedTextBlock : ScrollViewer
    {
        private readonly TextBlock _textBlock;
        
        public static readonly StyledProperty<string?> TextProperty =
            AvaloniaProperty.Register<SyntaxHighlightedTextBlock, string?>(nameof(Text));

        public string? Text
        {
            get => GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public SyntaxHighlightedTextBlock()
        {
            _textBlock = new TextBlock
            {
                FontFamily = new FontFamily("Cascadia Code, Cascadia Mono, Consolas, Courier New, monospace"),
                FontSize = 12,
                Foreground = new SolidColorBrush(Color.Parse("#2C2C2C")),
                Padding = new Thickness(16),
                TextWrapping = Avalonia.Media.TextWrapping.NoWrap
            };

            Content = _textBlock;
            HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto;
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto;
            Background = new SolidColorBrush(Color.Parse("#FAFAFA"));
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == TextProperty)
            {
                UpdateText(change.GetNewValue<string?>());
            }
        }

        private void UpdateText(string? text)
        {
            if (text == null)
            {
                _textBlock.Inlines?.Clear();
                return;
            }

            _textBlock.Inlines?.Clear();
            
            var lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            
            foreach (var line in lines)
            {
                ApplySyntaxHighlighting(line);
                _textBlock.Inlines?.Add(new Avalonia.Controls.Documents.Run("\n"));
            }
        }

        private void ApplySyntaxHighlighting(string line)
        {
            if (string.IsNullOrEmpty(line))
            {
                _textBlock.Inlines?.Add(new Avalonia.Controls.Documents.Run(" "));
                return;
            }

            // ERROR lines - Red
            if (Regex.IsMatch(line, @"\bERROR\b", RegexOptions.IgnoreCase))
            {
                _textBlock.Inlines?.Add(new Avalonia.Controls.Documents.Run(line)
                {
                    Foreground = new SolidColorBrush(Color.Parse("#E74856"))
                });
                return;
            }

            // WARNING lines - Orange
            if (Regex.IsMatch(line, @"\bWARN(ING)?\b", RegexOptions.IgnoreCase))
            {
                _textBlock.Inlines?.Add(new Avalonia.Controls.Documents.Run(line)
                {
                    Foreground = new SolidColorBrush(Color.Parse("#F59B00"))
                });
                return;
            }

            // INFO lines - Blue
            if (Regex.IsMatch(line, @"\bINFO\b", RegexOptions.IgnoreCase))
            {
                _textBlock.Inlines?.Add(new Avalonia.Controls.Documents.Run(line)
                {
                    Foreground = new SolidColorBrush(Color.Parse("#0078D4"))
                });
                return;
            }

            // DEBUG lines - Purple
            if (Regex.IsMatch(line, @"\bDEBUG\b", RegexOptions.IgnoreCase))
            {
                _textBlock.Inlines?.Add(new Avalonia.Controls.Documents.Run(line)
                {
                    Foreground = new SolidColorBrush(Color.Parse("#8B5CF6"))
                });
                return;
            }

            // Highlight special patterns within the line
            HighlightPatterns(line);
        }

        private void HighlightPatterns(string line)
        {
            // Pattern: Timestamp at start (e.g., 2025-11-13 19:45:01)
            var timestampMatch = Regex.Match(line, @"^(\d{4}-\d{2}-\d{2}\s+\d{2}:\d{2}:\d{2})");
            if (timestampMatch.Success)
            {
                _textBlock.Inlines?.Add(new Avalonia.Controls.Documents.Run(timestampMatch.Value)
                {
                    Foreground = new SolidColorBrush(Color.Parse("#16825D"))
                });
                line = line.Substring(timestampMatch.Length);
            }

            // Pattern: IP addresses
            var ipPattern = @"\b(?:\d{1,3}\.){3}\d{1,3}(?::\d+)?\b";
            var lastIndex = 0;
            
            foreach (Match match in Regex.Matches(line, ipPattern))
            {
                // Add text before match
                if (match.Index > lastIndex)
                {
                    _textBlock.Inlines?.Add(new Avalonia.Controls.Documents.Run(line.Substring(lastIndex, match.Index - lastIndex)));
                }
                
                // Add colored IP
                _textBlock.Inlines?.Add(new Avalonia.Controls.Documents.Run(match.Value)
                {
                    Foreground = new SolidColorBrush(Color.Parse("#D73B48"))
                });
                
                lastIndex = match.Index + match.Length;
            }
            
            // Add remaining text
            if (lastIndex < line.Length)
            {
                _textBlock.Inlines?.Add(new Avalonia.Controls.Documents.Run(line.Substring(lastIndex)));
            }
            else if (lastIndex == 0)
            {
                // No patterns matched, add whole line
                _textBlock.Inlines?.Add(new Avalonia.Controls.Documents.Run(line)
                {
                    Foreground = new SolidColorBrush(Color.Parse("#2C2C2C"))
                });
            }
        }
    }
}
