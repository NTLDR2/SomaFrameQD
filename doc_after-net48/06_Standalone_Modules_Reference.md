# 06 — Standalone Modules Reference (`SomaFrameQD.Modules.*`, `.NET Framework 4.8` `x64`)

> **Documentation Status:** **Authoritative Post-Conversion Reference (`2026-11-23`)** — Verified against frozen `SomaFrameQD` `0.9.0` sources after Stage 1 Retargeting and Stage 2 Optimization.  
> **Scope:** All **7** standalone `0.9.0` NuGet module projects under `src/SomaFrameQD.Modules.*` (`147` core files, `6,453` Core SLOC / `13,157` total text lines across all 7 module folders).  
> **Target Framework & Packaging:** `<TargetFramework>net48</TargetFramework>` (`AnyCPU` `x64`-clean `.dll`s under `lib/net48/`, Portable PDB `.snupkg`s, stamped `0.9.0` via root [`version.json`](../version.json), locked in [`eng/package-contract.json`](../eng/package-contract.json)).  
> **Companion Guides:** [01 — Architecture & Bootstrapper Overview](./01_Architecture_and_Bootstrapper_Overview.md) | [02 — Core Framework Primitives](./02_Core_Framework_Primitives_and_Coroutines.md) | [03 — Commands, Menus & ToolBars](./03_Commands_Menus_ToolBars_and_KeyGestures.md) | [04 — Shell, AvalonDock 5 & Built-In Modules](./04_Shell_Docking_Persistence_and_BuiltIn_Modules.md) | [05 — Themes, Shaders & Win32 `x64` Interop](./05_Themes_Rendering_and_Win32_Interop.md) | [07 — Demo Application & Extension Patterns](./07_Demo_Application_and_Extension_Patterns.md)

---

## 1. Standalone `0.9.0` NuGet Modules Matrix

Every standalone module compiles as a single-target `net48` `AnyCPU` (`x64`-clean) class library inheriting shared settings (`LangVersion` `12.0`, `DebugType` `portable`, `Deterministic` `true`, and native `.NET Framework 4.8` GAC `<Reference Include="System.ComponentModel.Composition" />`) from [`src/Directory.Build.props`](../src/Directory.Build.props). Dropping any module assembly into a host application's directory integrates its tools, editors, commands, menus, toolbars, and settings pages automatically via `AppBootstrapper`'s MEF `DirectoryCatalog`.

