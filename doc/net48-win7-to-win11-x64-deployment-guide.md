# `SomaFrameQD` — `.NET Framework 4.8` (`x64`) Deployment & QA Guide (`Windows 7 SP1` → `Windows 11`)

> **Target Platform Continuum:** `.NET Framework 4.8` (`net48`), **64-bit (`x64` `PE32+`)** host executables paired with `AnyCPU` `x64`-clean NuGet libraries, verified across **Windows 7 SP1 `x64` → Windows 8.1 `x64` → Windows 10 `x64` → Windows 11 `x64`**.  
> **Companion References:** [`AGENTS.md`](../AGENTS.md) | [`REPO_MAP.md`](../REPO_MAP.md) | [`TODO.md`](../TODO.md) | [`caliburn-5-migration.md`](./caliburn-5-migration.md)

---

## 1. Supported Operating System Matrix (`64-Bit x64`)

| Operating System (`x64`) | `.NET Framework 4.8` Runtime | Window Chrome & DWM Behavior | DPI & Text Rendering (`Q2 = 2A`) | Network TLS Behavior |
| :--- | :--- | :--- | :--- | :--- |
| **Windows 7 SP1 `x64`** / Windows Server 2008 R2 SP1 `x64` | Install `.NET Framework 4.8` Offline Installer (`NDP48-x86-x64-AllOS-ENU.exe`) + required Microsoft KBs below | **Aero Glass (DWM on):** Hardware Direct3D9Ex + MahApps `GlowBrush`<br/>**Classic / Aero Basic / RDP (DWM off):** Explicit `1px` `BorderThickness` (`MainWindowActiveBorder` / `MainWindowInactiveBorder`) + CPU `ps_2_0` shader fallback | System-DPI (`<dpiAware>true</dpiAware>`): `Display` ClearType at `100%` (`96 DPI`), `Ideal` at `125%`/`150%`; startup bounds clamped to `SystemParameters.WorkArea` (e.g., `1024×728` on `1024×768`) | `AppBootstrapper` upgrades `SecurityProtocolType.SystemDefault` (`0`) to `SecurityProtocolType.Tls12` automatically on `NT 6.1` |
| **Windows 8.1 `x64`** / Windows Server 2012 R2 `x64` | Install `.NET Framework 4.8` (if not present via Windows Update) | Always-on DWM composition + `GlowBrush` + `1px` border | System-DPI (`Display` at `96 DPI`; `Ideal` on `>105%` displays) | Preserves `SecurityProtocolType.SystemDefault` (OS Schannel TLS 1.2) |
| **Windows 10 `x64`** (`1607`–`22H2`) / Server 2016–2022 | Built into Windows 10 `1903+` (installable on `1607`–`1809`) | Native DWM composition, Aero Snap, and `MetroWindow` borderless chrome | `<dpiAwareness>system</dpiAwareness>` guarantees zero child-`HWND` / docking airspace tearing across mixed-DPI monitors | Preserves `SecurityProtocolType.SystemDefault` (OS Schannel TLS 1.2 / 1.3) |
| **Windows 11 `x64`** (`21H2`–`24H2+`) / Server 2025 | Built into OS (`.NET Framework 4.8` / `4.8.1` backward-compatible CLR `v4.0.30319`) | Native DWM composition, Snap Layouts, and crisp `1px` border + glow | System-DPI (`Display` at `100%`, `Ideal` at `125%`–`250%`) | Preserves `SecurityProtocolType.SystemDefault` so **TLS 1.2 and TLS 1.3** negotiate natively |

### Mandatory Prerequisite Updates on Clean Windows 7 SP1 `x64` Installations

Before installing `.NET Framework 4.8` and running WPF 4.8 applications on a fresh Windows 7 SP1 `x64` or Windows Server 2008 R2 SP1 `x64` machine, ensure the following Microsoft updates are installed:

1. **`KB4474419` & `KB4490628`:** SHA-2 code-signing support updates required by Microsoft's `.NET Framework 4.8` installer.
2. **`KB4019990`:** Deploys `d3dcompiler_47.dll` required by WPF `.NET Framework 4.7.1`–`4.8` shader/D3D pipelines on Windows 7 SP1.
3. **`.NET Framework 4.8` Runtime (`KB4486129` / Offline Installer):** Installs CLR `4.8` (`v4.0.30319`), pre-NGen'd `64-bit` GAC assemblies (`PresentationCore`, `PresentationFramework`, `PresentationFramework.Aero`, `System.ComponentModel.Composition`, `System.Drawing`), and RyuJIT `x64`.

---

## 2. Host Application Configuration Requirements

Host executables consuming the `SomaFrameQDWpf` and `SomaFrameQD.Modules.*` NuGet packages should mirror [`SomaFrameQD.Demo.csproj`](../src/SomaFrameQD.Demo/SomaFrameQD.Demo.csproj), [`app.config`](../src/SomaFrameQD.Demo/app.config), and [`app.manifest`](../src/SomaFrameQD.Demo/app.manifest):

