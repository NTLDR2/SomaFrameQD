# SomaFrameQD `v0.9.0` — Inaugural Release Notes

**Release Date:** November 23, 2026  
**Framework & NuGet Module Family Version:** **`0.9.0`** (`SomaFrameQDWpf` + 7 `SomaFrameQD.Modules.*` packages)  
**Showcase & Sample Application Version:** **`0.1.0` (Alpha)** (`SomaFrameQD.Demo`)  
**Target Runtime & Architecture:** **`.NET Framework 4.8` (`net48`)** — **64-Bit (`x64`)**  
**Supported Operating Systems:** **Windows 7 SP1 `x64` · Windows 8.1 `x64` · Windows 10 `x64` · Windows 11 `x64`** *(and corresponding Windows Server editions)*  
**Authors & Attribution:** **Dmitriy Novikov, Tim Jones, and contributors**  
**License:** Dual-licensed under **Apache License 2.0** or **Microsoft Public License (Ms-PL)**

---

## Introducing SomaFrameQD: Practicality and Broad Compatibility Over Runtime Churn

Today marks the **inaugural release of SomaFrameQD `0.9.0`**—a rebranded, modernized, and deeply optimized 64-bit fork of the *Gemini* WPF IDE/workbench application framework.

Most modern `.NET` frameworks race toward the newest annual runtime target, leaving desktop software authors—and their users—paying the tax. Anyone who has downloaded a Windows desktop utility only to be halted by an unexpected prompt demanding they download and install *yet another* out-of-band `.NET` Desktop Runtime knows how frustrating and disruptive that experience is. Developers are forced into an unpleasant choice: frustrate end users and IT administrators with runtime prerequisite installers that break every November, or bloat lightweight desktop tools into 150+ MB self-contained bundles.

**SomaFrameQD takes the opposite stance by design.**

The mission of **SomaFrameQD** is unapologetically practical: deliver a fast, modular, Visual Studio-style WPF workbench framework engineered specifically for **`.NET Framework 4.8`** and **64-bit (`x64`) Windows—spanning all the way from Windows 7 SP1 `x64` through Windows 8.1, Windows 10, and Windows 11 `x64`**.

* **Zero Runtime-Installer Friction on Modern Windows:** `.NET Framework 4.8` ships pre-installed inside the operating system on every Windows 10 (`1903+`) and Windows 11 PC worldwide. Applications built on SomaFrameQD launch immediately against OS-resident, pre-NGen'd framework assemblies—with small deployment sizes and zero "Install `.NET` to run this app" roadblocks.
* **Unbroken 15-Year Enterprise, Lab & Industrial Reach:** From air-gapped Windows 7 SP1 `x64` engineering workstations, CNC/laboratory instrument controllers, and Remote Desktop (RDP) servers to multi-monitor High-DPI Windows 11 `x64` developer desktops, SomaFrameQD runs a single unified `net48` binary family across the entire 64-bit Windows continuum.
* **Compatibility and Longevity First:** Rather than chasing bleeding-edge language/runtime churn, SomaFrameQD pairs modern compile-time ergonomics (`C# 12` syntax, deterministic Portable PDB symbol packages, async/await lifecycle guarantees) with the most widely deployed, stable managed desktop runtime Microsoft has ever shipped.

---

## Inaugural Versioning Policy: `0.9.0` Framework vs. `0.1.0` Alpha Samples

SomaFrameQD uses a split SemVer 2.0 versioning scheme managed via `Nerdbank.GitVersioning`:

