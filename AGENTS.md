# `AGENTS.md` — AI Coding Agent Operating Manual for `SomaFrameQD`

> **Start Here:** This file is the authoritative entry point for AI coding agents working in `C:\SOFTDEV\SomaFrameQD`. Follow these rules to avoid context-window exhaustion, respect non-negotiable architectural invariants, and obey the staged `.NET Framework 4.8` conversion roadmap.

---

## 1. Quick Orientation & Current Repository State

### What `SomaFrameQD` Is
`SomaFrameQD` is a modular WPF IDE/workbench application framework (derived from *Gemini* and modernized to **Caliburn.Micro `5.0.258`**, **Dirkster.AvalonDock `5.0.1`**, **MahApps.Metro `2.4.11`**, **AvalonEdit `6.3.1.120`**, and **Extended WPF Toolkit `5.0.129`**). Host applications and plugins compose documents, dockable tool windows, declarative menus, toolbars, keyboard shortcuts, settings dialogs, and themes via **MEF (`System.ComponentModel.Composition`)**.

### Current Repository State (`.NET Framework 4.8` `x64` Retargeting & Optimization Complete)
- **Current On-Disk State (Stages 1 & 2 Complete):** All 10 solution projects plus `Caliburn5PackageHost` single-target `<TargetFramework>net48</TargetFramework>` (`<LangVersion>12.0</LangVersion>`, `<DebugType>portable</DebugType>`, `<Deterministic>true</Deterministic>`) under `.NET SDK 8.0.400` (`rollForward: "latestPatch"` in [`global.json`](./global.json)). Executables and tests (`SomaFrameQD.Demo`, `SomaFrameQD.Tests`, `Caliburn5PackageHost`) compile as **64-bit (`x64` `PE32+`, `<Prefer32Bit>false</Prefer32Bit>`)** with automatic `.exe.config` binding redirects, while all 8 published NuGet library `.dll`s remain `AnyCPU` `x64`-clean against native `.NET Framework 4.8` GAC references (`System.ComponentModel.Composition`, `System.Drawing`, `PresentationFramework.Aero`).
- **Supported OS Continuum:** **Windows 7 SP1 64-bit (`x64`) through Windows 8.1, Windows 10, and Windows 11 `x64`** (see [`doc/net48-win7-to-win11-x64-deployment-guide.md`](./doc/net48-win7-to-win11-x64-deployment-guide.md)).
- **Completed Two-Stage Conversion ([`TODO.md`](./TODO.md)):**
  1. **Stage 1 — Simple Retargeting to `net48`:** **100% COMPLETED & VERIFIED** ([`Implementation-Plans_net48-retargeting-ONLY/`](./Implementation-Plans_net48-retargeting-ONLY/)).
  2. **Stage 2 — Optimization for `net48` + Windows 7 SP1 → Windows 11 `x64`:** **100% COMPLETED & VERIFIED** across 8 sequential plans ([`Implementation-Plans_net48-optimizations/`](./Implementation-Plans_net48-optimizations/)).

### Critical Git Working-Tree Warning (`Gemini` → `SomaFrameQD` Rename)
The repository currently has **uncommitted rename changes** from `Gemini` to `SomaFrameQD`: `git status` reports deleted `src/Gemini*` and `doc/gemini-*` paths in the Git index alongside untracked `src/SomaFrameQD*` directories on disk.
- **Never** rely on `git ls-files --cached` alone to list files (it returns deleted `src/Gemini*` entries). Always verify paths on disk (`src/SomaFrameQD*`).
- **Never** attempt to read or restore deleted `src/Gemini*` or `doc/gemini-*` paths.

---

## 2. Context-Window Survival Protocol

The deliverable repository spans **`38,771` SLOC (`49,032` lines)** across `576` text files (`489` core source & UI files = `31,518` Core SLOC; pre-conversion baseline: `36,671` SLOC / `46,373` lines across `575` text files). To prevent context-window exhaustion:

1. **Navigate via [`REPO_MAP.md`](./REPO_MAP.md) First:** Use [`REPO_MAP.md`](./REPO_MAP.md) to look up exact file paths, MEF `[Export]`/`[Import]` contracts, command definitions, shortcuts, NuGet dependencies, and mapped unit tests in one read.
2. **Load Only One Topic Guide from [`doc_after-net48/`](./doc_after-net48/00_INDEX.md):** Pick the single post-conversion guide (`01`–`09`) matching your task from Section 5 below instead of reading raw source directories broadly.
3. **Never Bulk-Read High-Token Noise Files Unless Directly Editing Them:**
   - **Skip `.resx` & `.Designer.cs`:** `43` `.resx` files consume `6,838` lines across 7 locales (`en`, `de`, `fr`, `ko`, `ru`, `zh-Hans`, `zh-TW`), plus `945` lines in `17` `.Designer.cs` files. Read only the neutral `Properties/Resources.resx` keys you actively need.
   - **Skip Bulk Control Templates:** `src/SomaFrameQD/Themes/VS2013/Controls/*.xaml` contains ~2,600+ lines of standard control templates; inspect only the specific control file you are changing.
   - **Skip Base64 Sample Blobs:** Do not read the literal arrays inside `src/SomaFrameQD.Demo/Modules/FilterDesigner/Design/DesignTimeImages.cs` (`1,016` lines).
   - **Sample Large Test Files Selectively:** `AvalonDockPersistenceTests.cs` (`1,160` lines), `ShellViewModelTests.cs` (`1,122` lines), and `LayoutItemStatePersisterTests.cs` (`836` lines) are indexed in [`doc_after-net48/08_Test_Suite_and_Characterization_Harness.md`](./doc_after-net48/08_Test_Suite_and_Characterization_Harness.md); read targeted line ranges around the test method under investigation.
4. **Preserve [`doc_before-net48/`](./doc_before-net48/00_INDEX.md) as Immutable:** `doc_before-net48/` is the permanent *"Before `.NET 4.8` Retargeting & Optimization"* snapshot (`2026-11-22`). Do **not** overwrite those files; keep active architectural documentation in [`doc_after-net48/`](./doc_after-net48/00_INDEX.md).

---

## 3. Build, Test, Pack & Validation Commands

Run commands from the repository root (`C:\SOFTDEV\SomaFrameQD`):

```powershell
# 1. Deterministic NuGet restore (ignores machine-wide feeds via src\NuGet.config)
dotnet restore src\SomaFrameQD.sln --configfile src\NuGet.config

# 2. Build entire solution in Release (net48)
dotnet build src\SomaFrameQD.sln --configuration Release --no-restore

# 3. Run 64-bit MSTest characterization + optimization suite (174 executions on [net48|x64])
dotnet test src\SomaFrameQD.Tests\SomaFrameQD.Tests.csproj --configuration Release --framework net48 --no-build --no-restore

# 4. Pack all 8 NuGet libraries (.nupkg + .snupkg)
dotnet pack src\SomaFrameQD.sln --configuration Release --no-build --no-restore --output build\packages

# 5. Run declarative artifact, .nuspec, shader, lockfile & consumer smoke validation
#    (Compatible with both Windows PowerShell 5.1 locally and pwsh 7 in CI)
powershell -NoProfile -ExecutionPolicy Bypass -File .\eng\validate-artifacts.ps1
```

> [!IMPORTANT]
> **Lockstep Packaging Gate (`eng/package-contract.json`):**  
> [`eng/validate-artifacts.ps1`](./eng/validate-artifacts.ps1) checks compiled Demo HLSL shaders (`AddEffect.ps`, `MultiplyEffect.ps`), all 8 `.nupkg`/`.snupkg` archives, `.nuspec` dependency groups, `Caliburn5PackageHost`'s `project.assets.json` resolved DLL paths, and live runtime execution against [`eng/package-contract.json`](./eng/package-contract.json). Any TFM or NuGet dependency change in packable `src/` projects **must** update `eng/package-contract.json` and [`eng/compatibility/Caliburn5PackageHost/Caliburn5PackageHost.csproj`](./eng/compatibility/Caliburn5PackageHost/Caliburn5PackageHost.csproj) in lockstep.