| Module / NuGet ID (`0.9.0`) | Core Files / Core SLOC | Internal NuGet Deps (`0.9.0`) | External NuGet Deps (`eng/package-contract.json`) | Extra `.NET 4.8` GAC Refs | Primary MEF Exports | Default Workbench Surface |
| :--- | ---: | :--- | :--- | :--- | :--- | :--- |
| [`SomaFrameQD.Modules.CodeCompiler`](../src/SomaFrameQD.Modules.CodeCompiler/) | `2` / `120` | `SomaFrameQDWpf`, `ErrorList`, `Output` | `Microsoft.CodeAnalysis.CSharp` `3.4.0` | *(inherited MEF)* | `ICodeCompiler` | Headless Roslyn compiler service (writes to `IOutput` & `IErrorList`) |
| [`SomaFrameQD.Modules.CodeEditor`](../src/SomaFrameQD.Modules.CodeEditor/) | `21` / `935` | `SomaFrameQDWpf` | `AvalonEdit` `6.3.1.120` | *(inherited MEF)* | `LanguageDefinitionManager`, `IEditorProvider`, `CodeEditorViewModel` (`NonShared`), `ISettingsEditor` (`NonShared`), toolbar/command defs | Center document + `CodeEditorToolBar` + **Code Editor → General** settings |
| [`SomaFrameQD.Modules.ErrorList`](../src/SomaFrameQD.Modules.ErrorList/) | `19` / `690` | `SomaFrameQDWpf` | *(none — `0` external NuGet deps)* | *(inherited MEF)* | `IErrorList`, `View.ErrorList` command/menu, filter toggle commands/toolbar items | `PaneLocation.Bottom` tool with pane-local `ErrorListToolBar` |
| [`SomaFrameQD.Modules.GraphEditor`](../src/SomaFrameQD.Modules.GraphEditor/) | `23` / `1,593` | `SomaFrameQDWpf` | *(none — `0` external NuGet deps)* | *(inherited MEF)* | `IModule` (`GraphEditor.Module`) | Reusable MVVM node-graph, connector, bezier-wire & zoom/pan WPF controls |
| [`SomaFrameQD.Modules.Inspector`](../src/SomaFrameQD.Modules.Inspector/) | `63` / `2,611` | `SomaFrameQDWpf` | *(none — `0` external NuGet deps)* | `System.Drawing` | `IInspectorTool`, `View.Inspector` command/menu | `PaneLocation.Right` (`Width = 300`) undo-aware property/group inspector + High-DPI eyedropper |
| [`SomaFrameQD.Modules.Output`](../src/SomaFrameQD.Modules.Output/) | `11` / `323` | `SomaFrameQDWpf` | *(none — `0` external NuGet deps)* | *(inherited MEF)* | `IOutput`, `View.Output` command/menu | `PaneLocation.Bottom` thread-safe coalesced text log tool |
| [`SomaFrameQD.Modules.PropertyGrid`](../src/SomaFrameQD.Modules.PropertyGrid/) | `8` / `181` | `SomaFrameQDWpf` | *(none — uses transitive `DotNetProjects.Extended.Wpf.Toolkit` `5.0.129` `lib/net40`)* | *(inherited MEF)* | `IPropertyGrid`, `View.PropertiesWindow` command/menu | `PaneLocation.Right` `xctk:PropertyGrid` tool with `ICommandRerouter` |

---

## 2. `SomaFrameQD.Modules.CodeCompiler` (`120` Core SLOC)

Provides in-process Roslyn C# compilation with integrated `IOutput` logging and clickable `IErrorList` diagnostics ([`SomaFrameQD.Modules.CodeCompiler.csproj`](../src/SomaFrameQD.Modules.CodeCompiler/SomaFrameQD.Modules.CodeCompiler.csproj), [`ICodeCompiler.cs`](../src/SomaFrameQD.Modules.CodeCompiler/ICodeCompiler.cs), [`CodeCompiler.cs`](../src/SomaFrameQD.Modules.CodeCompiler/CodeCompiler.cs)).

### 2.1 Public Contract & In-Memory PE Emission
```csharp
public interface ICodeCompiler
{
    Assembly Compile(
        IEnumerable<SyntaxTree> syntaxTrees,
        IEnumerable<MetadataReference> references,
        string outputName);
}
```
* **`CodeCompiler` (`[Export(typeof(ICodeCompiler))]`):** Imports `IEditorProvider`, `IOutput`, and `IErrorList` via `[ImportingConstructor]`.
* Logs `"------ Compile started"` to `IOutput`, constructs `CSharpCompilation.Create(outputName).WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)).AddReferences(references).AddSyntaxTrees(syntaxTrees)`, and emits the PE image into a pre-sized `new MemoryStream(InitialPeStreamCapacity)` (`16,384` bytes) to minimize `byte[]` buffer doublings.
* Clears `_errorList.Items` before processing every `EmitResult`:
  * **On success (`result.Success`):** Logs `"------ Compile finished"` and loads the compiled assembly directly into `AppDomain.CurrentDomain` via `Assembly.Load(ms.ToArray())`.
  * **On failure (`!result.Success`):** Populates `_errorList` via `ProcessResult(result)`, logs `"------ Compile failed"`, and returns `null`.

