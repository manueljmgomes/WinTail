using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using WinTail.Avalonia.Services;
using WinTail.Avalonia.ViewModels;
using WinTail.Avalonia.Views;

namespace WinTail.Avalonia;

public partial class App : Application
{
    private MainWindowViewModel? _mainViewModel;
    private SingleInstanceService? _singleInstanceService;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Disable duplicate validations from Avalonia and CommunityToolkit
            DisableAvaloniaDataAnnotationValidation();

            // Get command line arguments
            var args = desktop.Args ?? Array.Empty<string>();
            var filePathArg = args.Length > 0 ? args[0] : null;

            // Initialize single instance service
            _singleInstanceService = new SingleInstanceService();

            if (!_singleInstanceService.IsFirstInstance)
            {
                // Not the first instance - send file to main instance and exit
                if (!string.IsNullOrEmpty(filePathArg) && System.IO.File.Exists(filePathArg))
                {
                    _ = SingleInstanceService.SendFilePathToMainInstanceAsync(filePathArg);
                }
                
                desktop.Shutdown();
                return;
            }

            // First instance - create main window
            _mainViewModel = new MainWindowViewModel();
            
            desktop.MainWindow = new MainWindow
            {
                DataContext = _mainViewModel
            };

            // If a file was passed as argument, open it
            if (!string.IsNullOrEmpty(filePathArg) && System.IO.File.Exists(filePathArg))
            {
                _mainViewModel.OpenLogFile(filePathArg);
            }

            // Handle application exit
            desktop.Exit += (s, e) =>
            {
                _mainViewModel?.Cleanup();
                _singleInstanceService?.Dispose();
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}