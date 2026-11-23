# 04 — Shell, AvalonDock 5 Persistence & Built-In Core Modules (`.NET Framework 4.8` `x64`)

> **Documentation Status:** **Authoritative Post-Conversion Reference (`2026-11-23`)** — Verified against frozen `SomaFrameQD` `0.9.0` sources after Stage 1 Retargeting and Stage 2 Optimization.  
> **Scope:** All **8** built-in core modules under [`src/SomaFrameQD/Modules/*`](../src/SomaFrameQD/Modules/): `Shell`, `MainWindow`, `MainMenu`, `ToolBars`, `StatusBar`, `Toolbox`, `UndoRedo`, and `Settings`.  
> **Companion Guides:** [01 — Architecture & Bootstrapper Overview](./01_Architecture_and_Bootstrapper_Overview.md) | [02 — Core Framework Primitives](./02_Core_Framework_Primitives_and_Coroutines.md) | [03 — Commands, Menus, ToolBars & KeyGestures](./03_Commands_Menus_ToolBars_and_KeyGestures.md) | [05 — Themes, Shaders & Win32 `x64` Interop](./05_Themes_Rendering_and_Win32_Interop.md)

---

## 1. Overview of the 8 Built-In Core Modules

Core `SomaFrameQD` (`SomaFrameQDWpf` `0.9.0`) embeds **eight** built-in modules directly inside `SomaFrameQD.dll` (note that legacy [`README.md`](../README.md) lists only seven, omitting `Settings`):

| Built-In Module Folder | Primary MEF Exports | Core Responsibility |
| :--- | :--- | :--- |
| [`Modules/MainWindow`](../src/SomaFrameQD/Modules/MainWindow/) | `IMainWindow` (`MainWindowViewModel`) | Root MahApps `MetroWindow` host (`1000×800` default clamped to `SystemParameters.WorkArea`), System-DPI ClearType tuning, `SnapsToDevicePixels`, and top-level `KeyGesture` binding |
| [`Modules/Shell`](../src/SomaFrameQD/Modules/Shell/) | `IShell` (`ShellViewModel`), `ILayoutItemStatePersister`, `IEditorOpeningService`, File/Window/View commands & menus | Document conductor, tool registry, `TransitionCoordinator` serialized FIFO async queue, AvalonDock `5.0.1` `DockingManager`, and `16 KB`-buffered transactional `ApplicationState.bin` persistence |
| [`Modules/MainMenu`](../src/SomaFrameQD/Modules/MainMenu/) | `IMenu` (`MainMenuViewModel`), `IMenuBuilder`, `ISettingsEditor` (`MainMenuSettingsViewModel`), `MenuDefinitions` | Top menu bar (`File`, `Edit`, `View`, `Tools`, `Window`, `Help`), `MenuEx` keyboard auto-hide, and **Environment → General** settings page |
| [`Modules/ToolBars`](../src/SomaFrameQD/Modules/ToolBars/) | `IToolBars` (`ToolBarsViewModel`), `IToolBarBuilder`, `IModule` (`ToolBars.Module`), `StandardToolBar` | Top `ToolBarTray` (hidden until `Shell.ToolBars.Visible = true`) and per-pane `ToolPaneToolBar` controls |
| [`Modules/StatusBar`](../src/SomaFrameQD/Modules/StatusBar/) | `IStatusBar` (`StatusBarViewModel`) | Bottom multi-column status bar with `GridLength`-sized `StatusBarItemViewModel` panes |
| [`Modules/Toolbox`](../src/SomaFrameQD/Modules/Toolbox/) | `IToolbox` (`ToolboxViewModel`), `IToolboxService`, `View.Toolbox` command | Left-docked, category-grouped, searchable drag-and-drop `[ToolboxItem]` palette scoped to the active document's CLR `Type` |
| [`Modules/UndoRedo`](../src/SomaFrameQD/Modules/UndoRedo/) | `IHistoryTool` (`HistoryViewModel`), `Edit.Undo`/`Redo` & `View.History` commands/toolbars | Per-document `IUndoRedoManager` action stack (`IUndoableAction`) and interactive right-docked History tool window |
| [`Modules/Settings`](../src/SomaFrameQD/Modules/Settings/) | `SettingsViewModel` (`CreationPolicy.NonShared`), `Tools.Options` command & menu item | Modal tree-navigated settings dialog aggregating all `ISettingsEditor` and `ISettingsEditorAsync` pages |

---

## 2. `Shell` Module Deep Dive ([`Modules/Shell/*`](../src/SomaFrameQD/Modules/Shell/))

