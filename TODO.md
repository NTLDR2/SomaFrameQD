# SomaFrameQD TODO

**Target Platform:** `.NET Framework 4.8` (`net48`) exclusively, targeting **Windows 7 SP1 64-bit (`x64`) and higher**.

**Baseline & Post-Conversion Reference Documents:**
- [BEFORE_NET48_LOC_Statistics_2026-11-22_17-23-51.md](./LOC-statistics_plus_net48-retargeting/BEFORE_NET48_LOC_Statistics_2026-11-22_17-23-51.md) (`486` core code files / `35,860` core lines; `575` total text files / `46,373` total text lines)
- [AFTER_NET48_LOC_Statistics_2026-11-23_00-20-00.md](./LOC-statistics_plus_net48-retargeting/AFTER_NET48_LOC_Statistics_2026-11-23_00-20-00.md) (post-Stage-1 + Stage-2 `net48` `x64` LOC statistics & Before-vs-After deltas)
- [Simple_Retargeting_vs_Optimization_NET48_Win7_x64_2026-11-22_17-36-26.md](./LOC-statistics_plus_net48-retargeting/Simple_Retargeting_vs_Optimization_NET48_Win7_x64_2026-11-22_17-36-26.md)
- [doc/net48-win7-to-win11-x64-deployment-guide.md](./doc/net48-win7-to-win11-x64-deployment-guide.md)
- [VNIR.md](./VNIR.md)

---

## Mandatory Execution Rules & Context-Window Governance

> [!IMPORTANT]
> **1. Strict Separation Between Retargeting and Optimization**  
> **Stage 1 (Simple Retargeting)** and **Stage 2 (Optimization)** are completely separate workstreams. Stage 1 ONLY removes `.NET 10` / `.NET 5+` / `.NET Core 3.1` multi-targeting so the solution compiles, packs, and passes tests on `net48`. Do **NOT** mix runtime, Win32, GAC, or WPF optimizations into Stage 1.
>
> **2. Hard Phase Gate — Retargeting MUST Be 100% Complete First**  
> **Do NOT start any Stage 2 Optimization work** until Stage 1 is 100% finished, built cleanly on `net48`, validated by `eng/validate-artifacts.ps1`, and passing the full `SomaFrameQD.Tests` suite.
>
> **3. Context-Window Management — Split Stage 2 Into Sequential Implementation Plans**  
> Optimization is deep and spans `35,860` core code lines (`46,373` total text lines). Attempting Stage 2 in a single pass risks context-window exhaustion and regressions. Stage 2 **MUST** be divided into **separate, small Implementation Plans (`Plan 2.1` → `Plan 2.2` → `Plan 2.3` → `Plan 2.4` → `Plan 2.5` → `Plan 2.6`) executed strictly in sequential order**:
> - Execute **only one Implementation Plan per session/cycle**.
> - Verify a clean build and passing `SomaFrameQD.Tests` at the gate of **each** plan before advancing to the next.
> - Use targeted file searches and line-range reads rather than loading entire projects, XAML theme folders, or `.resx` files (`6,838` lines) wholesale into context.

---

## Stage 0: Baseline Preparation (Completed)

- [x] Generate timestamped "Before" conversion LOC baseline report ([LOC-statistics_plus_net48-retargeting/BEFORE_NET48_LOC_Statistics_2026-11-22_17-23-51.md](./LOC-statistics_plus_net48-retargeting/BEFORE_NET48_LOC_Statistics_2026-11-22_17-23-51.md)).
- [x] Document Simple Retargeting vs. Optimization engineering differences ([LOC-statistics_plus_net48-retargeting/Simple_Retargeting_vs_Optimization_NET48_Win7_x64_2026-11-22_17-36-26.md](./LOC-statistics_plus_net48-retargeting/Simple_Retargeting_vs_Optimization_NET48_Win7_x64_2026-11-22_17-36-26.md)).

