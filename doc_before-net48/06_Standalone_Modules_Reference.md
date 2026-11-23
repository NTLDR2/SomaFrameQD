# 06 — Standalone Modules Reference (`SomaFrameQD.Modules.*`, Before `.NET 4.8` Retargeting)

> **Baseline Snapshot:** Pre-`.NET Framework 4.8`-only retargeting and optimization (`2026-11-22`).  
> **Scope:** All 7 standalone NuGet module projects under `src/SomaFrameQD.Modules.*` (`147` core files, `6,183` Core SLOC / `8,185` core lines).  
> **Target Frameworks in Snapshot:** `<TargetFrameworks>net48;net10.0-windows</TargetFrameworks>` across all 7 projects (`0.9.0` via root `version.json`).

---

## 1. Standalone Modules Matrix

Every standalone module compiles into its own assembly and NuGet package (matching its project name) and integrates into `SomaFrameQD` automatically when present in the application directory via MEF discovery:

| Module / NuGet ID | Core SLOC | Internal Package Deps | External NuGet Deps (`net48;net10.0-windows`) | Primary MEF Exports | Default Dock / Surface |
| :--- | ---: | :--- | :--- | :--- | :--- |
| `SomaFrameQD.Modules.CodeCompiler` | 104 | `SomaFrameQDWpf`, `ErrorList`, `Output` | `Microsoft.CodeAnalysis.CSharp` `3.4.0` | `ICodeCompiler` | Headless service (writes to `IOutput` & `IErrorList`) |
| `SomaFrameQD.Modules.CodeEditor` | 886 | `SomaFrameQDWpf` | `AvalonEdit` `6.3.1.120` | `LanguageDefinitionManager`, `IEditorProvider`, `CodeEditorViewModel` (`NonShared`), `ISettingsEditor`, toolbar defs | Center document + `CodeEditorToolBar` + Settings page |
| `SomaFrameQD.Modules.ErrorList` | 669 | `SomaFrameQDWpf` | *(none)* | `IErrorList`, `View.ErrorList` command/menu, filter toggle commands/toolbar | `PaneLocation.Bottom` tool with pane toolbar |
| `SomaFrameQD.Modules.GraphEditor` | 1,548 | `SomaFrameQDWpf` | *(none)* | `IModule` (`GraphEditor.Module`) | Reusable WPF node-graph & zoom/pan custom controls |
| `SomaFrameQD.Modules.Inspector` | 2,536 | `SomaFrameQDWpf` | `System.Drawing.Common` `6.0.0` *(conditional `< net10.0`)* | `IInspectorTool`, `View.Inspector` command/menu | `PaneLocation.Right` (`Width = 300`) property/group editor tool |
| `SomaFrameQD.Modules.Output` | 259 | `SomaFrameQDWpf` | *(none)* | `IOutput`, `View.Output` command/menu | `PaneLocation.Bottom` buffered text log tool |
| `SomaFrameQD.Modules.PropertyGrid` | 181 | `SomaFrameQDWpf` | *(uses transitive Extended WPF Toolkit)* | `IPropertyGrid`, `View.PropertiesWindow` command/menu | `PaneLocation.Right` `xctk:PropertyGrid` tool |

---

## 2. `SomaFrameQD.Modules.CodeCompiler`

Provides in-memory Roslyn C# compilation with automatic `Output` and `ErrorList` reporting (`src/SomaFrameQD.Modules.CodeCompiler/*`, SDK `Microsoft.NET.Sdk`).

### Key Contracts & Behavior
- **`ICodeCompiler` (`ICodeCompiler.cs`):**
  ```csharp
  Assembly Compile(IEnumerable<SyntaxTree> syntaxTrees, IEnumerable<MetadataReference> references, string outputName);
  ```