### 2.1 `ShellViewModel` & `TransitionCoordinator` ([`ShellViewModel.cs`](../src/SomaFrameQD/Modules/Shell/ViewModels/ShellViewModel.cs), [`TransitionCoordinator.cs`](../src/SomaFrameQD/Modules/Shell/ViewModels/TransitionCoordinator.cs))

`ShellViewModel` (`[Export(typeof(IShell))]`) inherits from `Conductor<IDocument>.Collection.OneActive` and coordinates both center-pane `Documents` (`Items`) and docked/floating `Tools` (`BindableCollection<ITool>`).

#### Serialized FIFO Async Transitions Without Self-Deadlock
Every mutating shell transition—`OpenDocumentAsync`, `CloseDocumentAsync`, `ShowToolAsync`, `CloseToolAsync`, layout restoration steps, and two-way `ActiveLayoutItem` updates—routes through `TransitionCoordinator.Enqueue(Func<Task>)`:
* **Independent Callers (FIFO):** Queued in `_requests` and executed strictly one at a time in arrival order. A faulted or canceled transition completes its own `TaskCompletionSource` (`TaskCreationOptions.RunContinuationsAsynchronously`) without stalling subsequent queued requests.
* **Nested Causal Transitions (Inline via `AsyncLocal`):** `TransitionCoordinator` tracks the currently executing request in `AsyncLocal<TransitionRequest> _flowRequest`. When an active transition awaits a nested shell operation on its own causal chain (`ReferenceEquals(_executingRequest, _flowRequest.Value)`), `Enqueue` runs `ExecuteNestedAsync(transition)` inline rather than queueing behind itself—eliminating re-entrant deadlocks.

#### AvalonDock Two-Way Echo Suppression & Shutdown Latching
* **Echo Suppression (`_activeLayoutItemEcho`):** [`ShellView.xaml`](../src/SomaFrameQD/Modules/Shell/Views/ShellView.xaml) binds `DockingManager.ActiveContent` two-way to `ShellViewModel.ActiveLayoutItem`. Because AvalonDock synchronously echoes `ActiveContent` back during `PropertyChanged`, `SetActiveLayoutItemFromTransition` assigns `_activeLayoutItemEcho = item` inside a `try/finally` block so synchronous bounce-backs are ignored while genuine subsequent user selections queue normally.
* **Shutdown Guard (`_closing`):**
  * Non-closing deactivation (`OnDeactivateAsync(close: false)`) keeps `_closing == false` and **does not** write layout state, leaving the shell reactivatable.
  * Actual shutdown (`OnDeactivateAsync(close: true)`) latches `_closing = true` (ignoring AvalonDock focus-shift echoes while Caliburn closes documents), invokes `_layoutItemStatePersister.SaveState(...)`, and awaits `base.OnDeactivateAsync(true, ...)`. If shutdown faults or is vetoed, `catch` resets `_closing = false` so a subsequent close attempt works normally.

### 2.2 Dirkster.AvalonDock `5.0.1` Integration ([`LayoutInitializer.cs`](../src/SomaFrameQD/Modules/Shell/Controls/LayoutInitializer.cs), [`LayoutUtility.cs`](../src/SomaFrameQD/Modules/Shell/Views/LayoutUtility.cs))
* **Template & Style Selectors:**
  * [`PanesTemplateSelector`](../src/SomaFrameQD/Modules/Shell/Controls/PanesTemplateSelector.cs) wraps both `IDocument` and `ITool` views in a two-row `Grid` placing `<toolbarcontrols:ToolPaneToolBar ItemsSource="{Binding ToolBar}" />` above `<ContentControl cal:View.Model="{Binding}" />`.
  * [`PanesStyleSelector`](../src/SomaFrameQD/Modules/Shell/Controls/PanesStyleSelector.cs) binds `LayoutItem` (`DocumentStyle` with `TruncateMiddleConverter` on titles) and `LayoutAnchorableItem` (`ToolStyle`, mapping `Model.IsVisible` two-way to `Visibility.Hidden` when closed).
* **`LayoutInitializer` (`ILayoutUpdateStrategy`):** Routes `ITool.PreferredLocation` (`Left`, `Right`, `Bottom`) into named `LayoutAnchorablePane` containers (`"LeftPane"`, `"RightPane"`, `"BottomPane"`), creating missing panes dynamically inside `ShellView`'s `HorizontalPanel` or `VerticalPanel` and applying `PreferredWidth` / `PreferredHeight` when inserting the first tool.
* **Floating Windows & Focus Reset:** `UpdateFloatingWindows` synchronizes floating window icons with `MainWindow.Icon` and sets `ShowInTaskbar = ShellViewModel.ShowFloatingWindowsInTaskbar`; `LayoutUtility.LoadLayout` clears AvalonDock's `LayoutDocument.IsLastFocusedDocument` flag after XML deserialization so documents opened programmatically after restore receive focus reliably.