### 2.2 Stage 2 Hardening & `.NET Framework 4.8` Assembly Lifecycle
1. **Zero UI-Blocking `GC.Collect()` & Documented `AppDomain` Semantics (`Q7 = 7A`):**
   * Both [`ICodeCompiler.cs`](../src/SomaFrameQD.Modules.CodeCompiler/ICodeCompiler.cs) and [`CodeCompiler.cs`](../src/SomaFrameQD.Modules.CodeCompiler/CodeCompiler.cs) carry comprehensive `///` XML `<summary>` and `<remarks>` documenting that `Assembly.Load(byte[])` loads compiled assemblies into `AppDomain.CurrentDomain` so host types (such as live 3D scripts) can share object graphs directly without `MarshalByRefObject` remoting overhead.
   * Because the `.NET Framework 4.8` CLR pins dynamic assemblies loaded into the default `AppDomain` until process exit, forcing `GC.Collect()` on compile cannot reclaim them and only stalls the WPF UI thread in synchronous Gen-2 stop-the-world pauses. `CodeCompiler` omits `GC.Collect()` completely; callers performing live keystroke compilation should debounce edits and skip unchanged source buffers (demonstrated in `HelixViewModel`).
2. **Safe Handling of Hidden & Metadata-Level Roslyn Diagnostics:**
   * `ProcessResult(EmitResult)` skips `DiagnosticSeverity.Hidden` analyzer/unnecessary-`using` diagnostics (`CS8019`) via `continue;` instead of polluting `IErrorList` as informational messages.
   * Source-backed diagnostics (`diagnostic.Location != null && diagnostic.Location.IsInSource`) extract `lineSpan = diagnostic.Location.GetLineSpan()`, normalize empty tree paths to `filePath = null`, attach `openAction` (`new OpenDocumentResult(filePath)` + `IoC.BuildUp` + `Execute(null)`) when `filePath != null`, and report `1`-based/editor-mapped coordinates (`lineSpan.StartLinePosition.Line`, `lineSpan.StartLinePosition.Character`).
   * Assembly- or metadata-level diagnostics (`Location.None` / `!IsInSource`, such as `CS0009`, `CS0012`, or `CS1701`) report cleanly via `_errorList.AddItem(itemType, description)` instead of throwing `NotSupportedException`.
3. **Verified `[net48|x64]` Binding-Redirect Coexistence:**
   * `Microsoft.CodeAnalysis.CSharp` `3.4.0` (`System.Collections.Immutable`, `System.Reflection.Metadata`, `System.Memory`, `System.Runtime.CompilerServices.Unsafe`) is verified in `CodeCompilerAndOutputTests` to coexist cleanly inside the same 64-bit `AppDomain` as `Dirkster.AvalonDock` `5.0.1` and `System.Text.Json` `10.0.12` via automatic `.exe.config` binding redirects plus `AppBootstrapper.TryUnifyStrongNamedAssembly`.

---

## 3. `SomaFrameQD.Modules.CodeEditor` (`935` Core SLOC)

Integrates **AvalonEdit `6.3.1.120`** (`ICSharpCode.AvalonEdit`) as a multi-language syntax-highlighted document editor with persistent layout state, toolbar toggles, search panel, and settings integration ([`SomaFrameQD.Modules.CodeEditor.csproj`](../src/SomaFrameQD.Modules.CodeEditor/SomaFrameQD.Modules.CodeEditor.csproj)).