1. **64-Bit (`x64`) Host Executable & Automatic Binding Redirects (`Q1 = 1A`, `Q6 = 6A`):**
   ```xml
   <PropertyGroup>
     <OutputType>WinExe</OutputType>
     <TargetFramework>net48</TargetFramework>
     <PlatformTarget>x64</PlatformTarget>
     <Prefer32Bit>false</Prefer32Bit>
     <AutoGenerateBindingRedirects>true</AutoGenerateBindingRedirects>
     <GenerateBindingRedirectsOutputType>true</GenerateBindingRedirectsOutputType>
     <ApplicationManifest>app.manifest</ApplicationManifest>
     <DebugType>portable</DebugType>
     <UseWPF>true</UseWPF>
   </PropertyGroup>
   ```
2. **Exit Legacy `.NET 4.6.1` Quirk Mode in `app.config`:**
   ```xml
   <startup>
     <supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.8" />
   </startup>
   ```
   Setting `sku=".NETFramework,Version=v4.8"` unlocks WPF 4.7–4.8 star-Grid sizing fixes, automation-peer/image memory-leak fixes, SHA-256 defaults, and `ServicePointManager` `SystemDefault` TLS negotiation.
3. **Embed Cross-OS `app.manifest` (`Win7 SP1` → `Win11` + System-DPI `Q2 = 2A`):**
   - Include all four `<supportedOS>` GUIDs (`{35138b9a-5d96-4fbd-8e2d-a2440225f93a}` Win7, `{4a2f28e3-53b9-4441-ba9c-d69d4a4a6e38}` Win8, `{1f676c76-80e1-4239-95bb-83d0f6d0da78}` Win8.1, `{8e0f7a12-bfb3-4fe8-b9a5-48fd50a15a9a}` Win10/11) so `Environment.OSVersion` reports true OS versions without compatibility shims.
   - Declare `<dpiAware>true</dpiAware>` + `<dpiAwareness>system</dpiAwareness>` so child Win32 `HwndHost` viewports (`HwndWrapper`, `ClippingHwndHost`) and AvalonDock floating windows never tear across monitors.

---

## 3. Runtime Diagnostics & Plugin Author Reference (`Q1`–`Q7`)

### 3.1 Multi-Core Background JIT & Fallback Debug Log (`Q5 = 5A`)
- During `AppBootstrapper.PreInitialize()`, `ConfigureMultiCoreJit()` enables `System.Runtime.ProfileOptimization` under:
  `%LocalAppData%\<EntryAssemblyName>\JitProfiles\Startup.net48.jitprofile`
  *(or `SomaFrameQD.Demo.Startup.net48.jitprofile` in [`DemoAppBootstrapper.cs`](../src/SomaFrameQD.Demo/DemoAppBootstrapper.cs)).*
- On subsequent launches, CLR 4.8 compiles recorded startup methods concurrently on background CPU cores.
- **Silent Fallback + Diagnostic File:** If the profile folder cannot be created or accessed (locked-down kiosk profile, read-only volume, or invalid custom override), startup degrades cleanly to standard RyuJIT `x64` and appends a timestamped diagnostic entry (`OSVersion`, `Is64BitProcess`, `AttemptedProfileDirectory`, full exception stack) to:
  1. `%LocalAppData%\<EntryAssemblyName>\Logs\SomaFrameQD.JitFallback.debug.log` *(primary)*
  2. `%TEMP%\<EntryAssemblyName>.JitFallback.debug.log` *(fallback if `%LocalAppData%` itself is inaccessible)*
- Host subclasses can disable or redirect Multi-Core JIT by overriding `EnableMultiCoreJit`, `JitProfileDirectory`, `JitProfileFileName`, or `JitFallbackDebugLogFilePath` on [`AppBootstrapper`](../src/SomaFrameQD/AppBootstrapper.cs).

### 3.2 Filtered MEF Probing (`Q4 = 4A`) & Dynamic Strong-Name Unification
- `AppBootstrapper.PopulateAssemblySource()` probes `AppDomain.CurrentDomain.BaseDirectory` (immune to working-directory shifts from Desktop shortcuts or file associations).
- `ShouldInspectAssemblyFile(string filePath)` skips known non-MEF BCL and UI vendor prefixes (`System.*`, `Microsoft.*`, `netstandard.*`, `mscorlib.*`, `Caliburn.Micro*`, `AvalonDock*`, `MahApps.Metro*`, `ControlzEx*`, `ICSharpCode.AvalonEdit*`, `DotNetProjects.Wpf.Extended.Toolkit*`, `HelixToolkit*`, and `*.resources.dll`), ignores native C/C++ DLLs (`BadImageFormatException`), and `using`-disposes `AssemblyCatalog` probes—while automatically discovering all `SomaFrameQD*` and custom plugin `.dll`s.
- An idempotent `AppDomain.CurrentDomain.AssemblyResolve` handler (`TryUnifyStrongNamedAssembly`) unifies strong-name version skew (`Name` + `PublicKeyToken` match where available `Version >= requestedVersion`) so dynamically dropped MEF plugins (such as `CodeCompiler` / Roslyn `3.4.0` alongside `AvalonDock` / `System.Text.Json` `10.0.12`) resolve cleanly without `FileLoadException`.