### 2.3 Built-In Shell Commands ([`Modules/Shell/Commands/*`](../src/SomaFrameQD/Modules/Shell/Commands/))

| Command Definition | Shortcut | Handler Behavior |
| :--- | :--- | :--- |
| `NewFileCommandListDefinition` (`File.NewFile`) | *(submenu list)* | `NewFileCommandHandler` enumerates `[ImportMany] IEditorProvider` where `CanCreateNew == true`, populates one item per `EditorFileType`, and awaits `IEditorOpeningService.NewFileAsync` (`"Untitled<N>.<ext>"`). |
| `OpenFileCommandDefinition` (`File.OpenFile`) | `Ctrl+O` | `OpenFileCommandHandler` builds an `"All Supported Files\|..."` `OpenFileDialog` filter and awaits `IEditorOpeningService.OpenFileAsync`. |
| `CloseFileCommandDefinition` (`File.CloseFile`) | *(none)* | `CloseFileCommandHandler` (enabled when `Shell.ActiveItem != null`) awaits `Shell.CloseDocumentAsync(Shell.ActiveItem)`. |
| `SaveFileCommandDefinition` (`File.SaveFile`) | `Ctrl+S` | Handled contextually on `Document` (`Enabled = this is IPersistedDocument`). |
| `SaveFileAsCommandDefinition` (`File.SaveFileAs`) | *(none)* | Handled contextually on `Document` (`PromptForSaveFilePath` + `Save`). |
| `SaveAllFilesCommandDefinition` (`File.SaveAllFiles`) | `Ctrl+Shift+S` | `SaveAllFilesCommandHandler` saves every open non-new `IPersistedDocument`. |
| `SwitchToDocumentCommandListDefinition` (`Window.SwitchToDocument`) | *(menu list)* | `SwitchToDocumentListCommandHandler` populates numbered `_1 <DisplayName>` entries (`Checked` on active document) and activates via `Shell.OpenDocumentAsync`. |
| `ViewFullScreenCommandDefinition` (`View.FullScreen`) | `Shift+Alt+Enter` | `ViewFullScreenCommandHandler` toggles `Application.Current.MainWindow.WindowState` between `Maximized` and `Normal`. |
| `ExitCommandDefinition` (`File.Exit`) | `Alt+F4` | `ExitCommandHandler` calls `Shell.Close()` (`Application.Current.MainWindow.Close()`). |

---

## 3. Buffered Transactional `ApplicationState.bin` Persistence ([`LayoutItemStatePersister.cs`](../src/SomaFrameQD/Modules/Shell/Services/LayoutItemStatePersister.cs))

By default, `ShellViewModel.StateFile` points to `@".\ApplicationState.bin"`. `LayoutItemStatePersister` (`[Export(typeof(ILayoutItemStatePersister))]`) combines custom per-ViewModel binary payloads with `AvalonDock.Serializer.Xml`'s `XmlLayoutSerializer` inside one crash-safe envelope, using a **`16,384`-byte (`16 KB`) `FileStreamBufferSize`** (`FileOptions.SequentialScan` on read) to minimize Win32 `ReadFile`/`WriteFile` syscall overhead across Windows 7 SP1 → Windows 11 storage while keeping the wire format 100% byte-identical.

### 3.1 On-Disk Wire Format (`ApplicationState.bin`)

```text
+-----------------------------------------------------------------------+
| Int32: itemCount (number of serialized ShouldReopenOnStart items)     |
+-----------------------------------------------------------------------+
| Item Record [0 .. itemCount - 1]:                                     |
|   - String (7-bit length-prefixed UTF-8): Persisted Export Type Name  |
|     (AssemblyQualifiedName, e.g. "...SomaFrameQD..., Version=0.9.0.0")|
|   - String (7-bit length-prefixed UTF-8): item.ContentId (Guid string)|
|   - Int64: payloadLength (bytes)                                      |
|   - Byte[payloadLength]: output of item.SaveState(BinaryWriter)       |
+-----------------------------------------------------------------------+
| Trailing UTF-8 XML Document: AvalonDock XmlLayoutSerializer output    |
+-----------------------------------------------------------------------+
```