### 3.1 Three-Tier Language & `.xshd` Registry ([`LanguageDefinitionManager.cs`](../src/SomaFrameQD.Modules.CodeEditor/LanguageDefinitionManager.cs))
`LanguageDefinitionManager` (`[Export(typeof(LanguageDefinitionManager))]`) lazily initializes `LanguageDefinitions` from three tiers in priority order:
1. **16 Built-In AvalonEdit Languages (`DefaultLanguageDefinition`):** `C#` (`.cs`), `JavaScript` (`.js`), `HTML` (`.htm`, `.html`), `ASP/XHTML`, `Boo`, `Coco`, `CSS`, `C++`, `Java`, `Patch`, `PowerShell` (`.ps1`, `.psm1`, `.psd1`), `PHP`, `TeX`, `VBNET`, `XML` (29 XML/MSBuild extensions including `.xml`, `.xaml`, `.csproj`, `.props`, `.targets`, `.config`, `.manifest`, `.nuspec`), and `MarkDown` (`.md`).
2. **MEF `ILanguageDefinition` Exports (`IoC.GetAll<ILanguageDefinition>()`):** Replaces any built-in definition with a matching `Name` (`OrdinalIgnoreCase`) or appends a new language.
3. **Base-Directory-Anchored External `.xshd` Grammars (`SyntaxHighlighting/*.xshd`):**
   * `GetSyntaxHighlightingDirectories()` probes **`Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SyntaxHighlighting")` first** (preventing missed grammars when Windows 7–11 launches the app via file association, Start Menu shortcut, or `OpenFileDialog` with a shifted working directory), falling back to `Path.Combine(Directory.GetCurrentDirectory(), "SyntaxHighlighting")` only if distinct.
   * Parses custom `.xshd` XML grammars securely via `XmlReader.Create(reader, new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit })` and `HighlightingLoader.LoadXshd`.

### 3.2 `EditorProvider`, `CodeEditorViewModel` & `Controls.CodeEditor`
* **[`EditorProvider`](../src/SomaFrameQD.Modules.CodeEditor/EditorProvider.cs) (`[Export(typeof(IEditorProvider))]`):** Declares `CanCreateNew => true`, maps `FileTypes` across all registered `ILanguageDefinition` extensions, checks `Handles(path)` via `GetDefinitionByExtension`, and creates non-shared `CodeEditorViewModel` instances via `IoC.Get<CodeEditorViewModel>()`.
* **[`CodeEditorViewModel`](../src/SomaFrameQD.Modules.CodeEditor/ViewModels/CodeEditorViewModel.cs) (`[Export(typeof(CodeEditorViewModel))]`, `[PartCreationPolicy(CreationPolicy.NonShared)]`):**
  * Inherits `PersistedDocument` and implements **`IAsyncLayoutItemStateRestorer`** (`ShouldReopenOnStart => true`, `SaveState` writing `FilePath`, `LoadState` reading `_restoredFilePath`, and `RestoreStateAsync` calling `await Load(_restoredFilePath)` after `ApplicationState.bin` stream closure).
  * Coordinates early `DoNew()` / `DoLoad()` calls prior to view attachment via `_delayedViewLoaded` and sets `SyntaxHighlighting` from `Path.GetExtension(FileName)`.
  * Evaluates `IsDirty` on `TextChanged` using **`!string.Equals(_originalText, _view.TextEditor.Text, StringComparison.Ordinal)`**—eliminating culture-sensitive Unicode collation scans on every keystroke in large source files.
  * Implements contextual `ICommandHandler<...>` for all five `CodeEditorToolBar` toggle commands (`CodeEditor.ShowLineNumbers`, `ShowSpaces`, `ShowTabs`, `ShowEndOfLine`, and `WordWrap`).
* **[`Controls/CodeEditor.cs`](../src/SomaFrameQD.Modules.CodeEditor/Controls/CodeEditor.cs) (`ICSharpCode.AvalonEdit.TextEditor` subclass):** Defaults to `Consolas` `12pt` with `ConvertTabsToSpaces = true`, binds options to `Properties.Settings.Default`, and installs one per-editor **`OwnedSearchPanel`** (`SearchPanel.Install(this)`, `Ctrl+F`) without leaking static event handlers across closed tabs.
* **`CodeEditorSettingsViewModel` (`[Export(typeof(ISettingsEditor))]`, `NonShared`):** Contributes **Code Editor → General** inside `Tools -> Options` and pushes saved preferences live to all open `CodeEditorViewModel` tabs. All `.resx` headers in `CodeEditor` use normalized `.NET Framework 4.8` `Version=4.0.0.0` `System.Windows.Forms` reader/writer metadata.

---

## 4. `SomaFrameQD.Modules.ErrorList` (`690` Core SLOC)

