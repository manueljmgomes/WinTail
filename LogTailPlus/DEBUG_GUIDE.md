# WinTail Debug Guide - Log Content Display Issue

## Current Status
The application builds successfully but log content is not displaying in the TextEditor control.

## Debug Version Created
I've added extensive Debug.WriteLine() logging throughout the code to trace the issue.

## How to Run and Collect Debug Output

### Option 1: Run with Visual Studio
1. Open LogTailPlus.csproj in Visual Studio 2022
2. Press F5 to run in Debug mode
3. Open the Output window (View > Output)
4. Select "Debug" from the "Show output from:" dropdown
5. Open a log file in the application
6. Copy ALL output from the Output window

### Option 2: Run from Command Line
```powershell
# Navigate to project directory
cd e:\personalProjects\WinTail\LogTailPlus

# Run with debug output
dotnet run > debug_output.txt 2>&1

# Then open test.log file in the app
# Press Ctrl+C to stop
# Check debug_output.txt file
```

### Option 3: Use DebugView (Recommended for detailed logging)
1. Download DebugView from Microsoft Sysinternals: https://learn.microsoft.com/en-us/sysinternals/downloads/debugview
2. Run DebugView.exe as Administrator
3. Enable "Capture Global Win32"
4. Run WinTail: `dotnet run`
5. Open test.log in WinTail
6. Save DebugView output

## What to Look For in Debug Output

### 1. FileWatcherService Logs
```
[FileWatcher] Created for: <path>
[FileWatcher] ReadLastLines: Checking file exists
[FileWatcher] ReadLastLines: Read X total lines
[FileWatcher] ReadLastLines: Returning Y lines
[FileWatcher] StartWatching: Watcher started successfully
```

### 2. LogTabViewModel Logs
```
[LogTabViewModel] Constructor called for: <path>
[LogTabViewModel] LoadInitialContent: Got X lines from FileWatcher
[LogTabViewModel] UpdateLogDisplay: Starting with X lines
[LogTabViewModel] ***** LogContent CHANGED ***** Length: X
[LogTabViewModel] LogContent first 200 chars: <content preview>
```

### 3. LogTabView Logs
```
[LogTabView] OnLoaded called
[LogTabView] Editor found: True
[LogTabView] Setting up editor for: <path>
[LogTabView] LogContent changed, updating editor
[LogTabView] Updating editor with X characters
[LogTabView] Editor text updated successfully
```

## Expected Flow
1. LogTabViewModel constructor creates FileWatcherService
2. LoadInitialContent() calls ReadLastLines()
3. FileWatcher reads file and returns lines
4. UpdateLogDisplay() joins lines and sets LogContent property
5. OnLogContentChanged() fires (should show content length)
6. LogTabView receives PropertyChanged event
7. UpdateEditorContent() sets Editor.Text

## Test Steps

### Test 1: Simple TextBox Version
1. Edit `MainWindow.axaml` line 87
2. Change: `<views:LogTabView DataContext="{Binding}"/>`
3. To: `<views:LogTabViewSimple DataContext="{Binding}"/>`
4. Run and check if content appears in TextBox
5. If YES ? Problem is with AvaloniaEdit TextEditor
6. If NO ? Problem is with data binding or LogContent property

### Test 2: Manual Content Test
Add this to LogTabViewModel constructor (after LoadInitialContent):
```csharp
// TEMPORARY TEST CODE
LogContent = "TEST CONTENT - If you see this, LogContent property works!\n" +
             "Line 2\nLine 3\nLine 4\nLine 5";
Debug.WriteLine($"[TEST] Manually set LogContent to test string");
```

### Test 3: File Path Verification
Check debug output for:
- Is the file path correct?
- Does File.Exists() return true?
- How many lines were read?
- What is the LogContent length?

## Common Issues and Solutions

### Issue 1: File Not Found
**Symptoms**: `[FileWatcher] ReadLastLines: File does not exist!`
**Solution**: Check file path, use absolute path

### Issue 2: LogContent is Empty
**Symptoms**: `[LogTabViewModel] LogContent length: 0`
**Solution**: File is empty or ReadLastLines failed

### Issue 3: Editor Not Found
**Symptoms**: `[LogTabView] Editor found: False`
**Solution**: AvaloniaEdit control not initialized, timing issue

### Issue 4: PropertyChanged Not Firing
**Symptoms**: No `[LogTabView] LogContent changed` message
**Solution**: Binding issue, DataContext not set

## Files Modified with Logging
1. `Services/FileWatcherService.cs` - File reading logging
2. `ViewModels/LogTabViewModel.cs` - Property change logging
3. `Views/LogTabView.axaml.cs` - UI update logging

## Quick Test Command
```powershell
cd e:\personalProjects\WinTail\LogTailPlus
dotnet run -- test.log 2>&1 | Select-String -Pattern "\[FileWatcher\]|\[LogTabViewModel\]|\[LogTabView\]"
```

## Alternative: Use Simple Version
If AvaloniaEdit is the problem, we can:
1. Use simple TextBox (no syntax highlighting)
2. Use SelectableTextBlock
3. Try different AvaloniaEdit version
4. Create custom text display control

## Next Steps
1. Run the app and collect debug output
2. Share the output showing the log messages
3. We'll identify exactly where the flow breaks
4. Implement targeted fix based on findings

## Test File Location
`LogTailPlus/test.log` - 32 lines of sample log data
