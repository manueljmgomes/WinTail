using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using AvaloniaEdit;
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
            
            // Subscribe to when the control is loaded
            Loaded += OnLoaded;
        }

        private void OnLoaded(object? sender, RoutedEventArgs e)
        {
            // Get reference to the TextEditor
            _editor = this.FindControl<TextEditor>("Editor");
            
            if (DataContext is LogTabViewModel viewModel)
            {
                _viewModel = viewModel;
                SetupViewModel();
            }
            
            // Also handle DataContext changes
            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object? sender, System.EventArgs e)
        {
            if (DataContext is LogTabViewModel viewModel && _editor != null)
            {
                _viewModel = viewModel;
                SetupViewModel();
            }
        }

        private void SetupViewModel()
        {
            if (_viewModel == null || _editor == null) return;

            // Apply syntax highlighting based on file path
            var highlighting = SyntaxHighlightingService.GetHighlightingForFile(_viewModel.FilePath);
            _editor.SyntaxHighlighting = highlighting;
            
            // Subscribe to log content changes
            _viewModel.PropertyChanged += (s, args) =>
            {
                if (args.PropertyName == nameof(_viewModel.LogContent))
                {
                    Dispatcher.UIThread.Post(() =>
                    {
                        if (_editor != null && _viewModel != null)
                        {
                            _editor.Text = _viewModel.LogContent;
                            
                            // Auto-scroll to end when new content arrives
                            if (_editor.Document != null && _editor.Document.LineCount > 0)
                            {
                                _editor.ScrollToEnd();
                            }
                        }
                    });
                }
            };
            
            // Set initial content
            if (!string.IsNullOrEmpty(_viewModel.LogContent))
            {
                _editor.Text = _viewModel.LogContent;
            }
        }
    }
}
