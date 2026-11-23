# 07 — Demo Application & Client Extension Patterns (`SomaFrameQD.Demo` `0.1.0` Alpha, `.NET Framework 4.8` `x64`)

> **Documentation Status:** **Authoritative Post-Conversion Reference (`2026-11-23`)** — Verified against frozen `SomaFrameQD.Demo` `0.1.0` sources after Stage 1 Retargeting and Stage 2 Optimization.  
> **Scope:** [`src/SomaFrameQD.Demo/*`](../src/SomaFrameQD.Demo/) (`64` core files, `3,461` Core SLOC / `4,867` total text lines; `<IsPackable>false</IsPackable>`, stamped `0.1.0` via [`src/SomaFrameQD.Demo/version.json`](../src/SomaFrameQD.Demo/version.json)).  
> **Target Framework & Architecture:** `<TargetFramework>net48</TargetFramework>`, **64-bit (`x64` `PE32+`) `WinExe`** (`<PlatformTarget>x64</PlatformTarget>`, `<Prefer32Bit>false</Prefer32Bit>`).  
> **Companion Guides:** [01 — Architecture & Bootstrapper Overview](./01_Architecture_and_Bootstrapper_Overview.md) | [02 — Core Framework Primitives](./02_Core_Framework_Primitives_and_Coroutines.md) | [03 — Commands, Menus & ToolBars](./03_Commands_Menus_ToolBars_and_KeyGestures.md) | [04 — Shell, AvalonDock 5 & Built-In Modules](./04_Shell_Docking_Persistence_and_BuiltIn_Modules.md) | [05 — Themes, Shaders & Win32 `x64` Interop](./05_Themes_Rendering_and_Win32_Interop.md) | [06 — Standalone Modules Reference](./06_Standalone_Modules_Reference.md)

---

## 1. Role & Reference `x64` Host Configuration of `SomaFrameQD.Demo`

`SomaFrameQD.Demo` serves three roles in the repository:
1. **Reference `net48` `x64` Host Blueprint:** Demonstrates the canonical `.csproj`, `app.config`, `app.manifest`, and `AppBootstrapper` subclass configuration recommended for downstream `.NET Framework 4.8` workbench applications targeting **Windows 7 SP1 `x64` through Windows 11 `x64`** (see [Deployment Guide](../doc/net48-win7-to-win11-x64-deployment-guide.md)).
2. **End-to-End Extension Showcase (`0.1.0` Alpha):** Exercises `SomaFrameQD` plus all **7** `SomaFrameQD.Modules.*` libraries across priority MEF service overrides, async exit confirmation, merged settings pages, dynamic menu exclusions, binary document state persistence, live Roslyn 3D scripting, node-graph editing, unsafe `Pbgra32` bitmap manipulation, and custom `ps_2_0` HLSL pixel shaders.
3. **Artifact & Shader Gate Target:** [`eng/validate-artifacts.ps1`](../eng/validate-artifacts.ps1) inspects `src/SomaFrameQD.Demo/bin/Release/net48` to verify `PE32+` `x64` headers, generated `SomaFrameQD.Demo.exe.config` binding redirects, and DirectX `ps_2_0` (`0xFFFF0200`) compiled shader outputs (`AddEffect.ps`, `MultiplyEffect.ps`).

### 1.1 Key Project, Runtime & Manifest Settings

