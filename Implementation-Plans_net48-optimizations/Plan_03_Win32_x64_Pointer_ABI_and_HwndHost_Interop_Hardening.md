# Optimization Implementation Plan #3 — 64-Bit (`x64`) Win32 P/Invoke ABI & `HwndHost` Interop Hardening (`Win7 SP1` → `Win11`)

> **Purpose:** Eliminate latent 64-bit (`x64`) `OverflowException` crashes, 32-bit pointer/handle truncations, unguarded `IntPtr.Zero` P/Invokes, and `double.NaN` (`CW_USEDEFAULT`) child-window sizing bugs across `NativeMethods.cs`, `HwndWrapper.cs`, `ClippingHwndHost.cs`, and `WindowOptionsBehavior.cs` so hosted native/DirectX/OpenGL viewports and custom window behaviors run reliably on 64-bit Windows 7 SP1 through Windows 11.  
> **Generated Timestamp:** `2026-11-22T20:22:00-05:00` (`2026-11-22`)  
> **Prerequisite:** **[Plan #1](./Plan_01_Open_Questions_Native_GAC_References_x64_and_Binding_Redirects.md)** and **[Plan #2](./Plan_02_CLR48_Runtime_SKU_AppManifest_Startup_JIT_and_MEF_Probing.md) MUST be 100% completed and verified green first.**  
> **Target Platform Continuum:** `.NET Framework 4.8` (`net48`), **Windows 7 SP1 `x64` → Windows 8.1 `x64` → Windows 10 `x64` → Windows 11 `x64`**.  
> **Execution Order:** [Plan #1](./Plan_01_Open_Questions_Native_GAC_References_x64_and_Binding_Redirects.md) → [Plan #2](./Plan_02_CLR48_Runtime_SKU_AppManifest_Startup_JIT_and_MEF_Probing.md) → **Plan #3 (This Plan)** → [Plan #4](./Plan_04_WPF_RenderLoop_Lifecycle_and_VS2013_Theme_Resource_Freezing.md) → [Plan #5](./Plan_05_WindowChrome_Docking_DPI_and_Win7_to_Win11_OS_Adaptation.md) → [Plan #6](./Plan_06_Standalone_Modules_Part1_CodeCompiler_CodeEditor_ErrorList_Output.md) → [Plan #7](./Plan_07_Standalone_Modules_Part2_Inspector_GraphEditor_PropertyGrid_and_Demo.md) → [Plan #8](./Plan_08_End_to_End_x64_Win7_to_Win11_Verification_and_After_LOC_Report.md).  
> **Reference Documents:** [`TODO.md`](../TODO.md) (`Plan 2.3`) | [`doc_before-net48/05_Themes_Rendering_and_Win32_Interop.md`](../doc_before-net48/05_Themes_Rendering_and_Win32_Interop.md) | [`AGENTS.md`](../AGENTS.md)

---

## Stage 2 Eight-Plan Sequential Roadmap (`Implementation-Plans_net48-optimizations`)

| Plan | Optimization Focus (Win7 SP1 `x64` → Win11 `x64`) | Context Budget | Gate Verification |
| :--- | :--- | :--- | :--- |
| **[Plan #1](./Plan_01_Open_Questions_Native_GAC_References_x64_and_Binding_Redirects.md)** | Locked Decisions (`Q1`–`Q7`) + Native `net48` GAC references (`System.ComponentModel.Composition`, `System.Drawing`), `x64` target config (`1A`), `portable` PDBs (`6A`), automatic binding redirects, & lockstep `package-contract.json` | `~15` files (`~650` lines) | Clean `x64` build; `SomaFrameQD.Tests` runs as `[net48\|x64]`; `validate-artifacts.ps1` passes |
| **[Plan #2](./Plan_02_CLR48_Runtime_SKU_AppManifest_Startup_JIT_and_MEF_Probing.md)** | CLR 4.8 `sku="v4.8"` (`app.config`), `app.manifest` (`<supportedOS>` Win7 SP1 → Win10/11 + System-DPI `2A`), **clean removal** of `PublishSingleFile` APIs + clean `net48` `DemoAppBootstrapper.cs` (`3B`), filtered MEF probing (`4A`), Multi-Core JIT + `.debug.log` fallback logger (`5A`), & `AssemblyResolve` unification | `~7` files (`~650` lines) | Clean `x64` build + startup/MEF/JIT-fallback-log/assembly-unification tests pass |
| **Plan #3 (This Plan)** | 64-bit (`x64`) Win32 P/Invoke & `HwndHost` hardening (`NativeMethods.cs`, `HwndWrapper.cs`, `ClippingHwndHost.cs`, `WindowOptionsBehavior.cs`) | `~6` files (`~950` lines) | Clean `x64` build + dedicated 64-bit pointer/message overflow unit tests pass |
| **[Plan #4](./Plan_04_WPF_RenderLoop_Lifecycle_and_VS2013_Theme_Resource_Freezing.md)** | `HwndWrapper` `CompositionTarget.Rendering` visibility/loaded gating, `po:Freeze="True"` across ~340 VS2013 `SolidColorBrush` resources, & `ps_2_0` `GrayscaleEffect` check | `~8` files (`~1,050` lines) | Clean `x64` build + render-loop gating & frozen theme dictionary tests pass |
| **[Plan #5](./Plan_05_WindowChrome_Docking_DPI_and_Win7_to_Win11_OS_Adaptation.md)** | Cross-OS Window/Docking adaptation (`Win7 SP1` Aero & Classic/RDP ↔ `Win10`/`Win11` Snap/High-DPI), work-area clamping, TLS 1.2/1.3 safety, & buffered `ApplicationState.bin` I/O | `~8` files (`~750` lines) | Clean `x64` build + persistence/chrome/bounds/TLS characterization tests pass |
| **[Plan #6](./Plan_06_Standalone_Modules_Part1_CodeCompiler_CodeEditor_ErrorList_Output.md)** | Standalone Modules Part 1 (`CodeCompiler`, `Output`, `ErrorList`, `CodeEditor`) | `~9` files (`~650` lines) | Clean `x64` build + combined `AvalonDock` × `CodeCompiler` `x64` test passes |
| **[Plan #7](./Plan_07_Standalone_Modules_Part2_Inspector_GraphEditor_PropertyGrid_and_Demo.md)** | Standalone Modules Part 2 (`Inspector`, `GraphEditor`, `PropertyGrid`) & `SomaFrameQD.Demo` (`FastBitmap`, `HelixViewModel`, `ScreenColorPicker`) | `~10` files (`~900` lines) | Clean `x64` build + `Inspector`, `GraphEditor`, `PropertyGrid`, & `FastBitmap` tests pass |
| **[Plan #8](./Plan_08_End_to_End_x64_Win7_to_Win11_Verification_and_After_LOC_Report.md)** | Clean-room end-to-end `x64` verification, Win7 SP1 → Win11 QA runbook, live doc sync, & timestamped `AFTER_NET48_LOC_Statistics` report | Scripts + root docs (`~500` lines) | All tests & `validate-artifacts.ps1` green on `x64`; `AFTER` LOC delta report generated |

---

## Forensic Audit of `x64` Win32 Interop Defects Being Fixed

Inspecting the actual C# sources on disk reveals **five distinct `x64` Win32 interop bugs** that compile with `0` warnings yet crash or misbehave inside a 64-bit (`[net48|x64]`) process across Windows 7 SP1 through Windows 11:

1. **`WM_MOUSEWHEEL` `OverflowException` on `x64` (`HwndWrapper.cs` line `356`):**
   * Code currently executes `int delta = NativeMethods.GetWheelDeltaWParam(wParam.ToInt32());`.
   * On 64-bit Windows (`IntPtr.Size == 8`), scrolling **down** sets the upper 16-bit word of the low 32 bits of `WPARAM` to `-120` (`0xFF88`), making `wParam` equal `0x00000000FF880000` (`4,287,102,976` > `int.MaxValue`). `IntPtr.ToInt32()` **immediately throws `System.OverflowException`**!
2. **Checked `(int)wParam` and `(int)lParam` `OverflowException`s on `x64` (`HwndWrapper.cs` lines `394–420` & `430–431`):**
   * In C#, `(int)lParam` on an `IntPtr` compiles to `IntPtr.op_Explicit(IntPtr) -> int32`, which performs a **checked** range test even in `Release` builds (`unchecked` only suppresses primitive numeric casts unless converted via `ToInt64()` first!).
   * Whenever the mouse moves to negative client coordinates during mouse capture (`y = -1` → `0x00000000FFFFxxxx` or sign-extended `0xFFFFFFFFFFFFxxxx`) or on multi-monitor desktops where a secondary monitor sits left/above the primary monitor on Windows 7–11, `(int)lParam` **throws `System.OverflowException`**!
3. **32-Bit `GetWindowLong` / `SetWindowLong` & Truncated `SetFocus` Return Handle (`NativeMethods.cs` lines `102–107`, `151–152`):**
   * `GetWindowLong` / `SetWindowLong` use 32-bit `int` instead of Win32's 64-bit-safe `GetWindowLongPtr` / `SetWindowLongPtr` (`IntPtr`, `EntryPoint = "GetWindowLongPtrW"` / `"SetWindowLongPtrW"` when `IntPtr.Size == 8`, with `GetWindowLongW` / `SetWindowLongW` fallback if `AnyCPU` library Option `1A` is loaded in a 32-bit designer).
   * `SetFocus(IntPtr hWnd)` is declared returning `int` (line 152), truncating the returned `HWND` (`IntPtr`) on 64-bit Windows when Windows allocates >32-bit handle/pointer values, and `RegisterClassEx` (`WNDCLASSEX`) lacks `CharSet = CharSet.Unicode` (causing ANSI marshaling on `WNDCLASSEX` while `CreateWindowEx` uses `CharSet.Auto`!).
4. **`(int)Width` / `(int)Height` on `double.NaN` Passing `CW_USEDEFAULT` (`-2147483648`) to `WS_CHILD` Windows (`HwndWrapper.cs` line `327` & `ClippingHwndHost.cs` line `54`):**
   * Default WPF `FrameworkElement.Width` and `Height` are `double.NaN`. In C#, `(int)double.NaN` evaluates to `-2147483648` (`0x80000000` = `CW_USEDEFAULT`), which is invalid for `WS_CHILD` child windows in `CreateWindowEx` and `HwndSourceParameters`!
5. **Unguarded `IntPtr.Zero` HWND in `WindowOptionsBehavior.UpdateWindowStyle()` (`WindowOptionsBehavior.cs` lines `51–81`) & Repeated `RegisterClassEx` Calls (`HwndWrapper.cs` line `333`):**
   * If `ShowIcon`, `ShowMinimizeBox`, or `ShowMaximizeBox` is set in XAML *before* `Window.SourceInitialized`, `new WindowInteropHelper(AssociatedObject).Handle` is `IntPtr.Zero`, causing `GetWindowLong(IntPtr.Zero, ...)` to fail with `ERROR_INVALID_WINDOW_HANDLE`.

---

## Exact File-by-File Implementation Steps

### Step 3.1 — Harden `src/SomaFrameQD/Framework/Win32/NativeMethods.cs` for 64-Bit (`x64`) Windows 7–11 ABI

* **File:** [`C:\SOFTDEV\SomaFrameQD\src\SomaFrameQD\Framework\Win32\NativeMethods.cs`](../src/SomaFrameQD/Framework/Win32/NativeMethods.cs)
* **Changes:**
  1. Add `IntPtr.Size == 8`-aware `GetWindowLongPtr(IntPtr hWnd, int nIndex)` and `SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong)` wrappers backed by `GetWindowLongPtrW`/`SetWindowLongPtrW` (`x64`) and `GetWindowLongW`/`SetWindowLongW` (`x86` designer fallback), all with `CharSet = CharSet.Unicode, SetLastError = true`.
  2. Fix `WNDCLASSEX` (`[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]`) and `RegisterClassEx` (`EntryPoint = "RegisterClassExW", CharSet = CharSet.Unicode, SetLastError = true`), `CreateWindowEx` (`CharSet = CharSet.Unicode`), `GetModuleHandle` (`CharSet = CharSet.Unicode`), and `DefWindowProc` (`CharSet = CharSet.Unicode`) so class registration and window creation never mix ANSI and Unicode window procedures on Windows 7–11.
  3. Fix `SetFocus(IntPtr hWnd)` return type from `int` to `IntPtr`.
  4. Add 64-bit-safe, `OverflowException`-free `IntPtr` message-unpacking helpers alongside the existing `int` overloads:
     ```csharp
     public static int UncheckedInt32(IntPtr value)
     {
         return unchecked((int)value.ToInt64());
     }

     public static int GetXLParam(IntPtr lParam)
     {
         return LowWord(UncheckedInt32(lParam));
     }

     public static int GetYLParam(IntPtr lParam)
     {
         return HighWord(UncheckedInt32(lParam));
     }

     public static int GetWheelDeltaWParam(IntPtr wParam)
     {
         return HighWord(UncheckedInt32(wParam));
     }
     ```

---

### Step 3.2 — Fix `x64` `WndProc` Overflows, `NaN` Child Dimensions & Idempotent Class Registration in `HwndWrapper.cs`

* **File:** [`C:\SOFTDEV\SomaFrameQD\src\SomaFrameQD\Framework\Controls\HwndWrapper.cs`](../src/SomaFrameQD/Framework/Controls/HwndWrapper.cs) *(Note: `CompositionTarget.Rendering` visibility gating is deferred cleanly to [Plan #4](./Plan_04_WPF_RenderLoop_Lifecycle_and_VS2013_Theme_Resource_Freezing.md) to keep Plan #3 strictly focused on Win32 `x64` ABI!)*
* **Changes:**
  1. **Line `356` (`WM_MOUSEWHEEL`):** Replace `NativeMethods.GetWheelDeltaWParam(wParam.ToInt32())` with `NativeMethods.GetWheelDeltaWParam(wParam)`.
  2. **Lines `394–421` (`WM_XBUTTONDOWN`, `WM_XBUTTONUP`, `WM_XBUTTONDBLCLK`):** Replace checked `(int) wParam` with `int wParamBits = NativeMethods.UncheckedInt32(wParam);` (or check `NativeMethods.HighWord(wParamBits)` / `(wParamBits & NativeMethods.MK_XBUTTON1) != 0`).
  3. **Lines `429–431` (`WM_MOUSEMOVE`):** Replace `NativeMethods.GetXLParam((int) lParam)` and `NativeMethods.GetYLParam((int) lParam)` with `NativeMethods.GetXLParam(lParam)` and `NativeMethods.GetYLParam(lParam)`.
  4. **Lines `319–343` (`CreateHostWindow` & `RegisterWindowClass`):**
     * Guard `double.NaN` / negative initial dimensions via a helper `GetSafePixelDimension(ActualWidth, Width)` (`>= 0`, never `(int)double.NaN`).
     * Track `private static bool _windowClassRegistered;` (guarded by a static lock) so `RegisterClassEx` executes once per process rather than redundantly failing with `ERROR_CLASS_ALREADY_EXISTS` on every subsequent `HwndWrapper` instance, while holding `NativeMethods.DefaultWindowProc` rooted statically so the delegate can never be collected by GC on `x64`.

---

### Step 3.3 — Fix `double.NaN` Child Dimensions & Null Safety in `ClippingHwndHost.cs`

* **File:** [`C:\SOFTDEV\SomaFrameQD\src\SomaFrameQD\Framework\Controls\ClippingHwndHost.cs`](../src/SomaFrameQD/Framework/Controls/ClippingHwndHost.cs)
* **Changes:**
  * In `BuildWindowCore` (line 54), replace `(int) Width, (int) Height` with safe non-`NaN` integer pixel bounds (`0` when `Width`/`Height` is `NaN` or `<= 0`).
  * In `DestroyWindowCore` (line 70), guard `_source?.Dispose(); _source = null;`.

---

### Step 3.4 — Guard `IntPtr.Zero` & Switch to `GetWindowLongPtr`/`SetWindowLongPtr` in `WindowOptionsBehavior.cs`

* **File:** [`C:\SOFTDEV\SomaFrameQD\src\SomaFrameQD\Framework\Behaviors\WindowOptionsBehavior.cs`](../src/SomaFrameQD/Framework/Behaviors/WindowOptionsBehavior.cs)
* **Changes:**
  * In `UpdateWindowStyle()` (lines 46–82):
    * Add `if (handle == IntPtr.Zero) return;` immediately after `var handle = new WindowInteropHelper(AssociatedObject).Handle;`.
    * Use `NativeMethods.GetWindowLongPtr` and `NativeMethods.SetWindowLongPtr` with 64-bit-safe `long` bitmask operations (`|= (long)NativeMethods.WS_MINIMIZEBOX`, `&= ~(long)NativeMethods.WS_MINIMIZEBOX`, etc.) cast back to `new IntPtr(...)`.

---

### Step 3.5 — Add Dedicated `x64` Win32 Interop Regression Tests in `SomaFrameQD.Tests`

* **File:** `src/SomaFrameQD.Tests/Framework/Win32/Win32X64InteropTests.cs` *(new `[STATestClass]`)*
* **Tests to Add:**
  1. `RuntimeProcess_Is64BitOnTargetTestLane`: Asserts `Environment.Is64BitProcess == true` and `IntPtr.Size == 8`.
  2. `GetWheelDeltaWParam_NegativeWheelScrollWithHighBitSetOnX64_ReturnsMinus120WithoutOverflow`: Tests `new IntPtr(unchecked((long)0x00000000FF880000UL))` and `new IntPtr(unchecked((long)0xFFFFFFFFFF880000UL))`, asserting `-120` without `OverflowException`.
  3. `GetXLParam_GetYLParam_NegativeSignedMultiMonitorCoordinatesOnX64_UnpackSignedShortsWithoutOverflow`: Tests negative `X = -320, Y = -180` packed in both zero-extended (`0x00000000FF4CFECO`) and sign-extended (`0xFFFFFFFFFF4CFEC0`) 64-bit `IntPtr`s.
  4. `HwndWrapper_SimulatedMouseWheelAndNegativeMouseMoveMessagesOnX64_DoNotThrowOverflowException`: Subclasses `HwndWrapper`, hosts it in an STA `Window`, dispatches `WM_MOUSEWHEEL` (`0xFF880000`) and `WM_MOUSEMOVE` (`X = -25, Y = -10`), and asserts exact `HwndMouseWheel` (`WheelDelta == -120`) and `HwndMouseMove` events fire cleanly.
  5. `WindowOptionsBehavior_BeforeAndAfterSourceInitialized_UpdatesStylesCleanlyOnX64`: Verifies setting `ShowMinimizeBox = false`, `ShowMaximizeBox = false`, `ShowIcon = false` before and after `Window` HWND creation succeeds with zero Win32 errors.

---

## Plan #3 Verification Gate (Must Pass Before Starting Plan #4)

```powershell
dotnet build src\SomaFrameQD.sln --configuration Release --no-restore
dotnet test src\SomaFrameQD.Tests\SomaFrameQD.Tests.csproj --configuration Release --framework net48 --no-build --no-restore
```

### Gate Pass Criteria
1. Clean `Release` (`x64`) build completes with `0 Warning(s)` and `0 Error(s)`.
2. All `Win32X64InteropTests` and all prior characterization tests pass under `[net48|x64]`.
3. Zero `wParam.ToInt32()`, `(int)wParam`, `(int)lParam`, or 32-bit `GetWindowLong`/`SetWindowLong` calls remain in `src/SomaFrameQD`.

Once all 3 criteria pass, proceed to **[Optimization Implementation Plan #4](./Plan_04_WPF_RenderLoop_Lifecycle_and_VS2013_Theme_Resource_Freezing.md)**.