### 3.3 64-Bit `HwndWrapper`, Frozen Themes & DirectX `ps_2_0` Shaders
- **64-Bit Win32 Interop ([`NativeMethods.cs`](../src/SomaFrameQD/Framework/Win32/NativeMethods.cs), [`HwndWrapper.cs`](../src/SomaFrameQD/Framework/Controls/HwndWrapper.cs)):** Unpacks `WM_MOUSEWHEEL`, `WM_MOUSEMOVE`, and `WM_XBUTTON*` `WPARAM`/`LPARAM` pointers via `UncheckedInt32(IntPtr)` without `OverflowException` on negative wheel deltas or negative multi-monitor coordinates, uses `GetWindowLongPtrW`/`SetWindowLongPtrW`, and gates `CompositionTarget.Rendering` strictly on `IsLoaded && IsVisible && _hWnd != IntPtr.Zero`.
- **Frozen Theme Brushes & `ps_2_0` Shaders:** All **562** VS2013 `SolidColorBrush` resources (`LightTheme`, `BlueTheme`, `DarkTheme`, `Window.xaml`), AvalonDock 5 generated `Freezable`s, and cached `PixelShader`s (`GrayscaleEffect.ps`, `AddEffect.ps`, `MultiplyEffect.ps`, all verified `ps_2_0` = `0xFFFF0200`) are frozen (`IsFrozen == true`) with `ShaderRenderMode.Auto` for hardware D3D9Ex + software CPU fallback parity.
- **Physical-Pixel Eyedropper & Finalizer-Safe `FastBitmap`:** `Inspector`'s [`ScreenColorPicker`](../src/SomaFrameQD.Modules.Inspector/Controls/ScreenColorPicker.cs) samples physical Win32 `GetSystemMetrics(SM_*VIRTUALSCREEN)` coordinates across High-DPI multi-monitor setups; Demo's [`FastBitmap`](../src/SomaFrameQD.Demo/Modules/FilterDesigner/Util/FastBitmap.cs) uses 64-bit `long` pointer strides without a cross-thread `WriteableBitmap` finalizer.

### 3.4 Roslyn In-Process Compilation on `.NET Framework 4.8` (`Q7 = 7A`)
- [`ICodeCompiler`](../src/SomaFrameQD.Modules.CodeCompiler/ICodeCompiler.cs) and [`CodeCompiler`](../src/SomaFrameQD.Modules.CodeCompiler/CodeCompiler.cs) emit in-memory PE bytes (`Microsoft.CodeAnalysis.CSharp` `3.4.0`) and load them into `AppDomain.CurrentDomain` via `Assembly.Load(byte[])`.
- Because `.NET Framework 4.8` pins dynamic assemblies loaded into the default `AppDomain` until process exit (`GC.Collect()` cannot unload them and is omitted to avoid UI-thread Gen-2 pauses), interactive callers should follow [`HelixViewModel.cs`](../src/SomaFrameQD.Demo/Modules/Home/ViewModels/HelixViewModel.cs): cache `MetadataReference[]` once per process, debounce `TextChanged` (`350ms`), and skip recompiling unchanged source text.

---

## 4. Cross-OS QA Verification Checklist (`Win7 SP1 x64` → `Win11 x64`)

- [ ] **Process Architecture:** Launch `SomaFrameQD.Demo.exe` and confirm Task Manager reports **64-bit** (`SomaFrameQD.Demo.exe`, *not* `*32` / `(32 bit)`).
- [ ] **Multi-Core JIT Profile Creation:** Confirm `%LocalAppData%\SomaFrameQD.Demo\JitProfiles\SomaFrameQD.Demo.Startup.net48.jitprofile` is generated after first startup (and zero `.debug.log` errors appear under normal permissions).
- [ ] **Theme Switching & Non-DWM Chrome:** Open **Tools → Options → Environment → General**, cycle **Light**, **Blue**, and **Dark** themes, and verify crisp window borders under both DWM Aero/Snap (Win7 Aero, Win10, Win11) and Classic/Basic/RDP sessions (Win7 non-DWM).
- [ ] **Docking & Persistence:** Float, redock, and close/reopen `Inspector`, `Toolbox`, `Output`, and `Error List`, exit `SomaFrameQD.Demo`, relaunch, and verify `ApplicationState.bin` restores the exact layout.
- [ ] **Filter Designer (`GraphEditor` + `FastBitmap` + `ps_2_0` Shaders + Eyedropper):** Open **Filter Designer** from Sample Browser, drag nodes from `Toolbox`, test `Add` / `Multiply` shader previews, and sample screen colors with `Inspector`'s eyedropper at `100%` and `125%`–`200%` System DPI.
- [ ] **Helix 3D Live Roslyn Scripting:** Open **View → Helix**, edit the C# script, verify `350ms` debounced compilation without UI stalls, switch to another document tab (confirming 3D rotation pauses while backgrounded), and switch back.
