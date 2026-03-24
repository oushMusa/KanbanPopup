# Kanban Popup (WPF, .NET 10)

Minimal desktop Kanban board for Windows 10/11 with popup behavior from the right screen edge.

## Features

- 3 columns: `ToDo`, `InProgress`, `Done`
- Task cards with:
  - title (required)
  - description (optional)
- Add task via top input + `+` button
- Delete task from any column
- Drag-and-drop cards between columns
- Popup board:
  - hidden by default
  - appears when cursor touches the **right** screen edge
  - hides when cursor leaves (with ~500 ms delay)
  - smooth slide animation
- Global hotkey: `Ctrl + ~` to show/hide board
- System tray support:
  - Open
  - Exit
- JSON persistence:
  - auto-save
  - auto-load on app start

## Tech Stack

- C#
- .NET 10 (`net10.0-windows`)
- WPF
- MVVM
- No heavy dependencies

## Project Structure

```text
KanbanPopup/
├─ App.xaml
├─ App.xaml.cs
├─ KanbanPopup.csproj
├─ Models/
│  ├─ TaskItem.cs
│  ├─ ColumnType.cs
│  └─ Column.cs
├─ ViewModels/
│  ├─ RelayCommand.cs
│  └─ MainViewModel.cs
├─ Services/
│  ├─ MouseTrackerService.cs
│  ├─ HotkeyService.cs
│  └─ StorageService.cs
└─ Views/
   ├─ MainWindow.xaml
   └─ MainWindow.xaml.cs
```

## Requirements

- Windows 10/11
- .NET 10 SDK
- Visual Studio 2022+ (recommended) or `dotnet` CLI

## Run (Visual Studio)

1. Open solution/project folder.
2. Set startup project: `KanbanPopup`.
3. Press `F5`.

## Run (CLI)

```powershell
cd d:\projects\kanban2
dotnet run
```

## Build

```powershell
cd d:\projects\kanban2
dotnet build -c Release
```

## Publish EXE

### Self-contained (recommended for distribution)

```powershell
cd d:\projects\kanban2
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

Output:

`bin\Release\net10.0-windows\win-x64\publish\KanbanPopup.exe`

### Framework-dependent (smaller, requires installed runtime)

```powershell
cd d:\projects\kanban2
dotnet publish -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true
```

## Data Storage

Board data is saved to:

`%AppData%\KanbanPopup\board.json`

## Usage

- Move cursor to the **right edge** of the screen to open the board.
- Move cursor away from the board to hide it.
- Press `Ctrl + ~` to toggle visibility manually.
- Use tray icon menu (`Open` / `Exit`) for background usage.

## Notes

- App runs in tray and does not appear on taskbar.
- Window is borderless, topmost, and optimized for quick access.
