using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using AvaloniaEdit;
using System.ComponentModel;
using WinTail.Avalonia.Services;
using WinTail.Avalonia.ViewModels;

namespace WinTail.Avalonia.Views
{
    public partial class LogTabView : UserControl
    {
        private TextEditor? _editor;
        private LogTabViewModel? _viewModel;
        private bool _isInitialized = false;

        public LogTabView()
        {
            InitializeComponent();
            
            // Subscribe to when the control is attached to visual tree
            AttachedToVisualTree += OnAttachedToVisualTree;
        }

        private void OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
        {
            if (_isInitialized) return;

            // Get reference to the TextEditor
            _editor = this.FindControl<TextEditor>("Editor");
            
            if (_editor == null)
            {
                // Try again after a short delay
                Dispatcher.UIThread.Post(() =>
                {
                    _editor = this.FindControl<TextEditor>("Editor");
                    InitializeEditor();
                }, DispatcherPriority.Loaded);
            }
            else
            {
                InitializeEditor();
            }
        }

        private void InitializeEditor()
        {
            if (_editor == null || _isInitialized) return;

            _isInitialized = true;

            if (DataContext is LogTabViewModel viewModel)
            {
                _viewModel = viewModel;
                SetupViewModel();
            }
            
            // Subscribe to DataContext changes
            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object? sender, System.EventArgs e)
        {
            if (DataContext is LogTabViewModel viewModel && _editor != null && !_isInitialized)
            {
                _viewModel = viewModel;
                _isInitialized = true;
                SetupViewModel();
            }
        }

        private void SetupViewModel()
        {
            if (_viewModel == null || _editor == null) return;

            // Apply syntax highlighting based on file path
            try
            {
                var highlighting = SyntaxHighlightingService.GetHighlightingForFile(_viewModel.FilePath);
                if (highlighting != null)
                {
                    _editor.SyntaxHighlighting = highlighting;
                }
            }
            catch
            {
                // Syntax highlighting failed, continue without it
            }
            
            // Subscribe to log content changes
            _viewModel.PropertyChanged += OnViewModelPropertyChanged;
            
            // Set initial content immediately
            UpdateEditorContent();
        }

        private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(LogTabViewModel.LogContent))
            {
                UpdateEditorContent();
            }
        }

        private void UpdateEditorContent()
        {
            if (_editor == null || _viewModel == null) return;

            // Use Dispatcher to ensure we're on UI thread
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (_editor != null && _viewModel != null)
                {
                    var content = _viewModel.LogContent ?? string.Empty;
                    
                    // Only update if content changed
                    if (_editor.Text != content)
                    {
                        _editor.Text = content;
                        
                        // Scroll to end if there's content
                        if (!string.IsNullOrEmpty(content))
                        {
                            _editor.ScrollToEnd();
                        }
                    }
                }
            }, DispatcherPriority.Background);
        }
    }
}
