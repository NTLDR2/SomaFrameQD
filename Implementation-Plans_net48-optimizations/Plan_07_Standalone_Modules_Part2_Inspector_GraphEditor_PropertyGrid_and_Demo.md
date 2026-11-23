# Optimization Implementation Plan #7 — Standalone Modules Part 2 (`Inspector`, `GraphEditor`, `PropertyGrid`) & `SomaFrameQD.Demo` Optimization (`Win7 SP1` → `Win11` `x64`)

> **Purpose:** Fix the High-DPI / multi-monitor physical-pixel screen-capture bug in `Inspector`'s `ScreenColorPicker` across Windows 7 SP1 (`125%`/`150%` System DPI) through Windows 11 (`150%`/`200%+` multi-monitor setups), eliminate idle layout-pass churn in `GraphEditor`'s `ConnectorItem`, fix the **fatal CLR 4.8 GC Finalizer Thread `DispatcherObject` crash** and 64-bit pointer arithmetic in `SomaFrameQD.Demo`'s `FastBitmap`, and debounce/cache live Roslyn compilation in `HelixViewModel` with **explicit `///` XML code comments explaining Roslyn assembly handling on `.NET Framework 4.8` (`Q7 = 7A` Locked)**.  
> **Generated / Updated Timestamp:** `2026-11-22T20:58:30-05:00` (`2026-11-22` — Updated with Locked User Decisions `Q2=2A`, `Q3=3B`, `Q7=7A` + `///` Roslyn Docs)  
> **Prerequisite:** **[Plans #1](./Plan_01_Open_Questions_Native_GAC_References_x64_and_Binding_Redirects.md)–[#6](./Plan_06_Standalone_Modules_Part1_CodeCompiler_CodeEditor_ErrorList_Output.md) MUST be 100% completed and verified green first.**  
> **Target Platform Continuum:** `.NET Framework 4.8` (`net48`), **Windows 7 SP1 `x64` → Windows 8.1 `x64` → Windows 10 `x64` → Windows 11 `x64`**.  
> **Execution Order:** [Plan #1](./Plan_01_Open_Questions_Native_GAC_References_x64_and_Binding_Redirects.md) → [Plan #2](./Plan_02_CLR48_Runtime_SKU_AppManifest_Startup_JIT_and_MEF_Probing.md) → [Plan #3](./Plan_03_Win32_x64_Pointer_ABI_and_HwndHost_Interop_Hardening.md) → [Plan #4](./Plan_04_WPF_RenderLoop_Lifecycle_and_VS2013_Theme_Resource_Freezing.md) → [Plan #5](./Plan_05_WindowChrome_Docking_DPI_and_Win7_to_Win11_OS_Adaptation.md) → [Plan #6](./Plan_06_Standalone_Modules_Part1_CodeCompiler_CodeEditor_ErrorList_Output.md) → **Plan #7 (This Plan)** → [Plan #8](./Plan_08_End_to_End_x64_Win7_to_Win11_Verification_and_After_LOC_Report.md).  
> **Reference Documents:** [`TODO.md`](../TODO.md) (`Plan 2.5` Part B) | [`DRMA.md`](../DRMA.md) (`S2-5`, `S2-6`) | [`doc_before-net48/06_Standalone_Modules_Reference.md`](../doc_before-net48/06_Standalone_Modules_Reference.md) | [`doc_before-net48/07_Demo_Application_and_Extension_Patterns.md`](../doc_before-net48/07_Demo_Application_and_Extension_Patterns.md) | [`AGENTS.md`](../AGENTS.md)

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
| **Plan #7 (This Plan)** | Standalone Modules Part 2 (`Inspector`, `GraphEditor`, `PropertyGrid`) & `SomaFrameQD.Demo` (`FastBitmap`, `HelixViewModel` + `///` docs `7A`, `ScreenColorPicker`) | `~10` files (`~900` lines) | Clean `x64` build + `Inspector`, `GraphEditor`, `PropertyGrid`, & `FastBitmap` tests pass |
| **[Plan #8](./Plan_08_End_to_End_x64_Win7_to_Win11_Verification_and_After_LOC_Report.md)** | Clean-room end-to-end `x64` verification, Win7 SP1 → Win11 QA runbook, live doc sync, & timestamped `AFTER_NET48_LOC_Statistics` report | Scripts + root docs (`~500` lines) | All tests & `validate-artifacts.ps1` green on `x64`; `AFTER` LOC delta report generated |

---

## Context-Window Guardrails

* **Context Budget (`~10` targeted files, `~900` lines):**
  * **NEVER read `src/SomaFrameQD.Demo/Modules/FilterDesigner/Design/DesignTimeImages.cs` (`1,016` lines of Base64 image blobs)** or any `.resx` / `.Designer.cs` files ([`AGENTS.md`](../AGENTS.md) Section 2 Rule #3).
  * Read targeted ranges in `SimpleGridSplitter.cs` only if layout invalidation lines require inspection.

---

## Exact Module-by-Module Implementation Steps

### Step 7.1 — Fix High-DPI & Multi-Monitor Eyedropper Bugs in `SomaFrameQD.Modules.Inspector`

* **Files:**
  * [`src/SomaFrameQD.Modules.Inspector/Win32/NativeMethods.cs`](../src/SomaFrameQD.Modules.Inspector/Win32/NativeMethods.cs) (`20` lines)
  * [`src/SomaFrameQD.Modules.Inspector/Util/ScreenShotUtility.cs`](../src/SomaFrameQD.Modules.Inspector/Util/ScreenShotUtility.cs) (`26` lines)
  * [`src/SomaFrameQD.Modules.Inspector/Controls/ScreenColorPicker.cs`](../src/SomaFrameQD.Modules.Inspector/Controls/ScreenColorPicker.cs) (`147` lines)
* **Forensic Bug Diagnosis (`Win7 SP1` `125%`/`150%` System DPI → `Win10`/`Win11` `150%`/`200%` System DPI):**
  1. **Coordinate-Unit Mismatch:** Under System-DPI awareness (`Q2 = 2A` in `app.manifest`), `ScreenShotUtility.Take()` (lines 11–20) and `ScreenColorPicker.GetEventArgs()` (lines 100–101) currently query WPF's `SystemParameters.VirtualScreenLeft / Top / Width / Height` (**96-DPI DIPs**), whereas `Graphics.CopyFromScreen` and `user32.dll!GetCursorPos` operate in **physical Win32 pixels**! On any `125%`–`200%` display across Win7 SP1 → Win11, `ScreenShotUtility.Take()` captures only the top-left fraction of the desktop, and `ScreenColorPicker` samples offset coordinates or returns `Colors.Black`!
  2. **Off-by-One Top/Left Edge Bug (`ScreenColorPicker.cs` lines `103–104`):** Checks `cursorPosition.X > 0 && cursorPosition.Y > 0` instead of `>= 0`.
  3. **Null-Reference & Capture Leak (`ScreenColorPicker.cs` lines `57, 74–80`):** `OnLostMouseCapture` calls `_bitmap.Dispose()` without a null check (`_bitmap?.Dispose()`), and if `Focus()` or `CaptureMouse()` returns `false`, `_bitmap` leaks!
* **Exact Fixes:**
  * Declare `GetSystemMetrics(int nIndex)` (`user32.dll`, `SM_XVIRTUALSCREEN = 76`, `SM_YVIRTUALSCREEN = 77`, `SM_CXVIRTUALSCREEN = 78`, `SM_CYVIRTUALSCREEN = 79`) + `GetVirtualScreenPhysicalBounds()` in `Inspector/Win32/NativeMethods.cs`.
  * Update both `ScreenShotUtility.Take()` and `ScreenColorPicker.GetEventArgs()` to use physical Win32 virtual-desktop bounds, fix `>= 0` edge checks, dispose `_bitmap` if capture fails to start, and guard `_bitmap?.Dispose()` in `OnLostMouseCapture` and `Unloaded`.

---

### Step 7.2 — Eliminate Idle `LayoutUpdated` Allocation Churn in `SomaFrameQD.Modules.GraphEditor` & Verify `PropertyGrid`

* **Files:**
  * [`src/SomaFrameQD.Modules.GraphEditor/Controls/ConnectorItem.cs`](../src/SomaFrameQD.Modules.GraphEditor/Controls/ConnectorItem.cs)
  * [`src/SomaFrameQD.Modules.PropertyGrid/ViewModels/PropertyGridViewModel.cs`](../src/SomaFrameQD.Modules.PropertyGrid/ViewModels/PropertyGridViewModel.cs)
* **Optimizations:**
  * **`ConnectorItem.cs`:** Subscribe to global `LayoutUpdated` only while `IsLoaded` (unsubscribing on `Unloaded` to prevent CLR 4.8 GC root leaks on closed graph documents), and assign `Position = newPosition` only when the transformed `Point` actually changes—preventing redundant `BezierLine` measure invalidations during unrelated shell UI updates.
  * **`PropertyGrid`:** Verify `Extended.Wpf.Toolkit` `5.0.129` (`lib/net40`) template binding and `ICommandRerouter` (`Undo`/`Redo`) behavior under `[net48|x64]`.

---

### Step 7.3 — Fix `FastBitmap` Finalizer Thread Crash & `x64` Pointer Arithmetic in `SomaFrameQD.Demo`

* **File:** [`C:\SOFTDEV\SomaFrameQD\src\SomaFrameQD.Demo\Modules\FilterDesigner\Util\FastBitmap.cs`](../src/SomaFrameQD.Demo/Modules/FilterDesigner/Util/FastBitmap.cs) (`184` lines)
* **Exact Fixes:**
  1. Remove the `~FastBitmap()` finalizer (which previously called `_bitmap.Unlock()` on a UI-thread `WriteableBitmap` from CLR 4.8's background **GC Finalizer Thread**, causing a fatal unhandled `InvalidOperationException` process crash!) and ensure `Dispose(bool disposing)` touches `_bitmap` (`if (_bitmap != null && _bitmap.CheckAccess()) Unlock();`) and `_stream?.Dispose()` only when `disposing == true`.
  2. Widen pointer offset arithmetic in `unsafe public Color this[int x, int y]` (lines 58 & 72) to 64-bit `long`:
     `byte* b = (byte*)_startingPosition + ((long)y * _strideWidth) + ((long)x * BitsPerPixel);`
  3. Use `BitmapCacheOption.OnLoad` in `FastBitmap(byte[] bytes)` so decoded frames decouple cleanly from `_stream`.

---

### Step 7.4 — Debounce Live Roslyn Compilation, Cache `MetadataReference`s, Gate VSync & Add `///` Roslyn Code Docs in `HelixViewModel.cs` (`Q7 = 7A` Locked)

* **Files:**
  * [`C:\SOFTDEV\SomaFrameQD\src\SomaFrameQD.Demo\Modules\Home\ViewModels\HelixViewModel.cs`](../src/SomaFrameQD.Demo/Modules/Home/ViewModels/HelixViewModel.cs) (`155` lines)
  * [`C:\SOFTDEV\SomaFrameQD\src\SomaFrameQD.Demo\Modules\FilterDesigner\Util\ShaderEffectUtility.cs`](../src/SomaFrameQD.Demo/Modules/FilterDesigner/Util/ShaderEffectUtility.cs) *(verify `Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ...)` for `AddEffect.ps` / `MultiplyEffect.ps`)*
* **Exact Fixes in `HelixViewModel.cs` (`Q7 = 7A` + User `///` Documentation Requirement):**
  * **Add explicit `/// <summary>` and `/// <remarks>` XML documentation comments on `HelixViewModel`, `CachedScriptReferences`, and `CompileScripts()`** explaining:
    1. Why `.NET Framework 4.8` retains dynamic Roslyn assemblies (`ICodeCompiler.Compile` → `Assembly.Load(byte[])`) in `AppDomain.CurrentDomain` until application exit;
    2. How `HelixViewModel` mitigates `AppDomain` growth and UI stalls by caching the immutable PE `MetadataReference` array once per process (eliminating duplicate `mscorlib.dll` and per-compile disk reads), debouncing `TextEditor.TextChanged` via a `350ms` `DispatcherTimer`, and skipping compilation when the source text matches `_lastCompiledSource`.
  * Subscribe to `CompositionTarget.Rendering` in `OnActivateAsync` (when view is loaded) and unsubscribe in `OnDeactivateAsync` for **both** tab switches (`close: false`) and document close (`close: true`) so inactive 3D tabs consume zero VSync CPU/GPU.

---

### Step 7.5 — Add Targeted Unit Tests for `Inspector`, `GraphEditor` & `FastBitmap`

* **Files:** Add `<ProjectReference>` to `SomaFrameQD.Modules.GraphEditor` in [`src/SomaFrameQD.Tests/SomaFrameQD.Tests.csproj`](../src/SomaFrameQD.Tests/SomaFrameQD.Tests.csproj) and add tests covering:
  1. `InspectorVirtualScreenPhysicalBounds_MatchesPositiveDimensions_AndScreenshotMatchesPhysicalSize`: Confirms `ScreenShotUtility.Take()` matches Win32 `SM_CXVIRTUALSCREEN × SM_CYVIRTUALSCREEN` physical pixels.
  2. `ConnectorItem_UnloadedFromVisualTree_DoesNotLeakLayoutUpdatedOrRecomputeUnchangedPosition`: Confirms clean `Unloaded` detachment.
  3. `FastBitmap` 64-bit pixel read/write round-trip and `GC.Collect(); GC.WaitForPendingFinalizers();` disposal safety.

---

## Plan #7 Verification Gate (Must Pass Before Starting Plan #8)

```powershell
dotnet restore src\SomaFrameQD.sln --configfile src\NuGet.config
dotnet build src\SomaFrameQD.sln --configuration Release --no-restore
dotnet test src\SomaFrameQD.Tests\SomaFrameQD.Tests.csproj --configuration Release --framework net48 --no-build --no-restore
dotnet pack src\SomaFrameQD.sln --configuration Release --no-build --no-restore --output build\packages
powershell -NoProfile -ExecutionPolicy Bypass -File .\eng\validate-artifacts.ps1
```

### Gate Pass Criteria
1. Clean `Release` (`x64`) build of all 10 projects (including `SomaFrameQD.Demo`, `DemoAppBootstrapper`, `///` XML docs in `HelixViewModel`, and HLSL `ps_2_0` shaders `AddEffect.ps`, `MultiplyEffect.ps`) completes with `0 Warning(s)` and `0 Error(s)`.
2. All characterization and new `Inspector`/`GraphEditor`/`FastBitmap` tests pass on `[net48|x64]`.
3. `eng/validate-artifacts.ps1` passes 100%.

Once all 3 criteria pass, proceed to **[Optimization Implementation Plan #8](./Plan_08_End_to_End_x64_Win7_to_Win11_Verification_and_After_LOC_Report.md)**.