- **`CodeCompiler` (`[Export(typeof(ICodeCompiler))]`, `CodeCompiler.cs`):**
  - Imports `IEditorProvider`, `IOutput`, and `IErrorList` via `[ImportingConstructor]`.
  - Logs `"------ Compile started"` to `IOutput`, calls `GC.Collect()`, builds a `CSharpCompilation` (`OutputKind.DynamicallyLinkedLibrary`), and emits IL into a `MemoryStream`.
  - Clears `_errorList.Items` on every run:
    - **On failure (`!result.Success`):** Iterates `result.Diagnostics` (throws `NotSupportedException("Only support source file locations.")` if `!diagnostic.Location.IsInSource`), maps `DiagnosticSeverity` (`Info` → `Message`, `Warning` → `Warning`, `Error` → `Error`), and calls `_errorList.AddItem(...)` with an `onClick` callback that builds up and executes `new OpenDocumentResult(lineSpan.Path)`. Logs `"------ Compile failed"` and returns `null`.
    - **On success:** Logs `"------ Compile finished"` and loads the assembly bytes in-memory via `Assembly.Load(ms.ToArray())`.

---

## 3. `SomaFrameQD.Modules.CodeEditor`

Integrates **AvalonEdit** (`ICSharpCode.AvalonEdit`) as a multi-language syntax-highlighted document editor (`src/SomaFrameQD.Modules.CodeEditor/*`).

### 3.1 Language Registry (`ILanguageDefinition`, `LanguageDefinitionManager.cs`)
`LanguageDefinitionManager` (`[Export(typeof(LanguageDefinitionManager))]`) lazily initializes its language table from three sources in order:
1. **16 Built-In AvalonEdit Languages (`DefaultLanguageDefinition`):** `C#` (`.cs`), `JavaScript` (`.js`), `HTML`, `ASP/XHTML`, `Boo`, `Coco`, `CSS`, `C++`, `Java`, `Patch`, `PowerShell` (`.ps1`, `.psm1`, `.psd1`), `PHP`, `TeX`, `VBNET`, `XML` (29 XML/MSBuild extensions including `.xml`, `.xaml`, `.csproj`, `.props`, `.targets`, `.config`, `.nuspec`), and `MarkDown` (`.md`).
2. **MEF Plugin Languages (`IoC.GetAll<ILanguageDefinition>()`):** Any exported `ILanguageDefinition` replaces a built-in definition with the same `Name` (case-insensitive) or appends a new language.
3. **Disk `.xshd` Overrides (`./SyntaxHighlighting/*.xshd`):** Scans `Path.Combine(Directory.GetCurrentDirectory(), "SyntaxHighlighting")`. Matching filenames override `CustomSyntaxHighlightingFileName` on existing definitions; unrecognized `.xshd` files register new `DefaultLanguageDefinition(XshdSyntaxDefinition)` entries.

### 3.2 Editor Provider, Document Lifecycle & Async State Restore
- **`EditorProvider` (`[Export(typeof(IEditorProvider))]`):** `CanCreateNew => true`; `Handles(path)` checks `_languageDefinitionManager.GetDefinitionByExtension(Path.GetExtension(path)) != null`; `Create()` resolves `IoC.Get<CodeEditorViewModel>()`.
- **`CodeEditorViewModel` (`[Export(typeof(CodeEditorViewModel))]`, `[PartCreationPolicy(CreationPolicy.NonShared)]`):**
  - Inherits `PersistedDocument` and implements **`IAsyncLayoutItemStateRestorer`** (`ShouldReopenOnStart => true`):
    - `SaveState(BinaryWriter)` writes `FilePath`.
    - `LoadState(BinaryReader)` captures `_restoredFilePath = reader.ReadString()` synchronously while the state stream is open.
    - `RestoreStateAsync()` invokes `Load(filePath)` after layout parsing completes.
  - Synchronizes early `DoNew()` / `DoLoad()` calls made before view attachment using `_delayedViewLoaded` and sets `SyntaxHighlighting` based on `Path.GetExtension(FileName).ToLower()`.
  - Overrides `Equals(object)` comparing `FilePath` and `FileName` (`InvariantCultureIgnoreCase`).

