using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using AvaloniaEdit;
using System;
using System.ComponentModel;
using System.Diagnostics;
using WinTail.Avalonia.Services;
using WinTail.Avalonia.ViewModels;

namespace WinTail.Avalonia.Views
{
    public partial class LogTabView : UserControl
    {
        private TextEditor? _editor;
        private LogTabViewModel? _viewModel;

        public LogTabView()
        {
            InitializeComponent();
            
            // Set up when data context changes
            DataContextChanged += OnDataContextChanged;
            
            // Also try to initialize when loaded
            Loaded += OnLoaded;
        }

        private void OnLoaded(object? sender, RoutedEventArgs e)
        {
            Debug.WriteLine("LogTabView.OnLoaded called");
            TryInitialize();
        }

        private void OnDataContextChanged(object? sender, EventArgs e)
        {
            Debug.WriteLine($"LogTabView.OnDataContextChanged: DataContext = {DataContext?.GetType().Name}");
            
            if (DataContext is LogTabViewModel viewModel)
            {
                _viewModel = viewModel;
                TryInitialize();
            }
        }

        private void TryInitialize()
        {
            // Get editor if we don't have it yet
            if (_editor == null)
            {
                _editor = this.FindControl<TextEditor>("Editor");
                Debug.WriteLine($"Editor found: {_editor != null}");
            }

            // If we have both editor and viewmodel, set up
            if (_editor != null && _viewModel != null)
            {
                SetupEditor();
            }
        }

        private void SetupEditor()
        {
            if (_editor == null || _viewModel == null) return;

            Debug.WriteLine($"Setting up editor for: {_viewModel.FilePath}");

            // Apply syntax highlighting
            try
            {
                var highlighting = SyntaxHighlightingService.GetHighlightingForFile(_viewModel.FilePath);
                if (highlighting != null)
                {
                    _editor.SyntaxHighlighting = highlighting;
                    Debug.WriteLine($"Syntax highlighting applied: {highlighting.Name}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Syntax highlighting failed: {ex.Message}");
            }
            
            // Subscribe to content changes
            _viewModel.PropertyChanged += OnViewModelPropertyChanged;
            
            // Set initial content
            UpdateEditorContent();
        }

        private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(LogTabViewModel.LogContent))
            {
                Debug.WriteLine("LogContent changed, updating editor");
                UpdateEditorContent();
            }
        }

        private void UpdateEditorContent()
        {
            if (_editor == null || _viewModel == null)
            {
                Debug.WriteLine($"Cannot update content: Editor={_editor != null}, ViewModel={_viewModel != null}");
                return;
            }

            var content = _viewModel.LogContent ?? string.Empty;
            Debug.WriteLine($"Updating editor with {content.Length} characters");

            // Update on UI thread
            Dispatcher.UIThread.Post(() =>
            {
                try
                {
                    if (_editor != null)
                    {
                        _editor.Text = content;
                        
                        // Scroll to end if there's content
                        if (!string.IsNullOrEmpty(content) && _editor.Document != null)
                        {
                            _editor.ScrollToEnd();
                        }
                        
                        Debug.WriteLine("Editor text updated successfully");
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error updating editor: {ex.Message}");
                }
            }, DispatcherPriority.Background);
        }
    }
}
