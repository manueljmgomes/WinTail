using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using WinTail.Models;
using WinTail.Services;
using Windows.ApplicationModel.DataTransfer;

namespace WinTail.Controls
{
    public sealed class LogTabView : Grid
    {
        private readonly LogTab _logTab;
        private readonly FileWatcherService _fileWatcher;
        private readonly List<string> _logLines = new();
        private int _currentSearchIndex = -1;
        private string _currentSearchTerm = string.Empty;

        private readonly TextBlock _logTextBlock;
        private readonly ScrollViewer _logScrollViewer;
        private readonly Grid _searchPanel;
        private readonly TextBox _searchBox;
        private readonly ComboBox _languageComboBox;

        public LogTabView(LogTab logTab)
        {
            _logTab = logTab;
            _fileWatcher = new FileWatcherService(logTab.FilePath);
            _fileWatcher.NewLinesAdded += OnNewLinesAdded;

            // Define layout
            RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            // Toolbar
            var toolbar = new CommandBar
            {
                DefaultLabelPosition = CommandBarDefaultLabelPosition.Right
            };

            var searchButton = new AppBarButton
            {
                Icon = new SymbolIcon(Symbol.Find),
                Label = "Procurar"
            };
            searchButton.Click += SearchButton_Click;
            toolbar.PrimaryCommands.Add(searchButton);

            var copyButton = new AppBarButton
            {
                Icon = new SymbolIcon(Symbol.Copy),
                Label = "Copiar"
            };
            copyButton.Click += CopyButton_Click;
            toolbar.PrimaryCommands.Add(copyButton);

            toolbar.PrimaryCommands.Add(new AppBarSeparator());

            var refreshButton = new AppBarButton
            {
                Icon = new SymbolIcon(Symbol.Refresh),
                Label = "Atualizar"
            };
            refreshButton.Click += RefreshButton_Click;
            toolbar.PrimaryCommands.Add(refreshButton);

            // Language selector
            var languagePanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(12, 0, 0, 0)
            };

            languagePanel.Children.Add(new TextBlock
            {
                Text = "Linguagem:",
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 8, 0)
            });

            _languageComboBox = new ComboBox
            {
                MinWidth = 120,
                SelectedIndex = 0
            };
            _languageComboBox.Items.Add(CreateComboBoxItem("Plaintext", "plaintext"));
            _languageComboBox.Items.Add(CreateComboBoxItem("C#", "csharp"));
            _languageComboBox.Items.Add(CreateComboBoxItem("JavaScript", "javascript"));
            _languageComboBox.Items.Add(CreateComboBoxItem("JSON", "json"));
            _languageComboBox.Items.Add(CreateComboBoxItem("XML", "xml"));
            _languageComboBox.Items.Add(CreateComboBoxItem("SQL", "sql"));
            _languageComboBox.Items.Add(CreateComboBoxItem("Python", "python"));
            _languageComboBox.SelectionChanged += LanguageComboBox_SelectionChanged;

            languagePanel.Children.Add(_languageComboBox);
            toolbar.Content = languagePanel;

            SetRow(toolbar, 0);
            Children.Add(toolbar);

            // Search Panel
            _searchPanel = new Grid
            {
                Padding = new Thickness(12),
                Visibility = Visibility.Collapsed,
                VerticalAlignment = VerticalAlignment.Top
            };
            Canvas.SetZIndex(_searchPanel, 1);
            
            _searchPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            _searchPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            _searchPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            _searchBox = new TextBox
            {
                PlaceholderText = "Procurar..."
            };
            _searchBox.TextChanged += SearchBox_TextChanged;
            SetColumn(_searchBox, 0);
            _searchPanel.Children.Add(_searchBox);

            var prevButton = new Button
            {
                Content = "Anterior",
                Margin = new Thickness(8, 0, 0, 0)
            };
            prevButton.Click += SearchPrevious_Click;
            SetColumn(prevButton, 1);
            _searchPanel.Children.Add(prevButton);

            var nextButton = new Button
            {
                Content = "Próximo",
                Margin = new Thickness(8, 0, 0, 0)
            };
            nextButton.Click += SearchNext_Click;
            SetColumn(nextButton, 2);
            _searchPanel.Children.Add(nextButton);

            SetRow(_searchPanel, 1);
            Children.Add(_searchPanel);

            // Log Content
            _logScrollViewer = new ScrollViewer
            {
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            };

