using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using AvaloniaEdit;
using AvaloniaEdit.Document;
using AvaloniaEdit.Highlighting;
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
        private TextDocument? _document;
        private LogTabViewModel? _viewModel;

        public LogTabView()
        {
            InitializeComponent();
            
            DataContextChanged += OnDataContextChanged;
            Loaded += OnLoaded;
        }

        private void OnLoaded(object? sender, RoutedEventArgs e)
        {
            Debug.WriteLine("[LogTabView] OnLoaded called");
            TryInitialize();
        }

        private void OnDataContextChanged(object? sender, EventArgs e)
        {
            Debug.WriteLine($"[LogTabView] OnDataContextChanged: DataContext = {DataContext?.GetType().Name ?? "null"}");
            
            // Don't process if DataContext is null (temporary state during tab switching)
            if (DataContext is not LogTabViewModel viewModel)
            {
                Debug.WriteLine("[LogTabView] DataContext is null or not LogTabViewModel, skipping update");
                return;
            }

            // Unsubscribe from old ViewModel if it exists
            if (_viewModel != null)
            {
                _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
                Debug.WriteLine("[LogTabView] Unsubscribed from old ViewModel");
            }

            // Store the new ViewModel
            _viewModel = viewModel;
            
            TryInitialize();
        }

        private void TryInitialize()
        {
            // Step 1: Get the editor control
            if (_editor == null)
            {
                _editor = this.FindControl<TextEditor>("Editor");
                Debug.WriteLine($"[LogTabView] Editor found: {_editor != null}");
                
                if (_editor == null)
                {
                    Debug.WriteLine("[LogTabView] Editor is null, cannot initialize");
                    return;
                }
            }

            // Step 2: Create and assign document if needed
            if (_document == null)
            {
                _document = new TextDocument();
                _editor.Document = _document;
                Debug.WriteLine("[LogTabView] TextDocument created and assigned to editor");
            }

            // Step 3: Setup editor with ViewModel if we have both
            if (_viewModel != null)
            {
                SetupEditor();
            }
        }

        private void SetupEditor()
        {
            if (_editor == null || _viewModel == null || _document == null)
            {
                Debug.WriteLine($"[LogTabView] Cannot setup: Editor={_editor != null}, VM={_viewModel != null}, Doc={_document != null}");
                return;
            }

            Debug.WriteLine($"[LogTabView] Setting up editor for: {_viewModel.FilePath}");

            // Apply syntax highlighting
            try
            {
                var highlighting = SyntaxHighlightingService.GetHighlightingForFile(_viewModel.FilePath);
                if (highlighting != null)
                {
                    _editor.SyntaxHighlighting = highlighting;
                    Debug.WriteLine($"[LogTabView] Syntax highlighting applied: {highlighting.Name}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[LogTabView] Syntax highlighting failed: {ex.Message}");
            }

            // Subscribe to content changes (already unsubscribed in OnDataContextChanged)
            _viewModel.PropertyChanged += OnViewModelPropertyChanged;
            Debug.WriteLine("[LogTabView] Subscribed to ViewModel PropertyChanged");

            // Set initial content
            UpdateEditorContent();
            
            Debug.WriteLine("[LogTabView] Setup complete");
        }

        private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(LogTabViewModel.LogContent))
            {
                Debug.WriteLine("[LogTabView] LogContent changed, updating editor");
                UpdateEditorContent();
            }
        }

        private void UpdateEditorContent()
        {
            if (_editor == null || _viewModel == null || _document == null)
            {
                Debug.WriteLine($"[LogTabView] Cannot update content: Editor={_editor != null}, VM={_viewModel != null}, Doc={_document != null}");
                return;
            }

            var content = _viewModel.LogContent ?? string.Empty;
            Debug.WriteLine($"[LogTabView] Updating editor with {content.Length} characters");

            // Use InvokeAsync instead of Post for more immediate execution
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                try
                {
                    if (_document != null && _editor != null)
                    {
                        // Update document text (more reliable than Text property)
                        _document.Text = content;

                        // Scroll to end if there's content
                        if (!string.IsNullOrEmpty(content))
                        {
                            _editor.ScrollToEnd();
                        }

                        Debug.WriteLine($"[LogTabView] Editor document updated successfully - Document now has {_document.TextLength} chars");
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[LogTabView] Error updating editor: {ex.Message}");
                    Debug.WriteLine($"[LogTabView] Stack trace: {ex.StackTrace}");
                }
            }, DispatcherPriority.Render); // Use Render priority for immediate visual update
        }
    }
}
