# Simple Retargeting to `.NET Framework 4.8` (`net48`) vs. Optimization for `.NET Framework 4.8` (+ Windows 7 SP1 x64)

> **Purpose:** Architectural and engineering comparison for SomaFrameQD comparing minimal single-targeting (`net48`) against full CLR 4.8 and Windows 7 SP1 64-bit (`x64`) platform optimization.  
> **Generated Timestamp:** `2026-11-22T17:36:26-05:00` (`2026-11-22 17:36:26 -05:00`)  
> **Repository Root:** `C:\SOFTDEV\SomaFrameQD`  
> **Related Baseline:** [BEFORE_NET48_LOC_Statistics_2026-11-22_17-23-51.md](./BEFORE_NET48_LOC_Statistics_2026-11-22_17-23-51.md)

---

## Executive Overview

At a high level, the difference comes down to **"making the project compile and run exclusively on `net48`"** versus **"engineering the runtime, memory, Win32/WPF layer, and build pipeline specifically around CLR 4.8 and Windows 7 SP1 x64 constraints."**

* **Simple Retargeting** is a **build-configuration subtraction**: you drop `net10.0-windows`, remove `.NET 5+` conditional shims, and verify existing tests still pass on `net48`. You inherit whatever legacy defaults, multi-targeting compromises, and latent `x64`/Win7 quirks already exist in the codebase.
* **Optimization for `.NET Framework 4.8` (+ Win7 SP1 x64)** is a **platform-specific specialization**: because CLR 4.8 does **not** have modern `.NET 8/10` runtime crutches (no Dynamic PGO, no Tiered JIT, no `Span<T>`-vectorized BCL internals, no automatic single-file/`deps.json` assembly resolution), you tune assembly loading, JIT startup, GC, `app.config` quirk switches, native `x64` P/Invoke marshaling, and WPF rendering specifically for CLR 4.8 (`clr.dll` / RyuJIT x64) and Windows 7's OS limits.

---

## Side-by-Side Comparison

| Dimension | Simple Retargeting to `net48` | Optimization for `net48` (+ Win7 SP1 x64) |
| :--- | :--- | :--- |
| **Goal** | Single-target `net48` build that compiles and passes tests | Fast cold startup, low memory/GC overhead, native `x64` safety, and clean Win7–Win11 behavior |
| **MSBuild / Target** | `<TargetFramework>net48</TargetFramework>` (defaults to `AnyCPU`) | Explicit `x64` (or `AnyCPU` with `<Prefer32Bit>false</Prefer32Bit>`), deterministic PDBs, explicit binding redirects |
| **Dependencies** | Keeps existing NuGet packages that happen to support `net48` | Replaces out-of-band `.NET Standard`/`.NET Core` shim NuGets with built-in, pre-NGen'd `.NET 4.8` GAC assemblies |
| **CLR 4.8 Quirk Mode** | Leaves legacy `<supportedRuntime>` settings untouched | Sets `sku=".NETFramework,Version=v4.8"` + `app.manifest` OS GUIDs so CLR 4.8 & WPF actually enable 4.7–4.8 fixes and perf paths |
| **Startup & JIT** | Standard cold RyuJIT + unfiltered MEF directory scanning | `ProfileOptimization` (Multi-Core background JIT), streamlined `net48` MEF catalog loading, removal of `.NET 5+` single-file checks |
| **Win32 / `x64` / WPF** | Unchanged if it compiles | Fixes 32-bit pointer truncations, hardens `WndProc`/`HwndHost` for `x64`, tunes WPF rendering/DPI/theme fallbacks for Win7 SP1 |

---

## Concrete Examples Directly in `SomaFrameQD`

Inspecting the current codebase reveals six key areas where **simple retargeting** stops short of **true `.NET 4.8` + Win7 SP1 x64 optimization**:

### 1. Out-of-Band NuGet Shims vs. Pre-NGen'd `.NET 4.8` GAC Assemblies