| Configuration File | Active Post-Conversion Setting | Architectural Purpose |
| :--- | :--- | :--- |
| [`SomaFrameQD.Demo.csproj`](../src/SomaFrameQD.Demo/SomaFrameQD.Demo.csproj) | `<TargetFramework>net48</TargetFramework>`, `<OutputType>WinExe</OutputType>`, `<PlatformTarget>x64</PlatformTarget>`, `<Prefer32Bit>false</Prefer32Bit>` | Compiles a pure 64-bit (`PE32+`) desktop executable with full 64-bit virtual address space and `x64` RyuJIT code generation. |
| [`SomaFrameQD.Demo.csproj`](../src/SomaFrameQD.Demo/SomaFrameQD.Demo.csproj) | `<AutoGenerateBindingRedirects>true</AutoGenerateBindingRedirects>` + `<GenerateBindingRedirectsOutputType>true</GenerateBindingRedirectsOutputType>` | Emits `SomaFrameQD.Demo.exe.config` binding redirects unifying `System.Text.Json` `10.0.12`, Roslyn `3.4.0`, and BCL ValueTuple/Memory/Unsafe assemblies at load time. |
| [`SomaFrameQD.Demo.csproj`](../src/SomaFrameQD.Demo/SomaFrameQD.Demo.csproj) | `HelixToolkit.Wpf` `2.10.0` + `JeremyAnsel.HLSL.Targets` `1.0.9` (`<ShaderProfile>ps_2_0</ShaderProfile>`) + `<AllowUnsafeBlocks>true</AllowUnsafeBlocks>` | Unconditional `net48` 3D viewport package, build-time `.fx` → `.ps` `ps_2_0` shader compilation, and `unsafe` pointer support for `FastBitmap`. |
| [`version.json`](../src/SomaFrameQD.Demo/version.json) | `"version": "0.1.0"` (isolated via `pathFilters` in root `version.json`) | Signals independent alpha sample-app maturity decoupled from the `0.9.0` production NuGet framework packages. |
| [`app.config`](../src/SomaFrameQD.Demo/app.config) | `<supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.8"/>` + `ConfirmExit = True` | Enforces `.NET Framework 4.8` SKU detection at OS launch and seeds user-scoped Demo settings. |
| [`app.manifest`](../src/SomaFrameQD.Demo/app.manifest) | `asInvoker` UAC, all 4 `<supportedOS>` GUIDs (`Win7 SP1`, `Win8`, `Win8.1`, `Win10/11`), `<dpiAware>true</dpiAware>` + `<dpiAwareness>system</dpiAwareness>`, Common Controls `6.0.0.0` | Unlocks truthful OS version APIs, crisp DWM System-DPI scaling without layout tearing on `HwndHost`/shader surfaces (`Q2 = 2A`), and modern Win32 visual styles. |

---

## 2. Bootstrapper Wiring & Multi-Core Startup Profile (`App.xaml` & `DemoAppBootstrapper.cs`)

* **[`App.xaml`](../src/SomaFrameQD.Demo/App.xaml):** Declares `xmlns:local="clr-namespace:SomaFrameQD.Demo"` and instantiates `<local:DemoAppBootstrapper x:Key="bootstrapper" />` directly inside `Application.Resources` (`MergedDictionaries`).
* **[`DemoAppBootstrapper.cs`](../src/SomaFrameQD.Demo/DemoAppBootstrapper.cs) (`Q3 = 3B`, `Q5 = 5A`):** Minimal, idiomatic `.NET Framework 4.8` subclass of `AppBootstrapper` demonstrating how host applications customize their CLR `ProfileOptimization` startup profile filename (`%LocalAppData%\SomaFrameQD\JitProfiles\SomaFrameQD.Demo.Startup.net48.jitprofile`):
  ```csharp
  public class DemoAppBootstrapper : AppBootstrapper
  {
      protected override string JitProfileFileName => "SomaFrameQD.Demo.Startup.net48.jitprofile";
  }
  ```

---

## 3. Six Canonical Extension Patterns & Stage 2 Optimizations

### Pattern 1: Priority MEF `IShell` Override, View Reuse & Merged Settings ([`Modules/Shell/*`](../src/SomaFrameQD.Demo/Modules/Shell/))
1. **Two-Tier MEF Priority Override (`Demo.Modules.Shell.ViewModels.ShellViewModel`):**
   * Exports `[Export(typeof(IShell))]` and inherits from `SomaFrameQD.Modules.Shell.ViewModels.ShellViewModel`. Because `AppBootstrapper` places the entry assembly (`SelectAssemblies()`) in `priorityProvider` ahead of `mainProvider`, `IoC.Get<IShell>()` resolves Demo's `ShellViewModel` without modifying Core `SomaFrameQD`.
2. **Caliburn View-Namespace Aliasing:**
   * Registers `ViewLocator.AddNamespaceMapping(typeof(ShellViewModel).Namespace, typeof(ShellView).Namespace)` in its static constructor so the subclass reuses Core's `ShellView.xaml` without duplicating XAML markup.
3. **Ordered Async Exit Confirmation (`CanCloseAsync`):**
   * Awaits `await base.CanCloseAsync(cancellationToken)` **first** so dirty `PersistedDocument` tabs prompt Save/Discard/Cancel before showing the application exit confirmation coroutine (`MessageBoxResult` gated on `Settings.Default.ConfirmExit`, bridged via `TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously)`).
4. **Merging onto an Existing Settings Page (`DemoApplicationSettingsViewModel`):**
   * Exports `[Export(typeof(ISettingsEditor))]` (`[PartCreationPolicy(CreationPolicy.NonShared)]`) with `SettingsPagePath = "Environment"` and `SettingsPageName = "General"`, appending the `ConfirmExit` option directly beneath Core's `MainMenuSettingsViewModel` on **Environment → General**.