---

## 4. Ten Non-Negotiable Architectural Invariants

1. **Two-Tier MEF Priority Resolution (`AppBootstrapper.cs`):**
   - `AppBootstrapper` builds `new CompositionContainer(priorityProvider, mainProvider)`, where `priorityProvider` wraps `SelectAssemblies()` (the entry assembly) and `mainProvider` wraps `DirectoryCatalog`-discovered assemblies.
   - Single-instance `IoC.Get<T>()` calls (`exports.First().Value`) and `CommandRouter` global `[CommandHandler]` registration let host applications override built-in contracts (such as `IShell`) without forking `SomaFrameQD`. Missing imports must continue to fail loudly—never swallow cardinality errors with silent `null` fallbacks.
2. **`CreationPolicy.NonShared` on Documents, Settings Editors & Modal Dialogs:**
   - MEF defaults exports to `CreationPolicy.Shared` (singleton). Every multi-instance `IDocument` (`CodeEditorViewModel`, `GraphViewModel`, custom editors), `ISettingsEditor`/`ISettingsEditorAsync`, and `SettingsViewModel` **must** declare `[PartCreationPolicy(CreationPolicy.NonShared)]`.
3. **Caliburn.Micro 5 Asynchronous Lifecycle Only:**
   - All screens/conductors use `OnInitializedAsync`, `OnActivateAsync`, `OnDeactivateAsync(bool close, CancellationToken)`, `CanCloseAsync`, and `AsyncEventHandler<ActivationEventArgs>` / `AsyncEventHandler<DeactivationEventArgs>` (see [`doc/caliburn-5-migration.md`](./doc/caliburn-5-migration.md)).
   - Every WPF test touching UI controls or screens must run on STA (`[STATestClass]` / `[STATestMethod]`) with `<UseSTASynchronizationContext>true</UseSTASynchronizationContext>`.
4. **Serialized Shell Transitions via `TransitionCoordinator`:**
   - `ShellViewModel` serializes `OpenDocumentAsync`, `CloseDocumentAsync`, `ShowToolAsync`, `CloseToolAsync`, layout restore, and `ActiveLayoutItem` updates through `TransitionCoordinator` (FIFO across independent callers; inline via `AsyncLocal` for nested causal transitions; echo-suppressed via `_activeLayoutItemEcho`). Never mutate `ShellViewModel.Items` or tool visibility behind `TransitionCoordinator`'s back.
5. **Shell-Owned Tool Close Semantics (`Tool.TryCloseAsync` → `IShell.CloseToolAsync`):**
   - Tools in `ShellViewModel.Tools` are **not** parented to a Caliburn conductor. Closing a tool calls `IShell.CloseToolAsync(tool)`, which checks `CanCloseAsync`, deactivates with `close: true`, sets `IsVisible = false`, and **leaves the tool in `Shell.Tools`** so `ShowToolAsync` can reopen it.
   - Conversely, `Document` disposes its `UndoRedoManager` **only** inside `OnDeactivateAsync(close: true)`—never on tab switches or vetoed/faulted closes.
6. **View-Ready Editor Opening (`EditorOpeningService`):**
   - `EditorOpeningService` subscribes to `ViewAttached` + `FrameworkElement.Loaded` **before** calling `Shell.OpenDocumentAsync(document)` and awaits view readiness before calling `IEditorProvider.Open` or `New`. Preserve this ordering whenever touching editor creation or startup document loading.
