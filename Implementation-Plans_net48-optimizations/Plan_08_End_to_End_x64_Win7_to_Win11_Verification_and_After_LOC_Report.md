# Optimization Implementation Plan #8 — Final End-to-End `x64` Verification (`Win7 SP1` → `Win11`), Cross-OS Deployment Guide & Timestamped "AFTER" LOC Statistics Report

> **Purpose:** Execute a zero-cache, clean-room end-to-end `x64` build, full characterization + optimization test suite, NuGet package/symbol audit (`validate-artifacts.ps1`), PE32+ header & `.exe.config` binding-redirect inspection across all **7 Locked Architectural Decisions (`Q1=1A, Q2=2A, Q3=3B, Q4=4A, Q5=5A, Q6=6A, Q7=7A`)**, author the **Windows 7 SP1 `x64` → Windows 11 `x64` Deployment & QA Verification Guide**, synchronize live repository documentation (`TODO.md`, `AGENTS.md`, `REPO_MAP.md`), and generate the timestamped **`AFTER_NET48_LOC_Statistics_<timestamp>.md`** report comparing post-conversion metrics against [`BEFORE_NET48_LOC_Statistics_2026-11-22_17-23-51.md`](../LOC-statistics_plus_net48-retargeting/BEFORE_NET48_LOC_Statistics_2026-11-22_17-23-51.md).  
> **Generated / Updated Timestamp:** `2026-11-22T20:58:30-05:00` (`2026-11-22` — Updated with Locked User Decisions `Q1`–`Q7`)  
> **Prerequisite:** **[Plans #1](./Plan_01_Open_Questions_Native_GAC_References_x64_and_Binding_Redirects.md)–[#7](./Plan_07_Standalone_Modules_Part2_Inspector_GraphEditor_PropertyGrid_and_Demo.md) MUST all be 100% completed and verified green first.**  
> **Target Platform Continuum:** `.NET Framework 4.8` (`net48`), **Windows 7 SP1 `x64` → Windows 8.1 `x64` → Windows 10 `x64` → Windows 11 `x64`**.  
> **Execution Order:** [Plan #1](./Plan_01_Open_Questions_Native_GAC_References_x64_and_Binding_Redirects.md) → [Plan #2](./Plan_02_CLR48_Runtime_SKU_AppManifest_Startup_JIT_and_MEF_Probing.md) → [Plan #3](./Plan_03_Win32_x64_Pointer_ABI_and_HwndHost_Interop_Hardening.md) → [Plan #4](./Plan_04_WPF_RenderLoop_Lifecycle_and_VS2013_Theme_Resource_Freezing.md) → [Plan #5](./Plan_05_WindowChrome_Docking_DPI_and_Win7_to_Win11_OS_Adaptation.md) → [Plan #6](./Plan_06_Standalone_Modules_Part1_CodeCompiler_CodeEditor_ErrorList_Output.md) → [Plan #7](./Plan_07_Standalone_Modules_Part2_Inspector_GraphEditor_PropertyGrid_and_Demo.md) → **Plan #8 (This Plan — Final Stage 2 Completion Gate)**.  
> **Reference Documents:** [`TODO.md`](../TODO.md) (`Plan 2.6`) | [`BEFORE_NET48_LOC_Statistics_2026-11-22_17-23-51.md`](../LOC-statistics_plus_net48-retargeting/BEFORE_NET48_LOC_Statistics_2026-11-22_17-23-51.md) | [`DRMA.md`](../DRMA.md) | [`AGENTS.md`](../AGENTS.md)

---

## Stage 2 Eight-Plan Sequential Roadmap (`Implementation-Plans_net48-optimizations`)

| Plan | Optimization Focus (Win7 SP1 `x64` → Win11 `x64`) | Context Budget | Gate Verification |
| :--- | :--- | :--- | :--- |
| **[Plan #1](./Plan_01_Open_Questions_Native_GAC_References_x64_and_Binding_Redirects.md)** | Locked Decisions (`Q1`–`Q7`) + Native `net48` GAC references (`System.ComponentModel.Composition`, `System.Drawing`), `x64` target config (`1A`), `portable` PDBs (`6A`), automatic binding redirects, & lockstep `package-contract.json` | `~15` files (`~650` lines) | Clean `x64` build; `SomaFrameQD.Tests` runs as `[net48\|x64]`; `validate-artifacts.ps1` passes |
| **[Plan #2](./Plan_02_CLR48_Runtime_SKU_AppManifest_Startup_JIT_and_MEF_Probing.md)** | CLR 4.8 `sku="v4.8"` (`app.config`), `app.manifest` (`<supportedOS>` Win7 SP1 → Win10/11 + System-DPI `2A`), **clean removal** of `PublishSingleFile` APIs + clean `net48` `DemoAppBootstrapper.cs` (`3B`), filtered MEF probing (`4A`), Multi-Core JIT + `.debug.log` fallback logger (`5A`), & `AssemblyResolve` unification | `~7` files (`~650` lines) | Clean `x64` build + startup/MEF/JIT-fallback-log/assembly-unification tests pass |
| **[Plan #3](./Plan_03_Win32_x64_Pointer_ABI_and_HwndHost_Interop_Hardening.md)** | 64-bit (`x64`) Win32 P/Invoke & `HwndHost` hardening (`NativeMethods.cs`, `HwndWrapper.cs`, `ClippingHwndHost.cs`, `WindowOptionsBehavior.cs`) | `~6` files (`~950` lines) | Clean `x64` build + dedicated 64-bit pointer/message overflow unit tests pass |
| **[Plan #4](./Plan_04_WPF_RenderLoop_Lifecycle_and_VS2013_Theme_Resource_Freezing.md)** | `HwndWrapper` `CompositionTarget.Rendering` visibility/loaded gating, `po:Freeze="True"` across ~340 VS2013 `SolidColorBrush` resources, & `ps_2_0` `GrayscaleEffect` check | `~8` files (`~1,050` lines) | Clean `x64` build + render-loop gating & frozen theme dictionary tests pass |
| **[Plan #5](./Plan_05_WindowChrome_Docking_DPI_and_Win7_to_Win11_OS_Adaptation.md)** | Cross-OS Window/Docking adaptation (`Win7 SP1` Aero & Classic/RDP ↔ `Win10`/`Win11` Snap + System-DPI `2A`), work-area clamping, TLS 1.2/1.3 safety, & buffered `ApplicationState.bin` I/O | `~8` files (`~750` lines) | Clean `x64` build + persistence/chrome/bounds/TLS characterization tests pass |
| **[Plan #6](./Plan_06_Standalone_Modules_Part1_CodeCompiler_CodeEditor_ErrorList_Output.md)** | Standalone Modules Part 1 (`CodeCompiler` + `///` Roslyn/`AppDomain` docs `7A`, `Output`, `ErrorList`, `CodeEditor`) | `~9` files (`~650` lines) | Clean `x64` build + combined `AvalonDock` × `CodeCompiler` `x64` test passes |
| **[Plan #7](./Plan_07_Standalone_Modules_Part2_Inspector_GraphEditor_PropertyGrid_and_Demo.md)** | Standalone Modules Part 2 (`Inspector`, `GraphEditor`, `PropertyGrid`) & `SomaFrameQD.Demo` (`FastBitmap`, `HelixViewModel` + `///` docs `7A`, `ScreenColorPicker`) | `~10` files (`~900` lines) | Clean `x64` build + `Inspector`, `GraphEditor`, `PropertyGrid`, & `FastBitmap` tests pass |
| **Plan #8 (This Plan)** | Clean-room end-to-end `x64` verification, Win7 SP1 → Win11 QA runbook, live doc sync, & timestamped `AFTER_NET48_LOC_Statistics` report | Scripts + root docs (`~500` lines) | All tests & `validate-artifacts.ps1` green on `x64`; `AFTER` LOC delta report generated |

---

## Strict Immutability Guardrails

* **Preserve All Pre-Conversion Baselines Intact:**
  * Do **NOT** modify [`BEFORE_NET48_LOC_Statistics_2026-11-22_17-23-51.md`](../LOC-statistics_plus_net48-retargeting/BEFORE_NET48_LOC_Statistics_2026-11-22_17-23-51.md), [`Simple_Retargeting_vs_Optimization_NET48_Win7_x64_2026-11-22_17-36-26.md`](../LOC-statistics_plus_net48-retargeting/Simple_Retargeting_vs_Optimization_NET48_Win7_x64_2026-11-22_17-36-26.md), [`doc_before-net48/*`](../doc_before-net48/00_INDEX.md), [`Implementation-Plans_doc_before-net48/*`](../Implementation-Plans_doc_before-net48/), [`Implementation-Plans_net48-retargeting-ONLY/*`](../Implementation-Plans_net48-retargeting-ONLY/), [`VNIR.md`](../VNIR.md), [`DRMA.md`](../DRMA.md), or [`src/SomaFrameQD.Tests/TestData/*`](../src/SomaFrameQD.Tests/TestData/).

---

## Exact Execution Steps

### Step 8.1 — Clean-Room End-to-End `x64` Build, Test, Pack, PE32+ Header & Artifact Audit

Execute a full clean-room verification from `C:\SOFTDEV\SomaFrameQD`:

1. **Purge all cached build, test, and package outputs:** Remove `build\packages`, `src\SomaFrameQD.Tests\TestResults`, and every `bin` and `obj` directory under `src\` and `eng\compatibility\Caliburn5PackageHost`.
2. **Restore, Build (`Release`), Test (`[net48|x64]`), Pack, and Validate:**
   * `dotnet restore src\SomaFrameQD.sln --configfile src\NuGet.config`
   * `dotnet build src\SomaFrameQD.sln --configuration Release --no-restore` (`0` warnings, `0` errors)
   * `dotnet test src\SomaFrameQD.Tests\SomaFrameQD.Tests.csproj --configuration Release --framework net48 --no-build --no-restore` (confirming **`[net48|x64]`** banner and 100% pass rate across all original + Stage 2 tests)
   * `dotnet pack src\SomaFrameQD.sln --configuration Release --no-build --no-restore --output build\packages`
   * `powershell -NoProfile -ExecutionPolicy Bypass -File .\eng\validate-artifacts.ps1`
3. **Forensic Post-Optimization Verification Against Locked Decisions (`Q1`–`Q7`):**
   * **`Q1 = 1A` (`x64` Executables + `AnyCPU` Libraries):** Inspect PE headers (`IMAGE_FILE_MACHINE_AMD64` `0x8664` on `SomaFrameQD.Demo.exe`, `SomaFrameQD.Tests.exe`, and `Caliburn5PackageHost.exe`; `AnyCPU` IL-only on all 8 `.nupkg` DLLs).
   * **`Q2 = 2A` (System-DPI) & OS GUIDs:** Verify embedded Win32 manifest inside `SomaFrameQD.Demo.exe` contains all 4 `<supportedOS>` GUIDs (`Win7 SP1` → `Win10/11`) and `<dpiAwareness>system</dpiAwareness>`.
   * **`Q3 = 3B` (Clean Removal):** Confirm `0` `PublishSingleFile` symbols remain anywhere in `src\` and `DemoAppBootstrapper` is wired cleanly in `Demo/App.xaml`.
   * **`Q4 = 4A` & `Q5 = 5A`:** Confirm MEF vendor skip-filter and Multi-Core JIT + `.debug.log` fallback diagnostics are verified by unit tests.
   * **`Q6 = 6A` (`portable` PDBs):** Verify Portable PDB magic header (`BSJB`) across all 8 `.snupkg` symbol packages and `Demo`/`Tests` PDBs.
   * **`Q7 = 7A` (`///` Roslyn `AppDomain` Code Docs):** Confirm `///` XML remarks exist on `ICodeCompiler`, `CodeCompiler`, and `HelixViewModel`.
   * **GAC & Shaders:** Confirm **zero** `System.Drawing.Common.dll` or `System.ComponentModel.Composition` NuGet dependencies exist in `build\packages\*.nupkg` or `bin\Release\net48\`, all 3 pixel shaders (`GrayscaleEffect.ps`, `AddEffect.ps`, `MultiplyEffect.ps`) have `ps_2_0` header `0xFFFF0200`, and `AvalonDock501/ApplicationState.bin` SHA-256 is untouched.

---

### Step 8.2 — Author the `Windows 7 SP1 x64` → `Windows 11 x64` Deployment & QA Compatibility Guide

Create `C:\SOFTDEV\SomaFrameQD\doc\net48-win7-to-win11-x64-deployment-guide.md` documenting:
1. **Target OS Matrix (`64-bit x64`):**
   * **Windows 7 SP1 `x64` / Windows Server 2008 R2 SP1 `x64`:** Mandatory Microsoft prerequisite KBs (`KB4019990` for `d3dcompiler_47.dll`, plus SHA-2 updates `KB4474419` and `KB4490628`), Aero Glass vs. Classic/Basic/RDP `1px` border fallback, and automatic TLS 1.2 enablement.
   * **Windows 8.1 `x64`, Windows 10 `x64`, and Windows 11 `x64`:** Built-in `.NET Framework 4.8` / `4.8.1` runtime behavior, System-DPI (`2A`) stability across multi-monitor setups, Windows 11 Snap Layouts / borderless `MetroWindow` chrome, and native `SystemDefault` TLS 1.2 + TLS 1.3 negotiation.
2. **Runtime Diagnostics & Plugin Author Reference:**
   * Location of Multi-Core JIT profiles (`%LocalAppData%\<EntryAssemblyName>\JitProfiles`) and fallback diagnostic log (`%LocalAppData%\<EntryAssemblyName>\Logs\SomaFrameQD.JitFallback.debug.log` or `%TEMP%` fallback);
   * `ShouldInspectAssemblyFile` MEF probing rules & `AssemblyResolve` strong-name unification;
   * `HwndWrapper` 64-bit `IntPtr` & `IsVisible`-gated VSync guarantees;
   * `ICodeCompiler` Roslyn default-`AppDomain` assembly retention & debouncing guidance.

---

### Step 8.3 — Generate Timestamped `AFTER_NET48_LOC_Statistics_<timestamp>.md` Report

* **Output Directory:** `C:\SOFTDEV\SomaFrameQD\LOC-statistics_plus_net48-retargeting\`
* **Filename Pattern:** `AFTER_NET48_LOC_Statistics_<YYYY-MM-DD_HH-mm-ss>.md`
* **Exact Methodology Parity with [`BEFORE_NET48_LOC_Statistics_2026-11-22_17-23-51.md`](../LOC-statistics_plus_net48-retargeting/BEFORE_NET48_LOC_Statistics_2026-11-22_17-23-51.md):**
  * Replicate the exact same exclusions, categories (`A1` Handwritten `.cs`, `A2` `.Designer.cs`, `A3` `.xaml`, `A4` `.fx`, `B` Build/Config, `C` `.resx`/XML fixtures, `D` Docs/License), and `Total = SLOC + Comments + Blanks` rules—presenting both an **apples-to-apples comparison** against the `486` core files / `29,538` Core SLOC baseline and full repository totals including documentation/plan folders, with Before-vs-After delta tables.

---

### Step 8.4 — Synchronize `TODO.md`, `AGENTS.md` & `REPO_MAP.md` for Stage 2 Completion

1. **[`C:\SOFTDEV\SomaFrameQD\TODO.md`](../TODO.md):** Check off `[x]` across Stage 2 items (`2.1`–`2.6`), linking the 8 executed plans in `Implementation-Plans_net48-optimizations/` and the generated `AFTER_NET48_LOC_Statistics_<timestamp>.md` report.
2. **[`C:\SOFTDEV\SomaFrameQD\AGENTS.md`](../AGENTS.md) & [`C:\SOFTDEV\SomaFrameQD\REPO_MAP.md`](../REPO_MAP.md):** Update live repository status, GAC dependency matrix (`System.ComponentModel.Composition` + `System.Drawing`), `[net48|x64]` test count, `doc/net48-win7-to-win11-x64-deployment-guide.md`, and `AFTER` LOC report link—verifying zero broken relative links.

---

## Final Stage 2 Completion Gate Criteria

1. Zero-cache clean-room `Release` (`x64`) build, full `[net48|x64]` MSTest characterization + optimization suite, `dotnet pack`, and `eng/validate-artifacts.ps1` all exit `0` with `0` errors.
2. Timestamped `AFTER_NET48_LOC_Statistics_<timestamp>.md` report exists in `LOC-statistics_plus_net48-retargeting\` with exact Before-vs-After deltas.
3. All immutable pre-conversion baseline snapshots (`doc_before-net48/`, `BEFORE_NET48_LOC_Statistics_2026-11-22_17-23-51.md`, `TestData/`) remain 100% untouched, and all relative Markdown links resolve cleanly on disk.