Provides a Visual Studio-style Error List tool pane ([`SomaFrameQD.Modules.ErrorList.csproj`](../src/SomaFrameQD.Modules.ErrorList/SomaFrameQD.Modules.ErrorList.csproj), [`ErrorListViewModel.cs`](../src/SomaFrameQD.Modules.ErrorList/ViewModels/ErrorListViewModel.cs)):
* **`ErrorListViewModel` (`[Export(typeof(IErrorList))]`, `PaneLocation.Bottom`):**
  * Assigns `ToolBarDefinition = ToolBarDefinitions.ErrorListToolBar`—an **un-exported** `ToolBarDefinition` rendered locally inside the Error List pane header via `ToolPaneToolBar` (while its `ToolBarItemGroupDefinition` and `CommandToolBarItemDefinition` items are `[Export]`ed for `IToolBarBuilder` discovery).
  * Exposes `Items` (`BindableCollection<ErrorListItem>`) and `FilteredItems`:
    * **Zero-Allocation Default Filter Path:** When `ShowErrors && ShowWarnings && ShowMessages` (the default workbench state), `FilteredItems` returns `_items` directly without allocating LINQ iterator state machines; when all three toggles are `false`, returns `Enumerable.Empty<ErrorListItem>()`; otherwise filters via `_items.Where(IsItemVisible)`.
    * `ShowErrors`, `ShowWarnings`, and `ShowMessages` setters guard against redundant values so repeated command updates do not trigger unnecessary `FilteredItems` re-evaluations.
  * `AddItem(itemType, description, path, line, column, onClick)` appends numbered `ErrorListItem` entries; double-clicking a row in `ErrorListView` invokes `item.OnClick` (e.g., jumping to the diagnostic file via `OpenDocumentResult`).
* **Commands & Menus:** `ViewErrorListCommandDefinition` (`View.ErrorList` in `View -> Tools`, `SortOrder = 0`) and `ToggleErrorsWarningsMessagesCommandHandler` (`ErrorList.ToggleErrors`, `ToggleWarnings`, `ToggleMessages` with live singular/plural item counts and automatic zero-count button disabling).

---

## 5. `SomaFrameQD.Modules.GraphEditor` (`1,593` Core SLOC)

