# 07 — Demo Application & Client Extension Patterns (`SomaFrameQD.Demo`, Before `.NET 4.8` Retargeting)

> **Baseline Snapshot:** Pre-`.NET Framework 4.8`-only retargeting and optimization (`2026-11-22`).  
> **Scope:** `src/SomaFrameQD.Demo/*` (`64` core files, `3,368` Core SLOC / `3,778` core lines; `76` total text files / `4,718` lines).  
> **Target Frameworks in Snapshot:** `<TargetFrameworks>net48;net10.0-windows</TargetFrameworks>` (`OutputType = WinExe`, `IsPackable = false`, independent version `0.1.0` via `src/SomaFrameQD.Demo/version.json`).

---

## 1. Role & Project Configuration of `SomaFrameQD.Demo`

`SomaFrameQD.Demo` serves two roles in the repository:
1. **Reference Showcase:** Demonstrates how a host WPF executable composes `SomaFrameQD` and all 7 `SomaFrameQD.Modules.*` libraries via MEF, overrides built-in shell services, registers custom editors/tools/menus/settings, and integrates unsafe bitmap and HLSL shader pipelines.
2. **Build & Artifact Verification Target:** `eng/validate-artifacts.ps1` inspects `src/SomaFrameQD.Demo/bin/Release/<tfm>` to verify that custom HLSL shaders (`AddEffect.ps`, `MultiplyEffect.ps`) compiled cleanly for every retained framework.

### Key Project Settings (`src/SomaFrameQD.Demo/SomaFrameQD.Demo.csproj`)
- **`<AllowUnsafeBlocks>true</AllowUnsafeBlocks>`:** Required for pointer-based pixel access in `Modules/FilterDesigner/Util/FastBitmap.cs`.
- **HLSL Pixel Shader Compilation (`JeremyAnsel.HLSL.Targets` `1.0.9`):** Compiles `Modules/FilterDesigner/ShaderEffects/*.fx` using `<ShaderProfile>ps_2_0</ShaderProfile>` into `$(OutputPath)Modules/FilterDesigner/ShaderEffects/*.ps`.
- **Conditional 3D Viewport Package Split (Pre-Conversion Multi-Targeting):**
  - `net48`: `<PackageReference Include="HelixToolkit.Wpf" Version="2.10.0" />`
  - `!= net48` (`net10.0-windows`): `<PackageReference Include="HelixToolkit.Core.Wpf" Version="2.10.0" />`
- **Independent Versioning (`src/SomaFrameQD.Demo/version.json`):** Versioned at **`0.1.0`** (decoupled from the `0.9.0` NuGet library family via `pathFilters` in root `version.json`).
- **Pre-Conversion Config & Legacy Publish Profiles:**
  - `app.config` currently specifies `<supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.6.1"/>` and user setting `ConfirmExit = True`.
  - `Properties/PublishProfiles/` still contains legacy single-file profiles `FolderProfileNet5.pubxml` (`net5.0-windows`) and `FolderProfileNetCore31.pubxml` (`netcoreapp3.1`).
- **Historical Context (per `VNIR.md`):** Most sample domain code (`FilterDesigner`, `Home`, `Helix`, `SampleBrowser`, `Startup`, `TextEditor`) dates from 2011–2015, with targeted Sept–Oct 2026 updates adapting `Demo.csproj`, `ShellViewModel.CanCloseAsync`, and `FilterDesigner.Sample.Activate` to Caliburn.Micro 5's async lifecycle and `net48;net10.0-windows` dual-targeting.

---

## 2. Bootstrapper & Entry Wiring (`App.xaml` & `DemoAppBootstrapper.cs`)

- **`App.xaml`:** Declares `xmlns:SomaFrameQD="http://schemas.timjones.io/SomaFrameQD"` and merges `<SomaFrameQD:AppBootstrapper x:Key="bootstrapper" />` inside `Application.Resources`.
- **`DemoAppBootstrapper` (`DemoAppBootstrapper.cs`):** Optional `AppBootstrapper` subclass (commented out in `App.xaml` for `.NET 5+` `PublishSingleFile` testing) demonstrating how single-file hosts set `IsPublishSingleFileHandled => true` and yield all 8 library assemblies (`SomaFrameQD` + 7 `SomaFrameQD.Modules.*` assemblies) from `PublishSingleFileBypassAssemblies`.

