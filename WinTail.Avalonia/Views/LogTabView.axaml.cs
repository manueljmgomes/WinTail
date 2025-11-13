using Avalonia.Controls;
using AvaloniaEdit;
using WinTail.Avalonia.Services;
using WinTail.Avalonia.ViewModels;

namespace WinTail.Avalonia.Views
{
    public partial class LogTabView : UserControl
    {
        private readonly TextEditor? _editor;

        public LogTabView()
        {
            InitializeComponent();
            
            // Get reference to the TextEditor
            _editor = this.FindControl<TextEditor>("Editor");
            
            // Subscribe to DataContext changes to update syntax highlighting
            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object? sender, System.EventArgs e)
        {
            if (DataContext is LogTabViewModel viewModel && _editor != null)
            {
                // Apply syntax highlighting based on file path
                var highlighting = SyntaxHighlightingService.GetHighlightingForFile(viewModel.FilePath);
                _editor.SyntaxHighlighting = highlighting;
                
                // Subscribe to log content changes
                viewModel.PropertyChanged += (s, args) =>
                {
                    if (args.PropertyName == nameof(viewModel.LogContent) && _editor != null)
                    {
                        _editor.Text = viewModel.LogContent;
                        
                        // Auto-scroll to end when new content arrives
                        if (_editor.Document != null)
                        {
                            _editor.ScrollToEnd();
                        }
                    }
                };
                
                // Set initial content
                if (!string.IsNullOrEmpty(viewModel.LogContent))
                {
                    _editor.Text = viewModel.LogContent;
                }
            }
        }
    }
}