> **MEF Contract Rule for Persisted Items (`GetPersistedType`):** Every `IDocument` or `ITool` with `ShouldReopenOnStart == true` must have either **exactly one** `[Export(typeof(...))]` attribute assignable to `ILayoutItem` or zero `ILayoutItem` export attributes (in which case its concrete non-generic CLR type is serialized). Multiple `ILayoutItem` exports or open generic types throw `InvalidOperationException` on save.

### 3.2 Transactional Save Protocol (`SaveState`)
1. Opens `<StateFile>.tmp` exclusively (`FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 16384`).
2. Isolates each item's `SaveState(BinaryWriter)` inside an in-memory `MemoryStream`. If an individual item throws, writes `payloadLength = 0` so stream offsets stay intact and downgrades the save status to `LayoutItemStateSaveStatus.Partial`.
3. Seeks back to byte `0` to patch `itemCount`, seeks to end to write `shellView.SaveLayout(stream)`, and calls `stream.Flush(true)` to flush OS kernel buffers to disk.
4. Copies any existing `<StateFile>` to `<StateFile>.bak` and swaps atomically via `File.Replace(<tmp>, <StateFile>, null, true)` (or `File.Move(<tmp>, <StateFile>)` on first save).
5. Any file-level or XML failure deletes `<StateFile>.tmp`, leaves `<StateFile>` untouched, and returns `LayoutItemStateSaveStatus.Failed` (`ShellViewModel.OnStateSaveWarning`).

### 3.3 Two-Phase Load & Recovery Matrix (`LoadStateAsync`)
1. **Phase 1 — Synchronous Stream Parsing (`ParseState`):** Opens `<StateFile>` with `16 KB` buffer + `FileOptions.SequentialScan`. Missing plugin types, uncreatable instances, exceptions inside `item.LoadState(reader)`, or duplicate `ContentId`s record a **partial failure** and seek cleanly to `stateEndPosition`. Reading past `stateEndPosition` or malformed headers/AvalonDock XML throws and becomes `Corrupt`.
2. **Phase 2 — Post-Stream Async Restoration:** Closes the `FileStream`, then awaits `IAsyncLayoutItemStateRestorer.RestoreStateAsync()` on all implementing items. Faulting items are removed and downgrade the result to `Partial`. Hidden tools remain registered in `shell.Tools` without activation; visible tools activate once.

| `LayoutItemStateLoadStatus` | Trigger Condition | `ShellViewModel` Startup Behavior |
| :--- | :--- | :--- |
| `NotFound` | State file or parent directory missing | Opens module `DefaultDocuments` and `DefaultTools` (`OpenDefaultLayoutAsync`). |
| `Success` | All items, XML layout, and async restorers succeeded | Runs `RestoreLayoutAsync(restorePlan)`. |
| `Partial` | One or more items skipped/faulted; envelope + XML valid | Calls `OnStateRestoreWarning` and restores all valid items via `RestoreLayoutAsync` (does **not** overwrite with defaults). |
| `Failed` | File exists but cannot be opened (locked/ACL/I/O) | Preserves file on disk, calls `OnStateRestoreWarning`, falls back to `OpenDefaultLayoutAsync`. |
| `Corrupt` | Negative count, truncated payload, read overrun, or invalid XML | Preserves corrupt file in place for forensics, calls `OnStateRestoreWarning`, falls back to `OpenDefaultLayoutAsync`. |

---

## 4. Cross-OS `MainWindow` & Remaining Built-In Core Modules

### 4.1 `MainWindow` — Cross-OS Chrome, System-DPI ClearType & `WorkArea` Clamping ([`Modules/MainWindow/*`](../src/SomaFrameQD/Modules/MainWindow/))
* **[`MainWindowViewModel.cs`](../src/SomaFrameQD/Modules/MainWindow/ViewModels/MainWindowViewModel.cs) (`[Export(typeof(IMainWindow))]`):**
  * Derives from `Conductor<IShell>` with `WindowState = Normal`, `Title = Resources.MainWindowDefaultTitle`, and `Icon = SomaFrameQD-32.png`.
  * Initializes default `Width` (`1000.0`) and `Height` (`800.0`) via `GetInitialWorkAreaClampedDimension(...)` so initial view-model dimensions never exceed `SystemParameters.WorkArea` on compact displays (such as `1024×768` or `1366×768` Windows 7 SP1 VMs/consoles).