### 3.3 Custom `Controls.CodeEditor`, ToolBar Commands & Settings
- **`Controls.CodeEditor` (`ICSharpCode.AvalonEdit.TextEditor` subclass):** Configures `Consolas` `12pt`, `ConvertTabsToSpaces = true`, syncs editor options from `Properties.Settings.Default`, and installs AvalonEdit's built-in `SearchPanel.Install(this)` (`OwnedSearchPanel`, `Ctrl+F`).
- **Contextual Editor Commands & `CodeEditorToolBarDefinitions`:** Exports top-level `CodeEditorToolBar` containing five `IconOnly` toggle buttons handled directly on `CodeEditorViewModel`:
  - `CodeEditor.ShowLineNumbers`, `CodeEditor.ShowSpaces`, `CodeEditor.ShowTabs`, `CodeEditor.ShowEndOfLine` (each toggles the active editor **and** saves to `Properties.Settings.Default`).
  - `CodeEditor.WordWrap` (toggles `WordWrap` on the active editor instance).
- **`CodeEditorSettingsViewModel` (`[Export(typeof(ISettingsEditor))]`, `NonShared`):** Contributes **Code Editor → General** inside `Tools -> Options`; `ApplyChanges()` saves `Settings.Default` and immediately updates all open `CodeEditorViewModel` documents via `ApplySettings()`.
- **Pre-`net48` Resource Header Note:** `src/SomaFrameQD.Modules.CodeEditor/Properties/Resources.resx` currently contains a `System.Windows.Forms, Version=5.0.0.0` `resheader` entry (whereas the other 42 `.resx` files reference `Version=4.0.0.0`).

---

## 4. `SomaFrameQD.Modules.ErrorList`

Reproduces the Visual Studio Error List tool pane (`src/SomaFrameQD.Modules.ErrorList/*`).

- **`IErrorList` / `ErrorListViewModel` (`[Export(typeof(IErrorList))]`):**
  - Docks at `PaneLocation.Bottom` and assigns `ToolBarDefinition = ToolBarDefinitions.ErrorListToolBar` (an **un-exported** `ToolBarDefinition` rendered locally inside the Error List pane header via `ToolPaneToolBar`, while its groups/items are `[Export]`ed for `IToolBarBuilder` discovery).
  - Maintains `Items` (`BindableCollection<ErrorListItem>`) and computed `FilteredItems` gated by `ShowErrors`, `ShowWarnings`, and `ShowMessages` (all `true` by default).
  - `AddItem(itemType, description, path, line, column, onClick)` appends sequentially numbered `ErrorListItem` rows; double-clicking a row in `ErrorListView` invokes `item.OnClick`.
- **Commands & Menus:**
  - `ViewErrorListCommandDefinition` (`View.ErrorList`): Added to `View -> Tools` (`SortOrder = 0`).
  - `ToggleErrorsWarningsMessagesCommandHandler` (`[CommandHandler]`): Handles `ErrorList.ToggleErrors`, `ErrorList.ToggleWarnings`, and `ErrorList.ToggleMessages`, dynamically disabling zero-count severity buttons and pluralizing labels (`"1 Error"` vs `"0 Errors"`).

---

## 5. `SomaFrameQD.Modules.GraphEditor`

