# LogTailPlus - Cross-Platform Log Viewer (Avalonia UI)

A modern, cross-platform log file viewer built with Avalonia UI and .NET 10.

## ?? Features

- **Multi-Tab Interface** - View multiple log files simultaneously
- **Real-Time Monitoring** - Automatically updates when log files change
- **Search & Filter** - Find specific text or filter lines (grep-like functionality)
- **Syntax Highlighting** - Supports 15+ programming languages including Delphi, plus special log file highlighting
- **Single Instance** - Additional file opens in existing window via Named Pipes
- **Cross-Platform** - Runs on Windows, macOS, and Linux
- **Modern UI** - Fluent Design with system theme support
- **File Support** - Opens `.log`, `.txt`, `.vlog`, and all file types
- **Timestamps** - Shows last update time on each tab

## ?? Platform Support

| Platform | Status |
|----------|--------|
| ? Windows 10/11 | Fully Supported |
| ? macOS (10.15+) | Fully Supported |
| ? Linux | Fully Supported |

## ??? Building

### Prerequisites
- **.NET 10 SDK** or later
- (Optional) Visual Studio 2022, JetBrains Rider, or VS Code

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
dotnet publish -c Release -r osx-arm64 --self-contained
dotnet publish -c Release -r linux-x64 --self-contained
```

### macOS Build
For detailed macOS build instructions, see [MACOS_BUILD.md](MACOS_BUILD.md)

Quick macOS build:
```bash
chmod +x build-mac.sh
./build-mac.sh
```

## ?? Usage

### Opening Log Files
- Click **"Open Log..."** button in the toolbar
- Pass file path as command-line argument: `LogTailPlus mylog.log`
- Drag & drop files onto the window

### Search
- Click **Search** button (magnifying glass icon)
- Enter search term
- Navigate through matches

### Filter (Grep)
- Click **Filter** button (funnel icon)
- Enter filter text
- Click **Apply** to show only matching lines
- Click **Clear** to show all lines

### Syntax Highlighting
Automatically detects file type and applies appropriate syntax highlighting:

**Supported Languages:**
- C/C++ (.c, .cpp, .h)
- C# (.cs)
- Java (.java)
- JavaScript/TypeScript (.js, .ts)
- Python (.py)
- Delphi/Pascal (.pas, .dpr, .dfm)
- PHP (.php)
- Ruby (.rb)
- PowerShell (.ps1)
- SQL (.sql)
- XML/HTML (.xml, .html)
- JSON (.json)
- Shell scripts (.sh, .bash)

**Log Files:**
Special highlighting for:
- Error/Warning/Info/Debug levels (color-coded)
- Timestamps (multiple formats)
- IP addresses
- URLs
- HTTP status codes
- Apache log format

### Keyboard Shortcuts
- `Ctrl+O` - Open file
- `Ctrl+F` - Search
- `Ctrl+R` - Refresh
- `Ctrl+C` - Copy content

## ??? Architecture

### Project Structure
```
LogTailPlus/
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
    ??? SyntaxHighlightingService.cs
```

### Technologies
- **UI Framework**: Avalonia UI 11.3.8
- **Text Editor**: AvaloniaEdit 11.1.0
- **MVVM**: CommunityToolkit.Mvvm 8.4.0
- **Target Framework**: .NET 10
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
| Text Display | TextBox | AvaloniaEdit.TextEditor |

See [MIGRATION.md](MIGRATION.md) for detailed migration notes.

## ?? Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## ?? License

This project is licensed under the MIT License.

## ?? Acknowledgments

- Built with [Avalonia UI](https://avaloniaui.net/)
- Text editing with [AvaloniaEdit](https://github.com/AvaloniaUI/AvaloniaEdit)
- Uses [CommunityToolkit.Mvvm](https://github.com/CommunityToolkit/dotnet)
- Inspired by the Unix `tail` command

## ?? Contact

For bugs and feature requests, please open an issue on GitHub: https://github.com/manueljmgomes/LogTailPlus/issues

---

**Version**: 1.0.0  
**Target Framework**: .NET 10  
**Last Updated**: 2025-11-13
