# RepoQuill.Gui Implementation Plan

## Overview

A cross-platform Avalonia GUI for RepoQuill that allows users to visually select files from a repository and generate consolidated output. The design prioritizes minimal clicks and simplicity.

**Key Design Decisions (from user input):**
- Single window with resizable panels (tree + options)
- Checkbox-based file selection (no pattern inputs)
- Folder checkbox selects all children recursively
- Icon toggle for rare TreeOnly state (cycles: Full → TreeOnly → Excluded)
- Recent folders dropdown for quick access
- No live preview - generate on demand
- Save to file + copy to clipboard

---

## UI Flow

```
┌─────────────────────────────────────────────────────────────────────────────┐
│ Initial State                                                               │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│                     ┌─────────────────────┐                                 │
│                     │   Open Folder ▼     │  ← Dropdown shows recent        │
│                     └─────────────────────┘                                 │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────────┐
│ After Folder Selected                                                       │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  ┌─────────────────────────────┐ │ ┌───────────────────────────────────┐   │
│  │ ☑ src/                      │ │ │  Options                          │   │
│  │   ☑ Program.cs        1.2KB │ │ │                                   │   │
│  │   ☑ Utils.cs          0.8KB │ │ │  [✓] Honor .gitignore             │   │
│  │   ☐ assets/                 │ │ │  [✓] Hide binaries                │   │
│  │     ☐ logo.png  [T]   5.6KB │ │ │  [ ] Strip comments               │   │
│  │ ☑ tests/                    │ │ │  [ ] Normalize whitespace         │   │
│  │   ☑ Tests.cs          2.1KB │ │ │                                   │   │
│  │ ☑ README.md           0.5KB │ │ │  ┌─────────────────────────────┐  │   │
│  └─────────────────────────────┘ │ │  │      Save to File           │  │   │
│                                  │ │  └─────────────────────────────┘  │   │
│                                  │ │  ┌─────────────────────────────┐  │   │
│                                  │ │  │    Copy to Clipboard        │  │   │
│                                  │ │  └─────────────────────────────┘  │   │
│                                  │ └───────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────────────────┘

[T] = TreeOnly icon (clicking cycles state)
```

---

## Folder Structure

```
src/RepoQuill.Gui/
├── App.axaml                         # Update: register converters
├── App.axaml.cs                      # Existing
├── Program.cs                        # Existing
├── ViewLocator.cs                    # Existing
├── Assets/
│   └── avalonia-logo.ico             # Existing
├── Models/
│   ├── FileNodeState.cs              # NEW: enum for UI state
│   └── RecentFolder.cs               # NEW: recent folder record
├── ViewModels/
│   ├── ViewModelBase.cs              # Existing
│   ├── MainWindowViewModel.cs        # REWRITE: main orchestrator
│   ├── FileTreeNodeViewModel.cs      # NEW: tree node with checkbox
│   └── OptionsViewModel.cs           # NEW: options panel state
├── Views/
│   ├── MainWindow.axaml              # REWRITE: split panel layout
│   ├── MainWindow.axaml.cs           # Update
│   ├── FileTreeView.axaml            # NEW: tree control
│   ├── FileTreeView.axaml.cs         # NEW
│   ├── OptionsPanel.axaml            # NEW: options panel
│   └── OptionsPanel.axaml.cs         # NEW
├── Services/
│   ├── IFolderDialogService.cs       # NEW: folder picker
│   ├── FolderDialogService.cs        # NEW
│   ├── IClipboardService.cs          # NEW: clipboard
│   ├── ClipboardService.cs           # NEW
│   ├── ISettingsService.cs           # NEW: persistence
│   └── SettingsService.cs            # NEW
└── Converters/
    ├── FileStateToCheckConverter.cs  # NEW: state → bool?
    └── BytesToSizeConverter.cs       # NEW: bytes → "1.5 KB"
```

---

## Key Classes

### Models

**FileNodeState.cs**
```csharp
public enum FileNodeState
{
    Checked,      // Full content (checkbox checked)
    Unchecked,    // Excluded (checkbox unchecked)
    TreeOnly,     // Tree only (icon shows [T])
    Indeterminate // Folder with mixed children
}
```

**RecentFolder.cs**
```csharp
public record RecentFolder(string Path, DateTime LastOpened);
```

### ViewModels

**MainWindowViewModel** - Main orchestrator
- `HasFolder` - true when folder selected
- `RootPath` - current folder path
- `RecentFolders` - ObservableCollection for dropdown
- `RootNodes` - ObservableCollection<FileTreeNodeViewModel>
- `Options` - OptionsViewModel instance
- `IsBusy`, `ProgressPercent`, `ProgressText` - progress state
- Commands: `OpenFolderCommand`, `SaveCommand`, `CopyCommand`

**FileTreeNodeViewModel** - Tree node
- `Name`, `RelativePath`, `AbsolutePath`
- `IsDirectory`, `IsBinary`, `SizeBytes`
- `State` - FileNodeState with property changed
- `IsVisible` - for "Hide binaries" filter
- `Children` - ObservableCollection (lazy loaded)
- `Parent` - reference for state propagation
- Methods: `ToggleCheck()`, `CycleState()`, `SetStateRecursive()`, `RecalculateParentState()`