---

## 3. Six Canonical SomaFrameQD Extension Patterns in `SomaFrameQD.Demo`

### Pattern 1: Overriding `IShell` & Reusing Core `ShellView` (`Modules/Shell/*`)
- **MEF Priority Override:** `SomaFrameQD.Demo.Modules.Shell.ViewModels.ShellViewModel` is decorated with `[Export(typeof(IShell))]` and inherits from `SomaFrameQD.Modules.Shell.ViewModels.ShellViewModel`. Because `AppBootstrapper` places the entry assembly (`SomaFrameQD.Demo`) into `priorityProvider` ahead of `mainProvider`, `IoC.Get<IShell>()` resolves Demo's `ShellViewModel` automatically.
- **Caliburn Namespace Aliasing:** Its static constructor registers:
  ```csharp
  ViewLocator.AddNamespaceMapping(typeof(ShellViewModel).Namespace, typeof(ShellView).Namespace);
  ```
  allowing the derived `ShellViewModel` to reuse Core's `ShellView.xaml` without duplicating XAML.
- **Ordered Async Exit Confirmation (`CanCloseAsync`):**
  1. Awaits `await base.CanCloseAsync(cancellationToken)` **first** so dirty documents prompt Save/Discard/Cancel before the application exit dialog appears.
  2. Runs a custom `IResult` coroutine (`MessageBoxResult` checking `Settings.Default.ConfirmExit`) bridged via `TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously)`.
- **Merging onto an Existing Settings Page (`DemoApplicationSettingsViewModel`):** Exports `[Export(typeof(ISettingsEditor))]` (`NonShared`) with `SettingsPagePath = "Environment"` and `SettingsPageName = "General"`. `SettingsViewModel` appends `DemoApplicationSettingsViewModel` (`ConfirmExit` checkbox) directly below `MainMenuSettingsViewModel` on the **Environment → General** page.

### Pattern 2: Shell Chrome Setup & Active-Document Inspector Sync (`Modules/Startup/Module.cs`)
`Startup.Module` (`[Export(typeof(IModule))]`) demonstrates typical IDE workbench initialization:
- Overrides `DefaultTools` to yield `typeof(IInspectorTool)` when no `ApplicationState.bin` exists.
- In `Initialize()`:
  - Enables `Shell.ShowFloatingWindowsInTaskbar = true` and `Shell.ToolBars.Visible = true`.
  - Sets `MainWindow.Title = "SomaFrameQD Demo"`.
  - Populates three `IStatusBar` columns (`"Hello world!"` `*`, `"Ln 44"` `100px`, `"Col 79"` `100px`) and writes `"Started up"` to `IOutput`.
  - Subscribes to `Shell.ActiveDocumentChanged` and populates `IInspectorTool.SelectedObject` using `InspectableObjectBuilder.WithObjectProperties(Shell.ActiveItem, pd => pd.ComponentType == Shell.ActiveItem.GetType())`.

### Pattern 3: Dynamic Menu Exclusion, Custom Binary State & Live Roslyn Scripting (`Modules/Home/*`)
- **Conditional Startup Menu (`Home.Module`):** Exports `View.Home` and `View.Helix` into `ViewDemoMenuGroup` (`View` menu) and declares a top-level `"DEBUG"` `MenuDefinition` (`SortOrder = int.MaxValue`) using `.SetDynamicExclusionPredicate(_ => System.Diagnostics.Debugger.IsAttached == false)`, hiding the entire menu whenever launched without an attached debugger.
- **Custom Document State Serialization (`HomeViewModel.cs`):** Sets `ShouldReopenOnStart => true` and overrides `SaveState(BinaryWriter)` / `LoadState(BinaryReader)` to persist `Background` (4 ARGB bytes), `Foreground` (4 ARGB bytes), and `TextAlignment` (enum string) inside `ApplicationState.bin`. Also exposes typed and nullable numeric/color/enum/text properties to exercise `Inspector` and `PropertyGrid` simultaneously.
- **Live In-Memory C# Scripting + 3D Viewport (`HelixViewModel.cs`, `IDemoScript.cs`):**
  - Embeds `SomaFrameQD.Modules.CodeEditor.Controls.CodeEditor` beside a HelixToolkit 3D viewport (`CameraPosition`, `[Range(1.0, 180.0)] CameraFieldOfView`, `LightPosition`, `[ReadOnly(true)] RotationAngle`).
  - On `OnViewLoaded` and every `TextEditor.TextChanged`, `CompileScripts()` parses the editor buffer with `CSharpSyntaxTree.ParseText`, compiles an in-memory `"SomaFrameQDDemoScript"` assembly via `ICodeCompiler.Compile(...)` (referencing `mscorlib.dll`, `System.dll`, `System.Core.dll`, Caliburn, `SomaFrameQD`, and the entry assembly), instantiates all `IDemoScript` types, and executes `script.Execute(this)` on every `CompositionTarget.Rendering` tick (unsubscribing when `OnDeactivateAsync(close: true)` runs).

