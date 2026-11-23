# 04 — Shell, AvalonDock 5 Persistence & Built-In Core Modules (Before `.NET 4.8` Retargeting)

> **Baseline Snapshot:** Pre-`.NET Framework 4.8`-only retargeting and optimization (`2026-11-22`).  
> **Scope:** All 8 built-in module directories under `src/SomaFrameQD/Modules/*`: `Shell`, `MainWindow`, `MainMenu`, `ToolBars`, `StatusBar`, `Toolbox`, `UndoRedo`, and `Settings`.

---

## 1. Overview of the 8 Built-In Core Modules

Core `SomaFrameQD` (`SomaFrameQDWpf`) embeds **eight** built-in modules directly inside `SomaFrameQD.dll` (note that legacy `README.md` mentions only seven, omitting `Settings`):

| Built-In Module Folder | Primary MEF Exports | Core Responsibility |
| :--- | :--- | :--- |
| `Modules/MainWindow` | `IMainWindow` (`MainWindowViewModel`) | Root MahApps `MetroWindow` shell host (`1000x800` default), icon/title/window-state management, top-level `KeyGesture` binding |
| `Modules/Shell` | `IShell` (`ShellViewModel`), `ILayoutItemStatePersister`, `IEditorOpeningService`, File/Window/View commands & menus | Document conductor, tool registry, `TransitionCoordinator` async queue, AvalonDock 5 `DockingManager`, transactional `ApplicationState.bin` persistence |
| `Modules/MainMenu` | `IMenu` (`MainMenuViewModel`), `IMenuBuilder`, `ISettingsEditor` (`MainMenuSettingsViewModel`), standard `MenuDefinitions` | Top menu bar (`File`, `Edit`, `View`, `Tools`, `Window`, `Help`), `MenuEx` keyboard auto-hide, Environment → General settings page |
| `Modules/ToolBars` | `IToolBars` (`ToolBarsViewModel`), `IToolBarBuilder`, `IModule` (`ToolBars.Module`), `StandardToolBar` | Top `ToolBarTray` (hidden by default until `Shell.ToolBars.Visible = true`) and per-pane `ToolPaneToolBar` controls |
| `Modules/StatusBar` | `IStatusBar` (`StatusBarViewModel`) | Bottom multi-column status bar with `GridLength`-sized `StatusBarItemViewModel` panes |
| `Modules/Toolbox` | `IToolbox` (`ToolboxViewModel`), `IToolboxService`, View → Toolbox command | Left-docked, category-grouped, searchable drag-and-drop `[ToolboxItem]` palette scoped to the active document's CLR `Type` |
| `Modules/UndoRedo` | `IHistoryTool` (`HistoryViewModel`), Edit → Undo/Redo & View → History commands/toolbars | Per-document `IUndoRedoManager` action stack (`IUndoableAction`) and interactive right-docked History tool window |
| `Modules/Settings` | `SettingsViewModel` (`NonShared`), Tools → Options command & menu item | Modal tree-navigated settings dialog aggregating all `ISettingsEditor` and `ISettingsEditorAsync` pages |

---

## 2. `Shell` Module Deep Dive (`Modules/Shell/*`)

### 2.1 `ShellViewModel` & `TransitionCoordinator` (`ShellViewModel.cs`, `TransitionCoordinator.cs`)

`ShellViewModel` (`[Export(typeof(IShell))]`) inherits from `Conductor<IDocument>.Collection.OneActive` and coordinates both center-pane `Documents` (`Items`) and docked/floating `Tools` (`BindableCollection<ITool>`).

#### Serialized FIFO Async Transitions Without Self-Deadlock
Every mutating lifecycle call—`OpenDocumentAsync`, `CloseDocumentAsync`, `ShowToolAsync`, `CloseToolAsync`, layout restoration steps, and two-way `ActiveLayoutItem` binding updates—is routed through `TransitionCoordinator.Enqueue(Func<Task>)`:
- **Unrelated Callers (FIFO):** Queued in `_requests` and executed strictly one at a time in arrival order. A faulted or canceled transition completes its own `TaskCompletionSource` (`RunContinuationsAsynchronously`) without halting subsequent queued transitions.
- **Re-Entrant Nested Flows (Inline):** `TransitionCoordinator` tracks the active request in an `AsyncLocal<TransitionRequest> _flowRequest`. If a currently executing transition awaits a nested shell operation on its own async causal chain (`ReferenceEquals(_executingRequest, _flowRequest.Value)`), `Enqueue` executes `ExecuteNestedAsync(transition)` inline rather than enqueuing behind itself—preventing async re-entrancy deadlocks.