Provides MVVM-friendly custom WPF controls for node/connector/bezier-wire editors (`src/SomaFrameQD.Modules.GraphEditor/*`). Unlike tool modules, `GraphEditor` leaves graph/node/connector ViewModels to the host application (demonstrated in `SomaFrameQD.Demo`'s `FilterDesigner`).

### Control Suite (`Controls/*` & `Themes/Generic.xaml`)

| Control | Base Class | Responsibility & Key Bindings / Events |
| :--- | :--- | :--- |
| `GraphControl` | `Control` | Top-level canvas surface with a `25x25` tiled grid `VisualBrush` template hosting `PART_ConnectionItemsControl` beneath `PART_ElementItemsControl`. Exposes `ElementsSource`, `ConnectionsSource`, item templates/selectors/container styles, `SelectedElements`, `SelectedConnections`, `SelectionChanged`, `ConnectionSelectionChanged`, and routed `ConnectionDragStarted` / `ConnectionDragging` / `ConnectionDragCompleted` events. |
| `ElementItemsControl` & `ElementItem` | `ListBox` / `ListBoxItem` | Hosts draggable node containers on a `Canvas` (`SelectionMode.Extended`). `ElementItem` binds `X`, `Y`, and `ZIndex` two-way by default to `Canvas.Left`, `Canvas.Top`, and `Canvas.ZIndex`, handles left-drag repositioning, and calls `BringToFront()` (`MaxZIndex + 1`) on mouse down. |
| `ConnectionItemsControl` & `ConnectionItem` | `ListBox` / `ListBoxItem` | Hosts selectable wire connections on the lower `Canvas` layer; clicking a wire clears previous selections via `ParentGraphControl.UnselectAll()`. |
| `ConnectorItem` | `ContentControl` | Input/output pin placed inside an `ElementItem`'s `DataTemplate`. Recomputes `Position` (center point transformed into `GraphControl` coordinates via `TransformToAncestor`) on every `LayoutUpdated` and raises internal `ConnectorDrag*` routed events consumed by `GraphControl`. Setting `e.Connection = null` in `ConnectionDragStarted` cancels the drag. |
| `BezierLine` | `Shape` | Renders a smooth horizontal S-curve cubic `BezierSegment` from `(X1, Y1)` to `(X2, Y2)` using midpoint control points `((X1+X2)/2, Y1)` and `((X1+X2)/2, Y2)`. |
| `ZoomAndPanControl` | `ContentControl, IScrollInfo` | Partial class (`ZoomAndPanControl.cs` + `ZoomAndPanControl_IScrollInfo.cs` + `AnimationHelper.cs`, CPOL license by Ashley Davis) applying `ScaleTransform` + `TranslateTransform` to `PART_Content` with `ContentScale`, `MinContentScale`/`MaxContentScale`, `ContentOffsetX`/`Y`, `ZoomAboutPoint`, animated zoom/pan, and `ScrollViewer` `IScrollInfo` integration. |

---

## 6. `SomaFrameQD.Modules.Inspector`

Provides a customizable, undo-aware property and group inspector tool window (`src/SomaFrameQD.Modules.Inspector/*`).

### 6.1 Tool Window & Fluent/Convention Builders
- **`IInspectorTool` / `InspectorViewModel` (`[Export(typeof(IInspectorTool))]`):** Docks `PaneLocation.Right` (`PreferredWidth = 300`), exposes `IInspectableObject SelectedObject`, `SelectedObjectChanged` event, and `ResetAll()` (recursively resetting dirty editors). Registered under `View -> Inspector` (`View.Inspector` in `ViewPropertiesMenuGroup`).
- **`InspectableObjectBuilder` (`InspectorBuilder<TBuilder>`):** Supports mixing explicit fluent editors with reflection-driven conventions:
  - Explicit fluent methods: `WithCollapsibleGroup`, `WithCheckBoxEditor`, `WithColorEditor`, `WithEnumEditor`, `WithPoint3DEditor`, `WithRangeEditor` (`float`/`double`), `WithEditor<T, TProp, TEditor>`, `WithObjectProperty`.
  - Convention reflection (`WithObjectProperties(object instance, Func<PropertyDescriptor, bool> propertyFilter)`): Reflects `TypeDescriptor.GetProperties(instance)` where `IsBrowsable && propertyFilter(pd)`. If any property carries a non-default `[Category]`, automatically groups properties into `CollapsibleGroupViewModel` sections (defaulting uncategorized properties to `"Miscellaneous"`); otherwise emits a flat list.
- **`DefaultPropertyInspectors.InspectorBuilders`:** Static, extensible `List<PropertyEditorBuilder>` evaluated in order:
  1. `RangePropertyEditorBuilder` — numeric `int`, `float`, or `double` decorated with `System.ComponentModel.DataAnnotations.RangeAttribute`.
  2. `EnumPropertyEditorBuilder` — any `Enum` type (`EnumEditorViewModel`).
  3. `StandardPropertyEditorBuilder<T, TEditor>` — `bool` (`CheckBoxEditorViewModel`), `Color` (`ColorEditorViewModel`), `double`/`float`/`int` and `double?`/`float?`/`int?` (`TextBoxEditorViewModel<T>`), `Point3D` (`Point3DEditorViewModel`), `string` (`TextBoxEditorViewModel<string>`), and `BitmapSource` (`BitmapSourceEditorViewModel`).

### 6.2 Two-Tier Undo/Redo Integration (`EditorBase<TValue>` & `SelectiveUndoEditorBase<TValue>`)
- **`BoundPropertyDescriptor`:** Wraps `(PropertyOwner, PropertyDescriptor)` and subscribes to `PropertyDescriptor.AddValueChanged` (or `INotifyPropertyChanged` fallback).
- **`EditorBase<TValue>`:** Default `IsUndoEnabled = true`. Setting `Value` (when `_shell.ActiveItem != null`) pushes a `ChangeObjectValueAction` onto `_shell.ActiveItem.UndoRedoManager`; calling `Reset()` pushes a `ResetObjectValueAction`. Supports optional `Converter` and `StringConverter` (for human-readable undo action names) and `[DefaultValue]` `IsDirty` tracking.
- **`SelectiveUndoEditorBase<TValue>`:** Used by continuous/interactive editors (such as sliders, color pickers, and text boxes) so intermediate drag ticks do not flood the undo stack. `OnBeginEdit()` disables per-tick undo and snapshots `_originalValue = RawValue` (supporting `null` values); `OnEndEdit()` (also called on `Dispose()`) commits a single `ChangeObjectValueAction(_originalValue, value)` if the final value changed.

### 6.3 Custom Inspector Controls & Win32 Screen Color Picker
- **`InspectorGrid` & `SimpleGridSplitter` (`Controls/SimpleGridSplitter.cs`, `819` lines):** Synchronizes proportional label/editor column widths across nested `CollapsibleGroupView` levels.
- **`ScreenColorPicker` (`Controls/ScreenColorPicker.cs`, `Util/ScreenShotUtility.cs`, `Win32/NativeMethods.cs`):** Captures a full virtual-desktop `System.Drawing.Bitmap` (`Graphics.CopyFromScreen` across `SystemParameters.VirtualScreen*`), captures the mouse, polls `user32.dll!GetCursorPos` every `50ms` via `DispatcherTimer` to raise `ColorHovered` / `ColorPicked` (or cancels on `Escape`), and disposes the `Bitmap` when mouse capture ends. *(This GDI+ capture is why `SomaFrameQD.Modules.Inspector.csproj` references `System.Drawing.Common` `6.0.0` on `< net10.0`.)*

---

## 7. `SomaFrameQD.Modules.Output` & `SomaFrameQD.Modules.PropertyGrid`

### 7.1 `SomaFrameQD.Modules.Output` (`src/SomaFrameQD.Modules.Output/*`)
- **`IOutput` / `OutputViewModel` (`[Export(typeof(IOutput))]`):** Docks `PaneLocation.Bottom`, buffers log text in an internal `StringBuilder`, marshals UI updates (`SetText`, `Clear`) onto the WPF dispatcher via `Execute.OnUIThread(...)`, scrolls to end on `OnViewLoaded`, and exposes `TextWriter Writer` (`OutputWriter`) so `Console.SetOut` or diagnostic writers can stream directly into the pane.
- **Menu Command:** `ViewOutputCommandDefinition` (`View.Output`) in `View -> Tools` (`SortOrder = 1`).

### 7.2 `SomaFrameQD.Modules.PropertyGrid` (`src/SomaFrameQD.Modules.PropertyGrid/*`)
- **`IPropertyGrid` / `PropertyGridViewModel` (`[Export(typeof(IPropertyGrid))]`):** Docks `PaneLocation.Right` and hosts `xctk:PropertyGrid` (`ShowSortOptions="False"`, `IsCategorized="False"`) bound to `object SelectedObject`.
- **Undo/Redo Command Rerouting:** Implements `ICommandRerouter`, forwarding `UndoCommandDefinition` and `RedoCommandDefinition` to `_shell.ActiveItem` so `Ctrl+Z` / `Ctrl+Y` continue targeting the active document while keyboard focus is inside the PropertyGrid.
- **Menu Command:** `ViewPropertyGridCommandDefinition` (`View.PropertiesWindow`) in `View -> Properties` (`SortOrder = 0`).
