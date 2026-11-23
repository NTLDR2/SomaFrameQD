# `doc_after-net48` — Master Architectural Reference Index (Post-`.NET Framework 4.8` `x64` Retargeting & Optimization)

> **Documentation Timestamp:** `2026-11-23`  
> **Documentation Status:** **AUTHORITATIVE LIVE ARCHITECTURAL REFERENCE** — Verified 100% against the frozen post-conversion `SomaFrameQD` **`0.9.0`** (`SomaFrameQD.Demo` **`0.1.0` alpha**) codebase after completing **Stage 1** (`.NET Framework 4.8` Simple Retargeting) and **Stage 2** (`.NET Framework 4.8` + **Windows 7 SP1 `x64` → Windows 11 `x64`** Optimization).  
> **Active Target Platform:** `<TargetFramework>net48</TargetFramework>` (`LangVersion 12.0`, `.NET SDK 8.0.400` LTS); `AnyCPU` `x64`-clean NuGet libraries (`8` packages + `.snupkg`s) and **64-bit (`x64` `PE32+`)** executables/test hosts (`489` core source & UI files = **`31,518` Core SLOC** / `38,394` lines; `576` deliverable text files = **`38,771` SLOC** / `49,032` lines; `41` binary assets; **`174 / 174`** passing `[net48|x64]` STA test executions).

---

## 1. Purpose of `doc_after-net48/`