* **`0.9.0` — Core Framework & Modular NuGet Packages:** `SomaFrameQDWpf` (`SomaFrameQD.dll`) and all seven official add-on packages (`CodeCompiler`, `CodeEditor`, `ErrorList`, `GraphEditor`, `Inspector`, `Output`, and `PropertyGrid`) ship at **`0.9.0`**, backed by **`174` passing `[net48|x64]` STA characterization and optimization tests** and automated NuGet contract validation.
* **`0.1.0` (Alpha) — `SomaFrameQD.Demo` & Showcase Samples:** The companion demo executable and sample modules (`FilterDesigner`, `Helix` 3D live C# scripting, `SampleBrowser`, `TextEditor`) intentionally retain version **`0.1.0`** to signal that the historical showcase samples remain alpha-grade reference code while the `0.9.0` framework core is hardened for real-world host applications.

---

## What's Inside SomaFrameQD `0.9.0`

### 1. A Complete Modular IDE & Workbench Shell (`MEF` + Modernized WPF Ecosystem)
Host applications and drop-in plugins compose rich multi-window tools, tabbed editors, declarative menus, toolbars, keyboard shortcuts, undo/redo stacks, and settings dialogs via **MEF (`System.ComponentModel.Composition`)** atop a modernized dependency stack:

* **Caliburn.Micro `5.0.258` Asynchronous Lifecycle:** Fully awaitable `IShell`, `Document`, `PersistedDocument`, `Tool`, `WindowBase`, and `Show.*` `IResult` coroutines coordinated through a FIFO `TransitionCoordinator` that eliminates re-entrant docking and tab-activation race conditions.
* **Dirkster.AvalonDock `5.0.1` Docking & Transactional Layout Persistence:** Floating windows, auto-hide tool panes, split document groups, and crash-safe `.tmp`/`.bak` transactional `ApplicationState.bin` workspace persistence with isolated per-item binary recovery.
* **8 Core Built-In Modules + 7 Standalone NuGet Modules:**
  * *Built into `SomaFrameQDWpf`:* `MainWindow`, `Shell`, `MainMenu`, `ToolBars`, `StatusBar`, `Toolbox`, `UndoRedo`, and `Settings`.
  * *Optional NuGet Add-Ons:* `CodeEditor` (`AvalonEdit 6.3.1.120` with 16 built-in syntax definitions + custom `.xshd` loading), `CodeCompiler` (in-memory Roslyn `Microsoft.CodeAnalysis.CSharp 3.4.0` C# compiler), `ErrorList`, `Output`, `Inspector` (undo-aware property & group inspector with screen color eyedropper), `PropertyGrid` (`Extended.Wpf.Toolkit 5.0.129`), and `GraphEditor` (zoomable/pannable node-and-wire canvas).
* **Built-In Visual Studio 2013 Themes & 7-Locale Localization:** Runtime-switchable **Light**, **Blue**, and **Dark** themes (`MahApps.Metro 2.4.11` + AvalonDock 5 VS2013 dictionaries) with atomic rollback on load errors, plus localized UI resources in English, German (`de`), French (`fr`), Korean (`ko`), Russian (`ru`), Simplified Chinese (`zh-Hans`), and Traditional Chinese (`zh-TW`).

### 2. Deep `.NET Framework 4.8` & `x64` Runtime Optimizations
* **Native `.NET 4.8` GAC Binding (Zero Out-of-Band BCL Shims):** Replaced legacy `.NET Core` compatibility NuGet shims (`System.ComponentModel.Composition 6.0.0` and `System.Drawing.Common 6.0.0`) with `.NET Framework 4.8`'s built-in, pre-NGen'd 64-bit GAC assemblies—shrinking `bin\` output folders and accelerating cold startup.
* **CLR 4.8 Native Mode & Multi-Core Background JIT:** Upgraded runtime configuration out of legacy `.NET 4.6.1` quirk mode (`sku=".NETFramework,Version=v4.8"`) to unlock WPF 4.7–4.8 layout/memory fixes, and enabled CLR 4.8 **Multi-Core Background JIT** (`System.Runtime.ProfileOptimization`) under `%LocalAppData%\<App>\JitProfiles` with automatic non-fatal `.debug.log` fallback diagnostics on locked-down systems.
* **Fast, Resilient MEF Plugin Discovery:** Anchored plugin probing to `AppDomain.CurrentDomain.BaseDirectory`, added an overridable vendor/BCL assembly skip-filter (`ShouldInspectAssemblyFile`) so Roslyn, AvalonDock, and MahApps DLLs are not wastefully reflected at startup, safely ignored native C/C++ DLLs, and added runtime `AssemblyResolve` strong-name unification for drop-in plugins.

### 3. Hardened 64-Bit Win32 Interop, GPU/CPU Rendering & Cross-OS Polish
* **64-Bit (`x64`) Pointer-Safe Win32 & `HwndHost` Interop:** Hardened `NativeMethods`, `HwndWrapper`, `ClippingHwndHost` (`WS_CHILD | WS_VISIBLE` rectangular child-HWND clipping), and `WindowOptionsBehavior` with 64-bit `GetWindowLongPtrW`/`SetWindowLongPtrW`, explicit Unicode (`*W`) P/Invokes, and overflow-safe 64-bit `WPARAM`/`LPARAM` bit-unpacking for native Direct3D/OpenGL viewport hosting.
* **562 Frozen Theme Brushes, Zero-Idle VSync & `ps_2_0` Shaders:** Froze all **562** VS2013 `SolidColorBrush` XAML declarations (`po:Freeze="True"`), AvalonDock 5 generated `Freezable`s, and compiled DirectX `ps_2_0` (`0xFFFF0200`) pixel shaders (`ShaderRenderMode.Auto` for hardware Direct3D9Ex + automatic CPU SSE2 fallback over RDP/Basic themes). Gated `HwndWrapper` `CompositionTarget.Rendering` strictly on `IsLoaded && IsVisible` so background tabs consume **0% idle CPU/GPU**.
* **Seamless `Windows 7 SP1` → `Windows 11` Adaptation:** Embedded Win32 `<supportedOS>` compatibility GUIDs (`Win7 SP1` through `Win10/11`), System-DPI awareness (`<dpiAwareness>system</dpiAwareness>`) with dynamic ClearType tuning (`Display` at `96 DPI`, `Ideal` at `125%`–`250%` DPI) and `SnapsToDevicePixels="True"`, a crisp `1px` window border fallback when DWM Aero Glass is disabled (Win7 Classic/Basic/RDP) alongside Windows 11 Snap Layouts, `SystemParameters.WorkArea` startup clamping for compact displays, and OS-adaptive TLS security (`Tls12` auto-enablement on Windows 7 SP1 while preserving native OS `SystemDefault` TLS 1.2 + TLS 1.3 negotiation on Windows 10 and Windows 11).

---

## Notable Bug & Stability Fixes

* **Fixed `x64` Mouse-Wheel & Multi-Monitor Crashes:** Resolved `System.OverflowException` crashes in `HwndWrapper` when scrolling downward (`WM_MOUSEWHEEL` bit 31 set in 64-bit `WPARAM`) or moving the mouse across secondary monitors positioned at negative coordinates (`WM_MOUSEMOVE` signed `LPARAM`).
* **Fixed Fatal `FastBitmap` GC Finalizer Crash (`Demo`):** Removed a cross-thread `~FastBitmap()` finalizer that previously invoked `WriteableBitmap.Unlock()` from the CLR 4.8 GC Finalizer Thread, and widened pixel stride math to 64-bit `long`.
* **Fixed High-DPI Multi-Monitor Eyedropper Sampling (`Inspector`):** Switched `ScreenShotUtility` and `ScreenColorPicker` from logical 96-DPI DIPs to physical Win32 `GetSystemMetrics(SM_*VIRTUALSCREEN)` pixel coordinates, fixed `(0, 0)` top-left edge rejection, and handled locked-workstation `Win32Exception`s gracefully.
* **Eliminated Roslyn UI Stalls & Unbounded Keystroke Recompiles (`CodeCompiler` & `Demo`):** Removed unconditional blocking `GC.Collect()` calls during compilation, handled Roslyn `Hidden` and `Location.None` metadata diagnostics cleanly, and added `350ms` debouncing + cached `MetadataReference[]` in `HelixViewModel`.
* **Fixed Thread Contention & Visual-Tree Memory Leaks (`Output` & `GraphEditor`):** Made `OutputViewModel` thread-safe under multi-threaded logging via coalesced background UI flushes, and scoped `ConnectorItem.LayoutUpdated` to `Loaded`/`Unloaded` so closed node graphs release cleanly during garbage collection.

---

## System Requirements, Breaking Changes & Migration Guide

### Supported Environment Matrix (Administrators & End Users)

| Platform (`64-Bit x64` Only) | Runtime Prerequisite | Notes |
| :--- | :--- | :--- |
| **Windows 10 `x64` (`1903+`) & Windows 11 `x64`** | **None (Included in OS)** | Runs out of the box with native DWM Snap Layouts, System-DPI scaling, and OS TLS 1.2 / 1.3. |
| **Windows 8.1 `x64` / Windows 10 `x64` (`1607`–`1809`)** | `.NET Framework 4.8` Runtime | Standard Microsoft offline/web installer if not already present via Windows Update. |
| **Windows 7 SP1 `x64` / Server 2008 R2 SP1 `x64`** | `.NET Framework 4.8` + `KB4474419`, `KB4490628`, `KB4019990` | SHA-2 signing updates + `d3dcompiler_47.dll` (`KB4019990`); full Aero Glass, Classic/Basic `1px` border, and auto-TLS 1.2 support. |

### Migration Summary for *Gemini* & Plugin Developers

1. **Package IDs, Namespaces & XAML Schema:**
   * Core NuGet package ID is **`SomaFrameQDWpf`** (assembly and root CLR namespace: **`SomaFrameQD`**); add-on modules are **`SomaFrameQD.Modules.*`**.
   * Update C# `using Gemini...` directives to `using SomaFrameQD...`, pack URIs to `pack://application:,,,/SomaFrameQD;component/...`, and XAML namespace declarations to `http://schemas.timjones.io/SomaFrameQD`.
2. **Host Project Settings (`net48` + `x64`):**
   * Target `<TargetFramework>net48</TargetFramework>` with `<PlatformTarget>x64</PlatformTarget>`, `<Prefer32Bit>false</Prefer32Bit>`, `<AutoGenerateBindingRedirects>true</AutoGenerateBindingRedirects>`, and `<GenerateBindingRedirectsOutputType>true</GenerateBindingRedirectsOutputType>`.
   * Set `<supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.8" />` in `app.config` and embed an `app.manifest` declaring Windows 7–11 `<supportedOS>` GUIDs and `<dpiAwareness>system</dpiAwareness>` (see [`SomaFrameQD.Demo/app.manifest`](../src/SomaFrameQD.Demo/app.manifest)).
3. **GAC References & Bootstrapper Hooks:**
   * Reference `.NET Framework 4.8` GAC `System.ComponentModel.Composition` (and `System.Drawing` if needed) via `<Reference Include="..." />` rather than out-of-band `6.0.0` NuGet shims.
   * Remove any `.NET 5+` `IsPublishSingleFileHandled` / `PublishSingleFileBypassAssemblies` overrides on `AppBootstrapper` subclasses, and ensure all multi-instance `IDocument` / `ISettingsEditor` exports carry `[PartCreationPolicy(CreationPolicy.NonShared)]`.

---

## Further Technical Documentation

* **Post-`net48` Architectural Reference Suite (`00`–`09`):** [`doc_after-net48/00_INDEX.md`](../doc_after-net48/00_INDEX.md)
* **Granular Developer Changelog:** [`changelogs/20261123_CHANGELOG.md`](../changelogs/20261123_CHANGELOG.md)
* **Cross-OS Deployment & Runtime Diagnostics Guide:** [`doc/net48-win7-to-win11-x64-deployment-guide.md`](../doc/net48-win7-to-win11-x64-deployment-guide.md)
* **Caliburn.Micro 5 & AvalonDock 5 Migration Reference:** [`doc/caliburn-5-migration.md`](../doc/caliburn-5-migration.md)
* **Repository & MEF Contract Navigation Map:** [`REPO_MAP.md`](../REPO_MAP.md)