### Pattern 2: Workbench Chrome Setup & Active-Document Inspector Sync ([`Modules/Startup/Module.cs`](../src/SomaFrameQD.Demo/Modules/Startup/Module.cs))
`Startup.Module` (`[Export(typeof(IModule))]`) illustrates standard workbench startup wiring:
* Overrides `DefaultTools` to open `typeof(IInspectorTool)` automatically on first launch when no `ApplicationState.bin` exists.
* In `Initialize()`: enables `Shell.ShowFloatingWindowsInTaskbar = true` and `Shell.ToolBars.Visible = true`, sets `MainWindow.Title = "SomaFrameQD Demo"`, configures three `IStatusBar` columns (`"Hello world!"` `*`, `"Ln 44"` `100px`, `"Col 79"` `100px`), logs `"Started up"` to `IOutput`, and subscribes to `Shell.ActiveDocumentChanged` to populate `IInspectorTool.SelectedObject` via `InspectableObjectBuilder.WithObjectProperties`.

### Pattern 3: Dynamic Menu Exclusion, Binary Document State & Debounced Live Roslyn 3D Scripting ([`Modules/Home/*`](../src/SomaFrameQD.Demo/Modules/Home/))
1. **Debugger-Gated Top-Level Menu (`Home.Module`):**
   * Exports `View.Home` and `View.Helix` into `ViewDemoMenuGroup` and declares a top-level `"DEBUG"` `MenuDefinition` (`SortOrder = int.MaxValue`) with `.SetDynamicExclusionPredicate(_ => !Debugger.IsAttached)`, hiding the entire menu whenever no debugger is attached.
2. **Custom Binary Document State ([`HomeViewModel.cs`](../src/SomaFrameQD.Demo/Modules/Home/ViewModels/HomeViewModel.cs)):**
   * Sets `ShouldReopenOnStart => true` and overrides `SaveState(BinaryWriter)` / `LoadState(BinaryReader)` to persist `Background` (4 ARGB bytes), `Foreground` (4 ARGB bytes), and `TextAlignment` inside `ApplicationState.bin`, while exposing categorized/nullable properties for `Inspector` and `PropertyGrid` testing.
3. **Debounced Live Roslyn C# Scripting & Visibility-Gated 3D Render Loop ([`HelixViewModel.cs`](../src/SomaFrameQD.Demo/Modules/Home/ViewModels/HelixViewModel.cs), `Q7 = 7A`):**
   * Pairs an AvalonEdit `CodeEditor` with a `HelixToolkit.Wpf` 3D viewport (`CameraPosition`, `[Range(1.0, 180.0)] CameraFieldOfView`, `LightPosition`, `[ReadOnly(true)] RotationAngle`) driven by user-edited `IDemoScript` classes compiled in-memory via `ICodeCompiler.Compile(...)`.
   * **Stage 2 `.NET Framework 4.8` Performance & Memory Hardening:**
     * **Process-Wide `MetadataReference` Cache (`CachedScriptReferences`):** Lazily builds and deduplicates (`StringComparer.OrdinalIgnoreCase`) `MetadataReference.CreateFromFile(...)` descriptors once per process across `mscorlib.dll`, `System.dll`, `System.Core.dll`, `Caliburn.Micro.Core`, `SomaFrameQD`, `SomaFrameQD.Demo`, and the entry assembly—eliminating repeated disk PE header reads on every edit.
     * **`350ms` `DispatcherTimer` Debounce & Source Deduplication:** `TextEditor.TextChanged` restarts `_compileDebounceTimer` (`350ms`, `DispatcherPriority.Background`), and `CompileScripts()` exits immediately when `string.Equals(sourceText, _lastCompiledSource, StringComparison.Ordinal)`—preventing per-keystroke UI stalls and avoiding unnecessary default-`AppDomain` dynamic assembly accumulation (`"SomaFrameQDDemoScript_" + Interlocked.Increment(ref _scriptRevision)`).
     * **Active-Tab VSync Gating (`UpdateRenderingSubscription`):** `OnActivatedAsync` sets `_isActiveDocument = true` and `OnDeactivateAsync(bool close, ...)` sets `_isActiveDocument = false` + stops `_compileDebounceTimer`, attaching `CompositionTarget.Rendering` **only** while the Helix tab is the active foreground document (`close == false` tab switches detach VSync immediately so backgrounded 3D tabs consume zero CPU/GPU cycles).