### Pattern 4: Extensible Sample Launcher (`Modules/SampleBrowser/*`)
- Defines `ISample` (`string Name`, `Task Activate(IShell shell)`).
- `SampleBrowser.Module` opens `SampleBrowserViewModel` inside `PostInitializeAsync()`, which imports `[ImportMany] ISample[]` so feature modules can register interactive demos without coupling to the browser.

### Pattern 5: End-to-End Node Graph + Toolbox + Inspector + HLSL Shaders (`Modules/FilterDesigner/*`)
`FilterDesigner` combines **four** SomaFrameQD subsystems (`GraphEditor`, `Toolbox`, `Inspector`, and custom `ShaderEffect`s) into an image-processing node graph:
- **Entry Points:** Exported both as `FilterDesigner.Sample : ISample` (opening `GraphViewModel` and showing `IInspectorTool` + `IToolbox` via `await shell.ShowToolAsync<...>()`), and as `File -> Open Graph` (`File.OpenGraph` command).
- **Graph ViewModels (`GraphViewModel`, `ElementViewModel`, `ConnectorViewModel`, `ConnectionViewModel`):**
  - `GraphViewModel` (`[PartCreationPolicy(CreationPolicy.NonShared)]`) seeds a default graph (`ImageSource` + `ColorInput` → `Multiply`), snaps dragged output wires onto input pins within `10px` (`FindNearbyInputConnector`), replaces existing input connections on drop, deletes selected elements/wires on `Key.Delete`, and pushes the selected node or wire into `IInspectorTool` via `InspectableObjectBuilder` in `OnSelectionChanged()`.
- **Toolbox Node Discovery & Drop (`Elements/*`, `GraphView.xaml.cs`):**
  - Generator and filter nodes (`ImageSource`, `ColorInput`, `Add`, `Multiply`) are decorated with `[ToolboxItem(typeof(GraphViewModel), "<Name>", "<Category>", "pack://...")]`.
  - `GraphView.xaml.cs` handles `DragEnter` / `Drop` for `ToolboxDragDrop.DataFormat`, instantiating `Activator.CreateInstance(toolboxItem.ItemType)` at the mouse drop coordinates, plus right-mouse drag panning and mouse-wheel `ZoomAndPanControl.ZoomAboutPoint`.
- **GPU Pixel Shaders & `FastBitmap` (`ShaderEffects/*`, `Util/*`):**
  - `AddEffect.fx` and `MultiplyEffect.fx` are 2-sampler HLSL shaders compiled to `.ps` bytecode and loaded from `AppDomain.CurrentDomain.BaseDirectory + "Modules/FilterDesigner/ShaderEffects/<Name>.ps"`.
  - `FastBitmap` (`IDisposable`, `unsafe`) converts `BitmapSource` inputs to `PixelFormats.Pbgra32` `WriteableBitmap`s and exposes a `Color this[int x, int y]` pointer indexer (`Lock()` / `Unlock()`).

### Pattern 6: Lightweight `PersistedDocument` & `IEditorProvider` (`Modules/TextEditor/*`)
- `TextEditor.EditorProvider` (`[Export(typeof(IEditorProvider))]`) registers `.txt` (`FileTypes`) and handles `.txt` and `.cmd` extensions (`Handles`), creating `EditorViewModel : PersistedDocument` instances backed by a plain WPF `TextBox` (`DoNew`, `DoLoad`, `DoSave`, `IsDirty` tracking, and case-insensitive `Equals`).
