using Microsoft.UI.Xaml;
using WinTail.Core.Interfaces;
using WinTail.Core.Services;

namespace WinTail;

/// <summary>
/// Application entry point
/// </summary>
public partial class App : Application
{
    public static ISettingsService SettingsService { get; private set; } = null!;
    public static Window? MainWindow { get; private set; }

    public App()
    {
        this.InitializeComponent();
        
        // Initialize services
        SettingsService = new SettingsService();
    }

    /// <summary>
    /// Invoked when the application is launched.
    /// </summary>
    protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        MainWindow = new MainWindow();
        MainWindow.Activate();
    }
}
