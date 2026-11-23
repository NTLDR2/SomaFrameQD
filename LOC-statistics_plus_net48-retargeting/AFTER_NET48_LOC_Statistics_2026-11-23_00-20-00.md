# SomaFrameQD — "AFTER" `.NET Framework 4.8` (`x64`) Retargeting & Optimization Lines of Code (LOC) Statistics

> **Purpose:** Post-conversion ("After" Stage 1 Retargeting + Stage 2 Optimization) Lines of Code inventory for **SomaFrameQD** targeting **`.NET Framework 4.8` (`net48`)** across **Windows 7 SP1 64-bit (`x64`) through Windows 11 64-bit (`x64`)**, measured with the exact same line-classification rules and directory exclusions as [`BEFORE_NET48_LOC_Statistics_2026-11-22_17-23-51.md`](./BEFORE_NET48_LOC_Statistics_2026-11-22_17-23-51.md).

## Report Metadata & Counting Methodology

| Property | Value |
| :--- | :--- |
| **Generated Timestamp (ISO-8601)** | `2026-11-23T00:20:00-05:00` |
| **Generated Local Time** | `2026-11-23 00:20:00 -05:00` |
| **Report Filename** | `AFTER_NET48_LOC_Statistics_2026-11-23_00-20-00.md` |
| **Baseline Comparison Report** | [`BEFORE_NET48_LOC_Statistics_2026-11-22_17-23-51.md`](./BEFORE_NET48_LOC_Statistics_2026-11-22_17-23-51.md) |
| **Repository Root** | ``C:\SOFTDEV\SomaFrameQD`` |
| **Completed Target Platform** | `.NET Framework 4.8 (net48, x64 PE32+ hosts + AnyCPU libs)` — `Windows 7 SP1 x64 → Windows 11 x64` |
| **Apples-to-Apples Deliverable Files** | 617 files (576 text files + 41 binary assets) |
| **Full On-Disk Files (incl. Plans & Snapshots)** | 646 files (605 text files + 41 binary assets; excludes this self-report) |
| **Excluded Build/Cache Directories** | ``.git/``, ``.vs/``, ``bin/``, ``obj/``, ``artifacts/``, ``build/``, ``packages/``, ``TestResults/``, ``publish/``, ``LOCstatistics/`` |
| **Line Classification Identity** | ``Total Lines = Code Lines (SLOC) + Comment Lines + Blank Lines`` (100% parity-verified against untouched baseline files) |

---

## 1. Executive Summary & Before-vs-After Headline Deltas

### Key Headline Metrics (Apples-to-Apples Deliverable Codebase)

- **Core Source & UI Code (`.cs` + `.xaml` + `.fx`):** **489 files** (`+3` vs. `486`), **31,518 SLOC** (`+1,980` vs. `29,538`), **2,163 comment lines** (`+226` vs. `1,937`), **4,713 blank lines** (`+328` vs. `4,385`), **38,394 total lines** (`+2,534` vs. `35,860`).
  - Removed `eng/compatibility/MahApps.Net10DragHost/Program.cs` (`-1` `.NET 10`-only file) and added **4** new `[net48|x64]` STA test suites (`AppBootstrapperOptimizationTests.cs`, `Win32X64InteropTests.cs`, `CodeCompilerAndOutputTests.cs`, `StandaloneAndDemoOptimizationTests.cs`), raising automated test executions from **148** (`net48`) to **174** (`[net48|x64]`, `+26` executions).
  - Added thorough architectural `//` comments on HWND wrappers (`HwndWrapper.cs`, `ClippingHwndHost.cs`, `WindowOptionsBehavior.cs`, `NativeMethods.cs`) and `///` XML remarks on Roslyn default-`AppDomain` assembly handling (`ICodeCompiler.cs`, `CodeCompiler.cs`, `HelixViewModel.cs`).
- **Deliverable Repository Text Files (Categories A–D, excluding conversion-plan/snapshot meta folders):** **576 files** (`+1` vs. `575`), **38,771 SLOC** (`+2,100` vs. `36,671`), **4,374 comment lines** (`+206`), **5,887 blank lines** (`+353`), **49,032 total lines** (`+2,659` vs. `46,373`).
- **Binary Assets (Unchanged Byte-for-Byte):** **41 files** (797.48 KB — all `TestData` `.bin` fixtures and `GrayscaleEffect.ps` `0xFFFF0200` `ps_2_0` bytecode preserved intact).

### Breakdown by High-Level Category (Apples-to-Apples vs. BEFORE Baseline)