            _logTextBlock = new TextBlock
            {
                FontFamily = new FontFamily("Cascadia Mono, Consolas, Courier New"),
                FontSize = 12,
                Padding = new Thickness(12),
                TextWrapping = TextWrapping.NoWrap,
                IsTextSelectionEnabled = true
            };

            _logScrollViewer.Content = _logTextBlock;
            SetRow(_logScrollViewer, 1);
            Children.Add(_logScrollViewer);

            LoadInitialContent();
            _fileWatcher.StartWatching();
        }

        private ComboBoxItem CreateComboBoxItem(string content, string tag)
        {
            return new ComboBoxItem
            {
                Content = content,
                Tag = tag
            };
        }

        private void LoadInitialContent()
        {
            try
            {
                _logLines.Clear();
                _logLines.AddRange(_fileWatcher.ReadLastLines(1000));
                UpdateLogDisplay();
                ScrollToBottom();
            }
            catch (Exception ex)
            {
                _logTextBlock.Text = $"Erro ao carregar ficheiro: {ex.Message}";
            }
        }

        private void OnNewLinesAdded(object? sender, string newContent)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                var lines = newContent.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                foreach (var line in lines)
                {
                    if (!string.IsNullOrEmpty(line) || lines.Length > 1)
                    {
                        _logLines.Add(line);
                    }
                }

                // Mantém apenas as últimas 10000 linhas em memória
                while (_logLines.Count > 10000)
                {
                    _logLines.RemoveAt(0);
                }

                UpdateLogDisplay();
                ScrollToBottom();
            });
        }

        private void UpdateLogDisplay()
        {
            _logTextBlock.Text = string.Join(Environment.NewLine, _logLines);
        }

        private void ScrollToBottom()
        {
            _logScrollViewer.ChangeView(null, _logScrollViewer.ScrollableHeight, null, true);
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            _searchPanel.Visibility = _searchPanel.Visibility == Visibility.Visible
                ? Visibility.Collapsed
                : Visibility.Visible;

            if (_searchPanel.Visibility == Visibility.Visible)
            {
                _searchBox.Focus(FocusState.Programmatic);
            }
        }

        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedText = _logTextBlock.SelectedText;
            if (!string.IsNullOrEmpty(selectedText))
            {
                var dataPackage = new DataPackage();
                dataPackage.SetText(selectedText);
                Clipboard.SetContent(dataPackage);
            }
            else
            {
                var dataPackage = new DataPackage();
                dataPackage.SetText(_logTextBlock.Text);
                Clipboard.SetContent(dataPackage);
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadInitialContent();
        }

        private void LanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_languageComboBox.SelectedItem is ComboBoxItem item && item.Tag is string language)
            {
                _logTab.Language = language;
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _currentSearchTerm = _searchBox.Text;
            _currentSearchIndex = -1;
        }

        private void SearchNext_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_currentSearchTerm))
                return;

            var startIndex = _currentSearchIndex + 1;
            for (int i = startIndex; i < _logLines.Count; i++)
            {
                if (_logLines[i].Contains(_currentSearchTerm, StringComparison.OrdinalIgnoreCase))
                {
                    _currentSearchIndex = i;
                    HighlightLine(i);
                    return;
                }
            }

            // Se não encontrou, procura do início
            for (int i = 0; i < startIndex; i++)
            {
                if (_logLines[i].Contains(_currentSearchTerm, StringComparison.OrdinalIgnoreCase))
                {
                    _currentSearchIndex = i;
                    HighlightLine(i);
                    return;
                }
            }
        }

        private void SearchPrevious_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_currentSearchTerm))
                return;

            var startIndex = _currentSearchIndex - 1;
            if (startIndex < 0)
                startIndex = _logLines.Count - 1;

            for (int i = startIndex; i >= 0; i--)
            {
                if (_logLines[i].Contains(_currentSearchTerm, StringComparison.OrdinalIgnoreCase))
                {
                    _currentSearchIndex = i;
                    HighlightLine(i);
                    return;
                }
            }

            // Se não encontrou, procura do fim
            for (int i = _logLines.Count - 1; i > startIndex; i--)
            {
                if (_logLines[i].Contains(_currentSearchTerm, StringComparison.OrdinalIgnoreCase))
                {
                    _currentSearchIndex = i;
                    HighlightLine(i);
                    return;
                }
            }
        }

        private void HighlightLine(int lineIndex)
        {
            var lineHeight = 20;
            var scrollPosition = lineIndex * lineHeight;
            _logScrollViewer.ChangeView(null, scrollPosition, null, false);
        }

        public void Cleanup()
        {
            _fileWatcher?.Dispose();
        }
    }
}