`C:\SOFTDEV\SomaFrameQD\doc_after-net48\` is the **primary, up-to-date architectural reference** for developers and AI coding agents working in `SomaFrameQD`:

1. **Single Source of Post-Conversion Truth:** Reflects every Stage 1 and Stage 2 architectural invariant—native `.NET Framework 4.8` GAC `System.ComponentModel.Composition` / `System.Drawing` / `PresentationFramework.Aero` bindings, 64-bit `PE32+` host execution, Multi-Core Background JIT + `.debug.log` fallback, filtered MEF directory probing, `TryUnifyStrongNamedAssembly`, cross-OS TLS & `app.manifest` System-DPI rules, 64-bit Win32 `IntPtr` P/Invoke safety, visibility-gated VSync render loops, 562 frozen VS2013 `SolidColorBrush` resources, `ps_2_0` pixel shaders, and the 174-execution `[net48|x64]` test suite.
2. **Context-Window Protection for AI Agents:** Splits the `38,771`-SLOC repository into **nine focused subsystem guides (`01`–`09`)** so contributors and AI agents load **only** the single topic guide relevant to their task instead of scanning raw source trees.
3. **Clean Separation from Immutable Historical Baseline:** Preserves [`../doc_before-net48/00_INDEX.md`](../doc_before-net48/00_INDEX.md) untouched as the permanent *"Before `.NET 4.8` Conversion"* snapshot (`2026-11-22`) for historical comparison and provenance audits.

---

## 2. Complete Post-Conversion Guide Directory (`01`–`09`)

### Part I — Core Framework, Shell, Themes & Win32 `x64` Interop (`src/SomaFrameQD` — `13,019` Core SLOC)

| Guide | Scope & Key Subsystems Covered |
| :--- | :--- |
| **[01 — Architecture & Bootstrapper Overview](./01_Architecture_and_Bootstrapper_Overview.md)** | `SomaFrameQD` (`SomaFrameQDWpf` `0.9.0`) mission & stack, `AppBootstrapper`, native `.NET 4.8` GAC MEF two-tier composition (`priorityProvider` vs. `mainProvider`), `ShouldInspectAssemblyFile` vendor/native PE skip filter, Multi-Core Background JIT + `.debug.log` fallback (`Q5 = 5A`), `TryUnifyStrongNamedAssembly`, `ResolveCrossOsSecurityProtocol` (`Win7` TLS 1.2 vs. `Win8.1`–`Win11` `SystemDefault`), `app.manifest` (`Win7 SP1` → `Win10/11` + System-DPI `Q2 = 2A`), `ShellActivationTask` → `InitializationTask`, and `README.md` drift table. |
| **[02 — Core Framework Primitives, Editor Pipeline & Coroutines](./02_Core_Framework_Primitives_and_Coroutines.md)** | Caliburn.Micro 5 async screen hierarchy (`WindowBase`, `LayoutItemBase`, `Document`, `PersistedDocument`, `Tool`), dirty-document `CanCloseAsync` rules, `Tool.TryCloseAsync` → `IShell.CloseToolAsync`, view-ready `EditorOpeningService`, `OpenResultBase` (`Show.*`) state machines, `AsyncCommand` / `RelayCommand`, `TranslateExtension`, cached `ResourceManager`, weak settings listeners, and `x64`-safe `WindowOptionsBehavior` (`GetWindowLongPtr`/`SetWindowLongPtr`). |
| **[03 — Commands, Menus, ToolBars & KeyGestures](./03_Commands_Menus_ToolBars_and_KeyGestures.md)** | `[CommandDefinition]` / `CommandListDefinition`, `CommandService`, `TargetableCommand`, 3-stage `CommandRouter` (`ActiveLayoutItem` → `ActiveItem` → global `[CommandHandler]` + `ICommandRerouter`), declarative `MenuBuilder` & `ToolBarBuilder`, static/dynamic exclusions, `CommandKeyGestureService`, and `InputManager`. |
| **[04 — Shell, AvalonDock 5 Persistence & Built-In Core Modules](./04_Shell_Docking_Persistence_and_BuiltIn_Modules.md)** | All **8** built-in modules (`Shell`, `MainWindow`, `MainMenu`, `ToolBars`, `StatusBar`, `Toolbox`, `UndoRedo`, `Settings`), `MainWindowView` `1024×768` work-area clamping & System-DPI `TextFormattingMode` (`Display` @ `1.0x` vs. `Ideal` > `1.0x`), `Segoe UI, Verdana` Win7–Win11 typography, `TransitionCoordinator` serialized FIFO queue & `AsyncLocal` re-entrancy, transactional `ApplicationState.bin` (`.tmp`/`.bak`), and `IAsyncLayoutItemStateRestorer`. |
| **[05 — Themes, Custom Controls, Pixel Shaders & Win32 `x64` Interop](./05_Themes_Rendering_and_Win32_Interop.md)** | `ThemeManager` (`theme.GetType().Name` lookup), `BuiltInThemeResources` AvalonDock 5 precedence & recursive `Freezable.Freeze()`, **562** `po:Freeze="True"` VS2013 brushes, `MainWindowStyle` `1px` Win7 Classic/Basic/RDP border + Aero/Win11 glow, frozen `ps_2_0` (`0xFFFF0200`) `GrayscaleEffect.ps` (`ShaderRenderMode.Auto`), `ClippingHwndHost`, `HwndWrapper` (`IsLoaded && IsVisible` VSync gating), and 64-bit `NativeMethods.cs` (`UncheckedInt32`, `GetWheelDeltaWParam`, `GetXLParam`/`GetYLParam`, `WM_XBUTTONUP`). |

### Part II — Standalone Modules, Demo (`0.1.0` Alpha), `[net48|x64]` Test Suite & Build/CI

| Guide | Scope & Key Subsystems Covered |
| :--- | :--- |
| **[06 — Standalone Modules Reference](./06_Standalone_Modules_Reference.md)** | All **7** standalone `0.9.0` `net48` NuGet libraries (`6,453` Core SLOC): `CodeCompiler` (Roslyn `3.4.0` without `GC.Collect()`, `Hidden`/`Location.None` diagnostic safety, `AppDomain` `///` XML docs), `CodeEditor` (`AvalonEdit` `6.3.1.120`, `OwnedSearchPanel`, `BaseDirectory` `.xshd` probing, `Ordinal` dirty checks), `ErrorList` (zero-alloc `FilteredItems`), `GraphEditor` (`ConnectorItem` `Loaded`/`Unloaded` `LayoutUpdated` scoping), `Inspector` (GAC `System.Drawing`, `SelectiveUndoEditorBase`, High-DPI physical-pixel `ScreenColorPicker`), `Output` (`_syncRoot` + coalesced background flushes), and `PropertyGrid` (`ICommandRerouter`). |
| **[07 — Demo Application & Client Extension Patterns](./07_Demo_Application_and_Extension_Patterns.md)** | `SomaFrameQD.Demo` (`0.1.0` alpha, `3,461` Core SLOC, `x64` `PE32+` `WinExe`), active `DemoAppBootstrapper` (`SomaFrameQD.Demo.Startup.net48.jitprofile`), and 6 extension patterns: priority `IShell` override + `CanCloseAsync` + merged settings, `Startup` chrome/inspector sync, `Home` + debounced `HelixViewModel` live Roslyn 3D scripting (`CachedScriptReferences`, `350ms` timer, active-tab VSync gating), `SampleBrowser`, `FilterDesigner` (finalizer-safe 64-bit `FastBitmap` + frozen `ps_2_0` `AddEffect.ps`/`MultiplyEffect.ps`), and `TextEditor`. |
| **[08 — Test Suite, Characterization & Optimization Harness](./08_Test_Suite_and_Characterization_Harness.md)** | `SomaFrameQD.Tests` (`8,422` Core SLOC, `MSTest.Sdk` `4.1.0`, `[net48\|x64]` `PE32+`, `UseSTASynchronizationContext`), STA `DispatcherFrame` & `HwndWrapper` render-priority rules, complete inventory of all **27** `.cs` files (**174** passing executions = 148 baseline + 26 Stage 2 optimization executions), and SHA-256-locked append-only fixtures (`AvalonDock460`, `AvalonDock4741`, `AvalonDock501`). |
| **[09 — Build, Versioning, Packaging & CI Pipeline](./09_Build_Versioning_Packaging_and_CI_After_NET48.md)** | `.NET SDK 8.0.400` LTS (`global.json`), `src/NuGet.config`, `src/Directory.Build.props` (`x64` `Exe`/`WinExe` + `AnyCPU` libs + Portable PDB `.snupkg`s + GAC MEF), 11-project matrix (`10` in `SomaFrameQD.sln` + standalone `Caliburn5PackageHost`), split NBGV `version.json` (`0.9.0` vs. `0.1.0`), `eng/package-contract.json` + PS 5.1/`pwsh` 7 `eng/validate-artifacts.ps1`, hardened GitHub Actions CI + OIDC `publish`, and `changelogs/` vs. `releasenotes/`. |

