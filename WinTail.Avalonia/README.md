# WinTail - Cross-Platform Log Viewer (Avalonia UI)

A modern, cross-platform log file viewer built with Avalonia UI and .NET 8.

## ?? Features

- **Multi-Tab Interface** - View multiple log files simultaneously
- **Real-Time Monitoring** - Automatically updates when log files change
- **Search & Filter** - Find specific text or filter lines (grep-like functionality)
- **Single Instance** - Additional file opens in existing window via Named Pipes
- **Cross-Platform** - Runs on Windows, macOS, and Linux
- **Modern UI** - Fluent Design with system theme support
- **File Support** - Opens `.log`, `.txt`, `.vlog`, and all file types

## ?? Platform Support

| Platform | Status |
|----------|--------|
| ? Windows 10/11 | Fully Supported |
| ? macOS (10.15+) | Supported |
| ? Linux | Supported |

## ??? Building

### Prerequisites
- .NET 8 SDK
- (Optional) Visual Studio 2022 or JetBrains Rider

### Build Commands
```bash
# Restore dependencies
dotnet restore

# Build
dotnet build

# Run
dotnet run

# Publish (self-contained)
dotnet publish -c Release -r win-x64 --self-contained
dotnet publish -c Release -r osx-x64 --self-contained  
dotnet publish -c Release -r linux-x64 --self-contained
```

## ?? Usage

### Opening Log Files
- Click **"Open Log..."** button in the toolbar
- Pass file path as command-line argument: `WinTail.Avalonia mylog.log`
- Drag & drop (coming soon)

### Search
- Click ?? **Search** button
- Enter search term
- Navigate through matches

### Filter (Grep)
- Click ?? **Filter** button
- Enter filter text
- Click **Apply** to show only matching lines
- Click **Clear** to show all lines

### Keyboard Shortcuts
- `Ctrl+O` - Open file
- `Ctrl+F` - Search
- `Ctrl+R` - Refresh
- `Ctrl+C` - Copy content

## ??? Architecture

### Project Structure
```
WinTail.Avalonia/
??? App.axaml[.cs]           # Application entry point
??? Models/                  # Data models
?   ??? LogTab.cs
??? ViewModels/              # MVVM ViewModels
?   ??? MainWindowViewModel.cs
?   ??? LogTabViewModel.cs
?   ??? ViewModelBase.cs
??? Views/                   # UI Views
?   ??? MainWindow.axaml
?   ??? LogTabView.axaml
??? Services/                # Business logic
    ??? SingleInstanceService.cs
    ??? FileWatcherService.cs
```

### Technologies
- **UI Framework**: Avalonia UI 11.3
- **MVVM**: CommunityToolkit.Mvvm 8.4
- **Target Framework**: .NET 8
- **Design System**: Fluent Theme

## ?? Migration from WinUI 3

This is the cross-platform version of WinTail, migrated from WinUI 3 to Avalonia UI.

### Key Differences

| Feature | WinUI 3 | Avalonia |
|---------|---------|----------|
| Platform | Windows only | Cross-platform |
| XAML | `.xaml` | `.axaml` |
| TabControl | TabView | TabControl |
| Toolbar | CommandBar | Custom toolbar |
| File Picker | Windows.Storage | Avalonia.Platform.Storage |

See [MIGRATION.md](MIGRATION.md) for detailed migration notes.

## ?? Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## ?? License

This project is licensed under the MIT License.

## ?? Acknowledgments

- Built with [Avalonia UI](https://avaloniaui.net/)
- Uses [CommunityToolkit.Mvvm](https://github.com/CommunityToolkit/dotnet)
- Inspired by the Unix `tail` command

## ?? Contact

For bugs and feature requests, please open an issue on GitHub.

---

**Note**: This is the Avalonia (cross-platform) version. For the Windows-only WinUI 3 version, see the `WinTail` project in the parent directory.