* **Simple Retargeting:** Changes `<TargetFrameworks>net48;net10.0-windows</TargetFrameworks>` to `<TargetFramework>net48</TargetFramework>` across the `.csproj` files, drops `HelixToolkit.Core.Wpf` in `src/SomaFrameQD.Demo/SomaFrameQD.Demo.csproj` (lines 68–74), and removes `eng/compatibility/MahApps.Net10DragHost`.
* **Optimization:** Replaces multi-targeting NuGet compatibility packages with native `.NET Framework 4.8` framework references:
  * `src/SomaFrameQD/SomaFrameQD.csproj` (line 61) currently pulls `<PackageReference Include="System.ComponentModel.Composition" Version="6.0.0" />` (needed only because `.NET Core`/`.NET 10` lacks MEF). On `net48`, `System.ComponentModel.Composition` `4.0.0.0` is built into the `.NET Framework` GAC and has **native NGen images** pre-compiled by Windows—switching to `<Reference Include="System.ComponentModel.Composition" />` eliminates extra NuGet DLLs, binding redirects, and JIT startup cost.
  * `src/SomaFrameQD.Modules.Inspector/SomaFrameQD.Modules.Inspector.csproj` currently pulls NuGet `System.Drawing.Common` `6.0.0` on `net48` (`Condition="!$([MSBuild]::IsTargetFrameworkCompatible('$(TargetFramework)', 'net10.0'))"`) instead of referencing the built-in `<Reference Include="System.Drawing" />` GAC assembly.
  * Ensures `<AutoGenerateBindingRedirects>true</AutoGenerateBindingRedirects>` and `<GenerateBindingRedirectsOutputType>true</GenerateBindingRedirectsOutputType>` are enabled on executables and `src/SomaFrameQD.Tests/SomaFrameQD.Tests.csproj` so heavy transitive graphs like `Microsoft.CodeAnalysis.CSharp` `3.4.0` in `SomaFrameQD.Modules.CodeCompiler` (`System.Collections.Immutable`, `System.Reflection.Metadata`, `System.Memory`) don't trigger runtime assembly-resolve stalls or `FileLoadException`s on CLR 4.8.

---

### 2. CLR 4.8 "Quirk Mode" in `app.config` & OS Manifests

* **Simple Retargeting:** Leaves `src/SomaFrameQD/app.config` (lines 21–23) and `src/SomaFrameQD.Demo/app.config` (lines 8–10) as-is—both currently specify:
  ```xml
  <supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.6.1"/>
  ```
* **Optimization:** In `.NET Framework`, the `sku` in `<supportedRuntime>` (along with `TargetFrameworkAttribute` on the entry `.exe`) controls **CLR and WPF AppContext quirk switches**. Leaving `v4.6.1` tells CLR 4.8 to emulate `.NET 4.6.1` legacy behaviors—disabling `.NET 4.7–4.8` WPF layout/DPI fixes, `Star`-grid allocation improvements, SHA-256/TLS `SystemDefault` defaults, and accessibility/memory leak fixes in WPF.
  * Updating `sku=".NETFramework,Version=v4.8"`, adding an `app.manifest` declaring Windows 7 (`{35138b9a-5d96-4fbd-8e2d-a2440225f93a}`) through Windows 10/11 `<supportedOS>` GUIDs, and explicitly setting `x64` (`<PlatformTarget>x64</PlatformTarget>` or `<Prefer32Bit>false</Prefer32Bit>`) ensures the 64-bit RyuJIT compiler and full 64-bit address space are always used.

---

### 3. Startup, MEF Discovery & Multi-Core JIT (`ProfileOptimization`)

* **Simple Retargeting:** Leaves `src/SomaFrameQD/AppBootstrapper.cs` (lines 56–87, 116–170) and `src/SomaFrameQD.Demo/DemoAppBootstrapper.cs` running `.NET Core 3.1` / `.NET 5+` `PublishSingleFile` detection (`CheckIfSomaFrameQDAppearsPublishedToSingleFile()`, `PublishSingleFileBypassAssemblies`, and dual-directory `DirectoryCatalog` checks comparing `./` against `AppContext.BaseDirectory`).
* **Optimization:**
  * Strips out `.NET 5+` single-file runtime checks and `src/SomaFrameQD.Demo/Properties/PublishProfiles/FolderProfileNet5.pubxml` & `FolderProfileNetCore31.pubxml`.
  * Optimizes MEF `DirectoryCatalog` probing in `src/SomaFrameQD/AppBootstrapper.cs` (lines 133–140): calling `new DirectoryCatalog(path)` unscoped forces CLR 4.8 to open and inspect metadata of *every* `.dll`/`.exe` in the folder at startup (painful on older Win7 disks or when Roslyn/AvalonEdit/HelixToolkit satellite DLLs are present). Scoping or caching MEF assembly discovery cuts startup I/O significantly.
  * Adds **`System.Runtime.ProfileOptimization`** (`SetProfileRoot` + `StartProfile`) early in startup. Because `.NET Framework 4.8` lacks `.NET 10`'s Tiered JIT, Multi-Core JIT profile caching is the primary built-in CLR 4.8 mechanism to offload WPF/AvalonDock/MEF JIT compilation onto background cores on repeat launches—often cutting WPF shell cold/warm startup time significantly.

---

### 4. 64-Bit (`x64`) Win32 Interop Bugs That Compile Cleanly

