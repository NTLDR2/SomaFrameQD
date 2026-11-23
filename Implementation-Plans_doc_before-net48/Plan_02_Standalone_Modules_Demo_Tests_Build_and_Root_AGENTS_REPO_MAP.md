# Implementation Plan #2 — Standalone Modules, Demo, Tests, Build/CI Docs (`doc_before-net48/06`–`09` + `00_INDEX.md`) + Root `REPO_MAP.md` & `AGENTS.md`

> **Purpose:** Complete the **pre-`net48`-conversion** technical documentation in `C:\SOFTDEV\SomaFrameQD\doc_before-net48\` (`06`–`09` + master `00_INDEX.md`) and generate the root AI entry-point files (`C:\SOFTDEV\SomaFrameQD\REPO_MAP.md` and `C:\SOFTDEV\SomaFrameQD\AGENTS.md`).  
> **Generated Timestamp:** `2026-11-22T18:01:29-05:00` (`2026-11-22 18:01:29 -05:00`)  
> **Prerequisite:** **[Implementation Plan #1](./Plan_01_Core_Framework_Shell_and_BuiltIn_Modules_Docs.md) MUST be 100% completed first** (`doc_before-net48/01`–`05` verified on disk).  
> **LOC Baseline Reference:** [BEFORE_NET48_LOC_Statistics_2026-11-22_17-23-51.md](../LOC-statistics_plus_net48-retargeting/BEFORE_NET48_LOC_Statistics_2026-11-22_17-23-51.md)

---

## Context-Window Split Overview (Plan #1 vs. Plan #2)

| Plan | Source Inspection Scope | Context Budget | Output Deliverables |
| :--- | :--- | :--- | :--- |
| **[Plan #1](./Plan_01_Core_Framework_Shell_and_BuiltIn_Modules_Docs.md)** | Core `src/SomaFrameQD` (`Framework/*`, `Modules/*`, `Themes/*`, `AppBootstrapper.cs`) + [doc/caliburn-5-migration.md](../doc/caliburn-5-migration.md) | `~12,365` Core SLOC (skipping `.resx` XML; sampling XAML styles selectively) | **5 Core Architecture & Shell Guides** (`01`–`05`) in `C:\SOFTDEV\SomaFrameQD\doc_before-net48\` |
| **Plan #2 (This Plan)** | 7 `src/SomaFrameQD.Modules.*`, `src/SomaFrameQD.Demo`, `src/SomaFrameQD.Tests`, `eng/*`, [.github/workflows/ci.yml](../.github/workflows/ci.yml), [VNIR.md](../VNIR.md) | `~17,173` Core SLOC + `~1,200` build/CI SLOC (skipping `.resx` & large test boilerplate where summarized) | **4 Module/Demo/Test/Build Guides** (`06`–`09`) + `00_INDEX.md` in `C:\SOFTDEV\SomaFrameQD\doc_before-net48\` + root **`REPO_MAP.md`** & **`AGENTS.md`** |

> **Why `REPO_MAP.md` and `AGENTS.md` are generated at the end of Plan #2:** Both root files act as the master routing layer for AI coding agents. Writing them in Plan #2 guarantees every cross-link to `doc_before-net48/00_INDEX.md`–`09_*.md`, every module API summary, and every test/build command is verified against completed documentation rather than placeholders.

---

## Prerequisite & Strict Constraints

* **Prerequisite Check:** Before starting, confirm `01_Architecture_and_Bootstrapper_Overview.md` through `05_Themes_Rendering_and_Win32_Interop.md` exist in `C:\SOFTDEV\SomaFrameQD\doc_before-net48\`.
* **Hard Constraint — DO NOT EDIT CODE:** Read-only on all source code, `.csproj`, config, and existing build files.
  * Write `00_INDEX.md` and `06`–`09` inside `C:\SOFTDEV\SomaFrameQD\doc_before-net48\`.
  * Write `REPO_MAP.md` and `AGENTS.md` in the repository root `C:\SOFTDEV\SomaFrameQD\`.
* **Context-Window Guardrails:**
  * Inspect standalone modules sequentially.
  * Skip `.resx` files (`6,838` lines), `.Designer.cs` files, and large base64/image literal arrays (`src/SomaFrameQD.Demo/Modules/FilterDesigner/Design/DesignTimeImages.cs`, `1,016` lines).
  * Do **not** read `src/Gemini*` or `doc/gemini-*` git-deleted paths.
  * For large test files (`AvalonDockPersistenceTests.cs`, `ShellViewModelTests.cs`, `LayoutItemStatePersisterTests.cs`), extract test classes, `[TestMethod]` names, assertions, and helper infrastructure (`StaDispatcher`, `IoCOverrideScope`, `ResultTestDoubles`) efficiently rather than reading repetitive setup blocks verbatim.

---

## Deliverables to Create

### Part A — Remaining Baseline Docs in `C:\SOFTDEV\SomaFrameQD\doc_before-net48\`

#### 1. `06_Standalone_Modules_Reference.md`
* **Sources to inspect:** All 7 `src/SomaFrameQD.Modules.*` projects:
  * `SomaFrameQD.Modules.CodeCompiler` (`ICodeCompiler.cs`, `CodeCompiler.cs`, `.csproj`)
  * `SomaFrameQD.Modules.CodeEditor` (`ILanguageDefinition.cs`, `DefaultLanguageDefinition.cs`, `LanguageDefinitionManager.cs`, `EditorProvider.cs`, `CodeEditorToolBarDefinitions.cs`, `Controls/CodeEditor.cs`, `ViewModels/*`, `Views/*`, `Commands/*`, `.csproj`)
  * `SomaFrameQD.Modules.ErrorList` (`IErrorList.cs`, `ErrorListItem.cs`, `ErrorListItemType.cs`, `MenuDefinitions.cs`, `ToolBarDefinitions.cs`, `Commands/*`, `ViewModels/*`, `Views/*`, `.csproj`)
  * `SomaFrameQD.Modules.GraphEditor` (`Module.cs`, `Controls/*` [`GraphControl`, `ZoomAndPanControl`, `ZoomAndPanControl_IScrollInfo`, `ElementItemsControl`, `ElementItem`, `ConnectionItemsControl`, `ConnectionItem`, `ConnectorItem`, `BezierLine`, drag event args], `Themes/Generic.xaml`, `.csproj`)
  * `SomaFrameQD.Modules.Inspector` (`IInspectorTool.cs`, `IInspectableObject.cs`, `InspectableObject.cs`, `InspectableObjectBuilder.cs`, `InspectorBuilder.cs`, `BoundPropertyDescriptor.cs`, `Controls/*` [`InspectorGrid`, `SimpleGridSplitter`, `ScreenColorPicker`, `NumericTextBox`], `Conventions/*`, `Inspectors/*`, `Util/*`, `Win32/NativeMethods.cs`, `.csproj`)
  * `SomaFrameQD.Modules.Output` (`IOutput.cs`, `ViewModels/OutputViewModel.cs`, `ViewModels/OutputWriter.cs`, `Commands/*`, `Views/*`, `.csproj`)
  * `SomaFrameQD.Modules.PropertyGrid` (`IPropertyGrid.cs`, `ViewModels/PropertyGridViewModel.cs`, `Commands/*`, `Views/*`, `.csproj`)
* **Required Contents:** Per-module technical specification (NuGet package ID, target frameworks & NuGet dependencies in pre-`net48` state, MEF exports/imports, public interfaces, commands/menus/toolbars, custom WPF controls, `UndoRedo` integration in `Inspector`, and extension hooks).

#### 2. `07_Demo_Application_and_Extension_Patterns.md`
* **Sources to inspect:**
  * `src/SomaFrameQD.Demo/App.xaml`, `App.xaml.cs`, `DemoAppBootstrapper.cs`, `SomaFrameQD.Demo.csproj`, `app.config`, `version.json`, `Properties/PublishProfiles/*`
  * All 6 demo modules under `src/SomaFrameQD.Demo/Modules/*`: `Startup`, `Shell`, `SampleBrowser`, `Home` (`HomeViewModel`, `HelixViewModel`, `IDemoScript`), `FilterDesigner` (excluding `DesignTimeImages.cs` array body), `TextEditor`
  * Historical/age context from `VNIR.md`
* **Required Contents:** How client applications extend SomaFrameQD in practice: `DemoAppBootstrapper` (`PublishSingleFile` overrides), custom `ShellViewModel` exit confirmation & `ISettingsEditor`, `ISample` plugin browser, `Home` + `HelixViewModel` live Roslyn C# scripting (`CodeCompiler` + `HelixToolkit.Wpf` / `HelixToolkit.Core.Wpf` dual-TFM split), `FilterDesigner` end-to-end node graph + `Inspector` + `Toolbox` + `unsafe` `FastBitmap` + HLSL `.fx` (`JeremyAnsel.HLSL.Targets` `ps_2_0` shaders), and `TextEditor` `PersistedDocument` sample.

#### 3. `08_Test_Suite_and_Characterization_Harness.md`
* **Sources to inspect:**
  * `src/SomaFrameQD.Tests/SomaFrameQD.Tests.csproj`
  * `src/SomaFrameQD.Tests/TestInfrastructure/*` (`StaDispatcher.cs`, `IoCOverrideScope.cs`)
  * `src/SomaFrameQD.Tests/Composition/*`, `Framework/*`, `Modules/*`, `TestData/*`
* **Required Contents:** Inventory and guide to all 23 test files (`8,483` lines) under `MSTest.Sdk` `4.1.0`: STA thread pumping (`StaDispatcher`), temporary IoC overrides (`IoCOverrideScope`), MEF composition convention tests, coroutine `IResult` completion matrix tests (`ResultTestDoubles`), document/tool/window lifecycle & close-guard tests, AvalonDock 5 persistence/recovery/theme tests and historical binary fixtures (`AvalonDock460`, `AvalonDock4741`, `AvalonDock501`), plus AvalonEdit and Extended WPF Toolkit compatibility tests.

#### 4. `09_Build_Versioning_Packaging_and_CI_Before_NET48.md`
* **Sources to inspect:**
  * `global.json`, `version.json`, `src/SomaFrameQD.Demo/version.json`, `src/Directory.Build.props`, `src/NuGet.config`, `src/SomaFrameQD.sln`, `src/.vscode/launch.json`
  * `eng/package-contract.json`, `eng/validate-artifacts.ps1`
  * `eng/compatibility/Caliburn5PackageHost/*`, `eng/compatibility/MahApps.Net10DragHost/*`
  * `.github/workflows/ci.yml`, `VNIR.md`
* **Required Contents:** Complete documentation of the pre-retargeting build/release pipeline: `.NET SDK 10.0.400` pin, split `Nerdbank.GitVersioning` (`0.9.0` library family vs. `0.1.0` Demo), NuGet package ID mapping (`SomaFrameQDWpf` package ID vs. `SomaFrameQD` assembly), declarative `eng/package-contract.json` & `eng/validate-artifacts.ps1` verification gates, external compatibility hosts (`Caliburn5PackageHost` & `MahApps.Net10DragHost`), GitHub Actions CI/publish security model, and current git working-tree rename state (`Gemini` → `SomaFrameQD`).

#### 5. `00_INDEX.md`
* **Required Contents:** Timestamped master index for `C:\SOFTDEV\SomaFrameQD\doc_before-net48\`, explicitly labeling the folder as the **immutable "BEFORE `.NET Framework 4.8` Retargeting & Optimization" architectural snapshot**, summarizing `01`–`09`, and cross-referencing `TODO.md`, `VNIR.md`, `doc/caliburn-5-migration.md`, and `LOC-statistics_plus_net48-retargeting/`.

---

### Part B — AI Entry-Point Files in Repository Root (`C:\SOFTDEV\SomaFrameQD\`)

#### 6. `C:\SOFTDEV\SomaFrameQD\REPO_MAP.md`
* **Required Contents:** High-density structural & symbol navigation map designed so AI agents find exact files in one read:
  * Annotated directory tree (`src/`, `eng/`, `doc/`, `doc_before-net48/`, `Implementation-Plans_doc_before-net48/`, `LOC-statistics_plus_net48-retargeting/`, `.github/`) with per-project LOC & TFM summary.
  * Internal project & NuGet dependency graph (`Mermaid` diagram + table).
  * Master MEF Contract Directory: every major `[Export]` / `[Import]` interface (`IShell`, `IMainWindow`, `IModule`, `ICommandService`, `ICommandRouter`, `IMenuBuilder`, `IToolBarBuilder`, `IThemeManager`, `IEditorProvider`, `IEditorOpeningService`, `ILayoutItemStatePersister`, `IUndoRedoManager`, `IToolboxService`, `ICodeCompiler`, `IErrorList`, `IInspectorTool`, `IOutput`, `IPropertyGrid`, etc.) mapped to its exact implementation file path.
  * Built-in Commands, Menus, ToolBars & Keyboard Shortcuts lookup table.
  * Test-to-Subsystem mapping table (which test file covers which subsystem).

#### 7. `C:\SOFTDEV\SomaFrameQD\AGENTS.md`
* **Required Contents:** Authoritative operating manual for AI coding agents entering the repository:
  * **Quick Orientation & Repository State:** What SomaFrameQD is, current pre-retargeting dual-TFM state (`net48;net10.0-windows`) vs. planned `net48` + Win7 SP1 `x64` target in `TODO.md`, and warning about uncommitted `src/Gemini*` git deletions (never enumerate files with `git ls-files --cached` without checking disk existence).
  * **Context-Window Discipline Rules:** Start at `REPO_MAP.md` → load only the relevant single guide in `doc_before-net48/` → read targeted files/line ranges; avoid bulk-reading `.resx` (`6,838` lines), `Themes/VS2013/Controls/*.xaml`, or `DesignTimeImages.cs`; obey the strict Stage 1 vs. Stage 2 separation and sequential single-plan execution rules in `TODO.md`.
  * **Build, Test, Pack & Validation Commands:** Exact `dotnet` / MSBuild / `validate-artifacts.ps1` commands and how `eng/package-contract.json` enforces lockstep dependency/TFM changes.
  * **Non-Negotiable Architectural Invariants:** MEF priority vs. main catalog overrides; `CreationPolicy.NonShared` on documents; Caliburn.Micro 5 async `Activated` & `OnInitializedAsync` rules; serialized shell transitions via `TransitionCoordinator`; transactional layout persistence; split NBGV `version.json` (`0.9.0` vs. `0.1.0` Demo — never hardcode `<Version>` in `.csproj`).
  * **Task-to-Documentation Routing Table:** Direct links to `REPO_MAP.md`, `TODO.md`, `VNIR.md`, `doc/caliburn-5-migration.md`, `LOC-statistics_plus_net48-retargeting/*`, and `doc_before-net48/00_INDEX.md`–`09_*.md`.

---

## Plan #2 Verification Gate

1. Verify all 10 files exist inside `C:\SOFTDEV\SomaFrameQD\doc_before-net48\` (`00_INDEX.md` + `01`–`09`) and both `AGENTS.md` and `REPO_MAP.md` exist in `C:\SOFTDEV\SomaFrameQD\`.
2. Run a link-verification check across `AGENTS.md`, `REPO_MAP.md`, and `doc_before-net48/*.md` confirming **every relative file/doc link resolves on disk**.
3. Confirm via `git status` / `git diff` that **zero** source code, `.csproj`, config, or existing build files were modified.