**OptionsViewModel** - Options panel
- `HonorGitIgnore` (default: true)
- `HideBinaries` (default: true) - UI filter only
- `StripComments` (default: false)
- `NormalizeWhitespace` (default: false)

### Services

**ISettingsService** - Persists settings to `%APPDATA%/RepoQuill/settings.json`
- `GetRecentFoldersAsync()` → IReadOnlyList<RecentFolder>
- `AddRecentFolderAsync(path)` - adds/moves to top, max 10

**IFolderDialogService** - Wraps Avalonia OpenFolderDialog
- `PickFolderAsync(Window)` → string?

**IClipboardService** - Wraps Avalonia clipboard
- `SetTextAsync(text)`

---

## Tree Checkbox Behavior

**Standard checkbox click:**
- Checked → Unchecked
- Unchecked → Checked
- Indeterminate → Checked
- If folder: propagate to all children

**Icon toggle (small [T] button) for advanced users:**
- Cycles: Checked → TreeOnly → Unchecked → Checked
- Rarely used (5% of users)

**Parent state calculation:**
- All children same state → parent gets that state
- Mixed children → parent gets Indeterminate

---

## Building QuillConfig

When user clicks "Save" or "Copy":

```csharp
private QuillConfig BuildConfig()
{
    var include = new List<string>();
    var exclude = new List<string>();
    var treeOnly = new List<string>();

    foreach (var node in GetAllFileNodes())
    {
        var path = node.RelativePath;
        switch (node.State)
        {
            case FileNodeState.Checked:
                include.Add(path);
                break;
            case FileNodeState.TreeOnly:
                treeOnly.Add(path);
                break;
            case FileNodeState.Unchecked:
                exclude.Add(path);
                break;
        }
    }

    return new QuillConfig
    {
        RootPath = RootPath,
        IncludePatterns = include,
        ExcludePatterns = exclude,
        TreeOnlyPatterns = treeOnly,
        HonorGitIgnore = Options.HonorGitIgnore,
        StripComments = Options.StripComments,
        NormalizeWhitespace = Options.NormalizeWhitespace,
        Format = OutputFormat.PlainText
    };
}
```

---

## Implementation Order

### Phase 1: Foundation
1. `Models/FileNodeState.cs`
2. `Models/RecentFolder.cs`
3. `Services/ISettingsService.cs` + `SettingsService.cs`
4. `Services/IFolderDialogService.cs` + `FolderDialogService.cs`
5. `Services/IClipboardService.cs` + `ClipboardService.cs`
6. `Converters/BytesToSizeConverter.cs`
7. `Converters/FileStateToCheckConverter.cs`

### Phase 2: ViewModels
1. `ViewModels/OptionsViewModel.cs`
2. `ViewModels/FileTreeNodeViewModel.cs` (with full state management)
3. `ViewModels/MainWindowViewModel.cs` (full rewrite)

### Phase 3: Views
1. `Views/OptionsPanel.axaml` + code-behind
2. `Views/FileTreeView.axaml` + code-behind
3. `Views/MainWindow.axaml` (full rewrite with split layout)
4. Update `App.axaml` to register converters

### Phase 4: Integration
1. Wire folder scanning (build tree from FileScanner)
2. Implement QuillConfig generation from tree state
3. Implement progress reporting during generation
4. Implement save/copy operations

### Phase 5: Polish
1. Loading indicators
2. Error handling (permission errors, etc.)
3. Keyboard shortcuts (Ctrl+S, Ctrl+C)

---

## Files to Create

| File | Description |
|------|-------------|
| `Models/FileNodeState.cs` | UI state enum |
| `Models/RecentFolder.cs` | Recent folder record |
| `Services/ISettingsService.cs` | Settings interface |
| `Services/SettingsService.cs` | JSON persistence |
| `Services/IFolderDialogService.cs` | Folder dialog interface |
| `Services/FolderDialogService.cs` | Avalonia implementation |
| `Services/IClipboardService.cs` | Clipboard interface |
| `Services/ClipboardService.cs` | Avalonia implementation |
| `Converters/BytesToSizeConverter.cs` | Bytes → "1.5 KB" |
| `Converters/FileStateToCheckConverter.cs` | State → bool? |
| `ViewModels/OptionsViewModel.cs` | Options panel state |
| `ViewModels/FileTreeNodeViewModel.cs` | Tree node VM |
| `Views/OptionsPanel.axaml` | Options panel view |
| `Views/FileTreeView.axaml` | Tree view control |

## Files to Modify

| File | Changes |
|------|---------|
| `ViewModels/MainWindowViewModel.cs` | Complete rewrite |
| `Views/MainWindow.axaml` | Complete rewrite |
| `App.axaml` | Register converters |

---

## Critical Files

- `src/RepoQuill.Gui/ViewModels/MainWindowViewModel.cs` - Main orchestrator
- `src/RepoQuill.Gui/ViewModels/FileTreeNodeViewModel.cs` - Tree node with state management
- `src/RepoQuill.Gui/Views/MainWindow.axaml` - Main layout
- `src/RepoQuill.Core/Discovery/FileScanner.cs` - For initial tree population
- `src/RepoQuill.Core/QuillEngine.cs` - For generating output
