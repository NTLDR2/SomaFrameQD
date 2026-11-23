# Post-`net48` Documentation Plan #4 — `[net48|x64]` Test Harness, Build/Packaging/CI, Master `00_INDEX.md` & Root `AGENTS.md` / `REPO_MAP.md` Routing Sync (`doc_after-net48/08`–`09` + `00_INDEX`)

> **Purpose:** Complete the **post-`.NET Framework 4.8` (`x64`)** technical documentation suite in `C:\SOFTDEV\SomaFrameQD\doc_after-net48\` (`08_Test_Suite_and_Characterization_Harness.md`, `09_Build_Versioning_Packaging_and_CI_After_NET48.md`, and master `00_INDEX.md`), update [`AGENTS.md`](../AGENTS.md) and [`REPO_MAP.md`](../REPO_MAP.md) to route developers and AI coding agents to `doc_after-net48/`, and execute an automated repository-wide link & accuracy gate.  
> **Generated Timestamp:** `2026-11-23T01:53:00-05:00` (`2026-11-23`)  
> **Prerequisite:** **[Plans #1](./Plan_01_Architecture_Bootstrapper_Primitives_and_Commands_Docs.md)–[#3](./Plan_03_Standalone_Modules_and_Demo_Application_Docs.md) MUST all be 100% completed and verified first (`doc_after-net48/01`–`07` on disk).**  
> **Execution Order:** [Plan #1](./Plan_01_Architecture_Bootstrapper_Primitives_and_Commands_Docs.md) → [Plan #2](./Plan_02_Shell_Docking_Themes_Rendering_and_Win32_x64_Docs.md) → [Plan #3](./Plan_03_Standalone_Modules_and_Demo_Application_Docs.md) → **Plan #4 (This Plan — Final Post-`net48` Documentation Completion Gate)**.  
> **Authoritative Post-Conversion References:** [`AFTER_NET48_LOC_Statistics_2026-11-23_00-20-00.md`](../LOC-statistics_plus_net48-retargeting/AFTER_NET48_LOC_Statistics_2026-11-23_00-20-00.md) | [`changelogs/20261123_CHANGELOG.md`](../changelogs/20261123_CHANGELOG.md) | [`releasenotes/20261123_RELEASE_NOTES_v0.9.0.md`](../releasenotes/20261123_RELEASE_NOTES_v0.9.0.md)

---

## Four-Plan Context-Window Safety Split (`Implementation-Plans_doc_after-net48`)

| Plan | Source Inspection Scope (Read-Only) | Context Budget | Output Deliverables |
| :--- | :--- | :--- | :--- |
| **[Plan #1](./Plan_01_Architecture_Bootstrapper_Primitives_and_Commands_Docs.md)** | `AppBootstrapper.cs`, `DemoAppBootstrapper.cs`, `app.config` (x2), `app.manifest`, `src/Directory.Build.props`, `Framework/*` | `~4,800` targeted SLOC read | **`doc_after-net48/01`–`03`** |
| **[Plan #2](./Plan_02_Shell_Docking_Themes_Rendering_and_Win32_x64_Docs.md)** | `Modules/*` (all 8 built-in modules), `Framework/Themes/*`, `Themes/VS2013/*`, `ShaderEffects/*`, `HwndWrapper`, `ClippingHwndHost`, `NativeMethods.cs` | `~5,200` targeted SLOC read | **`doc_after-net48/04`–`05`** |
| **[Plan #3](./Plan_03_Standalone_Modules_and_Demo_Application_Docs.md)** | All 7 `src/SomaFrameQD.Modules.*` projects + `src/SomaFrameQD.Demo` (`FastBitmap`, `HelixViewModel`, `ShaderEffectUtility`, 6 demo modules) | `~5,800` targeted SLOC read | **`doc_after-net48/06`–`07`** |
| **Plan #4 (This Plan)** | `src/SomaFrameQD.Tests` (27 `.cs` files / 174 `[net48\|x64]` executions), `global.json`, `version.json` (x2), `SomaFrameQD.sln`, `eng/*`, `.github/workflows/ci.yml`, `changelogs/*`, `releasenotes/*` | `~3,500` targeted SLOC + scripts/configs | **`doc_after-net48/08`–`09` + `00_INDEX.md`** + **`AGENTS.md`** & **`REPO_MAP.md`** sync + full audit |

---

## Strict Guardrails

1. **ALL C# & SOURCE CODE IS STRICTLY FROZEN:** Read-only on all `.cs`, `.xaml`, `.fx`, `.csproj`, `.props`, `.sln`, `.json`, and `.ps1` files.
2. **NEVER Modify `doc_before-net48/`:** Verify `doc_before-net48/*` retains all `10` original `2026-11-22` files completely untouched.
3. **Sample Large Test Files Selectively:** Reuse the baseline method inventories from [`doc_before-net48/08_Test_Suite_and_Characterization_Harness.md`](../doc_before-net48/08_Test_Suite_and_Characterization_Harness.md) for untouched test classes while inspecting the 4 new Stage 2 test files (`AppBootstrapperOptimizationTests.cs`, `Win32X64InteropTests.cs`, `CodeCompilerAndOutputTests.cs`, `StandaloneAndDemoOptimizationTests.cs`), expanded `AvalonDockThemeTests.cs`, and re-keyed `AvalonDockPersistenceTests.cs` / `TestData/`.

---

## Exact Deliverables

### Part A — Final Post-`net48` Guides & Index in `C:\SOFTDEV\SomaFrameQD\doc_after-net48\`

#### 1. `08_Test_Suite_and_Characterization_Harness.md`
* **Frozen Sources to Verify Against:**
  * [`src/SomaFrameQD.Tests/SomaFrameQD.Tests.csproj`](../src/SomaFrameQD.Tests/SomaFrameQD.Tests.csproj), `TestInfrastructure/StaDispatcher.cs`, `TestInfrastructure/IoCOverrideScope.cs`
  * [`Composition/AppBootstrapperOptimizationTests.cs`](../src/SomaFrameQD.Tests/Composition/AppBootstrapperOptimizationTests.cs), [`Framework/Win32/Win32X64InteropTests.cs`](../src/SomaFrameQD.Tests/Framework/Win32/Win32X64InteropTests.cs), [`Modules/CodeCompiler/CodeCompilerAndOutputTests.cs`](../src/SomaFrameQD.Tests/Modules/CodeCompiler/CodeCompilerAndOutputTests.cs), [`Modules/StandaloneAndDemoOptimizationTests.cs`](../src/SomaFrameQD.Tests/Modules/StandaloneAndDemoOptimizationTests.cs), [`Modules/Shell/AvalonDockThemeTests.cs`](../src/SomaFrameQD.Tests/Modules/Shell/AvalonDockThemeTests.cs), [`Modules/Shell/AvalonDockPersistenceTests.cs`](../src/SomaFrameQD.Tests/Modules/Shell/AvalonDockPersistenceTests.cs)
  * [`src/SomaFrameQD.Tests/TestData/`](../src/SomaFrameQD.Tests/TestData/) (`AvalonDock460`, `AvalonDock4741`, `AvalonDock501`)
* **Required Technical Coverage:**
  * `[net48|x64]` `MSTest.Sdk 4.1.0` configuration (`PlatformTarget=x64`, `Prefer32Bit=false`, automatic `.exe.config` binding redirects, `<UseSTASynchronizationContext>true</UseSTASynchronizationContext>`, direct `GraphEditor` + `Demo` + all module references).
  * Critical STA / Dispatcher testing rules: `StaDispatcher`, `Dispatcher.PushFrame` pumping under `MSTest`'s `STASynchronizationContext`, why `StaDispatcher.DrainAsync()` (`DispatcherPriority.Background` = `4`) must not be awaited while an offscreen `HwndWrapper` drives `CompositionTarget.Rendering` (`DispatcherPriority.Render` = `7`), and OS-level process timeout best practices (`Microsoft.Testing.Platform` cooperative `--timeout` caveat).
  * Complete directory of all **27** `.cs` files (`8,422` Core SLOC) and all **174** `[net48|x64]` test executions, including detailed method breakdowns of the 26 new Stage 2 optimization tests.
  * Append-only `TestData/` fixtures with current `SomaFrameQD.Tests, Version=0.9.0.0` SHA-256 hashes (`0F9177CC...`, `0FF5F1CD...`, `74E90837...`).

#### 2. `09_Build_Versioning_Packaging_and_CI_After_NET48.md`
* **Frozen Sources to Verify Against:**
  * [`global.json`](../global.json), [`version.json`](../version.json), [`src/SomaFrameQD.Demo/version.json`](../src/SomaFrameQD.Demo/version.json), [`src/Directory.Build.props`](../src/Directory.Build.props), [`src/NuGet.config`](../src/NuGet.config), [`src/SomaFrameQD.sln`](../src/SomaFrameQD.sln), [`src/.vscode/launch.json`](../src/.vscode/launch.json), [`SomaFrameQD.code-workspace`](../SomaFrameQD.code-workspace)
  * [`eng/package-contract.json`](../eng/package-contract.json), [`eng/validate-artifacts.ps1`](../eng/validate-artifacts.ps1), [`eng/compatibility/Caliburn5PackageHost/`](../eng/compatibility/Caliburn5PackageHost/)
  * [`.github/workflows/ci.yml`](../.github/workflows/ci.yml), [`changelogs/20261123_CHANGELOG.md`](../changelogs/20261123_CHANGELOG.md), [`releasenotes/20261123_RELEASE_NOTES_v0.9.0.md`](../releasenotes/20261123_RELEASE_NOTES_v0.9.0.md)
* **Required Technical Coverage:**
  * Toolchain & MSBuild architecture: `.NET SDK 8.0.400` (`rollForward: "latestPatch"`) + `MSTest.Sdk 4.1.0`, `LangVersion 12.0`, `Deterministic=true`, `DebugType=portable` (`BSJB` `.snupkg`s, `Q6=6A`), shared GAC `<Reference Include="System.ComponentModel.Composition" />`, `x64` `PE32+` `Exe`/`WinExe` (`Q1=1A`) + automatic `.exe.config` binding redirects vs. `AnyCPU` `x64`-clean library `.dll`s, and 10-project `SomaFrameQD.sln` (`Debug|Any CPU`, `Debug|x64`, `Release|Any CPU`, `Release|x64`).
  * Split `Nerdbank.GitVersioning` `3.5.109` (`0.9.0` framework/modules vs. `0.1.0` alpha Demo) and core NuGet Package ID `SomaFrameQDWpf` vs. assembly/namespace `SomaFrameQD`.
  * Lockstep packaging verification gate (`eng/package-contract.json` + cross-shell PS 5.1 / `pwsh` 7 `eng/validate-artifacts.ps1` + standalone `Caliburn5PackageHost` `[net48|x64]`).
  * Hardened GitHub Actions CI & OIDC NuGet publishing (`windows-2025`, 40-char Action commit SHAs, `tgjones/SomaFrameQD` guard) and centralized `YYYYMMDD_` release housekeeping (`changelogs/` vs. `releasenotes/`).

#### 3. `00_INDEX.md`
* **Required Contents:**
  * Master index for `C:\SOFTDEV\SomaFrameQD\doc_after-net48\`, explicitly identifying it as the **authoritative live post-`.NET Framework 4.8` (`x64`, `Windows 7 SP1` → `Windows 11`) architectural documentation** (`0.9.0` framework / `0.1.0` alpha Demo; `489` core files = `31,518` Core SLOC; `576` deliverable text files = `38,771` SLOC).
  * Complete annotated table of guides `01`–`09` and cross-reference directory linking `AGENTS.md`, `REPO_MAP.md`, `doc/net48-win7-to-win11-x64-deployment-guide.md`, `doc/caliburn-5-migration.md`, `changelogs/20261123_CHANGELOG.md`, `releasenotes/20261123_RELEASE_NOTES_v0.9.0.md`, `AFTER_NET48_LOC_Statistics_2026-11-23_00-20-00.md`, and historical `doc_before-net48/00_INDEX.md`.

---

### Part B — Synchronize Root `AGENTS.md` & `REPO_MAP.md` Routing

1. **[`C:\SOFTDEV\SomaFrameQD\AGENTS.md`](../AGENTS.md):**
   * Update Section 2 Rule #2 so agents load topic guides from **`doc_after-net48/`** (`00_INDEX.md` + `01`–`09`) for current codebase tasks while Rule #4 preserves `doc_before-net48/` as the immutable historical baseline.
   * Update Section 2 SLOC callout to show current post-conversion deliverable totals (`38,771` SLOC / `49,032` lines across `576` text files) alongside the pre-conversion baseline.
   * Update Section 5 **Task-to-Documentation Routing Table** so all subsystem rows (`01`–`09`) point primarily to **`doc_after-net48/01_...`–`09_Build_Versioning_Packaging_and_CI_After_NET48.md`**, add a row for `changelogs/` & `releasenotes/`, and retain a dedicated row for the immutable pre-`net48` baseline (`doc_before-net48/00_INDEX.md`).
2. **[`C:\SOFTDEV\SomaFrameQD\REPO_MAP.md`](../REPO_MAP.md):**
   * Update the header companion entry point to point to `doc_after-net48/00_INDEX.md` as the active subsystem reference suite.
   * Add `doc_after-net48/` (`00_INDEX.md` + `01`–`09`) and `Implementation-Plans_doc_after-net48/` (`Plan_01`–`Plan_04`) to the Section 1 repository directory tree alongside `doc_before-net48/`.

---

## Final Post-`net48` Documentation Completion Gate

Execute a single automated PowerShell audit script verifying:
1. All **10** files (`00_INDEX.md` + `01`–`09`) exist in `C:\SOFTDEV\SomaFrameQD\doc_after-net48\` with strict UTF-8 (no Mojibake C1 control characters) and CRLF line endings.
2. **100% of relative Markdown links** across `doc_after-net48/*.md`, `Implementation-Plans_doc_after-net48/*.md`, `AGENTS.md`, `REPO_MAP.md`, `changelogs/20261123_CHANGELOG.md`, and `releasenotes/20261123_RELEASE_NOTES_v0.9.0.md` resolve cleanly on disk (`0` broken links).
3. **Stale-Claim Forensic Check:** Confirm zero active-tense claims of `net10.0-windows` targeting, `MahApps.Net10DragHost`, `System.Drawing.Common` / `System.ComponentModel.Composition 6.0.0` NuGet dependencies, `sku="v4.6.1"`, or `146` current tests inside `doc_after-net48/*.md`.
4. **Immutability & C# Freeze Check:** Confirm all `10` files in `doc_before-net48/` retain their original `2026-11-22` timestamps and **zero** `.cs`/source files were modified anywhere in `src/` or `eng/`.
