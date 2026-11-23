# `doc_before-net48` — Master Architectural Snapshot Index (Before `.NET Framework 4.8` Retargeting & Optimization)

> **Snapshot Timestamp:** `2026-11-22T18:30:00-05:00` (`2026-11-22`)  
> **Snapshot Status:** **IMMUTABLE PRE-CONVERSION BASELINE** — Captures the exact architecture, contracts, module boundaries, tests, and build/CI pipeline of `SomaFrameQD` **prior to** Stage 1 (`.NET Framework 4.8` Simple Retargeting) and Stage 2 (`.NET Framework 4.8` + Windows 7 SP1 `x64` Optimization).  
> **Baseline Target Frameworks:** `<TargetFrameworks>net48;net10.0-windows</TargetFrameworks>` across core, standalone modules, demo, and tests (`616` on-disk files; `486` core source files = `29,538` Core SLOC / `35,860` core lines; `575` total text files = `36,671` SLOC / `46,373` lines).

---

## 1. Purpose of This Documentation Folder

`C:\SOFTDEV\SomaFrameQD\doc_before-net48\` preserves a complete, code-verified architectural reference of `SomaFrameQD` **before** any `.NET Framework 4.8`-only project retargeting or runtime optimization edits occur:

1. **Historical & Architectural Truth:** Records how every subsystem works in the dual-targeted (`net48;net10.0-windows`, Caliburn.Micro `5.0.258`, AvalonDock `5.0.1`) codebase—correcting stale claims in the legacy root `README.md`.
2. **Context-Window Protection for AI Agents:** Splits the `36,671`-SLOC repository into **nine focused topic guides (`01`–`09`)** so AI coding agents can read **only** the single subsystem guide relevant to their current task instead of scanning dozens of `.cs` and `.xaml` files.
3. **Regression Baseline for Stages 1 & 2 (`TODO.md`):** Establishes the exact behavioral contracts, MEF exports, persistence formats, and 146-test characterization invariants that Stage 1 (Simple Retargeting) and Stage 2 (Sequential Optimization Plans `2.1`–`2.6`) must preserve.

> [!IMPORTANT]
> **Immutability Rule:**  
> Do **not** rewrite `doc_before-net48/*` to reflect post-conversion changes once `.NET Framework 4.8` retargeting and optimization begin. Keep this folder intact as the permanent *"Before `.NET 4.8` Conversion"* reference snapshot; document post-conversion changes in root docs (`AGENTS.md`, `REPO_MAP.md`, `TODO.md`) or a separate post-conversion documentation folder.

---

## 2. Complete Guide Directory (`01`–`09`)

### Part I — Core Framework, Shell & Built-In Modules (`src/SomaFrameQD` — Generated in Plan #1)

| Guide | Scope & Key Subsystems Covered |
| :--- | :--- |
| **[01 — Architecture & Bootstrapper Overview](./01_Architecture_and_Bootstrapper_Overview.md)** | `SomaFrameQD` (`SomaFrameQDWpf` `0.9.0`) stack, `AppBootstrapper`, two-tier MEF composition (`priorityProvider` vs. `mainProvider`), `IoC` / `IServiceProvider` bridges, deterministic async startup (`ShellActivationTask` → `InitializationTask`), `IModule` lifecycle hooks, and `README.md` drift corrections (`http://schemas.timjones.io/SomaFrameQD`). |
| **[02 — Core Framework Primitives, Editor Pipeline & Coroutines](./02_Core_Framework_Primitives_and_Coroutines.md)** | Screen hierarchy (`WindowBase`, `LayoutItemBase`, `Document`, `PersistedDocument`, `Tool`), dirty-document `CanCloseAsync` rules, `Tool.TryCloseAsync` → `IShell.CloseToolAsync`, `IEditorProvider` + view-ready `EditorOpeningService`, Caliburn 5 `OpenResultBase` coroutines (`Show.*`), `AsyncCommand`/`RelayCommand`, `TranslateExtension`, `ResourceManager`, weak settings listeners, and WPF behaviors. |
| **[03 — Commands, Menus, ToolBars & KeyGestures](./03_Commands_Menus_ToolBars_and_KeyGestures.md)** | `[CommandDefinition]` / `CommandListDefinition`, `ICommandService`, `TargetableCommand`, 3-stage `CommandRouter` (`ActiveLayoutItem` → `ActiveItem` → global `[CommandHandler]` + `ICommandRerouter`), declarative `MenuBuilder` & `ToolBarBuilder`, static/dynamic exclusions, `CommandKeyGestureService`, and `InputManager`. |
| **[04 — Shell, AvalonDock 5 Persistence & Built-In Core Modules](./04_Shell_Docking_Persistence_and_BuiltIn_Modules.md)** | All **8** built-in modules (`Shell`, `MainWindow`, `MainMenu`, `ToolBars`, `StatusBar`, `Toolbox`, `UndoRedo`, `Settings`), `TransitionCoordinator` serialized FIFO queue & nested re-entrancy, `ApplicationState.bin` binary+XML wire format, transactional `.tmp`/`.bak` saves, and `IAsyncLayoutItemStateRestorer` recovery matrix. |
| **[05 — Themes, Custom Controls, Pixel Shaders & Win32 Interop](./05_Themes_Rendering_and_Win32_Interop.md)** | `ITheme` & `ThemeManager`, AvalonDock 5 `BuiltInThemeResources` dictionary precedence & rollback, VS2013 Light/Blue/Dark palettes, `DynamicStyle`/`ExpanderEx`/`SliderEx`/`ClippingHwndHost`, `GrayscaleEffect.ps` shader, `HwndWrapper` native Win32 child host, and pre-conversion `x64`/Win32 baseline notes (`HwndWrapper` `WPARAM`/`LPARAM` casts, `GetWindowLong`, `app.config` SKU). |

### Part II — Standalone Modules, Demo, Tests & Build/CI (`SomaFrameQD.Modules.*`, `Demo`, `Tests`, `eng/` — Generated in Plan #2)

| Guide | Scope & Key Subsystems Covered |
| :--- | :--- |
| **[06 — Standalone Modules Reference](./06_Standalone_Modules_Reference.md)** | All **7** standalone NuGet packages (`SomaFrameQD.Modules.*`): `CodeCompiler` (Roslyn `ICodeCompiler`), `CodeEditor` (`AvalonEdit` 16 languages + `.xshd` + `IAsyncLayoutItemStateRestorer`), `ErrorList`, `GraphEditor` (`GraphControl`, `ZoomAndPanControl`, `BezierLine`), `Inspector` (`InspectableObjectBuilder`, `SelectiveUndoEditorBase`, `ScreenColorPicker`), `Output`, and `PropertyGrid` (`ICommandRerouter`). |
| **[07 — Demo Application & Client Extension Patterns](./07_Demo_Application_and_Extension_Patterns.md)** | `SomaFrameQD.Demo` (`0.1.0`) configuration (`AllowUnsafeBlocks`, `JeremyAnsel.HLSL.Targets`, HelixToolkit dual-TFM split), `DemoAppBootstrapper`, and 6 canonical extension patterns (`IShell` override + namespace mapping, `Startup`, `Home` + live Roslyn/Helix 3D scripting, `SampleBrowser`, `FilterDesigner` node graph + `FastBitmap` + `.fx` shaders, `TextEditor`). |
| **[08 — Test Suite & Characterization Harness](./08_Test_Suite_and_Characterization_Harness.md)** | `SomaFrameQD.Tests` (`MSTest.Sdk` `4.1.0`, `Microsoft.Testing.Platform`, `UseSTASynchronizationContext`), `StaDispatcher`, `IoCOverrideScope`, `ResultTestDoubles`, complete inventory of all **23** source files (**146** tests), and SHA-256-locked historical fixtures (`AvalonDock460`, `AvalonDock4741`, `AvalonDock501`). |
| **[09 — Build, Versioning, Packaging & CI Pipeline](./09_Build_Versioning_Packaging_and_CI_Before_NET48.md)** | `global.json` (`.NET SDK 10.0.400`), `src/NuGet.config`, `src/Directory.Build.props`, 12-project layout (`SomaFrameQD.sln` vs. standalone `Caliburn5PackageHost`), split NBGV `version.json` (`0.9.0` vs. `0.1.0`), `eng/package-contract.json` & `eng/validate-artifacts.ps1` gates, `MahApps.Net10DragHost`, GitHub Actions CI + OIDC `publish`, and uncommitted `Gemini` → `SomaFrameQD` git state. |

---

## 3. Related Repository Entry Points & Planning Artifacts

| Document / Folder | Relative Path | Role |
| :--- | :--- | :--- |
| **AI Agent Operating Manual** | [`../AGENTS.md`](../AGENTS.md) | Primary entry point for AI coding agents: invariants, build/test commands, context-window survival rules, and task routing. |
| **Repository & Symbol Map** | [`../REPO_MAP.md`](../REPO_MAP.md) | High-density directory map, project/NuGet DAG, master MEF contract directory, command/shortcut lookup table, and test map. |
| **Retargeting & Optimization Roadmap** | [`../TODO.md`](../TODO.md) | Enforces strict separation: **Stage 1 (Simple Retargeting to `net48`)** MUST finish before **Stage 2 (Sequential Optimization Plans `2.1`–`2.6`)**. |
| **LOC Baseline Statistics** | [`../LOC-statistics_plus_net48-retargeting/BEFORE_NET48_LOC_Statistics_2026-11-22_17-23-51.md`](../LOC-statistics_plus_net48-retargeting/BEFORE_NET48_LOC_Statistics_2026-11-22_17-23-51.md) | Exact per-module file, SLOC, comment, blank, and binary asset counts prior to `.NET 4.8` conversion. |
| **Retargeting vs. Optimization Analysis** | [`../LOC-statistics_plus_net48-retargeting/Simple_Retargeting_vs_Optimization_NET48_Win7_x64_2026-11-22_17-36-26.md`](../LOC-statistics_plus_net48-retargeting/Simple_Retargeting_vs_Optimization_NET48_Win7_x64_2026-11-22_17-36-26.md) | Deep engineering comparison of project-level retargeting vs. runtime/Win32/`x64`/Windows 7 SP1 optimization. |
| **Caliburn.Micro 5 Migration Guide** | [`../doc/caliburn-5-migration.md`](../doc/caliburn-5-migration.md) | Upstream migration notes for Caliburn.Micro `5.0.258` async screen/conductor/coroutine contracts and AvalonDock `5.0.1` persistence. |
| **Code Age & Provenance Report** | [`../VNIR.md`](../VNIR.md) | Git blame/commit-date audit distinguishing 2012–2021 legacy code from Sept–Oct 2026 modernization work. |
| **Documentation Implementation Plans** | [`../Implementation-Plans_doc_before-net48/`](../Implementation-Plans_doc_before-net48/) | Two-phase execution plans ([Plan #1](../Implementation-Plans_doc_before-net48/Plan_01_Core_Framework_Shell_and_BuiltIn_Modules_Docs.md) & [Plan #2](../Implementation-Plans_doc_before-net48/Plan_02_Standalone_Modules_Demo_Tests_Build_and_Root_AGENTS_REPO_MAP.md)) that produced this baseline suite. |