| Category | Before Files | After Files | Δ Files | Before SLOC | After SLOC | Δ SLOC | After Comments | Δ Comments | After Total Lines | Δ Total Lines |
| :--- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| A1: Core C# Handwritten (*.cs) | 410 | 413 | +3 | 24,409 | 26,375 | +1,966 | 1,476 | +226 | 32,183 | +2,520 |
| A2: Core C# Designer-Generated (*.Designer.cs) | 17 | 17 | 0 | 945 | 945 | 0 | 547 | 0 | 1,696 | 0 |
| A3: XAML UI & Themes (*.xaml) | 57 | 57 | 0 | 4,168 | 4,182 | +14 | 140 | 0 | 4,497 | +14 |
| A4: HLSL Shader Source (*.fx) | 2 | 2 | 0 | 16 | 16 | 0 | 0 | 0 | 18 | 0 |
| **Subtotal — Category A: Core Source & UI Code** | **486** | **489** | **+3** | **29,538** | **31,518** | **+1,980** | **2,163** | **+226** | **38,394** | **+2,534** |
| B: Build, Project, Packaging, CI, Manifest & Config | 35 | 33 | -2 | 1,497 | 1,534 | +37 | 145 | -20 | 1,886 | +20 |
| C: Localization Resources & XML Fixtures | 44 | 44 | 0 | 4,337 | 4,337 | 0 | 2,066 | 0 | 6,876 | 0 |
| D: Deliverable Documentation & License | 10 | 10 | 0 | 1,299 | 1,382 | +83 | 0 | 0 | 1,876 | +105 |
| **Grand Total (Deliverable Text Files, Categories A–D)** | **575** | **576** | **+1** | **36,671** | **38,771** | **+2,100** | **4,374** | **+206** | **49,032** | **+2,659** |

---

## 2. Project-by-Project Before-vs-After Comparison

| Project / Top-Level Area | Before Core Files | After Core Files | Before Core SLOC | After Core SLOC | Δ Core SLOC | Before All Text Lines | After All Text Lines | Δ All Text Lines |
| :--- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| `src/SomaFrameQD` | 250 | 250 | 12,365 | 13,019 | +654 | 16,931 | 17,783 | +852 |
| `src/SomaFrameQD.Demo` | 64 | 64 | 3,368 | 3,461 | +93 | 4,718 | 4,867 | +149 |
| `src/SomaFrameQD.Modules.CodeCompiler` | 2 | 2 | 104 | 120 | +16 | 132 | 211 | +79 |
| `src/SomaFrameQD.Modules.CodeEditor` | 21 | 21 | 886 | 935 | +49 | 2,038 | 2,088 | +50 |
| `src/SomaFrameQD.Modules.ErrorList` | 19 | 19 | 669 | 690 | +21 | 1,758 | 1,782 | +24 |
| `src/SomaFrameQD.Modules.GraphEditor` | 23 | 23 | 1,548 | 1,593 | +45 | 2,560 | 2,614 | +54 |
| `src/SomaFrameQD.Modules.Inspector` | 63 | 63 | 2,536 | 2,611 | +75 | 4,129 | 4,217 | +88 |
| `src/SomaFrameQD.Modules.Output` | 11 | 11 | 259 | 323 | +64 | 1,147 | 1,225 | +78 |
| `src/SomaFrameQD.Modules.PropertyGrid` | 8 | 8 | 181 | 181 | 0 | 1,052 | 1,052 | 0 |
| `src/SomaFrameQD.Tests` | 23 | 27 | 7,333 | 8,422 | +1,089 | 8,584 | 9,895 | +1,311 |
| `src/ Root & IDE Config` | 0 | 0 | 0 | 0 | 0 | 218 | 238 | +20 |
| **Subtotal — `src/` Solution Tree** | **484** | **488** | **29,249** | **31,355** | **+2,106** | **43,267** | **45,972** | **+2,705** |
| `eng/compatibility/Caliburn5PackageHost` | 1 | 1 | 163 | 163 | 0 | 219 | 223 | +4 |
| `eng/compatibility/MahApps.Net10DragHost` | 1 | 0 | 126 | 0 | -126 | 248 | 0 | -248 |
| `eng/ Packaging & Validation` | 0 | 0 | 0 | 0 | 0 | 598 | 608 | +10 |
| `.github/ CI & Workflows` | 0 | 0 | 0 | 0 | 0 | 111 | 109 | -2 |
| `doc/ Documentation` | 0 | 0 | 0 | 0 | 0 | 219 | 322 | +103 |
| `Repository Root Files` | 0 | 0 | 0 | 0 | 0 | 1,711 | 1,798 | +87 |
| **Grand Total (Deliverable Repository Scope)** | **486** | **489** | **29,538** | **31,518** | **+1,980** | **46,373** | **49,032** | **+2,659** |

---

## 3. Deliverable Text Files by Extension

