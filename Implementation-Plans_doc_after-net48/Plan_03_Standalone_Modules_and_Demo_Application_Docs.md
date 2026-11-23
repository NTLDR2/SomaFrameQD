# Post-`net48` Documentation Plan #3 — Standalone `SomaFrameQD.Modules.*` Packages & `SomaFrameQD.Demo` (`0.1.0` Alpha) (`doc_after-net48/06`–`07`)

> **Purpose:** Author the post-`net48` `x64` reference guides for all **7 standalone `0.9.0` NuGet modules** (`06_Standalone_Modules_Reference.md`) and **`SomaFrameQD.Demo` `0.1.0` alpha & client extension patterns** (`07_Demo_Application_and_Extension_Patterns.md`) inside `C:\SOFTDEV\SomaFrameQD\doc_after-net48\`, verified 100% against the frozen source tree.  
> **Generated Timestamp:** `2026-11-23T01:53:00-05:00` (`2026-11-23`)  
> **Prerequisite:** **[Plan #1](./Plan_01_Architecture_Bootstrapper_Primitives_and_Commands_Docs.md)** and **[Plan #2](./Plan_02_Shell_Docking_Themes_Rendering_and_Win32_x64_Docs.md) MUST be 100% completed and verified first.**  
> **Execution Order:** [Plan #1](./Plan_01_Architecture_Bootstrapper_Primitives_and_Commands_Docs.md) → [Plan #2](./Plan_02_Shell_Docking_Themes_Rendering_and_Win32_x64_Docs.md) → **Plan #3 (This Plan)** → [Plan #4](./Plan_04_Tests_Build_CI_Master_Index_and_Agent_Routing_Sync.md).  
> **Authoritative Post-Conversion References:** [`changelogs/20261123_CHANGELOG.md`](../changelogs/20261123_CHANGELOG.md) | [`eng/package-contract.json`](../eng/package-contract.json) | [`VNIR.md`](../VNIR.md) | [`AGENTS.md`](../AGENTS.md)

---

## Four-Plan Context-Window Safety Split (`Implementation-Plans_doc_after-net48`)

| Plan | Source Inspection Scope (Read-Only) | Context Budget | Output Deliverables |
| :--- | :--- | :--- | :--- |
| **[Plan #1](./Plan_01_Architecture_Bootstrapper_Primitives_and_Commands_Docs.md)** | `AppBootstrapper.cs`, `DemoAppBootstrapper.cs`, `app.config` (x2), `app.manifest`, `src/Directory.Build.props`, `Framework/*` | `~4,800` targeted SLOC read | **`doc_after-net48/01`–`03`** |
| **[Plan #2](./Plan_02_Shell_Docking_Themes_Rendering_and_Win32_x64_Docs.md)** | `Modules/*` (all 8 built-in modules), `Framework/Themes/*`, `Themes/VS2013/*`, `ShaderEffects/*`, `HwndWrapper`, `ClippingHwndHost`, `NativeMethods.cs` | `~5,200` targeted SLOC read | **`doc_after-net48/04`–`05`** |
| **Plan #3 (This Plan)** | All 7 `src/SomaFrameQD.Modules.*` projects + `src/SomaFrameQD.Demo` (`FastBitmap`, `HelixViewModel`, `ShaderEffectUtility`, 6 demo modules; skipping `DesignTimeImages.cs`) | `~5,800` targeted SLOC read (skipping `.resx` & base64 image blobs) | **`doc_after-net48/06`–`07`** (`2` standalone NuGet modules & Demo `0.1.0` alpha guides) |
| **[Plan #4](./Plan_04_Tests_Build_CI_Master_Index_and_Agent_Routing_Sync.md)** | `src/SomaFrameQD.Tests` (27 `.cs` files / 174 `[net48\|x64]` executions), `global.json`, `version.json` (x2), `SomaFrameQD.sln`, `eng/*`, `.github/workflows/ci.yml`, `changelogs/*`, `releasenotes/*` | `~3,500` targeted SLOC + scripts/configs | **`doc_after-net48/08`–`09` + `00_INDEX.md`** + **`AGENTS.md`** & **`REPO_MAP.md`** sync + full audit |

---

## Strict Guardrails

1. **ALL C# & SOURCE CODE IS STRICTLY FROZEN:** Read-only on all `.cs`, `.xaml`, `.fx`, `.csproj`, and config files.
2. **NEVER Modify `doc_before-net48/`:** Keep [`doc_before-net48/`](../doc_before-net48/00_INDEX.md) 100% untouched.
3. **Context-Window Discipline:**
   * **NEVER read `src/SomaFrameQD.Demo/Modules/FilterDesigner/Design/DesignTimeImages.cs`** (`1,016` lines of Base64 image blobs) or any `.resx` / `.Designer.cs` files.
   * Read the Stage 1 & Stage 2 modified files across `SomaFrameQD.Modules.*` and `SomaFrameQD.Demo` directly so every post-optimization signature, XML doc remark, and NuGet/GAC dependency is 100% verbatim accurate.

---

## Exact Deliverables in `C:\SOFTDEV\SomaFrameQD\doc_after-net48\`

### 1. `06_Standalone_Modules_Reference.md`
* **Frozen Sources to Verify Against:**
  * [`SomaFrameQD.Modules.CodeCompiler`](../src/SomaFrameQD.Modules.CodeCompiler/) ([`ICodeCompiler.cs`](../src/SomaFrameQD.Modules.CodeCompiler/ICodeCompiler.cs), [`CodeCompiler.cs`](../src/SomaFrameQD.Modules.CodeCompiler/CodeCompiler.cs), `.csproj`)
  * [`SomaFrameQD.Modules.CodeEditor`](../src/SomaFrameQD.Modules.CodeEditor/) ([`LanguageDefinitionManager.cs`](../src/SomaFrameQD.Modules.CodeEditor/LanguageDefinitionManager.cs), [`ViewModels/CodeEditorViewModel.cs`](../src/SomaFrameQD.Modules.CodeEditor/ViewModels/CodeEditorViewModel.cs), [`Controls/CodeEditor.cs`](../src/SomaFrameQD.Modules.CodeEditor/Controls/CodeEditor.cs), `.csproj`)
  * [`SomaFrameQD.Modules.ErrorList`](../src/SomaFrameQD.Modules.ErrorList/) ([`ViewModels/ErrorListViewModel.cs`](../src/SomaFrameQD.Modules.ErrorList/ViewModels/ErrorListViewModel.cs), `.csproj`)
  * [`SomaFrameQD.Modules.GraphEditor`](../src/SomaFrameQD.Modules.GraphEditor/) ([`Controls/ConnectorItem.cs`](../src/SomaFrameQD.Modules.GraphEditor/Controls/ConnectorItem.cs), `GraphControl.cs`, `ZoomAndPanControl.cs`, `BezierLine.cs`, `.csproj`)
  * [`SomaFrameQD.Modules.Inspector`](../src/SomaFrameQD.Modules.Inspector/) ([`Win32/NativeMethods.cs`](../src/SomaFrameQD.Modules.Inspector/Win32/NativeMethods.cs), [`Util/ScreenShotUtility.cs`](../src/SomaFrameQD.Modules.Inspector/Util/ScreenShotUtility.cs), [`Controls/ScreenColorPicker.cs`](../src/SomaFrameQD.Modules.Inspector/Controls/ScreenColorPicker.cs), `InspectableObjectBuilder.cs`, `SelectiveUndoEditorBase.cs`, `.csproj`)
  * [`SomaFrameQD.Modules.Output`](../src/SomaFrameQD.Modules.Output/) ([`ViewModels/OutputViewModel.cs`](../src/SomaFrameQD.Modules.Output/ViewModels/OutputViewModel.cs), `OutputWriter.cs`, `.csproj`)
  * [`SomaFrameQD.Modules.PropertyGrid`](../src/SomaFrameQD.Modules.PropertyGrid/) ([`ViewModels/PropertyGridViewModel.cs`](../src/SomaFrameQD.Modules.PropertyGrid/ViewModels/PropertyGridViewModel.cs), `.csproj`)
  * Baseline reference: [`doc_before-net48/06_Standalone_Modules_Reference.md`](../doc_before-net48/06_Standalone_Modules_Reference.md)
* **Required Technical Coverage:**
  * Master summary table of all **7** `0.9.0` `net48` (`AnyCPU` `x64`-clean) NuGet packages matching [`eng/package-contract.json`](../eng/package-contract.json) (showing `0` external NuGet dependencies on `ErrorList`, `GraphEditor`, `Inspector`, `Output`, `PropertyGrid`; native `.NET 4.8` GAC `<Reference Include="System.Drawing" />` on `Inspector`; `AvalonEdit 6.3.1.120` on `CodeEditor`; `Microsoft.CodeAnalysis.CSharp 3.4.0` on `CodeCompiler`) and post-conversion Core SLOC counts.
  * **`CodeCompiler` (`120` Core SLOC):** In-memory Roslyn PE emission (`Assembly.Load(byte[])`), removal of UI-blocking `GC.Collect()`, `DiagnosticSeverity.Hidden` skip + `Location.None` metadata diagnostic support, `///` XML documentation on `.NET Framework 4.8` default-`AppDomain` dynamic assembly retention (`Q7=7A`), and verified `[net48|x64]` coexistence with `AvalonDock` `5.0.1` + `System.Text.Json` `10.0.12`.
  * **`CodeEditor` (`935` Core SLOC):** `AvalonEdit 6.3.1.120`, single `OwnedSearchPanel` (`Ctrl+F`), 16 built-in language definitions + `AppDomain.CurrentDomain.BaseDirectory`-anchored `SyntaxHighlighting/*.xshd` loading (`DtdProcessing.Prohibit`, `XmlResolver = null`), `StringComparison.Ordinal` dirty tracking, `CreationPolicy.NonShared`, `IAsyncLayoutItemStateRestorer`, and normalized `Version=4.0.0.0` `.resx` header.
  * **`ErrorList` (`690` Core SLOC):** Pane-local un-exported `ErrorListToolBar`, `ErrorListItem` click callback, and zero-allocation `FilteredItems` short-circuiting (`ShowErrors && ShowWarnings && ShowMessages`).
  * **`Output` (`323` Core SLOC):** Thread-safe `_syncRoot`-guarded `_stringBuilder`/`Clear()`/`Text` snapshots + single-flight `Dispatcher.BeginInvoke(DispatcherPriority.Background)` coalesced incremental UI flushes (`_uiFlushScheduled`, `_flushedLength`).
  * **`Inspector` (`2,611` Core SLOC):** Native `.NET 4.8` GAC `System.Drawing` binding, fluent `InspectableObjectBuilder` & convention inspectors, `SelectiveUndoEditorBase` / `UndoRedo` integration, and High-DPI multi-monitor physical-pixel eyedropper (`GetSystemMetrics(SM_*VIRTUALSCREEN)`, `ScreenShotUtility.GetVirtualScreenPhysicalBounds()`, `ScreenColorPicker.TrySamplePixel` `>= 0` edge fix, failed-capture `_bitmap` disposal, and locked-desktop `Win32Exception` handling).
  * **`GraphEditor` (`1,593` Core SLOC):** `GraphControl`, `ZoomAndPanControl` (`IScrollInfo`), `ElementItem`, `ConnectionItem`, `BezierLine`, and `ConnectorItem` (`Loaded`/`Unloaded`-scoped `LayoutUpdated` subscription preventing `ContextLayoutManager` GC leaks, `ArrangeOverride` initial sync, and unchanged-`Point` DP write suppression).
  * **`PropertyGrid` (`181` Core SLOC):** `DotNetProjects.Extended.Wpf.Toolkit` `5.0.129` (`lib/net40`) wrapper and `ICommandRerouter` `Edit.Undo`/`Edit.Redo` forwarding to `Shell.ActiveItem`.

### 2. `07_Demo_Application_and_Extension_Patterns.md`
* **Frozen Sources to Verify Against:**
  * [`src/SomaFrameQD.Demo/SomaFrameQD.Demo.csproj`](../src/SomaFrameQD.Demo/SomaFrameQD.Demo.csproj), [`version.json`](../src/SomaFrameQD.Demo/version.json), [`app.config`](../src/SomaFrameQD.Demo/app.config), [`app.manifest`](../src/SomaFrameQD.Demo/app.manifest), [`App.xaml`](../src/SomaFrameQD.Demo/App.xaml), [`DemoAppBootstrapper.cs`](../src/SomaFrameQD.Demo/DemoAppBootstrapper.cs)
  * [`Modules/FilterDesigner/Util/FastBitmap.cs`](../src/SomaFrameQD.Demo/Modules/FilterDesigner/Util/FastBitmap.cs), [`Modules/FilterDesigner/Util/ShaderEffectUtility.cs`](../src/SomaFrameQD.Demo/Modules/FilterDesigner/Util/ShaderEffectUtility.cs), `AddEffect.fx`, `MultiplyEffect.fx`
  * [`Modules/Home/ViewModels/HelixViewModel.cs`](../src/SomaFrameQD.Demo/Modules/Home/ViewModels/HelixViewModel.cs), `HomeViewModel.cs`, plus `Startup`, `Shell`, `SampleBrowser`, `TextEditor`
  * Baseline reference: [`doc_before-net48/07_Demo_Application_and_Extension_Patterns.md`](../doc_before-net48/07_Demo_Application_and_Extension_Patterns.md)
* **Required Technical Coverage:**
  * **Reference Host Configuration (`0.1.0` Alpha):** Split `version.json` (`0.1.0` signaling alpha sample status vs. `0.9.0` framework), `<TargetFramework>net48</TargetFramework>`, `x64` `PE32+` `WinExe` (`Prefer32Bit=false`), automatic `.exe.config` binding redirects, `sku=".NETFramework,Version=v4.8"`, embedded `app.manifest` (`Win7 SP1` → `Win10/11` + System-DPI), single-target `HelixToolkit.Wpf 2.10.0` (`HelixToolkit.Core.Wpf` & obsolete `.pubxml` profiles removed), `AllowUnsafeBlocks=true`, and `JeremyAnsel.HLSL.Targets 1.0.9` (`ps_2_0` `AddEffect.ps` & `MultiplyEffect.ps` `0xFFFF0200`).
  * **Active `DemoAppBootstrapper` (`Q3=3B`, `Q5=5A`):** Clean `net48` subclass wired in `App.xaml` overriding `JitProfileFileName` (`SomaFrameQD.Demo.Startup.net48.jitprofile`) with zero `PublishSingleFile` dead code.
  * **Six Canonical Extension Patterns & Post-Stage-2 Sample Optimizations:**
    1. *Priority MEF `IShell` Override & `ISettingsEditor`:* Demo `ShellViewModel` (`CanCloseAsync` confirmation after `await base.CanCloseAsync`) and `DemoApplicationSettingsViewModel`.
    2. *`Startup` & Conditional `DEBUG` Menu:* Custom window title, status bar, and debugger-gated menu items.
    3. *`SampleBrowser` (`[ImportMany] ISample`):* Open-ended plugin discovery pattern.
    4. *`Home` & Debounced `HelixViewModel` Live Roslyn 3D Scripting (`Q7=7A`):* `DynamicExclusionPredicate` menu demo; `HelixViewModel` `350ms` `DispatcherTimer` debounce, process-wide cached `MetadataReference[]` (`s_defaultReferences`), `_lastCompiledSource` deduplication, `OnActivatedAsync`/`OnDeactivateAsync` `CompositionTarget.Rendering` VSync gating, and `///` XML Roslyn `AppDomain` documentation.
    5. *`FilterDesigner` (`GraphEditor` + `Inspector` + `Toolbox` + `FastBitmap` + `ps_2_0` Shaders):* Finalizer-safe `FastBitmap` (`~FastBitmap()` removed, `_bitmap.CheckAccess()` guard, `BitmapCacheOption.OnLoad`, 64-bit `long` pointer stride arithmetic, `[x, y]` bounds checks) and frozen cached `ShaderEffectUtility` (`ShaderRenderMode.Auto`).
    6. *`TextEditor`:* Custom `PersistedDocument` + `IEditorProvider` implementation.

---

## Plan #3 Verification Gate

1. Confirm `06_Standalone_Modules_Reference.md` and `07_Demo_Application_and_Extension_Patterns.md` exist in `C:\SOFTDEV\SomaFrameQD\doc_after-net48\`.
2. Run a PowerShell verification script confirming:
   * Strict UTF-8 + CRLF formatting;
   * Every relative link to `src/`, `eng/`, `doc/`, and `01`–`07` resolves cleanly on disk;
   * Zero stale references to `System.Drawing.Common`, `HelixToolkit.Core.Wpf`, `FolderProfileNet5.pubxml`, or `GC.Collect()` as active in `CodeCompiler`;
   * Zero modifications to any `.cs`/source file or to `doc_before-net48/*`.
3. Proceed to **[Post-`net48` Documentation Plan #4](./Plan_04_Tests_Build_CI_Master_Index_and_Agent_Routing_Sync.md)** only after Plan #3 passes verification.