* **Simple Retargeting:** Compiles `src/SomaFrameQD/Framework/Win32/NativeMethods.cs` and `src/SomaFrameQD/Framework/Controls/HwndWrapper.cs` with zero compiler warnings.
* **Optimization (for `x64`):** Audits pointer/handle math that breaks specifically in a 64-bit `.NET Framework` process:
  * In `src/SomaFrameQD/Framework/Controls/HwndWrapper.cs` (line 356), `WndProc` executes `wParam.ToInt32()` on `WM_MOUSEWHEEL`. In a 64-bit process (`IntPtr.Size == 8`), when scrolling down, the upper word of the low 32 bits of `WPARAM` has bit 31 set (e.g., `0x00000000FF880000`), which exceeds `int.MaxValue`—so **`IntPtr.ToInt32()` throws an `OverflowException` on `x64`** (whereas `unchecked((int)wParam.ToInt64())` succeeds). Lines 394–431 similarly use `(int)wParam` / `(int)lParam` (checked explicit casts on `IntPtr` in C#, which also throw `OverflowException` whenever `lParam` has negative signed 16-bit screen/client coordinates or high bits set on `x64`).
  * In `src/SomaFrameQD/Framework/Win32/NativeMethods.cs` (lines 102–107), `GetWindowLong` / `SetWindowLong` are declared with 32-bit `int` instead of `GetWindowLongPtr` / `SetWindowLongPtr` (`IntPtr`) recommended by Win32 for 64-bit Windows.

---

### 5. WPF Rendering & Windows 7 SP1 x64 Platform Realities

* **Simple Retargeting:** Assumes that if it runs on Windows 10/11 under `net48`, it works identically on Windows 7 SP1 x64.
* **Optimization:** Accounts for how WPF 4.8 behaves on Windows 7 SP1 x64 hardware and OS APIs:
  * **Render-loop CPU/GPU usage:** `src/SomaFrameQD/Framework/Controls/HwndWrapper.cs` (line 171) subscribes to `CompositionTarget.Rendering` in its constructor and stays subscribed until `Dispose()`, firing on every VSync frame (~60 Hz) even when the control is in a backgrounded/unloaded tab if `ActualWidth >= 1`. Subscribing on `Loaded`/`IsVisibleChanged` and unsubscribing when hidden saves idle CPU/GPU on Win7 machines.
  * **Windows 7 DPI & Theme/DWM Differences:** Windows 7 SP1 only supports **System DPI awareness** (it lacks Win 8.1 `PerMonitor` and Win 10 Creators Update `PerMonitorV2` APIs like `GetDpiForWindow`). If WPF 4.8 Per-Monitor V2 settings are blindly enabled without graceful fallback, or if UI styles assume Win10 fonts (`Segoe MDL2 Assets` / `Segoe Fluent Icons`) or DWM glass/borderless window behaviors (MahApps.Metro `WindowChrome` on Win7 Classic / Aero Basic with DWM disabled, e.g., over RDP or VM), visual glitches or P/Invoke `EntryPointNotFoundException`s can occur.
  * **TLS 1.2 on Win7 SP1:** Even on `.NET 4.8`, Windows 7 SP1's Schannel defaults can block TLS 1.2 unless the `v4.8` SKU is active (so `.NET 4.8` uses `SecurityProtocolType.SystemDefault` / explicit `Tls12` fallback safely if any module performs network I/O).

---

### 6. C# Language Level & Symbol/Packaging Pipeline

* **Simple Retargeting:** When `<TargetFrameworks>net48;net10.0-windows</TargetFrameworks>` becomes `<TargetFramework>net48</TargetFramework>`, the .NET SDK automatically drops the default C# `<LangVersion>` from **C# 14** (from `net10.0`) down to **C# 7.3** (the SDK default for `net48`). Any C# 8+ syntax (such as nullable annotations, switch expressions, `using var`, target-typed `new()`, file-scoped namespaces) fails to compile unless `<LangVersion>` is explicitly set in `src/Directory.Build.props`.
* **Optimization:**
  * Explicitly standardizes `<LangVersion>` in `src/Directory.Build.props` (either locking to C# 7.3 for strict legacy VS compatibility, or setting `<LangVersion>latest</LangVersion>` / `12.0` while staying strictly within `net48`-supported runtime features—avoiding runtime-dependent features like default interface methods or `ref struct` spans requiring `.NET Core` runtime types).
  * Reviews PDB format/packaging in `src/Directory.Build.props` (lines 8–11: `IncludeSymbols` / `SymbolPackageFormat` `snupkg`) for `Portable` vs `Full` PDBs depending on Win7 crash-dump/debugger tooling.
  * Simplifies `eng/package-contract.json` and `eng/validate-artifacts.ps1` to validate a clean, single-TFM `lib/net48` NuGet contract.

---

## Bottom Line

* **Simple retargeting** gets you from two target frameworks down to one (`net48`) with minimal diff size—focused primarily on project/build/config files.
* **Optimization for `.NET Framework 4.8` + Windows 7 SP1 x64** goes one layer deeper: eliminating `.NET Core` NuGet shims in favor of pre-NGen'd `.NET 4.8` GAC assemblies, unlocking true `v4.8` CLR/WPF runtime behavior in `app.config` + `app.manifest`, speeding up MEF + JIT cold startup (`ProfileOptimization`), and fixing latent 64-bit `IntPtr`/Win32 and Windows 7 DWM/DPI/WPF edge cases before they surface as crashes in production.
