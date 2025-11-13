# WinTail Avalonia Migration

## Architecture Overview

### Services (Cross-Platform)
- ? SingleInstanceService - Uses Named Pipes (works on all platforms)
- ? FileWatcherService - Uses FileSystemWatcher (cross-platform)

### Models
- ? LogTab - Simple data model with ObservableProperty

### ViewModels
- MainWindowViewModel - Main application logic
- LogTabViewModel - Individual log tab logic

### Views
- MainWindow - Main application window with tab management
- LogTabView - UserControl for displaying log content

## Key Differences from WinUI 3

### UI Framework
- **WinUI 3**: Microsoft.UI.Xaml namespace, Windows-only controls
- **Avalonia**: Avalonia.Controls namespace, cross-platform controls

### XAML Syntax
- **WinUI 3**: Uses `xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"`
- **Avalonia**: Uses `xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"` (same) but `.axaml` extension

### Controls
- **TabView** ? **TabControl** (standard Avalonia control)
- **CommandBar** ? **Menu** or custom toolbar
- **AppBarButton** ? **Button** with styling

### Dialogs
- **FileOpenPicker** ? **OpenFileDialog** (Avalonia.Controls)

### Threading
- **DispatcherQueue** ? **Dispatcher.UIThread.InvokeAsync()**

## Platform-Specific Features

### Single Instance (All Platforms)
- Uses Named Pipes which work on Windows, macOS, and Linux

### File Monitoring (All Platforms)
- Uses FileSystemWatcher which is cross-platform

### File Dialogs (All Platforms)
- Avalonia provides cross-platform file dialogs

## Migration Status

- [x] Project structure created
- [x] Services migrated
- [x] Models migrated
- [ ] MainWindowViewModel created
- [ ] LogTabViewModel created  
- [ ] MainWindow View updated
- [ ] LogTabView created
- [ ] App.axaml.cs updated
- [ ] Testing and refinement

## Next Steps

1. Create comprehensive ViewModels with MVVM pattern
2. Design Avalonia XAML views
3. Implement file opening and tab management
4. Add search and filter functionality
5. Styling and theming
6. Testing on multiple platforms
