# Post-`net48` Documentation Plan #1 — Core Architecture, Bootstrapper, Primitives & Command System (`doc_after-net48/01`–`03`)

> **Purpose:** Author the first three authoritative **post-`.NET Framework 4.8` (`x64`) retargeting & optimization** technical guides (`01`–`03`) inside `C:\SOFTDEV\SomaFrameQD\doc_after-net48\`, verified 100% line-by-line against the frozen `0.9.0` source tree.  
> **Generated Timestamp:** `2026-11-23T01:53:00-05:00` (`2026-11-23`)  
> **Execution Order:** **Execute Plan #1 FIRST** → [Plan #2](./Plan_02_Shell_Docking_Themes_Rendering_and_Win32_x64_Docs.md) → [Plan #3](./Plan_03_Standalone_Modules_and_Demo_Application_Docs.md) → [Plan #4](./Plan_04_Tests_Build_CI_Master_Index_and_Agent_Routing_Sync.md).  
> **Authoritative Post-Conversion References:** [`AFTER_NET48_LOC_Statistics_2026-11-23_00-20-00.md`](../LOC-statistics_plus_net48-retargeting/AFTER_NET48_LOC_Statistics_2026-11-23_00-20-00.md) | [`changelogs/20261123_CHANGELOG.md`](../changelogs/20261123_CHANGELOG.md) | [`doc/net48-win7-to-win11-x64-deployment-guide.md`](../doc/net48-win7-to-win11-x64-deployment-guide.md) | [`AGENTS.md`](../AGENTS.md)

---

## Four-Plan Context-Window Safety Split (`Implementation-Plans_doc_after-net48`)

To guarantee **zero context-window exhaustion** and **100% code-verified accuracy** across all `489` core source files (`31,518` Core SLOC / `38,771` deliverable SLOC), authoring `doc_after-net48/` is divided into **4 sequential, tightly budgeted Implementation Plans**:

| Plan | Source Inspection Scope (Read-Only) | Context Budget | Output Deliverables |
| :--- | :--- | :--- | :--- |
| **Plan #1 (This Plan)** | `AppBootstrapper.cs`, `DemoAppBootstrapper.cs`, `app.config` (x2), `app.manifest`, `src/Directory.Build.props`, `Framework/*` (Base classes, `Results/*`, `Services/*`, `Behaviors/*`, `Commands/*`, `Menus/*`, `ToolBars/*`) | `~4,800` targeted SLOC read (skipping `.resx` & XAML control templates) | **`doc_after-net48/01`–`03`** (`3` core architecture, lifecycle & command guides) |
| **[Plan #2](./Plan_02_Shell_Docking_Themes_Rendering_and_Win32_x64_Docs.md)** | `Modules/*` (all 8 built-in core modules), `Framework/Themes/*`, `Themes/VS2013/*` (sampled), `Framework/ShaderEffects/*`, `Framework/Controls/*` (`HwndWrapper`, `ClippingHwndHost`), `Framework/Win32/NativeMethods.cs` | `~5,200` targeted SLOC read (sampling VS2013 control XAML) | **`doc_after-net48/04`–`05`** (`2` shell/docking/persistence & themes/shaders/Win32 `x64` guides) |
| **[Plan #3](./Plan_03_Standalone_Modules_and_Demo_Application_Docs.md)** | All 7 `src/SomaFrameQD.Modules.*` projects + `src/SomaFrameQD.Demo` (`FastBitmap`, `HelixViewModel`, `ShaderEffectUtility`, 6 demo modules; skipping `DesignTimeImages.cs`) | `~5,800` targeted SLOC read (skipping `.resx` & base64 image blobs) | **`doc_after-net48/06`–`07`** (`2` standalone NuGet modules & Demo `0.1.0` alpha guides) |
| **[Plan #4](./Plan_04_Tests_Build_CI_Master_Index_and_Agent_Routing_Sync.md)** | `src/SomaFrameQD.Tests` (27 `.cs` files / 174 `[net48\|x64]` executions), `global.json`, `version.json` (x2), `SomaFrameQD.sln`, `eng/*`, `.github/workflows/ci.yml`, `changelogs/*`, `releasenotes/*` | `~3,500` targeted SLOC + scripts/configs | **`doc_after-net48/08`–`09` + `00_INDEX.md`** + routing sync in **`AGENTS.md`** & **`REPO_MAP.md`** + full forensic link/accuracy audit |

---

## Strict Guardrails (Non-Negotiable Across All 4 Plans)

1. **ALL C# & SOURCE CODE IS STRICTLY FROZEN:** Do **NOT** edit any `.cs`, `.xaml`, `.fx`, `.csproj`, `.props`, `.sln`, `.config`, `.manifest`, `.json`, or `.ps1` file in `src/` or `eng/`.
2. **NEVER Modify `doc_before-net48/`:** Preserve [`doc_before-net48/`](../doc_before-net48/00_INDEX.md) 100% untouched (`2026-11-22` timestamps intact) as the permanent pre-conversion snapshot.
3. **Never Copy Pre-Conversion Claims Blindly:** Every guide in `doc_after-net48/` must reflect the **actual post-Stage-2 `.NET Framework 4.8` (`net48`) + `x64` (`Windows 7 SP1 x64` → `Windows 11 x64`) codebase** verified directly from frozen source files on disk—with zero leftover references to `net10.0-windows`, `.NET SDK 10.0.400`, `MahApps.Net10DragHost`, active `PublishSingleFile` hooks, `System.ComponentModel.Composition 6.0.0` / `System.Drawing.Common 6.0.0` NuGet shims, `sku="v4.6.1"`, or 32-bit `GetWindowLong`/`IntPtr.ToInt32()` bugs.
4. **Context-Window Discipline:** Skip `.resx` files (`6,838` lines), `.Designer.cs` files, `DesignTimeImages.cs` (`1,016` lines), and `src/Gemini*` git-deleted index paths. Compare each `doc_before-net48/0X_*.md` guide side-by-side against the frozen on-disk `src/SomaFrameQD/*` files that changed in Stages 1 & 2 so no baseline architectural detail is lost and every post-`net48` optimization is captured.

---

## Exact Deliverables in `C:\SOFTDEV\SomaFrameQD\doc_after-net48\`

### 1. `01_Architecture_and_Bootstrapper_Overview.md`
* **Frozen Sources to Verify Against:**
  * [`src/SomaFrameQD/AppBootstrapper.cs`](../src/SomaFrameQD/AppBootstrapper.cs)
  * [`src/SomaFrameQD.Demo/DemoAppBootstrapper.cs`](../src/SomaFrameQD.Demo/DemoAppBootstrapper.cs) & [`src/SomaFrameQD.Demo/App.xaml`](../src/SomaFrameQD.Demo/App.xaml)
  * [`src/SomaFrameQD/SomaFrameQD.csproj`](../src/SomaFrameQD/SomaFrameQD.csproj) & [`src/Directory.Build.props`](../src/Directory.Build.props)
  * [`src/SomaFrameQD/app.config`](../src/SomaFrameQD/app.config), [`src/SomaFrameQD.Demo/app.config`](../src/SomaFrameQD.Demo/app.config), & [`src/SomaFrameQD.Demo/app.manifest`](../src/SomaFrameQD.Demo/app.manifest)
  * [`src/SomaFrameQD/Framework/IModule.cs`](../src/SomaFrameQD/Framework/IModule.cs), [`ModuleBase.cs`](../src/SomaFrameQD/Framework/ModuleBase.cs), [`Services/ServiceProvider.cs`](../src/SomaFrameQD/Framework/Services/ServiceProvider.cs), [`Services/ExtensionMethods.cs`](../src/SomaFrameQD/Framework/Services/ExtensionMethods.cs)
  * Baseline reference for structure parity: [`doc_before-net48/01_Architecture_and_Bootstrapper_Overview.md`](../doc_before-net48/01_Architecture_and_Bootstrapper_Overview.md)
* **Required Technical Coverage:**
  * **Platform & Dependency Stack (`0.9.0` Framework / `0.1.0` Alpha Demo):** Single-target `.NET Framework 4.8` (`net48`), 64-bit (`x64` `PE32+`, `Prefer32Bit=false`) host executables paired with `AnyCPU` `x64`-clean NuGet library `.dll`s (`Q1=1A`), native `.NET 4.8` GAC `<Reference Include="System.ComponentModel.Composition" />` + `PresentationFramework.Aero` (zero OOB MEF NuGet shim), Caliburn.Micro `5.0.258`, Dirkster.AvalonDock `5.0.1`, MahApps.Metro `2.4.11`, Extended WPF Toolkit `5.0.129`, and updated Mermaid architecture diagram.
  * **CLR 4.8 Runtime Configuration & Embedded Cross-OS Manifest:** `sku=".NETFramework,Version=v4.8"` (exiting `.NET 4.6.1` quirk mode), `app.manifest` (`asInvoker`, all 4 `<supportedOS>` GUIDs from Windows 7 SP1 `{35138b9a-5d96-4fbd-8e2d-a2440225f93a}` through Windows 10/11 `{8e0f7a12-bfb3-4fe8-b9a5-48fd50a15a9a}`, and System-DPI `<dpiAware>true</dpiAware>` + `<dpiAwareness>system</dpiAwareness>`, `Q2=2A`).
  * **`AppBootstrapper` `PreInitialize()` Pipeline & Host Customization Hooks:**
    1. `ConfigureCrossOsNetworkDefaults()` / `ResolveCrossOsSecurityProtocol` (auto-upgrading `SystemDefault` `0` to `Tls12` on Windows 7 SP1 `NT 6.1` while preserving `SystemDefault` on Win8.1/Win10/Win11 for native TLS 1.2 + TLS 1.3).
    2. `ConfigureMultiCoreJit()` (`ProfileOptimization.SetProfileRoot` / `StartProfile` under `%LocalAppData%\<EntryAssemblyName>\JitProfiles`, `DemoAppBootstrapper` override, non-fatal fallback + `.debug.log` diagnostics at `%LocalAppData%\<EntryAssemblyName>\Logs\SomaFrameQD.JitFallback.debug.log` or `%TEMP%`, `Q5=5A`).
    3. `RegisterAssemblyUnificationHandler()` / `TryUnifyStrongNamedAssembly()` (idempotent `AppDomain.CurrentDomain.AssemblyResolve` unifying strong-named version skew where `availableVersion >= requestedVersion`).
    4. Overridable virtual properties (`EnableMultiCoreJit`, `JitProfileDirectory`, `JitProfileFileName`, `JitFallbackDebugLogFilePath`, `ProbingDirectoryPath`, `ShouldInspectAssemblyFile`) and clean removal of `.NET 5+` `PublishSingleFile` APIs (`Q3=3B`).
  * **Two-Tier MEF Composition & Filtered Assembly Probing (`Q4=4A`):** `CompositionContainer(priorityProvider, mainProvider)` override rules, `AppDomain.CurrentDomain.BaseDirectory` probing, `ShouldInspectAssemblyFile` vendor/BCL skip-filter, `BadImageFormatException` native C/C++ DLL tolerance, `using`-disposed `AssemblyCatalog` probes, `IoC` / `IServiceProvider` bridges, deterministic async startup (`DisplayRootViewForAsync<IMainWindow>` → `ShellActivationTask` → `InitializationTask`), `IModule` lifecycle, and `README.md` drift warnings (`SomaFrameQDWpf` vs. `SomaFrameQD`, `http://schemas.timjones.io/SomaFrameQD`).

### 2. `02_Core_Framework_Primitives_and_Coroutines.md`
* **Frozen Sources to Verify Against:**
  * [`src/SomaFrameQD/Framework/`](../src/SomaFrameQD/Framework/) (`IWindow.cs`, `WindowBase.cs`, `ILayoutItem.cs`, `LayoutItemBase.cs`, `IDocument.cs`, `Document.cs`, `IPersistedDocument.cs`, `PersistedDocument.cs`, `ITool.cs`, `Tool.cs`, `IAsyncLayoutItemStateRestorer.cs`, `AsyncCommand.cs`, `RelayCommand.cs`, `VisualTreeUtility.cs`)
  * [`src/SomaFrameQD/Framework/Results/`](../src/SomaFrameQD/Framework/Results/) (`Show.cs`, `IOpenResult.cs`, `OpenResultBase.cs`, `OpenDocumentResult.cs`, `ShowToolResult.cs`, `ShowWindowResult.cs`, `ShowDialogResult.cs`, `ShowCommonDialogResult.cs`, `LambdaResult.cs`)
  * [`src/SomaFrameQD/Framework/Services/`](../src/SomaFrameQD/Framework/Services/) (`IEditorProvider.cs`, `EditorFileType.cs`, `IEditorOpeningService.cs`, `EditorOpeningService.cs`, `IResourceManager.cs`, `ResourceManager.cs`)
  * [`src/SomaFrameQD/Framework/Behaviors/`](../src/SomaFrameQD/Framework/Behaviors/) (**especially updated [`WindowOptionsBehavior.cs`](../src/SomaFrameQD/Framework/Behaviors/WindowOptionsBehavior.cs)**), [`Markup/TranslateExtension.cs`](../src/SomaFrameQD/Framework/Markup/TranslateExtension.cs), [`Threading/TaskUtility.cs`](../src/SomaFrameQD/Framework/Threading/TaskUtility.cs), [`Utils/ItemsControlUtility.cs`](../src/SomaFrameQD/Framework/Utils/ItemsControlUtility.cs)
  * Baseline reference: [`doc_before-net48/02_Core_Framework_Primitives_and_Coroutines.md`](../doc_before-net48/02_Core_Framework_Primitives_and_Coroutines.md)
* **Required Technical Coverage:**
  * Screen/docking class hierarchy (`WindowBase`, `LayoutItemBase`, `Document`, `PersistedDocument`, `Tool`), dirty close-guard matrix (`Save` / `Discard` / `Cancel`), deferred `UndoRedoManager.Dispose()` strictly on `close: true`, and shell-owned `Tool.TryCloseAsync` → `IShell.CloseToolAsync`.
  * View-ready `EditorOpeningService` (`ViewAttached` + `FrameworkElement.Loaded` barrier before `IEditorProvider.Open`/`New`) and `[PartCreationPolicy(CreationPolicy.NonShared)]` requirement.
  * Caliburn.Micro 5 `OpenResultBase` / `Show.*` coroutine completion semantics (`OpenDocumentResult` vs. `ShowToolResult`/`ShowWindowResult` vs. `ShowDialogResult`).
  * Localization (`TranslateExtension`, 7-locale satellite `.resx`), `ResourceManager`, weak settings event managers, and **post-Stage-2 64-bit `WindowOptionsBehavior`** (`GetWindowLongPtr`/`SetWindowLongPtr` `IntPtr` style masks, `Window.SourceInitialized` deferral when `Handle == IntPtr.Zero`, `OnDetaching()` cleanup, and `//` HWND wrapper rationale comments).

### 3. `03_Commands_Menus_ToolBars_and_KeyGestures.md`
* **Frozen Sources to Verify Against:**
  * [`src/SomaFrameQD/Framework/Commands/`](../src/SomaFrameQD/Framework/Commands/) (19 files), [`Menus/`](../src/SomaFrameQD/Framework/Menus/) (10 files), [`ToolBars/`](../src/SomaFrameQD/Framework/ToolBars/) (7 files), [`Services/IInputManager.cs`](../src/SomaFrameQD/Framework/Services/IInputManager.cs), [`Services/InputManager.cs`](../src/SomaFrameQD/Framework/Services/InputManager.cs)
  * Baseline reference: [`doc_before-net48/03_Commands_Menus_ToolBars_and_KeyGestures.md`](../doc_before-net48/03_Commands_Menus_ToolBars_and_KeyGestures.md)
* **Required Technical Coverage:**
  * Declarative `[CommandDefinition]` / `CommandListDefinition`, `Command`, `TargetableCommand`, and `ICommandService`.
  * Three-stage `CommandRouter` resolution (`ActiveLayoutItem` → `ActiveItem` → global MEF `[CommandHandler]` with priority-catalog override semantics) and `ICommandRerouter` (`CodeEditorViewModel`, `PropertyGridViewModel`).
  * Declarative `MenuBuilder` & `ToolBarBuilder` hierarchies, static `Exclude*` exports & `DynamicExclusionPredicate`, `CommandKeyboardShortcut` / `CommandKeyGestureService`, and `InputManager` under `net48` `x64`.

---

## Plan #1 Verification Gate

1. Confirm `01_Architecture_and_Bootstrapper_Overview.md`, `02_Core_Framework_Primitives_and_Coroutines.md`, and `03_Commands_Menus_ToolBars_and_KeyGestures.md` exist in `C:\SOFTDEV\SomaFrameQD\doc_after-net48\`.
2. Run a PowerShell verification script confirming:
   * Strict UTF-8 (no BOM / no Mojibake C1 control chars) + CRLF line endings;
   * Every relative link (`../src/...`, `../doc/...`, sibling `01`–`03` links) resolves cleanly on disk (note: forward links to `04`–`09` should either be plain code spans until created or verified in Plan #4);
   * Zero stale references to `net10.0-windows`, `MahApps.Net10DragHost`, `sku="v4.6.1"`, or active `PublishSingleFile`;
   * Zero modifications to any `.cs`/source file or to `doc_before-net48/*`.
3. Proceed to **[Post-`net48` Documentation Plan #2](./Plan_02_Shell_Docking_Themes_Rendering_and_Win32_x64_Docs.md)** only after Plan #1 passes verification.