---

## Stage 1: Simple Retargeting to `.NET Framework 4.8` (`net48` Only)

> **Execution Scope:** Mechanical single-targeting only (**~25 build/config/CI/doc files; ~1,700 lines**). Can be planned and executed as a single focused Stage 1 Implementation Plan (or two quick sub-steps: build/project retargeting, then CI/packaging/docs). **NO optimization changes belong here.**

- [x] **1.1 Retarget all `.csproj` files exclusively to `<TargetFramework>net48</TargetFramework>`**
  - Replace `<TargetFrameworks>net48;net10.0-windows</TargetFrameworks>` with `<TargetFramework>net48</TargetFramework>` across:
    - `src/SomaFrameQD/SomaFrameQD.csproj`
    - `src/SomaFrameQD.Demo/SomaFrameQD.Demo.csproj`
    - `src/SomaFrameQD.Modules.CodeCompiler/SomaFrameQD.Modules.CodeCompiler.csproj`
    - `src/SomaFrameQD.Modules.CodeEditor/SomaFrameQD.Modules.CodeEditor.csproj`
    - `src/SomaFrameQD.Modules.ErrorList/SomaFrameQD.Modules.ErrorList.csproj`
    - `src/SomaFrameQD.Modules.GraphEditor/SomaFrameQD.Modules.GraphEditor.csproj`
    - `src/SomaFrameQD.Modules.Inspector/SomaFrameQD.Modules.Inspector.csproj`
    - `src/SomaFrameQD.Modules.Output/SomaFrameQD.Modules.Output.csproj`
    - `src/SomaFrameQD.Modules.PropertyGrid/SomaFrameQD.Modules.PropertyGrid.csproj`
    - `src/SomaFrameQD.Tests/SomaFrameQD.Tests.csproj`
    - `eng/compatibility/Caliburn5PackageHost/Caliburn5PackageHost.csproj`
  - Remove the `net10.0-windows`-only `eng/compatibility/MahApps.Net10DragHost` project.
  - Standardize `<LangVersion>` in `src/Directory.Build.props` if needed once `net10.0-windows` is removed (dropping `net10.0` lowers the SDK default to C# 7.3), and align `src/SomaFrameQD.Modules.CodeEditor/Properties/Resources.resx` (`System.Windows.Forms, Version=4.0.0.0`) if required for clean `net48` resource generation.

- [x] **1.2 Remove `.NET 10` / `.NET 5+` / `.NET Core 3.1` SDK & multi-target build shims**
  - Remove or update the `.NET SDK 10.0.400` pin in `global.json`.
  - Remove multi-target MSBuild `Condition` wrappers in `.csproj` files (`HelixToolkit.Core.Wpf` in `src/SomaFrameQD.Demo/SomaFrameQD.Demo.csproj`, and TFM conditions around `PresentationFramework.Aero` in `src/SomaFrameQD/SomaFrameQD.csproj` and `System.Drawing.Common` in `src/SomaFrameQD.Modules.Inspector/SomaFrameQD.Modules.Inspector.csproj` — keeping existing `net48` packages intact for Stage 1).
  - Remove obsolete `.NET Core 3.1` / `.NET 5` publish profiles (`src/SomaFrameQD.Demo/Properties/PublishProfiles/FolderProfileNet5.pubxml`, `FolderProfileNetCore31.pubxml`) and obsolete TFM debug paths in `src/.vscode/launch.json`.

- [x] **1.3 Update packaging validation, CI workflow, and documentation for `net48`-only**
  - Update `eng/package-contract.json` and `eng/validate-artifacts.ps1` to validate `net48`-only outputs.
  - Update `.github/workflows/ci.yml` to build, test, pack, and validate solely for `net48` (without requiring `.NET SDK 10.0.400`).
  - Update `README.md` and `doc/caliburn-5-migration.md` to reflect `.NET Framework 4.8` (`net48`) targeting.

- [x] **1.4 STAGE 1 MANDATORY COMPLETION GATE (HARD STOP BEFORE STAGE 2)**
  - [x] Clean `net48` solution build succeeds with 0 errors.
  - [x] Full `SomaFrameQD.Tests` suite passes on `net48`.
  - [x] `eng/validate-artifacts.ps1` passes against packed `net48` NuGet artifacts.
  - **STOP & VERIFY:** Only proceed to Stage 2 once all Stage 1 items (`1.1`–`1.4`) above are checked off and green.

---

## Stage 2: Optimization for `.NET Framework 4.8` & Windows 7 SP1 (`x64`+)

> [!WARNING]
> **Prerequisites & Sequential Context-Window Execution Protocol**
> - **Prerequisite:** Stage 1 (`1.1`–`1.4`) MUST be 100% complete and verified green.
> - **Sequential Execution:** Draft and execute **one Implementation Plan at a time** in strict order (`2.1` → `2.2` → `2.3` → `2.4` → `2.5` → `2.6`). Do **not** skip ahead or merge plans together.
> - **Context-Window Discipline:** Never bulk-read `.resx` files (`6,838` lines) or unrelated XAML resource dictionaries into context. Keep each plan strictly inside its file/LOC budget and verify build + tests at every gate.

- [x] **Implementation Plan 2.1 — Native `net48` GAC References, `x64` Target Configuration & Binding Redirects** ([Plan_01](./Implementation-Plans_net48-optimizations/Plan_01_Open_Questions_Native_GAC_References_x64_and_Binding_Redirects.md))
  - *Prerequisite:* Stage 1 complete (`1.4` green) | *Context Budget:* `~20` files / `~900` lines (`.csproj` files, `src/Directory.Build.props`, `eng/package-contract.json`)
  - Replace out-of-band `.NET Core` compatibility NuGets with built-in, pre-NGen'd `.NET Framework 4.8` GAC references:
    - `System.ComponentModel.Composition` (`6.0.0` NuGet) → `<Reference Include="System.ComponentModel.Composition" />` in `src/Directory.Build.props` / `src/SomaFrameQD/SomaFrameQD.csproj`
    - `System.Drawing.Common` (`6.0.0` NuGet) → `<Reference Include="System.Drawing" />` in `src/SomaFrameQD.Modules.Inspector/SomaFrameQD.Modules.Inspector.csproj`
    - Update `eng/package-contract.json` accordingly.
  - Configure 64-bit (`x64` / `<Prefer32Bit>false</Prefer32Bit>`), explicit `<AutoGenerateBindingRedirects>true</AutoGenerateBindingRedirects>` + `<GenerateBindingRedirectsOutputType>true</GenerateBindingRedirectsOutputType>` on executables/tests (for Roslyn and transitive dependencies), and `net48`/Win7-appropriate `portable` PDB symbol settings in `src/Directory.Build.props`.
  - **Gate 2.1:** Verified clean `x64` build, `SomaFrameQD.Tests` (`[net48|x64]`), and `eng/validate-artifacts.ps1` passed.

- [x] **Implementation Plan 2.2 — CLR 4.8 Runtime SKU, `app.manifest`, OS Compatibility & Startup/JIT Optimization** ([Plan_02](./Implementation-Plans_net48-optimizations/Plan_02_CLR48_Runtime_SKU_AppManifest_Startup_JIT_and_MEF_Probing.md))
  - *Prerequisite:* Plan 2.1 complete | *Context Budget:* `~6` files / `~600` lines (`app.config` x2, `app.manifest`, `AppBootstrapper.cs`, `DemoAppBootstrapper.cs`)
  - Update `<supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.6.1"/>` to `sku=".NETFramework,Version=v4.8"` in `src/SomaFrameQD/app.config` and `src/SomaFrameQD.Demo/app.config` so CLR 4.8 and WPF enable `.NET 4.7–4.8` fixes/optimizations instead of running in `v4.6.1` quirk mode.
  - Add/configure `app.manifest` declaring Windows 7 SP1 (`{35138b9a-5d96-4fbd-8e2d-a2440225f93a}`) through Windows 10/11 `<supportedOS>` GUIDs and System-DPI awareness (`<dpiAware>true</dpiAware>` + `<dpiAwareness>system</dpiAwareness>`).
  - Cleanly remove dead `.NET Core 3.1` / `.NET 5+` `PublishSingleFile` detection/bypass code in `src/SomaFrameQD/AppBootstrapper.cs` and refactor `src/SomaFrameQD.Demo/DemoAppBootstrapper.cs` (`App.xaml`), optimize MEF `BaseDirectory` startup assembly probing (`ShouldInspectAssemblyFile`), enable CLR 4.8 Multi-Core JIT (`System.Runtime.ProfileOptimization`) with `.debug.log` fallback diagnostics, and register `AssemblyResolve` strong-name unification.
  - **Gate 2.2:** Verified clean `x64` build and `SomaFrameQD.Tests` passed.

- [x] **Implementation Plan 2.3 — 64-Bit (`x64`) Win32 P/Invoke & `HwndHost` Interop Hardening** ([Plan_03](./Implementation-Plans_net48-optimizations/Plan_03_Win32_x64_Pointer_ABI_and_HwndHost_Interop_Hardening.md))
  - *Prerequisite:* Plan 2.2 complete | *Context Budget:* `~5` files / `~900` lines (`Framework/Win32/NativeMethods.cs`, `Framework/Controls/HwndWrapper.cs`, `ClippingHwndHost.cs`, `WindowOptionsBehavior.cs` + `Win32X64InteropTests.cs`)
  - Fix `x64` `IntPtr` overflow bugs in `src/SomaFrameQD/Framework/Controls/HwndWrapper.cs` (`UncheckedInt32(IntPtr)`, `GetWheelDeltaWParam(IntPtr)`, `GetXLParam(IntPtr)`, `GetYLParam(IntPtr)`), clamp `double.NaN` child window dimensions, and add `//` comments documenting the architectural role of each HWND wrapper.
  - Replace 32-bit `GetWindowLong` / `SetWindowLong` in `src/SomaFrameQD/Framework/Win32/NativeMethods.cs` with 64-bit-safe `GetWindowLongPtr` / `SetWindowLongPtr` (`IntPtr`) patterns and Unicode (`*W`) P/Invoke signatures.
  - Add `x64` interop regression tests in `src/SomaFrameQD.Tests/Framework/Win32/Win32X64InteropTests.cs`.
  - **Gate 2.3:** Verified clean `x64` build and `SomaFrameQD.Tests` passed.

- [x] **Implementation Plan 2.4 — Core Shell, WPF Rendering & Windows 7 SP1 → Windows 11 Compatibility Optimization** ([Plan_04](./Implementation-Plans_net48-optimizations/Plan_04_WPF_RenderLoop_Lifecycle_and_VS2013_Theme_Resource_Freezing.md) & [Plan_05](./Implementation-Plans_net48-optimizations/Plan_05_WindowChrome_Docking_DPI_and_Win7_to_Win11_OS_Adaptation.md))
  - *Prerequisite:* Plan 2.3 complete | *Context Budget:* targeted searches/files in `src/SomaFrameQD` only
  - Optimize `HwndWrapper.cs` `CompositionTarget.Rendering` subscription lifecycle so VSync callbacks attach only when `IsLoaded && IsVisible && _hWnd != IntPtr.Zero`, freeze all `562` VS2013 `SolidColorBrush` resources (`po:Freeze="True"`) plus AvalonDock 5 `Freezable`s, and verify `ps_2_0` `GrayscaleEffect.ps` (`0xFFFF0200`) with frozen `PixelShader`.
  - Harden Windows 7 SP1 → Windows 11 `x64` behavior across MahApps.Metro `MetroWindow` (`1px` non-DWM fallback border + DWM glow), System-DPI ClearType (`Display` vs `Ideal`), `SystemParameters.WorkArea` startup clamping, adaptive Win7 `Tls12` / Win11 `SystemDefault` TLS 1.3 safety, and `16 KB` buffered `ApplicationState.bin` `FileStream` I/O.
  - **Gate 2.4:** Verified clean `x64` build and `SomaFrameQD.Tests` passed.

- [x] **Implementation Plan 2.5 — Standalone Modules (`SomaFrameQD.Modules.*`) & `SomaFrameQD.Demo` Optimization Pass** ([Plan_06](./Implementation-Plans_net48-optimizations/Plan_06_Standalone_Modules_Part1_CodeCompiler_CodeEditor_ErrorList_Output.md) & [Plan_07](./Implementation-Plans_net48-optimizations/Plan_07_Standalone_Modules_Part2_Inspector_GraphEditor_PropertyGrid_and_Demo.md))
  - *Prerequisite:* Plan 2.4 complete | *Split across `Plan_06` (Part 1) and `Plan_07` (Part 2)*
  - Optimized `CodeCompiler` (removed UI-thread `GC.Collect()`, handled `Hidden` + `Location.None` diagnostics, added `///` XML docs on `.NET 4.8` `AppDomain` retention, verified `AvalonDock` × `Roslyn` `x64` coexistence), `Output` (lock-protected `StringBuilder` + coalesced background UI flushes), `ErrorList` (short-circuited unfiltered `FilteredItems`), `CodeEditor` (`BaseDirectory` `.xshd` probing + `DtdProcessing.Prohibit` + ordinal dirty check), `Inspector` (Win32 physical-pixel `ScreenColorPicker` / `ScreenShotUtility` for High-DPI multi-monitor setups), `GraphEditor` (`ConnectorItem` `Loaded`/`Unloaded` `LayoutUpdated` gating), and `SomaFrameQD.Demo` (removed `~FastBitmap()` finalizer crash + 64-bit `long` pointer strides; debounced `HelixViewModel` `350ms` + cached `MetadataReference[]` + active-tab VSync gating + `///` Roslyn docs).
  - **Gate 2.5:** Verified clean `x64` build and `SomaFrameQD.Tests` (`174/174` on `[net48|x64]`) passed.

- [x] **Implementation Plan 2.6 — Final End-to-End `x64` Validation, Deployment Guide & "AFTER" LOC Statistics Report** ([Plan_08](./Implementation-Plans_net48-optimizations/Plan_08_End_to_End_x64_Win7_to_Win11_Verification_and_After_LOC_Report.md))
  - *Prerequisite:* Plan 2.5 complete | *Context Budget:* build/test verification, deployment guide & LOC script output
  - Executed zero-cache clean-room `x64` build, full `174`-execution `SomaFrameQD.Tests` `[net48|x64]` suite, `Caliburn5PackageHost`, `eng/validate-artifacts.ps1`, and forensic `Q1`–`Q7` binary/manifest/PDB/shader/GAC audit.
  - Authored [doc/net48-win7-to-win11-x64-deployment-guide.md](./doc/net48-win7-to-win11-x64-deployment-guide.md) and generated timestamped **"AFTER"** LOC statistics report ([AFTER_NET48_LOC_Statistics_2026-11-23_00-20-00.md](./LOC-statistics_plus_net48-retargeting/AFTER_NET48_LOC_Statistics_2026-11-23_00-20-00.md)) comparing against [BEFORE_NET48_LOC_Statistics_2026-11-22_17-23-51.md](./LOC-statistics_plus_net48-retargeting/BEFORE_NET48_LOC_Statistics_2026-11-22_17-23-51.md).