#### AvalonDock Two-Way Echo Suppression & Shutdown Latching
- **Echo Suppression (`_activeLayoutItemEcho`):** `ShellView.xaml` binds `DockingManager.ActiveContent` two-way to `ShellViewModel.ActiveLayoutItem`. Because AvalonDock synchronously writes `ActiveContent` back during `PropertyChanged` notifications, `SetActiveLayoutItemFromTransition` temporarily sets `_activeLayoutItemEcho = item` inside a `try/finally` block so the synchronous bounce-back is ignored while genuine future user selections of the same item still queue normally.
- **Shutdown Guard (`_closing`):**
  - Non-closing deactivation (`OnDeactivateAsync(close: false)`) leaves `_closing == false` and **does not** write layout state, keeping the shell reusable.
  - Real shutdown (`OnDeactivateAsync(close: true)`) latches `_closing = true` (causing `ActivateItemAsync` to ignore AvalonDock's focus-shift echoes while Caliburn closes documents), invokes `_layoutItemStatePersister.SaveState(...)`, and awaits `base.OnDeactivateAsync(true, ...)`. If shutdown throws or is aborted, `catch` resets `_closing = false` so a later close attempt succeeds.

### 2.2 AvalonDock 5 Integration (`ShellView.xaml`, `LayoutInitializer.cs`, `LayoutUtility.cs`)

- **Template & Style Selectors:**
  - `PanesTemplateSelector` wraps both `IDocument` and `ITool` views in a two-row `Grid` hosting `<toolbarcontrols:ToolPaneToolBar ItemsSource="{Binding ToolBar}" />` above `<ContentControl cal:View.Model="{Binding}" />`.
  - `PanesStyleSelector` binds `LayoutItem` (`DocumentStyle`, using `TruncateMiddleConverter` on titles) and `LayoutAnchorableItem` (`ToolStyle`, mapping `Model.IsVisible` two-way to `Visibility.Hidden` when closed).
- **`LayoutInitializer` (`ILayoutUpdateStrategy`):** Maps `ITool.PreferredLocation` (`PaneLocation.Left`, `Right`, `Bottom`) to named `LayoutAnchorablePane` containers (`"LeftPane"`, `"RightPane"`, `"BottomPane"`), creating them dynamically inside `ShellView`'s `HorizontalPanel` or `VerticalPanel` and applying `ITool.PreferredWidth` / `PreferredHeight` when inserting the first tool into a pane.
- **Floating Windows (`UpdateFloatingWindows`):** Synchronizes `window.Icon` with `MainWindow.Icon` and sets `window.ShowInTaskbar = ShellViewModel.ShowFloatingWindowsInTaskbar` on every `LayoutUpdated` event.
- **Focus Workaround (`LayoutUtility.LoadLayout`):** During deserialization, resets AvalonDock's `LayoutDocument.IsLastFocusedDocument` property to `false` via reflection so documents opened programmatically after state restore receive focus properly.

### 2.3 Built-In Shell Commands (`Modules/Shell/Commands/*`)

| Command Definition | Shortcut | Handler Behavior |
| :--- | :--- | :--- |
| `NewFileCommandListDefinition` (`File.NewFile`) | *(submenu list)* | `NewFileCommandHandler` enumerates `[ImportMany] IEditorProvider` where `CanCreateNew == true`, populates one item per `EditorFileType`, and calls `IEditorOpeningService.NewFileAsync` with `"Untitled<N>.<ext>"`. |
| `OpenFileCommandDefinition` (`File.OpenFile`) | `Ctrl+O` | `OpenFileCommandHandler` (enabled when any `IEditorProvider` exists) builds an `"All Supported Files\|..."` `OpenFileDialog` filter and awaits `IEditorOpeningService.OpenFileAsync`. |
| `CloseFileCommandDefinition` (`File.CloseFile`) | *(none)* | `CloseFileCommandHandler` (enabled when `Shell.ActiveItem != null`) awaits `Shell.CloseDocumentAsync(Shell.ActiveItem)`. |
| `SaveFileCommandDefinition` (`File.SaveFile`) | `Ctrl+S` | Handled contextually on `Document` (`Enabled = this is IPersistedDocument`). |
| `SaveFileAsCommandDefinition` (`File.SaveFileAs`) | *(none)* | Handled contextually on `Document` (`PromptForSaveFilePath` + `Save`). |
| `SaveAllFilesCommandDefinition` (`File.SaveAllFiles`) | `Ctrl+Shift+S` | `SaveAllFilesCommandHandler` iterates `Shell.Documents.OfType<IPersistedDocument>().Where(x => !x.IsNew)` and awaits `document.Save(document.FilePath)`. |
| `SwitchToDocumentCommandListDefinition` (`Window.SwitchToDocument`) | *(menu list)* | `SwitchToDocumentListCommandHandler` populates numbered `_1 <DisplayName>` entries (`Checked` on active document) and activates via `Shell.OpenDocumentAsync`. |
| `ViewFullScreenCommandDefinition` (`View.FullScreen`) | `Shift+Alt+Enter` | `ViewFullScreenCommandHandler` toggles `Application.Current.MainWindow.WindowState` between `Maximized` and `Normal`. |
| `ExitCommandDefinition` (`File.Exit`) | `Alt+F4` | `ExitCommandHandler` calls `Shell.Close()` (`Application.Current.MainWindow.Close()`). |

---

## 3. Binary + XML Layout Persistence (`LayoutItemStatePersister.cs`)

By default, `ShellViewModel.StateFile` points to `@".\ApplicationState.bin"`. `LayoutItemStatePersister` (`[Export(typeof(ILayoutItemStatePersister))]`) combines custom per-ViewModel binary payloads with AvalonDock 5's XML layout serializer in a single file.

### 3.1 On-Disk Wire Format (`ApplicationState.bin`)

```text
+-----------------------------------------------------------------------+
| Int32: itemCount (number of serialized ShouldReopenOnStart items)     |
+-----------------------------------------------------------------------+
| Item Record [0 .. itemCount - 1]:                                     |
|   - String (7-bit length-prefixed UTF-8): Persisted Export Type Name  |
|     (AssemblyQualifiedName of unique [Export(typeof(ILayoutItem-sub))])|
|   - String (7-bit length-prefixed UTF-8): item.ContentId (Guid string)|
|   - Int64: payloadLength (bytes)                                      |
|   - Byte[payloadLength]: output of item.SaveState(BinaryWriter)       |
+-----------------------------------------------------------------------+
| Trailing UTF-8 XML Document: AvalonDock XmlLayoutSerializer output    |
+-----------------------------------------------------------------------+
```

> **MEF Contract Rule for Persisted Items (`GetPersistedType`):** Any `IDocument` or `ITool` with `ShouldReopenOnStart == true` **must** either have **exactly one** `[Export(typeof(...))]` attribute whose `ContractType` (or `ContractName` type) is assignable to `ILayoutItem`, or have zero `ILayoutItem` export attributes (in which case its concrete non-generic CLR type is used). Having multiple `ILayoutItem` `[Export]` attributes or an open generic type throws `InvalidOperationException` during save.

### 3.2 Transactional Save Protocol (`SaveState`)
1. Opens `<StateFile>.tmp` exclusively (`FileMode.Create, FileAccess.Write, FileShare.None`).
2. Buffers each item's `SaveState(BinaryWriter)` into an isolated `MemoryStream` first. If an individual item throws, `LayoutItemStatePersister` writes `payloadLength = 0` so stream offsets remain valid and marks the save result `LayoutItemStateSaveStatus.Partial`.
3. Seeks back to byte `0` to patch `itemCount`, seeks to end to append `shellView.SaveLayout(stream)`, and calls `stream.Flush(true)` to flush OS file buffers to disk.
4. If `<StateFile>` already exists, copies it to `<StateFile>.bak` and atomically swaps via `File.Replace(<tmp>, <StateFile>, null, true)` (otherwise `File.Move(<tmp>, <StateFile>)`).
5. File-level I/O or AvalonDock errors delete `<StateFile>.tmp`, leave the existing `<StateFile>` untouched, and return `LayoutItemStateSaveStatus.Failed` (surfaced to `ShellViewModel.OnStateSaveWarning`).

### 3.3 Two-Phase Load & Recovery Matrix (`LoadStateAsync`)
1. **Phase 1 — Synchronous Stream Parsing (`ParseState`):**
   - Validates `count >= 0` and each record's `payloadLength` bounds; resolves `Type.GetType(typeName, false)` and `IoC.GetInstance(contentType, null)`.
   - Missing plugin types, uncreatable instances, exceptions inside `item.LoadState(reader)`, or duplicate `ContentId`s record a **partial failure**, seek cleanly to `stateEndPosition`, and continue! Reading *past* `stateEndPosition` or malformed headers/AvalonDock XML throws and becomes `Corrupt`.
   - Calls `shellView.LoadLayout(...)`, matching deserialized AvalonDock nodes by `ContentId` and setting `e.Cancel = true` for skipped/missing items.
2. **Phase 2 — Post-Stream Async Content Restoration:**
   - After closing the file stream, awaits `IAsyncLayoutItemStateRestorer.RestoreStateAsync()` on each restored item. Items that throw are removed from `shell.Documents` / `shell.Tools` and downgrade the result to `Partial`.
   - Builds `LayoutItemStateRestorePlan(Documents, Tools.Where(x => x.IsVisible), SelectedItem)`. Hidden tools remain registered in `shell.Tools` **without** being activated; visible tools activate once.

| `LayoutItemStateLoadStatus` | When Produced | `ShellViewModel` Startup Action |
| :--- | :--- | :--- |
| `NotFound` | File or directory missing | Opens module `DefaultDocuments` and `DefaultTools` silently (`OpenDefaultLayoutAsync`). |
| `Success` | All items + XML + async restorers succeeded | Runs `RestoreLayoutAsync(restorePlan)`. |
| `Partial` | One or more items skipped/failed, envelope + XML valid | Calls `OnStateRestoreWarning`, retains all recovered items via `RestoreLayoutAsync` (does **not** overwrite with defaults). |
| `Failed` | File exists but cannot be opened (locked/ACL/I/O) | Preserves file on disk, calls `OnStateRestoreWarning`, falls back to `OpenDefaultLayoutAsync`. |
| `Corrupt` | Negative count, truncated payload, read overrun, or invalid AvalonDock XML | Preserves corrupt file in place for forensics, calls `OnStateRestoreWarning`, falls back to `OpenDefaultLayoutAsync`. |

---

## 4. Remaining Built-In Core Modules

### 4.1 `MainWindow` (`Modules/MainWindow/*`)
- `MainWindowViewModel` (`[Export(typeof(IMainWindow))]`) derives from `Conductor<IShell>` with defaults `Width = 1000`, `Height = 800`, `WindowState = Normal`, `Title = Resources.MainWindowDefaultTitle`, and `Icon = SomaFrameQD-32.png`.
- `MainWindowView.xaml` subclasses `MahApps.Metro.Controls.MetroWindow` (`WindowTransitionsEnabled="False"`, `UseLayoutRounding="True"`, `ResizeMode="CanResizeWithGrip"`, `Style="{DynamicResource MainWindowStyle}"`) and exposes a named `<ResourceDictionary x:Name="ThemeDictionary" />` populated at runtime by `ThemeManager`.

### 4.2 `MainMenu` (`Modules/MainMenu/*`)
- `MainMenuViewModel` (`[Export(typeof(IMenu))]`) builds `MenuDefinitions.MainMenuBar` in `OnImportsSatisfied()` and tracks `Properties.Settings.Default.AutoHideMainMenu` via weak `SettingsPropertyChangedEventManager`.
- `MenuEx` (`Controls/MenuEx.cs`) collapses `Height = 0` when `AutoHide == true` and keyboard focus is outside the menu, restoring `Height = double.NaN` on focus (e.g., pressing `Alt`).
- `MainMenuSettingsViewModel` (`[Export(typeof(ISettingsEditor))]`, `CreationPolicy.NonShared`) registers the **Environment → General** settings page controlling `SelectedTheme`, `AutoHideMainMenu`, and `SelectedLanguage` (`""`, `"en"`, `"de"`, `"ru"`, `"zh-Hans"`, `"ko"`, `"fr"`), persisting changes to `SomaFrameQD.Properties.Settings.Default`.

### 4.3 `ToolBars` (`Modules/ToolBars/*`)
- `ToolBars.Module` (`[Export(typeof(IModule))]`) injects `/SomaFrameQD;component/Modules/ToolBars/Resources/Styles.xaml` into `GlobalResourceDictionaries`.
- `ToolBarsViewModel` (`[Export(typeof(IToolBars))]`) defaults `Visible = false`; modules or startup code must set `Shell.ToolBars.Visible = true` to display the top `ToolBarTray`. Note that `OnViewLoaded` populates `ToolBarTray.ToolBars` once from `IToolBarBuilder.BuildToolBars(this)`.

### 4.4 `StatusBar` (`Modules/StatusBar/*`)
- `StatusBarViewModel` (`[Export(typeof(IStatusBar))]`) maintains a custom `StatusBarItemCollection` that stamps `StatusBarItemViewModel.Index` on insert/set and notifies `IStatusBarView.RefreshGridColumns()` whenever items change so `GridLength` (`Auto`, `Star`, or fixed pixel) columns stay synchronized.

### 4.5 `Toolbox` (`Modules/Toolbox/*`)
- **`[ToolboxItem(typeof(TargetDocument), "Name", "Category", "pack://...icon.png")]`:** Declarative class attribute scanned once across `AssemblySource.Instance` by `ToolboxService` (`[Export(typeof(IToolboxService))]`).
- **`ToolboxViewModel` (`[Export(typeof(IToolbox))]`):** Docks `PaneLocation.Left`, groups items by `"Category"` via `CollectionViewSource`, listens to `IShell.ActiveDocumentChanged` to display items matching `shell.ActiveItem.GetType()`, and filters items via `SearchCommand` when `searchTerm.Length >= 2`.
- **Drag-and-Drop (`ToolboxView.xaml.cs` & `ToolboxDragDrop`):** Initiates `DragDrop.DoDragDrop(..., DragDropEffects.Move)` once mouse movement exceeds `SystemParameters.MinimumHorizontalDragDistance` / `MinimumVerticalDragDistance`, packaging the `ToolboxItem` model under `ToolboxDragDrop.DataFormat` (`"SomaFrameQDToolboxItem"`).

### 4.6 `UndoRedo` (`Modules/UndoRedo/*`)
- **`UndoRedoManager` (`IUndoRedoManager`):** Maintains a linear `ActionStack` split at `UndoActionCount` (`0 .. UndoActionCount - 1` are undoable; `UndoActionCount .. Count - 1` are redoable):
  - `ExecuteAction(action)` calls `action.Execute()` then `PushAction(action)`.
  - `PushAction(action)` truncates any redo tail (calling `(action as IDisposable)?.Dispose()` on evicted actions!), appends `action`, increments `UndoActionCount`, and enforces optional `UndoCountLimit`.
  - `Undo(n)`, `UndoTo(action)`, `UndoAll()`, `Redo(n)`, and `RedoTo(action)` bracket multi-step transitions with `BatchBegin` / `BatchEnd` events.
  - `Dispose()` disposes all `IDisposable` actions in reverse order when the owning `Document` closes.
- **`HistoryViewModel` (`[Export(typeof(IHistoryTool))]`):** Docks `PaneLocation.Right`, automatically follows `IShell.ActiveDocumentChanged` (`shell.ActiveItem?.UndoRedoManager`), prepends a synthetic `"Initial State"` row at index `0`, marks rows as `InitialState`, `Undo`, `Current`, or `Redo`, and lets users scrub history interactively.

### 4.7 `Settings` (`Modules/Settings/*`)
- Modules contribute options pages by exporting `[Export(typeof(ISettingsEditor))]` (synchronous `ApplyChanges()`) or `[Export(typeof(ISettingsEditorAsync))]` (asynchronous `ApplyChangesAsync()`), specifying `SettingsPagePath` (backslash-delimited tree path, e.g., `"Environment"`) and `SettingsPageName` (leaf node, e.g., `"General"`).
- `SettingsViewModel` (`[Export(typeof(SettingsViewModel))]`, `CreationPolicy.NonShared`, opened modally via `Tools -> Options`) wraps sync editors in `SettingsEditorWrapper`, builds a `SettingsPageViewModel` hierarchy inside `OnInitializedAsync`, selects the first leaf page via `GetFirstLeafPageRecursive`, and awaits `ApplyChangesAsync()` across every editor when `SaveChanges()` runs (`TryCloseAsync(true)` vs `Cancel()` → `TryCloseAsync(false)`).
