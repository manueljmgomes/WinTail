using Avalonia.Controls;
using Avalonia.Threading;
using AvaloniaEdit;
using System;
using System.ComponentModel;
using LogTailPlus.ViewModels;
using LogTailPlus.Services;

namespace LogTailPlus.Views
{
    public partial class LogTabView : UserControl
    {
        public LogTabView()
        {
            InitializeComponent();
            
            DataContextChanged += (s, e) =>
            {
                if (DataContext is LogTabViewModel vm)
                {
                    var editor = this.FindControl<TextEditor>("LogEditor");
                    if (editor != null)
                    {
                        // Apply syntax highlighting
                        try
                        {
                            var highlighting = SyntaxHighlightingService.GetHighlightingForFile(vm.FilePath);
                            if (highlighting != null)
                            {
                                editor.SyntaxHighlighting = highlighting;
                            }
                        }
                        catch (Exception)
                        {
                            // Syntax highlighting failed, continue without it
                        }
                        
                        // Update content immediately
                        UpdateContent(editor, vm.LogContent);
                        
                        // Subscribe to changes
                        vm.PropertyChanged += (sender, args) =>
                        {
                            if (args.PropertyName == nameof(LogTabViewModel.LogContent))
                            {
                                UpdateContent(editor, vm.LogContent);
                            }
                        };
                    }
                }
            };
        }

        private void UpdateContent(TextEditor editor, string? content)
        {
            if (content != null)
            {
                Dispatcher.UIThread.Post(() =>
                {
                    editor.Text = content;
                    if (content.Length > 0)
                    {
                        editor.ScrollToEnd();
                    }
                });
            }
        }
    }
}