| Extension | Files | Code Lines (SLOC) | Comment Lines | Blank Lines | Total Lines |
| :--- | ---: | ---: | ---: | ---: | ---: |
| `.cs` | 430 | 27,320 | 2,023 | 4,536 | 33,879 |
| `.resx` | 43 | 4,301 | 2,064 | 473 | 6,838 |
| `.xaml` | 57 | 4,182 | 140 | 175 | 4,497 |
| `.md` | 9 | 1,244 | 0 | 457 | 1,701 |
| `.ps1` | 1 | 360 | 58 | 75 | 493 |
| `.csproj` | 11 | 409 | 0 | 57 | 466 |
| `.gitignore` | 1 | 146 | 67 | 48 | 261 |
| `.json` | 5 | 185 | 0 | 0 | 185 |
| `.txt` | 1 | 138 | 0 | 37 | 175 |
| `.sln` | 1 | 134 | 0 | 1 | 135 |
| `.yml` | 2 | 79 | 13 | 17 | 109 |
| `.settings` | 5 | 59 | 0 | 0 | 59 |
| `.config` | 3 | 49 | 1 | 0 | 50 |
| `.editorconfig` | 1 | 45 | 0 | 2 | 47 |
| `.manifest` | 1 | 36 | 6 | 4 | 46 |
| `.xml` | 1 | 36 | 2 | 0 | 38 |
| `.props` | 1 | 24 | 0 | 3 | 27 |
| `.fx` | 2 | 16 | 0 | 2 | 18 |
| `.code-workspace` | 1 | 8 | 0 | 0 | 8 |
| **Total (Deliverable Text)** | **576** | **38,771** | **4,374** | **5,887** | **49,032** |

---

## 4. Full Repository Totals Including Baseline Snapshots, Agent Maps & Implementation Plans

| Engineering Documentation / Plan Suite | Text Files | SLOC | Comments | Blanks | Total Lines |
| :--- | ---: | ---: | ---: | ---: | ---: |
| `AGENTS.md`, `REPO_MAP.md` & `DRMA.md` (Root Agent/Risk Guides) | 3 | 536 | 0 | 117 | 653 |
| `doc_before-net48/` (Immutable Pre-Conversion Baseline Guides `00`–`09`) | 10 | 1,024 | 0 | 309 | 1,333 |
| `Implementation-Plans_doc_before-net48/` (`Plan_01`–`Plan_02`) | 2 | 168 | 0 | 42 | 210 |
| `Implementation-Plans_net48-retargeting-ONLY/` (Stage 1 `Plan_01`–`Plan_04`) | 4 | 859 | 0 | 163 | 1,022 |
| `Implementation-Plans_net48-optimizations/` (Stage 2 `Plan_01`–`Plan_08`) | 8 | 739 | 0 | 229 | 968 |
| `LOC-statistics_plus_net48-retargeting/` (`BEFORE` + Analysis Docs) | 2 | 314 | 0 | 74 | 388 |
| **Subtotal — Conversion Documentation, Baselines & Plans** | **29** | **3,640** | **0** | **934** | **4,574** |
| **Complete On-Disk Repository Grand Total (Deliverable + Meta)** | **605** | **42,411** | **4,374** | **6,821** | **53,606** |

---

## 5. Summary of Key Architectural & Line Changes Across Stages 1 & 2

1. **Obsolete Multi-Targeting & Single-File Bloat Removed:** Deleted `eng/compatibility/MahApps.Net10DragHost` (`-3` files / `-248` lines), `FolderProfileNet5.pubxml` & `FolderProfileNetCore31.pubxml` (`-2` `.pubxml` files / `-43` lines), and all `.NET 5+` `PublishSingleFile` dead branches in `AppBootstrapper.cs` and `DemoAppBootstrapper.cs`.
2. **Native `.NET Framework 4.8` GAC & `x64` `PE32+` Hardening:** Replaced out-of-band `System.ComponentModel.Composition` `6.0.0` and `System.Drawing.Common` `6.0.0` NuGet shims with pre-NGen'd `.NET 4.8` GAC `<Reference>` entries, added `src/SomaFrameQD.Demo/app.manifest` (`+1` `.manifest` file; Win7 SP1 → Win11 `<supportedOS>` GUIDs + System-DPI `Q2 = 2A`), enabled CLR 4.8 `sku=".NETFramework,Version=v4.8"`, configured Multi-Core Background JIT + `.debug.log` fallback diagnostics (`Q5 = 5A`), and froze **562** VS2013 `SolidColorBrush` resources (`po:Freeze="True"`).
3. **64-Bit Interop, Concurrency, High-DPI & Roslyn Safety:** Hardened `NativeMethods.cs`, `HwndWrapper.cs`, `ClippingHwndHost.cs`, and `WindowOptionsBehavior.cs` for 64-bit `IntPtr` ABI + `IsLoaded && IsVisible` VSync gating; eliminated `CodeCompiler` UI-thread `GC.Collect()` pauses and `FastBitmap` finalizer crashes; fixed High-DPI physical-pixel `ScreenColorPicker` bounds; debounced `HelixViewModel` (`350ms`) with cached `MetadataReference[]` and `///` XML Roslyn `AppDomain` documentation (`Q7 = 7A`); and expanded `[net48|x64]` automated verification to **27 test `.cs` files / 174 executions**.

---

*End of report — Generated at `2026-11-23T00:20:00-05:00`.*