* **[`MainWindowView.xaml`](../src/SomaFrameQD/Modules/MainWindow/Views/MainWindowView.xaml) & [`MainWindowView.xaml.cs`](../src/SomaFrameQD/Modules/MainWindow/Views/MainWindowView.xaml.cs):**
  * Subclasses `MahApps.Metro.Controls.MetroWindow` with `WindowTransitionsEnabled="False"`, `SnapsToDevicePixels="True"`, `UseLayoutRounding="True"`, `ResizeMode="CanResizeWithGrip"`, `Style="{DynamicResource MainWindowStyle}"`, and a named `<ResourceDictionary x:Name="ThemeDictionary" />` managed by `ThemeManager`.
  * **System-DPI ClearType Tuning (`OnSourceInitialized`):** Under System-DPI awareness (`Q2 = 2A` in `app.manifest`), reads `PresentationSource.FromVisual(this).CompositionTarget.TransformToDevice.M11` (`dpiScaleX`) and calls `TextOptions.SetTextFormattingMode(this, SelectTextFormattingModeForDpiScale(dpiScaleX))`—selecting pixel-sharp `TextFormattingMode.Display` at `<= 1.05x` (`96 DPI` / `100%`) and `TextFormattingMode.Ideal` on High-DPI (`125%`–`250%`) displays, paired with `TextOptions.SetTextRenderingMode(this, TextRenderingMode.Auto)`.
  * **Startup Bounds & Centering Guard (`ClampSizeToWorkArea`):** In `OnSourceInitialized`, clamps normal-state `Width`/`Height` (`MinimumClampedWindowDimension = 320.0`) to `SystemParameters.WorkArea` and re-centers `Left`/`Top` inside `workArea` if the window would otherwise clip beneath the taskbar or off-screen.

### 4.2 `MainMenu` ([`Modules/MainMenu/*`](../src/SomaFrameQD/Modules/MainMenu/))
* `MainMenuViewModel` (`[Export(typeof(IMenu))]`) builds `MenuDefinitions.MainMenuBar` in `OnImportsSatisfied()` and observes `Properties.Settings.Default.AutoHideMainMenu` via weak `SettingsPropertyChangedEventManager`.
* `MenuEx` collapses `Height = 0` when `AutoHide == true` and keyboard focus is outside the menu, restoring `Height = double.NaN` on `Alt`/focus.
* `MainMenuSettingsViewModel` (`[Export(typeof(ISettingsEditor))]`, `CreationPolicy.NonShared`) provides **Environment → General** (`SelectedTheme`, `AutoHideMainMenu`, and `SelectedLanguage`: `""`, `"en"`, `"de"`, `"ru"`, `"zh-Hans"`, `"ko"`, `"fr"`).

### 4.3 `ToolBars` & `StatusBar` ([`Modules/ToolBars/*`](../src/SomaFrameQD/Modules/ToolBars/), [`Modules/StatusBar/*`](../src/SomaFrameQD/Modules/StatusBar/))
* `ToolBars.Module` registers `/SomaFrameQD;component/Modules/ToolBars/Resources/Styles.xaml` in `GlobalResourceDictionaries`; `ToolBarsViewModel` defaults `Visible = false` until enabled (`Shell.ToolBars.Visible = true`).
* `StatusBarViewModel` (`[Export(typeof(IStatusBar))]`) stamps `StatusBarItemViewModel.Index` and calls `IStatusBarView.RefreshGridColumns()` whenever items change.

### 4.4 `Toolbox`, `UndoRedo` & `Settings` ([`Modules/Toolbox/*`](../src/SomaFrameQD/Modules/Toolbox/), [`Modules/UndoRedo/*`](../src/SomaFrameQD/Modules/UndoRedo/), [`Modules/Settings/*`](../src/SomaFrameQD/Modules/Settings/))
* **`Toolbox`:** `ToolboxService` scans `AssemblySource.Instance` for `[ToolboxItem(typeof(TargetDocument), ...)]` attributes; `ToolboxViewModel` (`PaneLocation.Left`) groups items by `Category`, tracks `IShell.ActiveDocumentChanged`, filters via `SearchCommand` (`Length >= 2`), and packages drags under `ToolboxDragDrop.DataFormat` (`"SomaFrameQDToolboxItem"`).
* **`UndoRedo`:** `UndoRedoManager` manages per-document `ActionStack` transitions (`ExecuteAction`, `PushAction` disposing truncated redo `IDisposable` items, `Undo`, `Redo`, `BatchBegin`/`BatchEnd`, and reverse-order `Dispose()` on document close); `HistoryViewModel` (`PaneLocation.Right`) renders an interactive scrubbable timeline.
* **`Settings`:** `SettingsViewModel` (`CreationPolicy.NonShared`, opened via `Tools -> Options`) aggregates `[ImportMany] ISettingsEditor` and `[ImportMany] ISettingsEditorAsync` into a backslash-delimited `SettingsPagePath` tree and awaits `ApplyChangesAsync()` across all editors on **OK**.