---

## 3. Related Repository Entry Points, Release Docs & Historical Archives

| Document / Folder | Relative Path | Role |
| :--- | :--- | :--- |
| **AI Agent Operating Manual** | [`../AGENTS.md`](../AGENTS.md) | Authoritative operating rules, 10 architectural invariants, build/pack commands, and task-to-guide routing table. |
| **Repository & Symbol Map** | [`../REPO_MAP.md`](../REPO_MAP.md) | One-read directory tree, project/NuGet DAG, MEF `[Export]`/`[Import]` directory, 29-command shortcut table, and test map. |
| **Cross-OS `x64` Deployment Guide** | [`../doc/net48-win7-to-win11-x64-deployment-guide.md`](../doc/net48-win7-to-win11-x64-deployment-guide.md) | End-to-end Windows 7 SP1 `x64` → Windows 11 `x64` prerequisite, manifest, Multi-Core JIT, MEF probing, and QA verification guide. |
| **Caliburn.Micro 5 Migration Guide** | [`../doc/caliburn-5-migration.md`](../doc/caliburn-5-migration.md) | Async screen/conductor/coroutine migration reference (`Caliburn.Micro` `5.0.258` + `AvalonDock` `5.0.1` + `net48`). |
| **Granular Technical Changelog** | [`../changelogs/20261123_CHANGELOG.md`](../changelogs/20261123_CHANGELOG.md) | Date-stamped developer/maintainer changelog detailing every Stage 1 & Stage 2 architectural, ABI, build, and bug-fix change. |
| **Inaugural Release Notes (`v0.9.0`)** | [`../releasenotes/20261123_RELEASE_NOTES_v0.9.0.md`](../releasenotes/20261123_RELEASE_NOTES_v0.9.0.md) | Date-stamped release announcement, compatibility mission statement, highlights, and NuGet adoption guide (`0.9.0` / Demo `0.1.0` alpha). |
| **Post-Conversion LOC Statistics** | [`../LOC-statistics_plus_net48-retargeting/AFTER_NET48_LOC_Statistics_2026-11-23_00-20-00.md`](../LOC-statistics_plus_net48-retargeting/AFTER_NET48_LOC_Statistics_2026-11-23_00-20-00.md) | Exact post-conversion file, SLOC, comment, and total line counts with apples-to-apples deltas vs. the `BEFORE_NET48` baseline. |
| **Dependency Risk Assessment** | [`../DRMA.md`](../DRMA.md) | Dependency Risk & Mitigation Assessment across all NuGet and `.NET Framework 4.8` GAC dependencies on Win7 SP1 → Win11 `x64`. |
| **Version & Provenance Report** | [`../VNIR.md`](../VNIR.md) | Split NBGV versioning rationale and Git commit-age provenance (`2012`–`2021` *Gemini* heritage vs. `2026` modernization). |
| **Completed Conversion Roadmap** | [`../TODO.md`](../TODO.md) | Completed Stage 1 (`Implementation-Plans_net48-retargeting-ONLY/`) and Stage 2 (`Implementation-Plans_net48-optimizations/`) checklist. |
| **Post-`net48` Documentation Plans** | [`../Implementation-Plans_doc_after-net48/`](../Implementation-Plans_doc_after-net48/) | Four-plan context-window-safe execution suite (`Plan_01`–`Plan_04`) that produced `doc_after-net48/`. |
| **Immutable Pre-`net48` Baseline** | [`../doc_before-net48/00_INDEX.md`](../doc_before-net48/00_INDEX.md) | Frozen pre-conversion (`2026-11-22`) architectural snapshot preserved strictly for historical diffing. |