### Pattern 4: Open-Ended `[ImportMany] ISample` Plugin Discovery ([`Modules/SampleBrowser/*`](../src/SomaFrameQD.Demo/Modules/SampleBrowser/))
* Defines `ISample` (`string Name`, `Task Activate(IShell shell)`) and opens `SampleBrowserViewModel` inside `SampleBrowser.Module.PostInitializeAsync()`. Any feature module exporting `[Export(typeof(ISample))]` appears automatically in the browser without coupling to `SampleBrowser`.

### Pattern 5: Node Graph + Toolbox + Inspector + Finalizer-Safe `FastBitmap` + `ps_2_0` Shaders ([`Modules/FilterDesigner/*`](../src/SomaFrameQD.Demo/Modules/FilterDesigner/))
Combines `GraphEditor`, `Toolbox`, `Inspector`, unsafe `WriteableBitmap` pixel processing, and custom HLSL pixel shaders into an interactive image-processing pipeline:
* **Graph & Toolbox Wiring:** `GraphViewModel` (`[PartCreationPolicy(CreationPolicy.NonShared)]`) seeds `ImageSource` + `ColorInput` → `Multiply`, snaps dragged wires within `10px`, deletes selected nodes/wires on `Key.Delete`, and pushes selected items into `IInspectorTool`. Generator/filter nodes (`ImageSource`, `ColorInput`, `Add`, `Multiply`) declare `[ToolboxItem(typeof(GraphViewModel), ...)]` for drag-and-drop instantiation onto `GraphView.xaml.cs` (which also implements right-drag panning and mouse-wheel `ZoomAndPanControl.ZoomAboutPoint`).
* **Finalizer-Safe 64-Bit `FastBitmap` ([`FastBitmap.cs`](../src/SomaFrameQD.Demo/Modules/FilterDesigner/Util/FastBitmap.cs), Stage 2 Plan #7):**
  * **No `~FastBitmap()` Finalizer:** WPF `WriteableBitmap` inherits from `DispatcherObject`; calling `_bitmap.Unlock()` from CLR 4.8's background GC Finalizer Thread throws an unhandled cross-thread `InvalidOperationException` that terminates the process (`2008` CLR 2.0 crash semantics are mandatory in `.NET Framework 4.8`). `FastBitmap` uses deterministic `Dispose()` only, checking `_bitmap != null && _bitmap.CheckAccess()` before `Unlock()` and always disposing `_stream`.
  * **Eager Stream Decode (`BitmapCacheOption.OnLoad`):** `FastBitmap(byte[])` decodes with `BitmapDecoder.Create(_stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad)` so decoded frames never depend on deferred stream lifetimes.
  * **Bounds-Checked 64-Bit Pointer Arithmetic:** `unsafe Color this[int x, int y]` validates `(uint)x >= (uint)_width || (uint)y >= (uint)_height` (throwing `ArgumentOutOfRangeException` instead of corrupting the 64-bit heap) and computes pixel offsets using 64-bit `long` stride math (`(byte*)_startingPosition + ((long)y * _strideWidth) + ((long)x * BitsPerPixel)`).
* **Frozen `ps_2_0` Shader Effects ([`ShaderEffectUtility.cs`](../src/SomaFrameQD.Demo/Modules/FilterDesigner/Util/ShaderEffectUtility.cs), [`AddEffect.fx`](../src/SomaFrameQD.Demo/Modules/FilterDesigner/ShaderEffects/AddEffect.fx), [`MultiplyEffect.fx`](../src/SomaFrameQD.Demo/Modules/FilterDesigner/ShaderEffects/MultiplyEffect.fx)):**
  * Two-sampler HLSL effects compiled to `ps_2_0` (`0xFFFF0200`) `.ps` bytecode under `$(OutputPath)Modules\FilterDesigner\ShaderEffects\`.
  * Demo's `ShaderEffectUtility.GetPixelShader` resolves shaders relative to `AppDomain.CurrentDomain.BaseDirectory`, sets `ShaderRenderMode = ShaderRenderMode.Auto` (GPU Direct3D9Ex on Aero/Win8.1/10/11; SSE2 CPU software fallback on Win7 Basic/Classic/RDP), and calls **`pixelShader.Freeze()`**.

### Pattern 6: Lightweight `PersistedDocument` & `IEditorProvider` ([`Modules/TextEditor/*`](../src/SomaFrameQD.Demo/Modules/TextEditor/))
* `TextEditor.EditorProvider` (`[Export(typeof(IEditorProvider))]`) registers `.txt` in `FileTypes` and handles `.txt` / `.cmd` in `Handles(path)`, instantiating a fresh `new EditorViewModel()` (`: PersistedDocument`) directly inside `Create()` backed by a WPF `TextBox` with `DoNew`, `DoLoad`, `DoSave`, `IsDirty` tracking, and case-insensitive `Equals`.
