using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage.Pickers;
using WinTail.ViewModels;
using WinRT.Interop;

namespace WinTail;

/// <summary>
/// Main window with tab-based file viewing interface
/// </summary>
public sealed partial class MainWindow : Window
{
    public MainViewModel ViewModel { get; }

    public MainWindow()
    {
        this.InitializeComponent();
        
        ViewModel = new MainViewModel();
        
        // Set window size
        var appWindow = this.AppWindow;
        appWindow.Resize(new Windows.Graphics.SizeInt32(1200, 800));
        
        // Center window
        var displayArea = Microsoft.UI.Windowing.DisplayArea.Primary;
        var centeredPosition = new Windows.Graphics.PointInt32(
            (displayArea.WorkArea.Width - 1200) / 2,
            (displayArea.WorkArea.Height - 800) / 2
        );
        appWindow.Move(centeredPosition);

        // Load settings and apply theme
        _ = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        await ViewModel.LoadSettingsAsync();
    }

    private async void OpenFileButton_Click(object sender, RoutedEventArgs e)
    {
        var openPicker = new FileOpenPicker();
        
        // Initialize with window handle
        var hwnd = WindowNative.GetWindowHandle(this);
        InitializeWithWindow.Initialize(openPicker, hwnd);

        openPicker.ViewMode = PickerViewMode.List;
        openPicker.FileTypeFilter.Add("*");

        var files = await openPicker.PickMultipleFilesAsync();
        
        if (files != null && files.Count > 0)
        {
            foreach (var file in files)
            {
                await ViewModel.OpenFileAsync(file.Path);
            }
        }
    }

    private async void TabView_AddTabButtonClick(TabView sender, object args)
    {
        var openPicker = new FileOpenPicker();
        
        var hwnd = WindowNative.GetWindowHandle(this);
        InitializeWithWindow.Initialize(openPicker, hwnd);

        openPicker.ViewMode = PickerViewMode.List;
        openPicker.FileTypeFilter.Add("*");

        var file = await openPicker.PickSingleFileAsync();
        
        if (file != null)
        {
            await ViewModel.OpenFileAsync(file.Path);
        }
    }

    private void TabView_TabCloseRequested(TabView sender, TabViewTabCloseRequestedEventArgs args)
    {
        if (args.Item is FileTabViewModel tabViewModel)
        {
            ViewModel.CloseTab(tabViewModel);
        }
    }
}
