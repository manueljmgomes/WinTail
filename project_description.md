You are an AI coding assistant inside Visual Studio. I want you to create a new Windows desktop application using **C#**, **.NET (latest stable)** and **WinUI 3 (Windows App SDK)**.

## Goal

Build an application called **WinTail** that behaves like a modern, lightweight "tail" tool for one or more text files, with syntax highlighting and theme support (light/dark/auto).

## Technology & Project Setup

- Create a **WinUI 3 desktop app** using the **Windows App SDK**.
- Language: **C#**.
- Target: **Windows 11**.
- Use a clean architecture, e.g. **MVVM** (with minimal extra libraries if possible).
- Structure the solution clearly into:
  - `WinTail` (UI / WinUI 3 project)
  - `WinTail.Core` (business logic, file tailing, models, settings)

## Design & Layout (match Windows Terminal & Windows 11 Notepad style)

The UI must follow the visual patterns of the **Windows Terminal** and the **new Windows 11 Notepad**:

- Use a modern **WinUI 3 title bar** with integrated controls (no classic Win32 title bar).
- Support **tabs** at the top, similar to Windows Terminal / Notepad:
  - Each tab represents one opened text file.
  - Tab shows file name and full path in tooltip.
  - Allow closing tabs with an “x”.
  - Have a “+” button to open a new file.
- Use padding, typography and spacing consistent with Windows 11:
  - Rounded corners.
  - Subtle borders.
  - Fluent Design / Mica-like background where appropriate (if easily supported in WinUI 3).
- Provide a top command area, similar to Windows Terminal:
  - A button or menu to open files.
  - A button or toggle to control tailing (Start / Stop following).
  - A theme selector (Light / Dark / System / Auto).
  - A selector (ComboBox) for **syntax highlighting language**.

## Core Functionality: Tail of One or More Files

The main purpose is to **tail** one or more text files:

- The user can select **one or more files** using an “Open file” dialog.
- Each selected file opens in its **own tab**.
- For each file:
  - Start reading from the **end of the file** (like `tail -f`).
  - Continuously append new lines to the view as the file grows.
  - Handle large files efficiently (do NOT load the whole file into memory; read from the end and follow).
  - Avoid blocking the UI thread; use async I/O or a background task.
  - Allow **pausing** and **resuming** the tail:
    - When paused, stop following new lines but keep content.
    - When resumed, catch up with the new content and continue following.
- Allow **manual scrolling**:
  - If the user scrolls up, do NOT auto-scroll to the bottom until the user scrolls back down or presses a “Follow” button.
  - This behavior should be similar to typical log viewers.

## Syntax Highlighting

The text view must support **syntax highlighting**:

- You may use any appropriate text editor control or library that works with WinUI 3, as long as it:
  - Supports colored syntax highlighting.
  - Works well with large text / appended lines.
- Provide a **language selector** (ComboBox or Menu) at the top, per tab:
  - At minimum, support:
    - Plain text (no highlight)
    - Log (simple highlight, e.g., timestamps, levels like INFO/WARN/ERROR)
    - JSON
    - XML
    - C#
- The highlight should update correctly as new lines are appended.

## Theme Support: Light / Dark / Auto

The app must support **dark mode / light mode / automatic**:

- Provide a **theme selector** in the UI with at least:
  - `Light`
  - `Dark`
  - `System` (follow the system theme)
- Optionally:
  - `Auto` mode that follows system theme changes at runtime.
- Make sure the syntax highlighting colors and background respect the selected theme.
- Use WinUI 3 theming APIs correctly to switch themes at runtime.

## Settings & Persistence

- Remember user preferences between sessions:
  - Last selected theme.
  - Last selected syntax highlighting language per file type if possible (nice-to-have).
- You can use a simple settings file (JSON) in the user’s AppData folder.

## UX Details

- The main window should open centered and with a reasonable default size (e.g. 1200x800).
- The text area should use a **monospaced font** (e.g. Cascadia Code) for better readability.
- Show the current file path and basic status information (Paused / Following, encoding, etc.) in a small status bar at the bottom.
- When a file is deleted or becomes unavailable while tailing:
  - Show a non-intrusive error message in the UI.
  - Stop tailing that file, but keep the content already read.

## Code Quality

- Use **async/await** for file watching and I/O where appropriate.
- Keep the UI responsive even with large or fast-growing files.
- Organize code with:
  - ViewModels for tabs and main window.
  - Services for:
    - File tailing (per file)
    - Settings storage.
- Add comments to explain key parts:
  - How tailing is implemented.
  - How syntax highlighting is wired.
  - How theme switching is implemented in WinUI 3.

## What I Expect You to Generate

1. Full WinUI 3 solution & project files.
2. All C# code files (Views, ViewModels, services).
3. XAML files for the main window and tab views, following the Windows Terminal / Windows 11 Notepad layout patterns.
4. Any necessary configuration, NuGet package references, and instructions in comments if something manual is needed.

Please output the complete solution structure and all relevant source files so I can copy them into a new Visual Studio WinUI 3 project.
