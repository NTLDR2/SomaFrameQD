# Optimization Implementation Plan #4 — WPF Render-Loop Lifecycle Gating, VS2013 Theme Brush Freezing & `ps_2_0` Pixel Shader Hardening

> **Purpose:** Eliminate continuous 60–144 Hz `CompositionTarget.Rendering` idle CPU/GPU wakeups in `HwndWrapper` when controls are hidden or in inactive AvalonDock tabs, freeze ~340 immutable `SolidColorBrush` palette resources across `LightTheme.xaml`, `BlueTheme.xaml`, and `DarkTheme.xaml` (`PresentationOptions:Freeze="True"`) to cut CLR 4.8 WPF dependency-object memory/event overhead, and verify `ps_2_0` software/hardware `ShaderEffect` safety across Windows 7 SP1 (including GPU-less VMs/RDP) through Windows 11.  
> **Generated Timestamp:** `2026-11-22T20:22:00-05:00` (`2026-11-22`)  
> **Prerequisite:** **[Plans #1](./Plan_01_Open_Questions_Native_GAC_References_x64_and_Binding_Redirects.md)–[#3](./Plan_03_Win32_x64_Pointer_ABI_and_HwndHost_Interop_Hardening.md) MUST be 100% completed and verified green first.**  
> **Target Platform Continuum:** `.NET Framework 4.8` (`net48`), **Windows 7 SP1 `x64` → Windows 8.1 `x64` → Windows 10 `x64` → Windows 11 `x64`**.  
> **Execution Order:** [Plan #1](./Plan_01_Open_Questions_Native_GAC_References_x64_and_Binding_Redirects.md) → [Plan #2](./Plan_02_CLR48_Runtime_SKU_AppManifest_Startup_JIT_and_MEF_Probing.md) → [Plan #3](./Plan_03_Win32_x64_Pointer_ABI_and_HwndHost_Interop_Hardening.md) → **Plan #4 (This Plan)** → [Plan #5](./Plan_05_WindowChrome_Docking_DPI_and_Win7_to_Win11_OS_Adaptation.md) → [Plan #6](./Plan_06_Standalone_Modules_Part1_CodeCompiler_CodeEditor_ErrorList_Output.md) → [Plan #7](./Plan_07_Standalone_Modules_Part2_Inspector_GraphEditor_PropertyGrid_and_Demo.md) → [Plan #8](./Plan_08_End_to_End_x64_Win7_to_Win11_Verification_and_After_LOC_Report.md).  
> **Reference Documents:** [`TODO.md`](../TODO.md) (`Plan 2.4` Part A) | [`DRMA.md`](../DRMA.md) (`S2-4`, `S2-6`) | [`doc_before-net48/05_Themes_Rendering_and_Win32_Interop.md`](../doc_before-net48/05_Themes_Rendering_and_Win32_Interop.md) | [`AGENTS.md`](../AGENTS.md)

---

## Stage 2 Eight-Plan Sequential Roadmap (`Implementation-Plans_net48-optimizations`)

| Plan | Optimization Focus (Win7 SP1 `x64` → Win11 `x64`) | Context Budget | Gate Verification |
| :--- | :--- | :--- | :--- |
| **[Plan #1](./Plan_01_Open_Questions_Native_GAC_References_x64_and_Binding_Redirects.md)** | Locked Decisions (`Q1`–`Q7`) + Native `net48` GAC references (`System.ComponentModel.Composition`, `System.Drawing`), `x64` target config (`1A`), `portable` PDBs (`6A`), automatic binding redirects, & lockstep `package-contract.json` | `~15` files (`~650` lines) | Clean `x64` build; `SomaFrameQD.Tests` runs as `[net48\|x64]`; `validate-artifacts.ps1` passes |
| **[Plan #2](./Plan_02_CLR48_Runtime_SKU_AppManifest_Startup_JIT_and_MEF_Probing.md)** | CLR 4.8 `sku="v4.8"` (`app.config`), `app.manifest` (`<supportedOS>` Win7 SP1 → Win10/11 + System-DPI `2A`), **clean removal** of `PublishSingleFile` APIs + clean `net48` `DemoAppBootstrapper.cs` (`3B`), filtered MEF probing (`4A`), Multi-Core JIT + `.debug.log` fallback logger (`5A`), & `AssemblyResolve` unification | `~7` files (`~650` lines) | Clean `x64` build + startup/MEF/JIT-fallback-log/assembly-unification tests pass |
| **[Plan #3](./Plan_03_Win32_x64_Pointer_ABI_and_HwndHost_Interop_Hardening.md)** | 64-bit (`x64`) Win32 P/Invoke & `HwndHost` hardening (`NativeMethods.cs`, `HwndWrapper.cs`, `ClippingHwndHost.cs`, `WindowOptionsBehavior.cs`) | `~6` files (`~950` lines) | Clean `x64` build + dedicated 64-bit pointer/message overflow unit tests pass |
| **Plan #4 (This Plan)** | `HwndWrapper` `CompositionTarget.Rendering` visibility/loaded gating, `po:Freeze="True"` across ~340 VS2013 `SolidColorBrush` resources, & `ps_2_0` `GrayscaleEffect` check | `~8` files (`~1,050` lines) | Clean `x64` build + render-loop gating & frozen theme dictionary tests pass |
| **[Plan #5](./Plan_05_WindowChrome_Docking_DPI_and_Win7_to_Win11_OS_Adaptation.md)** | Cross-OS Window/Docking adaptation (`Win7 SP1` Aero & Classic/RDP ↔ `Win10`/`Win11` Snap/High-DPI), work-area clamping, TLS 1.2/1.3 safety, & buffered `ApplicationState.bin` I/O | `~8` files (`~750` lines) | Clean `x64` build + persistence/chrome/bounds/TLS characterization tests pass |
| **[Plan #6](./Plan_06_Standalone_Modules_Part1_CodeCompiler_CodeEditor_ErrorList_Output.md)** | Standalone Modules Part 1 (`CodeCompiler`, `Output`, `ErrorList`, `CodeEditor`) | `~9` files (`~650` lines) | Clean `x64` build + combined `AvalonDock` × `CodeCompiler` `x64` test passes |
| **[Plan #7](./Plan_07_Standalone_Modules_Part2_Inspector_GraphEditor_PropertyGrid_and_Demo.md)** | Standalone Modules Part 2 (`Inspector`, `GraphEditor`, `PropertyGrid`) & `SomaFrameQD.Demo` (`FastBitmap`, `HelixViewModel`, `ScreenColorPicker`) | `~10` files (`~900` lines) | Clean `x64` build + `Inspector`, `GraphEditor`, `PropertyGrid`, & `FastBitmap` tests pass |
| **[Plan #8](./Plan_08_End_to_End_x64_Win7_to_Win11_Verification_and_After_LOC_Report.md)** | Clean-room end-to-end `x64` verification, Win7 SP1 → Win11 QA runbook, live doc sync, & timestamped `AFTER_NET48_LOC_Statistics` report | Scripts + root docs (`~500` lines) | All tests & `validate-artifacts.ps1` green on `x64`; `AFTER` LOC delta report generated |

---

## Context-Window Guardrails & Architectural Invariants

* **Context Budget (`8` files, `~1,050` lines):**
  * Read **only** `HwndWrapper.cs`, `LightTheme.xaml` (`245` lines), `BlueTheme.xaml` (`245` lines), `DarkTheme.xaml` (`245` lines), `ThemeManager.cs` (`159` lines), `BuiltInThemeResources.cs`, `ShaderEffectBase.cs` (`27` lines), and `AvalonDockThemeTests.cs` (targeted lines).
  * **Do NOT bulk-read `src/SomaFrameQD/Themes/VS2013/Controls/*.xaml` (`~2,600` lines)!**
* **Non-Negotiable Invariant ([`AGENTS.md`](../AGENTS.md) Invariant #8):**
  * Preserve `ThemeManager.SetCurrentTheme(string name)` lookup by `theme.GetType().Name` (`"LightTheme"`, `"BlueTheme"`, `"DarkTheme"`), AvalonDock 5 dictionary prepending (`BuiltInThemeResources`), and atomic rollback of both `Application` and `MainWindow` dictionaries if any URI fails to load.

---

## Exact File-by-File Implementation Steps

### Step 4.1 — Gate `HwndWrapper` `CompositionTarget.Rendering` Subscription by `IsLoaded && IsVisible`

* **File:** [`C:\SOFTDEV\SomaFrameQD\src\SomaFrameQD\Framework\Controls\HwndWrapper.cs`](../src/SomaFrameQD/Framework/Controls/HwndWrapper.cs)
* **Engineering Rationale ([`TODO.md`](../TODO.md) `Plan 2.4`):**
  * Currently, `HwndWrapper()` subscribes to `CompositionTarget.Rendering += OnCompositionTargetRendering;` in its constructor (line 171) and stays subscribed until `Dispose()` (line 181).
  * In WPF `.NET Framework 4.8`, having *any* active subscriber on `CompositionTarget.Rendering` forces WPF's `MediaContext` (`wpfgfx_v0400.dll`) to wake up on **every VSync frame** (60 Hz on Windows 7 monitors up to 144–240 Hz on modern Windows 11 displays!), draining laptop battery and burning idle CPU/GPU cycles even when the hosted viewport is in a backgrounded AvalonDock document tab (`IsVisible == false` while `ActualWidth` retains its last arranged non-zero width!).
  * **Also note a subtle null crash in unit/headless contexts:** Lines 167–175 dereference `Application.Current.Activated` and `Application.Current.Windows` unconditionally—which throws `NullReferenceException` if an `HwndWrapper` is instantiated in an STA test before `Application.Current` exists.
* **Implementation:**
  1. Track `private bool _isRenderingSubscribed;` and `private bool _isDisposed;`.
  2. Guard `Application.Current != null` in constructor when subscribing to `Activated` / `Deactivated` and checking `Windows`.
  3. Subscribe to `Loaded += OnLoaded;`, `Unloaded += OnUnloaded;`, and `IsVisibleChanged += OnIsVisibleChanged;`.
  4. Centralize subscription state in `UpdateRenderingSubscription()`:
     * Should subscribe iff `!_isDisposed && IsLoaded && IsVisible && _hWnd != IntPtr.Zero`.
     * When transitioning to subscribed: `CompositionTarget.Rendering += OnCompositionTargetRendering; _isRenderingSubscribed = true;`.
     * When transitioning to unsubscribed (hidden tab, collapsed tool pane, unloaded, or disposed): `CompositionTarget.Rendering -= OnCompositionTargetRendering; _isRenderingSubscribed = false;`.
  5. Call `UpdateRenderingSubscription()` at the end of `BuildWindowCore`, `DestroyWindowCore`, `OnLoaded`, `OnUnloaded`, `OnIsVisibleChanged`, and `Dispose(bool)`.

---

### Step 4.2 — Freeze All Immutable VS2013 Theme Palette Brushes (`po:Freeze="True"`)

* **Files:**
  * [`C:\SOFTDEV\SomaFrameQD\src\SomaFrameQD\Themes\VS2013\LightTheme.xaml`](../src/SomaFrameQD/Themes/VS2013/LightTheme.xaml) (`245` lines)
  * [`C:\SOFTDEV\SomaFrameQD\src\SomaFrameQD\Themes\VS2013\BlueTheme.xaml`](../src/SomaFrameQD/Themes/VS2013/BlueTheme.xaml) (`245` lines)
  * [`C:\SOFTDEV\SomaFrameQD\src\SomaFrameQD\Themes\VS2013\DarkTheme.xaml`](../src/SomaFrameQD/Themes/VS2013/DarkTheme.xaml) (`245` lines)
  * [`C:\SOFTDEV\SomaFrameQD\src\SomaFrameQD\Themes\VS2013\Controls\Window.xaml`](../src/SomaFrameQD/Themes/VS2013/Controls/Window.xaml) (line 24 `WindowTitleColorBrush`)
* **Engineering Rationale:**
  * Each of the three VS2013 theme dictionaries defines **~113 `SolidColorBrush` resources** without `PresentationOptions:Freeze="True"`.
  * On `.NET Framework 4.8`, unfrozen `Freezable` resources in `Application.Resources` allocate internal dependency-property change-notification lists for every control instance referencing them (`DynamicResource`), cannot be shared across threads, and increase Gen-2 GC heap scanning overhead.
  * Adding `xmlns:po="http://schemas.microsoft.com/winfx/2006/xaml/presentation/options"` and `xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"` + `mc:Ignorable="po"` on `<ResourceDictionary>` and `po:Freeze="True"` on every static `<SolidColorBrush ... />` entry in `LightTheme.xaml`, `BlueTheme.xaml`, `DarkTheme.xaml`, and `Window.xaml` freezes all ~340 brushes at XAML BAML load time.
  * Additionally, in [`BuiltInThemeResources.cs`](../src/SomaFrameQD/Framework/Themes/BuiltInThemeResources.cs), iterate top-level `Freezable` values in the generated AvalonDock `ThemeResourceDictionary` (`if (entry.Value is Freezable f && !f.IsFrozen && f.CanFreeze) f.Freeze();`) so AvalonDock 5's code-generated VS2013 brushes gain the exact same render-thread/GC optimization!

---

### Step 4.3 — Audit & Harden `ShaderEffectBase<T>` & `GrayscaleEffect.ps` (`ps_2_0` Cross-OS Safety)

* **Files:** [`src/SomaFrameQD/Framework/ShaderEffects/ShaderEffectBase.cs`](../src/SomaFrameQD/Framework/ShaderEffects/ShaderEffectBase.cs), [`ShaderEffectUtility.cs`](../src/SomaFrameQD/Framework/ShaderEffects/ShaderEffectUtility.cs), and [`GrayscaleEffect.cs`](../src/SomaFrameQD/Framework/ShaderEffects/GrayscaleEffect.cs)
* **Verification & Hardening (`DRMA.md` Risk `S2-6`):**
  * Confirm `GrayscaleEffect.ps` (`236` bytes) uses DirectX Shader Model **`ps_2_0`** (`0xFFFF0200` header word in `.ps` bytecode)—never `ps_3_0`—guaranteeing WPF 4.8 renders disabled menu/toolbar icons cleanly in **both** hardware Direct3D9Ex (Windows 7 Aero → Windows 11) **and** WPF CPU software rasterization (Windows 7 SP1 VMs without vGPU, Windows Basic/Classic, and unaccelerated RDP sessions).
  * Ensure `GrayscaleEffect()` calls `UpdateShaderValue(InputProperty)` in its constructor so WPF MILCore initializes sampler register `s0` deterministically on first attach across all OS versions.

---

### Step 4.4 — Add Render-Loop & Frozen-Theme Characterization Tests

* **Files:** [`src/SomaFrameQD.Tests/Modules/Shell/AvalonDockThemeTests.cs`](../src/SomaFrameQD.Tests/Modules/Shell/AvalonDockThemeTests.cs) & [`src/SomaFrameQD.Tests/Framework/Win32/Win32X64InteropTests.cs`](../src/SomaFrameQD.Tests/Framework/)
* **Tests to Add:**
  1. `BuiltInThemes_AllSolidColorBrushResources_AreFrozenAfterLoad`: Loads `LightTheme`, `BlueTheme`, and `DarkTheme` and asserts every `SolidColorBrush` in the SomaFrameQD palette dictionaries has `IsFrozen == true`.
  2. `HwndWrapper_RenderLoop_FiresOnlyWhileLoadedAndVisible`: Verifies `HwndWrapper` does not invoke `Render` or retain active VSync subscription when `Visibility = Visibility.Collapsed` or after `Unloaded` / `Dispose()`.
  3. `GrayscaleEffect_BytecodeHeader_IsShaderModelPs20ForWin7SoftwareFallback`: Reads embedded `GrayscaleEffect.ps` stream and asserts the `ps_2_0` version token (`0xFFFF0200`) and clean instantiation on STA.

---

## Plan #4 Verification Gate (Must Pass Before Starting Plan #5)

```powershell
dotnet build src\SomaFrameQD.sln --configuration Release --no-restore
dotnet test src\SomaFrameQD.Tests\SomaFrameQD.Tests.csproj --configuration Release --framework net48 --no-build --no-restore
```

### Gate Pass Criteria
1. Clean `Release` (`x64`) build completes with `0 Warning(s)` and `0 Error(s)`.
2. All existing tests (including all 5 `AvalonDockThemeTests` rollback/bridging tests) plus the 3 new theme-freezing, `HwndWrapper` visibility-gating, and `ps_2_0` shader tests pass on `[net48|x64]`.

Once both criteria pass, proceed to **[Optimization Implementation Plan #5](./Plan_05_WindowChrome_Docking_DPI_and_Win7_to_Win11_OS_Adaptation.md)**.