7. **Transactional `ApplicationState.bin` Persistence & Append-Only Test Fixtures:**
   - `LayoutItemStatePersister` stages writes via `<StateFile>.tmp` and `<StateFile>.bak` (`File.Replace`), isolates per-item binary payload lengths so failed items degrade to `Partial` rather than corrupting the stream, and awaits `IAsyncLayoutItemStateRestorer.RestoreStateAsync()` after closing the file stream.
   - **Never overwrite, reformat, or regenerate** [`src/SomaFrameQD.Tests/TestData/AvalonDock460/`](./src/SomaFrameQD.Tests/TestData/AvalonDock460/), [`AvalonDock4741/`](./src/SomaFrameQD.Tests/TestData/AvalonDock4741/), or [`AvalonDock501/`](./src/SomaFrameQD.Tests/TestData/AvalonDock501/)—their SHA-256 hashes are asserted in `AvalonDockPersistenceTests`.
8. **Theme Lookup Key & AvalonDock 5 Dictionary Ordering (`ThemeManager`):**
   - `ThemeManager.SetCurrentTheme(string name)` matches `theme.GetType().Name` (`"LightTheme"`, `"BlueTheme"`, `"DarkTheme"`), **never** the localized `ITheme.Name` display string.
   - Built-in themes prepend AvalonDock 5's code-generated `DictionaryTheme` **before** `SomaFrameQD` XAML URIs and roll back both `Application` and `MainWindow` resource dictionaries atomically if any URI fails to load.
9. **Split `Nerdbank.GitVersioning` (`version.json`):**
   - Never add `<Version>`, `<PackageVersion>`, `<AssemblyVersion>`, or `[assembly: AssemblyVersion]` to `.csproj` or `AssemblyInfo.cs`. Root [`version.json`](./version.json) (`0.9.0`) stamps all 8 NuGet packages; [`src/SomaFrameQD.Demo/version.json`](./src/SomaFrameQD.Demo/version.json) (`0.1.0`) versions Demo independently.
10. **Package ID vs. Namespace & Stale `README.md` Pitfalls:**
    - Core's NuGet Package ID is **`SomaFrameQDWpf`**, whereas its assembly name, root CLR namespace, and pack URI authority (`pack://application:,,,/SomaFrameQD;component/...`) are **`SomaFrameQD`**.
    - Ignore legacy [`README.md`](./README.md) snippets showing `http://schemas.timjones.tw/SomaFrameQD` (actual XAML namespace is **`http://schemas.timjones.io/SomaFrameQD`**), synchronous `Shell.OpenDocument`/`ShowTool` (actual `IShell` is async), or 7 core modules (there are **8**, including `Settings`).

---

## 5. Task-to-Documentation Routing Table

Read **only** the document(s) needed for your immediate task:

