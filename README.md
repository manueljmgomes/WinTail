# WinTail

A modern Windows desktop application for tailing text files, built with WinUI 3 and .NET 8.

## Features

- **Multi-file tailing**: Open multiple files in separate tabs, each actively monitoring for new content
- **Live file monitoring**: Automatically displays new lines as they're written to the file
- **Syntax highlighting**: Built-in support for:
  - Log files (with timestamp, ERROR, WARN, INFO highlighting)
  - JSON
  - XML
  - C#
  - Plain text
- **Theme support**: Light, Dark, and System (auto) themes
- **Pause/Resume**: Pause tailing to review content, then resume to catch up
- **Modern UI**: Windows 11-style interface with:
  - Tab-based layout (like Windows Terminal and Notepad)
  - Fluent Design elements
  - Modern WinUI 3 title bar
  - Status bar with file info and line count
- **Smart scrolling**: Manual scrolling prevents auto-scroll until you return to bottom
- **Settings persistence**: Theme and preferences saved between sessions

## Architecture

The solution is organized into two projects:

### WinTail (UI Project)
- **Views**: MainWindow with tab-based interface
- **ViewModels**: MVVM pattern with MainViewModel and FileTabViewModel
- **Controls**: Custom SyntaxTextBlock for syntax highlighting
- **Helpers**: SyntaxHighlighter for language-specific color schemes

### WinTail.Core (Business Logic)
- **Models**: TailState, SyntaxLanguage, ThemeMode, TailLine, AppSettings
- **Services**:
  - **FileTailService**: Efficient file tailing with FileSystemWatcher and async I/O
  - **SettingsService**: JSON-based settings persistence
- **Interfaces**: IFileTailService, ISettingsService for testability

## Technical Implementation

### File Tailing
- Reads from the end of the file (like Unix `tail -f`)
- Uses FileSystemWatcher for efficient change detection
- Async I/O to keep UI responsive
- Handles file deletion, truncation, and errors gracefully
- Maintains a rolling buffer of last 10,000 lines per file

### Syntax Highlighting
- Custom WinUI 3 control using RichTextBlock
- Regex-based pattern matching
- Theme-aware color schemes
- Efficient line-by-line processing

### Theme System
- Integrates with WinUI 3 ElementTheme
- Follows system theme changes when in System mode
- Persists user preference across sessions

## Requirements

- Windows 10 version 1809 (17763) or higher
- Windows 11 recommended
- .NET 8.0 Runtime
- Windows App SDK 1.5

## Building

```powershell
# Restore dependencies
dotnet restore WinTail.sln

# Build the solution
dotnet build WinTail.sln

# Run the application
dotnet run --project WinTail/WinTail.csproj
```

Or open `WinTail.sln` in Visual Studio 2022 and press F5.

## Usage

1. **Opening Files**:
   - Click the "Open" button in the toolbar
   - Use the "+" button on the tab bar
   - Select one or multiple files

2. **Tailing**:
   - Files automatically start tailing when opened
   - Use the "Pause/Resume" button to control following
   - Each tab has independent pause/resume state

3. **Syntax Highlighting**:
   - Automatically inferred from file extension
   - Change via the "Syntax" dropdown
   - Available options: Plain Text, Log, JSON, XML, C#

4. **Themes**:
   - Select from Light, Dark, or System in the "Theme" dropdown
   - Theme changes apply immediately
   - Preference is saved

5. **Status Bar**:
   - Shows current file path
   - Displays total lines read
   - Indicates tailing state (Running, Paused, Error, etc.)

## Settings Storage

Settings are stored in:
```
%LocalAppData%\WinTail\settings.json
```

## Known Limitations

- Syntax highlighting is simplified (regex-based, not full parsing)
- Maximum of 10,000 lines kept in memory per file
- Large files (>100MB) may have initial load delay
- No searching/filtering within tail content yet

## Future Enhancements

- Full syntax highlighting using libraries like ColorCode
- Search/filter capabilities
- Bookmarks and line navigation
- Export/save selected content
- Follow multiple files merged in one view
- Regex-based filtering
- Performance optimizations for very large files

## License

This is a demonstration project created for educational purposes.

## Credits

Built with:
- [WinUI 3](https://docs.microsoft.com/en-us/windows/apps/winui/winui3/) - Modern Windows UI framework
- [Windows App SDK](https://docs.microsoft.com/en-us/windows/apps/windows-app-sdk/) - Windows platform APIs
- [CommunityToolkit.Mvvm](https://docs.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/) - MVVM helpers
- .NET 8.0 - Runtime and SDK