Supplies reusable MVVM-friendly WPF custom controls for node, pin/connector, bezier-wire, and zoom/pan canvas editors ([`SomaFrameQD.Modules.GraphEditor.csproj`](../src/SomaFrameQD.Modules.GraphEditor/SomaFrameQD.Modules.GraphEditor.csproj)). Host applications supply their own domain ViewModels for graphs, nodes, connectors, and connections (demonstrated in `SomaFrameQD.Demo`'s `FilterDesigner`).

### 5.1 Control Suite (`Controls/*` & `Themes/Generic.xaml`)

| Control | Base Type | Responsibility & Key Bindings / Events |
| :--- | :--- | :--- |
| [`GraphControl`](../src/SomaFrameQD.Modules.GraphEditor/Controls/GraphControl.cs) | `Control` | Top-level canvas with a `25×25` tiled grid `VisualBrush` hosting `PART_ConnectionItemsControl` beneath `PART_ElementItemsControl`. Exposes `ElementsSource`, `ConnectionsSource`, item templates/selectors/styles, `SelectedElements`, `SelectedConnections`, and routed `ConnectionDragStarted` / `ConnectionDragging` / `ConnectionDragCompleted` events. |
| `ElementItemsControl` & `ElementItem` | `ListBox` / `ListBoxItem` | Hosts draggable node containers on a `Canvas` (`SelectionMode.Extended`). Two-way binds `X`, `Y`, and `ZIndex` to `Canvas.Left`, `Canvas.Top`, and `Canvas.ZIndex`, handles left-drag translation, and calls `BringToFront()` (`MaxZIndex + 1`) on mouse down. |
| `ConnectionItemsControl` & `ConnectionItem` | `ListBox` / `ListBoxItem` | Hosts selectable wire items on the lower `Canvas` layer; clicking a wire clears competing selections via `ParentGraphControl.UnselectAll()`. |
| [`ConnectorItem`](../src/SomaFrameQD.Modules.GraphEditor/Controls/ConnectorItem.cs) | `ContentControl` | Input/output pin inside an `ElementItem` template. Computes center `Position` in `GraphControl` coordinates via `TransformToAncestor` and raises internal `ConnectorDrag*` routed events consumed by `GraphControl` (setting `e.Connection = null` in `ConnectionDragStarted` cancels a drag). |
| [`BezierLine`](../src/SomaFrameQD.Modules.GraphEditor/Controls/BezierLine.cs) | `Shape` | Draws a smooth horizontal S-curve cubic `BezierSegment` between `(X1, Y1)` and `(X2, Y2)` using midpoint control points `((X1+X2)/2, Y1)` and `((X1+X2)/2, Y2)`. |
| [`ZoomAndPanControl`](../src/SomaFrameQD.Modules.GraphEditor/Controls/ZoomAndPanControl.cs) | `ContentControl, IScrollInfo` | Applies `ScaleTransform` + `TranslateTransform` to `PART_Content` with `ContentScale`, `MinContentScale`/`MaxContentScale`, `ContentOffsetX`/`Y`, `ZoomAboutPoint`, animated zoom/pan (`AnimationHelper`), and `ScrollViewer` `IScrollInfo` integration. |

### 5.2 Stage 2 Lifecycle & Layout Churn Hardening in `ConnectorItem`
In WPF `.NET Framework 4.8`, `UIElement.LayoutUpdated` registers delegates on the dispatcher-global `ContextLayoutManager` list without passing a sender—meaning a control that subscribes in its constructor and never unsubscribes both leaks closed graph documents and runs on every unrelated layout pass across the entire workbench. [`ConnectorItem.cs`](../src/SomaFrameQD.Modules.GraphEditor/Controls/ConnectorItem.cs) eliminates both hazards:
1. **Scoped `Loaded` / `Unloaded` Subscription (`_isLayoutUpdatedSubscribed`):** Attaches `LayoutUpdated += OnLayoutUpdated` strictly on `Loaded` (calling `UpdatePosition()` immediately) and detaches `LayoutUpdated -= OnLayoutUpdated` on `Unloaded`, allowing closed `GraphControl` trees to be garbage-collected immediately.
2. **Immediate Initial Arrange Sync (`ArrangeOverride`):** Calls `UpdatePosition()` at the end of `ArrangeOverride(Size arrangeBounds)` and falls back to `RenderSize` when `ActualWidth`/`ActualHeight == 0`, guaranteeing accurate wire endpoints on initial render even before `Loaded` fires.
3. **No-Op DependencyProperty Write Suppression:** Writes `Position = newPosition` **only when `Position != newPosition`**, preventing redundant two-way binding notifications and `BezierLine` `AffectsMeasure`/`AffectsRender` invalidations during unrelated UI updates.

---

## 6. `SomaFrameQD.Modules.Inspector` (`2,611` Core SLOC)

Provides a rich, undo-integrated property and collapsible-group inspector tool window with fluent and convention builders plus a High-DPI multi-monitor screen eyedropper ([`SomaFrameQD.Modules.Inspector.csproj`](../src/SomaFrameQD.Modules.Inspector/SomaFrameQD.Modules.Inspector.csproj), binding directly to `.NET Framework 4.8`'s native GAC `<Reference Include="System.Drawing" />` with zero external NuGet packages).

### 6.1 `InspectorViewModel` & Fluent/Convention Builders ([`InspectableObjectBuilder.cs`](../src/SomaFrameQD.Modules.Inspector/InspectableObjectBuilder.cs))
* **`InspectorViewModel` (`[Export(typeof(IInspectorTool))]`, `PaneLocation.Right`, `PreferredWidth = 300`):** Exposes `IInspectableObject SelectedObject`, `SelectedObjectChanged`, and recursive `ResetAll()`. Registered under `View -> Inspector` (`View.Inspector`).
* **`InspectableObjectBuilder` (`InspectorBuilder<TBuilder>`):** Combines explicit fluent editors (`WithCollapsibleGroup`, `WithCheckBoxEditor`, `WithColorEditor`, `WithEnumEditor`, `WithPoint3DEditor`, `WithRangeEditor`, `WithEditor<T, TProp, TEditor>`, `WithObjectProperty`) with reflection-driven `WithObjectProperties(instance, filter)`—automatically grouping categorized properties into `CollapsibleGroupViewModel` sections (`"Miscellaneous"` fallback) or emitting a flat editor list.
* **`DefaultPropertyInspectors.InspectorBuilders`:** Ordered extensible registry matching `RangePropertyEditorBuilder` (`[Range(min, max)]` `int`/`float`/`double`), `EnumPropertyEditorBuilder`, and `StandardPropertyEditorBuilder<T, TEditor>` (`bool`, `Color`, `double`/`float`/`int` and nullable counterparts, `Point3D`, `string`, `BitmapSource`).

### 6.2 Two-Tier Undo/Redo & Proportional Column Splitting
* **`EditorBase<TValue>` & `BoundPropertyDescriptor`:** Pushes `ChangeObjectValueAction` onto `_shell.ActiveItem.UndoRedoManager` when `IsUndoEnabled == true` (default) and `ResetObjectValueAction` on `Reset()`, tracking `[DefaultValue]` dirty state.
* **`SelectiveUndoEditorBase<TValue>`:** Used by continuous interactive controls (`SliderEx` thumb drags, eyedropper color hovers, text edits) so intermediate drag ticks do not flood the undo stack; `OnBeginEdit()` snapshots `_originalValue = RawValue` while suppressing per-tick undo, and `OnEndEdit()` (also invoked on `Dispose()`) commits one `ChangeObjectValueAction(_originalValue, value)` if changed.
* **[`SimpleGridSplitter.cs`](../src/SomaFrameQD.Modules.Inspector/Controls/SimpleGridSplitter.cs) & `InspectorGrid`:** Synchronizes proportional label/value column widths across nested `CollapsibleGroupView` hierarchies.

### 6.3 High-DPI & Multi-Monitor `ScreenColorPicker` Hardening ([`NativeMethods.cs`](../src/SomaFrameQD.Modules.Inspector/Win32/NativeMethods.cs), [`ScreenShotUtility.cs`](../src/SomaFrameQD.Modules.Inspector/Util/ScreenShotUtility.cs), [`ScreenColorPicker.cs`](../src/SomaFrameQD.Modules.Inspector/Controls/ScreenColorPicker.cs))
WPF's `SystemParameters.VirtualScreen*` reports 96-DPI device-independent units, whereas `user32.dll!GetCursorPos` and GDI+ `Graphics.CopyFromScreen` operate in **physical Win32 pixels**—causing cropped virtual-desktop captures and mis-indexed cursor lookups on `125%`–`250%` High-DPI displays or negative-coordinate secondary monitors. Stage 2 Plan #6 hardened all three layers:
1. **Physical-Pixel Virtual Desktop Bounds (`NativeMethods.GetVirtualScreenPhysicalBounds`):** Queries `user32.dll!GetSystemMetrics` (`SM_XVIRTUALSCREEN = 76`, `SM_YVIRTUALSCREEN = 77`, `SM_CXVIRTUALSCREEN = 78`, `SM_CYVIRTUALSCREEN = 79`), clamping dimensions `>= 1` so [`ScreenShotUtility.Take()`](../src/SomaFrameQD.Modules.Inspector/Util/ScreenShotUtility.cs) captures the exact physical desktop rectangle across multi-monitor setups (disposing the `Bitmap` on exception).
2. **Exact Zero-Inclusive Bounds & Negative-Origin Offset (`ScreenColorPicker.TrySamplePixel`):**
   ```csharp
   int bitmapX = physicalCursorX - virtualScreenPhysicalBounds.Left;
   int bitmapY = physicalCursorY - virtualScreenPhysicalBounds.Top;
   if (bitmapX >= 0 && bitmapX < bitmap.Width && bitmapY >= 0 && bitmapY < bitmap.Height)
   ```
   Translates negative left/top monitor coordinates into non-negative `Bitmap` indices and includes the `(0, 0)` top-left edge cleanly.
3. **Deterministic GDI+ `HBITMAP` Disposal & Locked-Station Resilience:** `ScreenColorPicker.OnMouseDown` catches `Win32Exception` gracefully on locked/non-interactive Win32 desktop stations, disposes `_bitmap` immediately via `DisposeBitmap()` if `Focus() && CaptureMouse()` fails, and guarantees cleanup on both `OnLostMouseCapture` and `Unloaded`.

---

## 7. `SomaFrameQD.Modules.Output` & `SomaFrameQD.Modules.PropertyGrid`

### 7.1 `SomaFrameQD.Modules.Output` (`323` Core SLOC — [`SomaFrameQD.Modules.Output.csproj`](../src/SomaFrameQD.Modules.Output/SomaFrameQD.Modules.Output.csproj), [`OutputViewModel.cs`](../src/SomaFrameQD.Modules.Output/ViewModels/OutputViewModel.cs))
* **`OutputViewModel` (`[Export(typeof(IOutput))]`, `PaneLocation.Bottom`):** Exposes `Append(string)`, `AppendLine(string)`, `Clear()`, and `TextWriter Writer` (`OutputWriter` for `Console.SetOut` / diagnostic trace redirection), plus `View.Output` (`View -> Tools`, `SortOrder = 1`).
* **Thread-Safe Locking & Coalesced Background UI Flushing (Stage 2 Plan #5):**
  * Synchronizes all `_stringBuilder` mutations (`Append`, `AppendLine`, `Clear`) and `GetSnapshotText()` reads behind `private readonly object _syncRoot = new object()`, preventing race-condition `IndexOutOfRangeException` / corrupted character buffers when background `Task.Run` build or log threads write concurrently.
  * Coalesces high-frequency log bursts via single-flight `Interlocked.CompareExchange(ref _uiRefreshPending, 1, 0)` + `dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(FlushToView))`. `FlushToView()` resets `_uiRefreshPending` via `Interlocked.Exchange` *before* calling `GetSnapshotText()`, guaranteeing that any concurrent append racing with an active flush schedules one trailing UI update (verified under 8-thread × 250-line stress in `CodeCompilerAndOutputTests`).

### 7.2 `SomaFrameQD.Modules.PropertyGrid` (`181` Core SLOC — [`SomaFrameQD.Modules.PropertyGrid.csproj`](../src/SomaFrameQD.Modules.PropertyGrid/SomaFrameQD.Modules.PropertyGrid.csproj), [`PropertyGridViewModel.cs`](../src/SomaFrameQD.Modules.PropertyGrid/ViewModels/PropertyGridViewModel.cs))
* **`PropertyGridViewModel` (`[Export(typeof(IPropertyGrid))]`, `PaneLocation.Right`):** Wraps Extended WPF Toolkit's `xctk:PropertyGrid` (`DotNetProjects.Extended.Wpf.Toolkit` `5.0.129` `lib/net40` inherited transitively from `SomaFrameQDWpf`) bound to `object SelectedObject`, exposed under `View -> Properties` (`View.PropertiesWindow`, `SortOrder = 0`).
* **Active-Document Undo/Redo Forwarding (`ICommandRerouter`):** Implements `ICommandRerouter` forwarding `UndoCommandDefinition` (`Ctrl+Z`) and `RedoCommandDefinition` (`Ctrl+Y`) to `_shell.ActiveItem` so document undo/redo shortcuts remain active while keyboard focus resides inside property-grid editors.
