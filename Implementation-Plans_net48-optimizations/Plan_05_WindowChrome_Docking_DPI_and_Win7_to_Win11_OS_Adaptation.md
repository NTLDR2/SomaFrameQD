# Optimization Implementation Plan #5 — Cross-OS Window Chrome, Docking, Multi-Monitor Bounds, TLS Safety & Buffered State Persistence (`Win7 SP1` → `Win11`)

> **Purpose:** Harden `SomaFrameQD`'s shell window chrome (`MahApps.Metro` `MetroWindow`), AvalonDock 5 floating/docked windows, multi-monitor bounds clamping under **System-DPI awareness (`Q2 = 2A`)**, typography, network TLS negotiation, and `ApplicationState.bin` disk I/O across the **entire 64-bit OS continuum from Windows 7 SP1 `x64` (Aero Glass, Basic/Classic non-DWM, and RDP) through Windows 8.1, Windows 10, and Windows 11 `x64` (Snap Layouts, High-DPI, and TLS 1.3)**.  
> **Generated / Updated Timestamp:** `2026-11-22T20:58:30-05:00` (`2026-11-22` — Updated with Locked User Decision `Q2 = 2A` System-DPI Everywhere)  
> **Prerequisite:** **[Plans #1](./Plan_01_Open_Questions_Native_GAC_References_x64_and_Binding_Redirects.md)–[#4](./Plan_04_WPF_RenderLoop_Lifecycle_and_VS2013_Theme_Resource_Freezing.md) MUST be 100% completed and verified green first.**  
> **Target Platform Continuum:** `.NET Framework 4.8` (`net48`), **Windows 7 SP1 `x64` → Windows 8.1 `x64` → Windows 10 `x64` → Windows 11 `x64`**.  
> **Execution Order:** [Plan #1](./Plan_01_Open_Questions_Native_GAC_References_x64_and_Binding_Redirects.md) → [Plan #2](./Plan_02_CLR48_Runtime_SKU_AppManifest_Startup_JIT_and_MEF_Probing.md) → [Plan #3](./Plan_03_Win32_x64_Pointer_ABI_and_HwndHost_Interop_Hardening.md) → [Plan #4](./Plan_04_WPF_RenderLoop_Lifecycle_and_VS2013_Theme_Resource_Freezing.md) → **Plan #5 (This Plan)** → [Plan #6](./Plan_06_Standalone_Modules_Part1_CodeCompiler_CodeEditor_ErrorList_Output.md) → [Plan #7](./Plan_07_Standalone_Modules_Part2_Inspector_GraphEditor_PropertyGrid_and_Demo.md) → [Plan #8](./Plan_08_End_to_End_x64_Win7_to_Win11_Verification_and_After_LOC_Report.md).  
> **Reference Documents:** [`TODO.md`](../TODO.md) (`Plan 2.4` Part B) | [`DRMA.md`](../DRMA.md) (`S2-3`, `S2-4`) | [`doc_before-net48/04_Shell_Docking_Persistence_and_BuiltIn_Modules.md`](../doc_before-net48/04_Shell_Docking_Persistence_and_BuiltIn_Modules.md) | [`AGENTS.md`](../AGENTS.md)

---

## Stage 2 Eight-Plan Sequential Roadmap (`Implementation-Plans_net48-optimizations`)

| Plan | Optimization Focus (Win7 SP1 `x64` → Win11 `x64`) | Context Budget | Gate Verification |
| :--- | :--- | :--- | :--- |
| **[Plan #1](./Plan_01_Open_Questions_Native_GAC_References_x64_and_Binding_Redirects.md)** | Locked Decisions (`Q1`–`Q7`) + Native `net48` GAC references (`System.ComponentModel.Composition`, `System.Drawing`), `x64` target config (`1A`), `portable` PDBs (`6A`), automatic binding redirects, & lockstep `package-contract.json` | `~15` files (`~650` lines) | Clean `x64` build; `SomaFrameQD.Tests` runs as `[net48\|x64]`; `validate-artifacts.ps1` passes |
| **[Plan #2](./Plan_02_CLR48_Runtime_SKU_AppManifest_Startup_JIT_and_MEF_Probing.md)** | CLR 4.8 `sku="v4.8"` (`app.config`), `app.manifest` (`<supportedOS>` Win7 SP1 → Win10/11 + System-DPI `2A`), **clean removal** of `PublishSingleFile` APIs + clean `net48` `DemoAppBootstrapper.cs` (`3B`), filtered MEF probing (`4A`), Multi-Core JIT + `.debug.log` fallback logger (`5A`), & `AssemblyResolve` unification | `~7` files (`~650` lines) | Clean `x64` build + startup/MEF/JIT-fallback-log/assembly-unification tests pass |
| **[Plan #3](./Plan_03_Win32_x64_Pointer_ABI_and_HwndHost_Interop_Hardening.md)** | 64-bit (`x64`) Win32 P/Invoke & `HwndHost` hardening (`NativeMethods.cs`, `HwndWrapper.cs`, `ClippingHwndHost.cs`, `WindowOptionsBehavior.cs`) | `~6` files (`~950` lines) | Clean `x64` build + dedicated 64-bit pointer/message overflow unit tests pass |
| **[Plan #4](./Plan_04_WPF_RenderLoop_Lifecycle_and_VS2013_Theme_Resource_Freezing.md)** | `HwndWrapper` `CompositionTarget.Rendering` visibility/loaded gating, `po:Freeze="True"` across ~340 VS2013 `SolidColorBrush` resources, & `ps_2_0` `GrayscaleEffect` check | `~8` files (`~1,050` lines) | Clean `x64` build + render-loop gating & frozen theme dictionary tests pass |
| **Plan #5 (This Plan)** | Cross-OS Window/Docking adaptation (`Win7 SP1` Aero & Classic/RDP ↔ `Win10`/`Win11` Snap + System-DPI `2A`), work-area clamping, TLS 1.2/1.3 safety, & buffered `ApplicationState.bin` I/O | `~8` files (`~750` lines) | Clean `x64` build + persistence/chrome/bounds/TLS characterization tests pass |
| **[Plan #6](./Plan_06_Standalone_Modules_Part1_CodeCompiler_CodeEditor_ErrorList_Output.md)** | Standalone Modules Part 1 (`CodeCompiler` + `///` Roslyn/`AppDomain` docs `7A`, `Output`, `ErrorList`, `CodeEditor`) | `~9` files (`~650` lines) | Clean `x64` build + combined `AvalonDock` × `CodeCompiler` `x64` test passes |
| **[Plan #7](./Plan_07_Standalone_Modules_Part2_Inspector_GraphEditor_PropertyGrid_and_Demo.md)** | Standalone Modules Part 2 (`Inspector`, `GraphEditor`, `PropertyGrid`) & `SomaFrameQD.Demo` (`FastBitmap`, `HelixViewModel` + `///` docs `7A`, `ScreenColorPicker`) | `~10` files (`~900` lines) | Clean `x64` build + `Inspector`, `GraphEditor`, `PropertyGrid`, & `FastBitmap` tests pass |
| **[Plan #8](./Plan_08_End_to_End_x64_Win7_to_Win11_Verification_and_After_LOC_Report.md)** | Clean-room end-to-end `x64` verification, Win7 SP1 → Win11 QA runbook, live doc sync, & timestamped `AFTER_NET48_LOC_Statistics` report | Scripts + root docs (`~500` lines) | All tests & `validate-artifacts.ps1` green on `x64`; `AFTER` LOC delta report generated |

---

## Context-Window Guardrails & Architectural Invariants

* **Context Budget (`~8` files, `~750` targeted lines):**
  * [`src/SomaFrameQD/Modules/MainWindow/Views/MainWindowView.xaml`](../src/SomaFrameQD/Modules/MainWindow/Views/MainWindowView.xaml), [`MainWindowView.xaml.cs`](../src/SomaFrameQD/Modules/MainWindow/Views/MainWindowView.xaml.cs), [`MainWindowViewModel.cs`](../src/SomaFrameQD/Modules/MainWindow/ViewModels/MainWindowViewModel.cs)
  * [`src/SomaFrameQD/Themes/VS2013/Controls/Window.xaml`](../src/SomaFrameQD/Themes/VS2013/Controls/Window.xaml) and [`WindowCommands.xaml`](../src/SomaFrameQD/Themes/VS2013/Controls/WindowCommands.xaml)
  * [`src/SomaFrameQD/Modules/Shell/Services/LayoutItemStatePersister.cs`](../src/SomaFrameQD/Modules/Shell/Services/LayoutItemStatePersister.cs) (stream I/O lines only)
  * [`src/SomaFrameQD/AppBootstrapper.cs`](../src/SomaFrameQD/AppBootstrapper.cs) (TLS initialization hook)
  * Do **not** read unrelated XAML control templates or `.resx` files, and **never** modify `src/SomaFrameQD.Tests/TestData/*` ([`AGENTS.md`](../AGENTS.md) Invariant #7).

---

## Exact File-by-File Implementation Steps

### Step 5.1 — Harden `MainWindowView` (`MetroWindow`) & Window Chrome Under System-DPI (`Q2 = 2A`) Across Win7 SP1 (Aero & Non-DWM Classic/Basic/RDP) Through Win11

* **Files:** [`MainWindowView.xaml`](../src/SomaFrameQD/Modules/MainWindow/Views/MainWindowView.xaml), [`MainWindowView.xaml.cs`](../src/SomaFrameQD/Modules/MainWindow/Views/MainWindowView.xaml.cs), [`MainWindowViewModel.cs`](../src/SomaFrameQD/Modules/MainWindow/ViewModels/MainWindowViewModel.cs), [`Themes/VS2013/Controls/Window.xaml`](../src/SomaFrameQD/Themes/VS2013/Controls/Window.xaml), and [`WindowCommands.xaml`](../src/SomaFrameQD/Themes/VS2013/Controls/WindowCommands.xaml)
* **Cross-OS Engineering Details:**
  1. **DWM Enabled (Win7 Aero Glass, Win8.1, Win10, Win11) vs. Non-DWM (Win7 Classic / Aero Basic / Unaccelerated RDP):**
     * On Windows 8+, DWM desktop composition is always active; on **Windows 7 SP1**, VMs, Classic-theme desktops, and unaccelerated RDP sessions frequently run with DWM disabled (`DwmIsCompositionEnabled == false`).
     * In `MainWindowStyle` (`Themes/VS2013/Controls/Window.xaml`), ensure `BorderThickness="1"` and `BorderBrush="{DynamicResource MainWindowActiveBorder}"` (with inactive trigger setting `BorderBrush="{DynamicResource MainWindowInactiveBorder}"`) accompany `GlowBrush` / `NonActiveGlowBrush`, so non-DWM Windows 7 desktops and RDP sessions always render a crisp `1px` window border while preserving glow + Windows 11 Snap/Maximize bounds when DWM is enabled.
  2. **System-DPI ClearType Text Sharpness (`Q2 = 2A` Locked):**
     * Under System-DPI awareness (`<dpiAware>true</dpiAware>` + `<dpiAwareness>system</dpiAwareness>`), the system DPI factor (`VisualTreeHelper.GetDpi(this)` / `PresentationSource.FromVisual(this).CompositionTarget.TransformToDevice.M11`) is constant for the Windows login session.
     * In `MainWindowView.xaml.cs` on `SourceInitialized`, inspect the device scale factor: apply `TextOptions.SetTextFormattingMode(this, scale <= 1.05 ? TextFormattingMode.Display : TextFormattingMode.Ideal)` so `96 DPI` (`100%`) Windows 7–11 displays get pixel-sharp ClearType menu/toolbar glyphs (`Display`), while `125%`–`200%+` High-DPI displays get smooth subpixel scaling (`Ideal`).
  3. **Work-Area Startup Bounds Clamping (`MainWindowView` / `MainWindowViewModel`):**
     * Clamp initial normal-state `Width`/`Height` (`1000×800` default) against `SystemParameters.WorkArea.Width` and `SystemParameters.WorkArea.Height` on startup when the primary screen work area is smaller (such as `1024×768` Windows 7 VMs/consoles where taskbar leaves `1024×728`), preventing bottom status-bar/resize-grip clipping.
  4. **Zero Win10/11-Only Icon Fonts Verification:**
     * Verify `WindowCommands.xaml` and all core/module XAML files use pure WPF vector `<Path Data="..." />` geometries and `Segoe UI, Lucida Sans Unicode, Verdana` text fallbacks—with **zero** dependency on `Segoe MDL2 Assets` or `Segoe Fluent Icons`.

---

### Step 5.2 — Cross-OS Network Security Protocol Safety (`Windows 7 SP1 TLS 1.2` + `Windows 11 TLS 1.3`)

* **File:** [`src/SomaFrameQD/AppBootstrapper.cs`](../src/SomaFrameQD/AppBootstrapper.cs) (inside `ConfigureCrossOsNetworkDefaults()` called from `PreInitialize()`)
* **Engineering Rationale ([`TODO.md`](../TODO.md) `Plan 2.4`, [`DRMA.md`](../DRMA.md)):**
  * With `sku=".NETFramework,Version=v4.8"` active ([Plan #2](./Plan_02_CLR48_Runtime_SKU_AppManifest_Startup_JIT_and_MEF_Probing.md)), `ServicePointManager.SecurityProtocol` defaults to `SecurityProtocolType.SystemDefault` (`0`):
    * On **Windows 10 and Windows 11**, `SystemDefault` automatically negotiates **TLS 1.2 and TLS 1.3**. **Never overwrite `SecurityProtocol = SecurityProtocolType.Tls12` unconditionally**, which would disable TLS 1.3 on Windows 11!
    * On **Windows 7 SP1 `x64`**, if `DefaultSecureProtocols` has not been set in the Schannel registry, `SystemDefault` can fail TLS 1.2 handshakes unless `SecurityProtocolType.Tls12` is enabled.
  * **Adaptive Win7 → Win11 Solution:** Only when `Environment.OSVersion.Version.Major == 6 && Environment.OSVersion.Version.Minor == 1` (Windows 7 / Server 2008 R2) and `ServicePointManager.SecurityProtocol == SecurityProtocolType.SystemDefault`, safely enable `SecurityProtocolType.Tls12` inside a `try/catch`, while leaving `SecurityProtocolType.SystemDefault` **100% untouched on Windows 8.1, Windows 10, and Windows 11**.

---

### Step 5.3 — Buffered Sequential FileStream I/O for `LayoutItemStatePersister` (`ApplicationState.bin`)

* **File:** [`src/SomaFrameQD/Modules/Shell/Services/LayoutItemStatePersister.cs`](../src/SomaFrameQD/Modules/Shell/Services/LayoutItemStatePersister.cs)
* **Engineering Rationale:**
  * Preserving **100% byte-for-byte compatibility** with the transactional `.tmp` / `.bak` binary envelope + trailing AvalonDock XML format ([`AGENTS.md`](../AGENTS.md) Invariant #7), configure `FileStream` in `SaveState` and `LoadStateAsync` with `bufferSize: 16384` and `FileOptions.SequentialScan` on reads to minimize syscall latency on Windows 7 spinning HDDs and network profiles.

---

### Step 5.4 — Add Cross-OS Chrome, TLS & Persistence Characterization Tests

* **Files:** Targeted tests in `src/SomaFrameQD.Tests/Modules/Shell/` and `src/SomaFrameQD.Tests/Composition/`
* **Tests to Add:**
  1. `MainWindowStyle_DefinesFallbackBorderAndVectorWindowCommandGlyphs_WithoutWin10IconFonts`: Loads `MainWindowView` / `Window.xaml` / `WindowCommands.xaml` on STA and asserts zero `Segoe MDL2 Assets` / `Segoe Fluent Icons` references and valid active/inactive border + glow setters.
  2. `NetworkDefaults_PreservesSystemDefaultOnWin10Win11_AndSupportsTls12OnWin7`: Verifies `ConfigureCrossOsNetworkDefaults()` preserves `SystemDefault` (`0`) on NT `>= 6.2` (Win8–Win11) while guaranteeing `Tls12` on NT `6.1` (Win7 SP1).
  3. Re-run all 43 existing docking and persistence tests (`LayoutItemStatePersisterTests`, `AvalonDockPersistenceTests`, `AvalonDockDockingTests`) to verify zero layout drift.

---

## Plan #5 Verification Gate (Must Pass Before Starting Plan #6)

```powershell
dotnet build src\SomaFrameQD.sln --configuration Release --no-restore
dotnet test src\SomaFrameQD.Tests\SomaFrameQD.Tests.csproj --configuration Release --framework net48 --no-build --no-restore
```

### Gate Pass Criteria
1. Clean `Release` (`x64`) build completes with `0 Warning(s)` and `0 Error(s)`.
2. All characterization tests (including all `AvalonDockPersistenceTests`, `LayoutItemStatePersisterTests`, `AvalonDockDockingTests`, and new chrome/TLS tests) pass on `[net48|x64]`.
3. `TestData/AvalonDock4741/ApplicationState.bin` and `TestData/AvalonDock501/ApplicationState.bin` SHA-256 hashes remain 100% untouched.

Once all 3 criteria pass, proceed to **[Optimization Implementation Plan #6](./Plan_06_Standalone_Modules_Part1_CodeCompiler_CodeEditor_ErrorList_Output.md)**.