| If Your Task Involves... | Read First |
| :--- | :--- |
| Finding file paths, MEF exports/imports, commands, shortcuts, or test mappings | [`REPO_MAP.md`](./REPO_MAP.md) |
| Overview of the live post-`net48` `x64` architecture (`0.9.0` framework / `0.1.0` alpha Demo) | [`doc_after-net48/00_INDEX.md`](./doc_after-net48/00_INDEX.md) |
| `AppBootstrapper`, MEF catalogs, `IoC`, Multi-Core JIT, `app.manifest`, startup (`InitializationTask`), or `IModule` | [`doc_after-net48/01_Architecture_and_Bootstrapper_Overview.md`](./doc_after-net48/01_Architecture_and_Bootstrapper_Overview.md) |
| `Document`, `PersistedDocument`, `Tool`, `WindowBase`, `EditorOpeningService`, or `Show.*` `IResult` coroutines | [`doc_after-net48/02_Core_Framework_Primitives_and_Coroutines.md`](./doc_after-net48/02_Core_Framework_Primitives_and_Coroutines.md) + [`doc/caliburn-5-migration.md`](./doc/caliburn-5-migration.md) |
| Commands, `CommandRouter`, menus, toolbars, exclusions, or keyboard shortcuts | [`doc_after-net48/03_Commands_Menus_ToolBars_and_KeyGestures.md`](./doc_after-net48/03_Commands_Menus_ToolBars_and_KeyGestures.md) |
| `ShellViewModel`, `TransitionCoordinator`, AvalonDock 5, `ApplicationState.bin`, `Toolbox`, `UndoRedo`, or `Settings` | [`doc_after-net48/04_Shell_Docking_Persistence_and_BuiltIn_Modules.md`](./doc_after-net48/04_Shell_Docking_Persistence_and_BuiltIn_Modules.md) |
| VS2013 themes (`po:Freeze`), custom controls, `ps_2_0` shaders (`GrayscaleEffect`), `HwndWrapper`, or Win32/`x64` P/Invoke | [`doc_after-net48/05_Themes_Rendering_and_Win32_Interop.md`](./doc_after-net48/05_Themes_Rendering_and_Win32_Interop.md) |
| Any of the 7 `SomaFrameQD.Modules.*` packages (`CodeCompiler`, `CodeEditor`, `ErrorList`, `GraphEditor`, `Inspector`, `Output`, `PropertyGrid`) | [`doc_after-net48/06_Standalone_Modules_Reference.md`](./doc_after-net48/06_Standalone_Modules_Reference.md) |
| `SomaFrameQD.Demo` (`FilterDesigner`, `HelixViewModel`, `FastBitmap`, HLSL `.fx` shaders, `DemoAppBootstrapper`) | [`doc_after-net48/07_Demo_Application_and_Extension_Patterns.md`](./doc_after-net48/07_Demo_Application_and_Extension_Patterns.md) |
| Writing/debugging `[net48\|x64]` tests in `SomaFrameQD.Tests`, STA pumping, `IoCOverrideScope`, or `TestData` fixtures | [`doc_after-net48/08_Test_Suite_and_Characterization_Harness.md`](./doc_after-net48/08_Test_Suite_and_Characterization_Harness.md) |
| `.csproj` TFMs, `global.json`, NBGV `version.json`, `eng/package-contract.json`, `validate-artifacts.ps1`, or GitHub Actions CI | [`doc_after-net48/09_Build_Versioning_Packaging_and_CI_After_NET48.md`](./doc_after-net48/09_Build_Versioning_Packaging_and_CI_After_NET48.md) |
| Auditing `.NET Framework 4.8` retargeting (Stage 1), `x64` optimization (Stage 2), or Win7–Win11 deployment | [`TODO.md`](./TODO.md) + [`doc/net48-win7-to-win11-x64-deployment-guide.md`](./doc/net48-win7-to-win11-x64-deployment-guide.md) + [Simple_Retargeting_vs_Optimization_NET48_Win7_x64_2026-11-22_17-36-26.md](./LOC-statistics_plus_net48-retargeting/Simple_Retargeting_vs_Optimization_NET48_Win7_x64_2026-11-22_17-36-26.md) |
| Checking per-module file counts & SLOC budgets (Post-Conversion vs. Pre-Conversion Baseline) | [`AFTER_NET48_LOC_Statistics_2026-11-23_00-20-00.md`](./LOC-statistics_plus_net48-retargeting/AFTER_NET48_LOC_Statistics_2026-11-23_00-20-00.md) + [`BEFORE_NET48_LOC_Statistics_2026-11-22_17-23-51.md`](./LOC-statistics_plus_net48-retargeting/BEFORE_NET48_LOC_Statistics_2026-11-22_17-23-51.md) |
| Release changelogs (`YYYYMMDD_CHANGELOG.md`) or release notes (`YYYYMMDD_RELEASE_NOTES_v*.md`) | [`changelogs/20261123_CHANGELOG.md`](./changelogs/20261123_CHANGELOG.md) + [`releasenotes/20261123_RELEASE_NOTES_v0.9.0.md`](./releasenotes/20261123_RELEASE_NOTES_v0.9.0.md) |
| Comparing against the immutable pre-`net48` (`2026-11-22`) architectural baseline | [`doc_before-net48/00_INDEX.md`](./doc_before-net48/00_INDEX.md) |
| Code age / git blame provenance (2012–2021 legacy vs. 2026 modernization) | [`VNIR.md`](./VNIR.md) |
