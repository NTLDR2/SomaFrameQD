# Optimization Implementation Plan #2 — CLR 4.8 Runtime SKU, Cross-OS `app.manifest` (`Win7 SP1` → `Win11`), Clean `AppBootstrapper` / `DemoAppBootstrapper`, Multi-Core JIT (+ Fallback Debug Log) & MEF Probing

> **Purpose:** Unlock native CLR 4.8 and WPF 4.8 runtime behaviors (`sku=".NETFramework,Version=v4.8"`), declare full OS compatibility from **Windows 7 SP1 `x64` through Windows 11 `x64`** with **System-DPI awareness (`Q2 = 2A`)** via `app.manifest`, **cleanly remove** obsolete `.NET 5+` `PublishSingleFile` APIs while refactoring `DemoAppBootstrapper.cs` into an elegant `.NET Framework 4.8` showcase bootstrapper (`Q3 = 3B`), filter cold-startup MEF assembly probing (`Q4 = 4A`), enable CLR 4.8 Multi-Core Background JIT (`ProfileOptimization`) with **automatic `.debug.log` file diagnostics if JIT profiling degrades (`Q5 = 5A`)**, and add `AssemblyResolve` strong-name unification for dynamic MEF plugins.  
> **Generated / Updated Timestamp:** `2026-11-22T20:58:30-05:00` (`2026-11-22` — Updated with Locked User Decisions `Q2=2A`, `Q3=3B`, `Q4=4A`, `Q5=5A`)  
> **Prerequisite:** **[Optimization Implementation Plan #1](./Plan_01_Open_Questions_Native_GAC_References_x64_and_Binding_Redirects.md) MUST be 100% completed and verified green first.**  
> **Target Platform Continuum:** `.NET Framework 4.8` (`net48`), **Windows 7 SP1 `x64` → Windows 8.1 `x64` → Windows 10 `x64` → Windows 11 `x64`**.  
> **Execution Order:** [Plan #1](./Plan_01_Open_Questions_Native_GAC_References_x64_and_Binding_Redirects.md) → **Plan #2 (This Plan)** → [Plan #3](./Plan_03_Win32_x64_Pointer_ABI_and_HwndHost_Interop_Hardening.md) → [Plan #4](./Plan_04_WPF_RenderLoop_Lifecycle_and_VS2013_Theme_Resource_Freezing.md) → [Plan #5](./Plan_05_WindowChrome_Docking_DPI_and_Win7_to_Win11_OS_Adaptation.md) → [Plan #6](./Plan_06_Standalone_Modules_Part1_CodeCompiler_CodeEditor_ErrorList_Output.md) → [Plan #7](./Plan_07_Standalone_Modules_Part2_Inspector_GraphEditor_PropertyGrid_and_Demo.md) → [Plan #8](./Plan_08_End_to_End_x64_Win7_to_Win11_Verification_and_After_LOC_Report.md).  
> **Reference Documents:** [`TODO.md`](../TODO.md) (`Plan 2.2`) | [`DRMA.md`](../DRMA.md) (`S2-1`, `S2-3`) | [`doc_before-net48/01_Architecture_and_Bootstrapper_Overview.md`](../doc_before-net48/01_Architecture_and_Bootstrapper_Overview.md) | [`AGENTS.md`](../AGENTS.md)

---

## Stage 2 Eight-Plan Sequential Roadmap (`Implementation-Plans_net48-optimizations`)

| Plan | Optimization Focus (Win7 SP1 `x64` → Win11 `x64`) | Context Budget | Gate Verification |
| :--- | :--- | :--- | :--- |
| **[Plan #1](./Plan_01_Open_Questions_Native_GAC_References_x64_and_Binding_Redirects.md)** | Locked Decisions (`Q1`–`Q7`) + Native `net48` GAC references (`System.ComponentModel.Composition`, `System.Drawing`), `x64` target config (`1A`), `portable` PDBs (`6A`), automatic binding redirects, & lockstep `package-contract.json` | `~15` files (`~650` lines) | Clean `x64` build; `SomaFrameQD.Tests` runs as `[net48\|x64]`; `validate-artifacts.ps1` passes |
| **Plan #2 (This Plan)** | CLR 4.8 `sku="v4.8"` (`app.config`), `app.manifest` (`<supportedOS>` Win7 SP1 → Win10/11 + System-DPI `2A`), **clean removal** of `PublishSingleFile` APIs + clean `net48` `DemoAppBootstrapper.cs` (`3B`), filtered MEF probing (`4A`), Multi-Core JIT + `.debug.log` fallback logger (`5A`), & `AssemblyResolve` unification | `~7` files (`~650` lines) | Clean `x64` build + startup/MEF/JIT-fallback-log/assembly-unification tests pass |
| **[Plan #3](./Plan_03_Win32_x64_Pointer_ABI_and_HwndHost_Interop_Hardening.md)** | 64-bit (`x64`) Win32 P/Invoke & `HwndHost` hardening (`NativeMethods.cs`, `HwndWrapper.cs`, `ClippingHwndHost.cs`, `WindowOptionsBehavior.cs`) | `~6` files (`~950` lines) | Clean `x64` build + dedicated 64-bit pointer/message overflow unit tests pass |
| **[Plan #4](./Plan_04_WPF_RenderLoop_Lifecycle_and_VS2013_Theme_Resource_Freezing.md)** | `HwndWrapper` `CompositionTarget.Rendering` visibility/loaded gating, `po:Freeze="True"` across ~340 VS2013 `SolidColorBrush` resources, & `ps_2_0` `GrayscaleEffect` check | `~8` files (`~1,050` lines) | Clean `x64` build + render-loop gating & frozen theme dictionary tests pass |
| **[Plan #5](./Plan_05_WindowChrome_Docking_DPI_and_Win7_to_Win11_OS_Adaptation.md)** | Cross-OS Window/Docking adaptation (`Win7 SP1` Aero & Classic/RDP ↔ `Win10`/`Win11` Snap + System-DPI `2A`), work-area clamping, TLS 1.2/1.3 safety, & buffered `ApplicationState.bin` I/O | `~8` files (`~750` lines) | Clean `x64` build + persistence/chrome/bounds/TLS characterization tests pass |
| **[Plan #6](./Plan_06_Standalone_Modules_Part1_CodeCompiler_CodeEditor_ErrorList_Output.md)** | Standalone Modules Part 1 (`CodeCompiler` + `///` Roslyn/`AppDomain` docs `7A`, `Output`, `ErrorList`, `CodeEditor`) | `~9` files (`~650` lines) | Clean `x64` build + combined `AvalonDock` × `CodeCompiler` `x64` test passes |
| **[Plan #7](./Plan_07_Standalone_Modules_Part2_Inspector_GraphEditor_PropertyGrid_and_Demo.md)** | Standalone Modules Part 2 (`Inspector`, `GraphEditor`, `PropertyGrid`) & `SomaFrameQD.Demo` (`FastBitmap`, `HelixViewModel` + `///` docs `7A`, `ScreenColorPicker`) | `~10` files (`~900` lines) | Clean `x64` build + `Inspector`, `GraphEditor`, `PropertyGrid`, & `FastBitmap` tests pass |
| **[Plan #8](./Plan_08_End_to_End_x64_Win7_to_Win11_Verification_and_After_LOC_Report.md)** | Clean-room end-to-end `x64` verification, Win7 SP1 → Win11 QA runbook, live doc sync, & timestamped `AFTER_NET48_LOC_Statistics` report | Scripts + root docs (`~500` lines) | All tests & `validate-artifacts.ps1` green on `x64`; `AFTER` LOC delta report generated |

---

## Context Budget & Architectural Invariants

* **Files in Scope (`7` files, `~650` lines):**
  1. [`src/SomaFrameQD/app.config`](../src/SomaFrameQD/app.config) (`24` lines)
  2. [`src/SomaFrameQD.Demo/app.config`](../src/SomaFrameQD.Demo/app.config) (`18` lines)
  3. `src/SomaFrameQD.Demo/app.manifest` *(new file, `~65` lines)* + `<ApplicationManifest>app.manifest</ApplicationManifest>` in [`src/SomaFrameQD.Demo/SomaFrameQD.Demo.csproj`](../src/SomaFrameQD.Demo/SomaFrameQD.Demo.csproj)
  4. [`src/SomaFrameQD/AppBootstrapper.cs`](../src/SomaFrameQD/AppBootstrapper.cs) (`236` lines)
  5. [`src/SomaFrameQD.Demo/DemoAppBootstrapper.cs`](../src/SomaFrameQD.Demo/DemoAppBootstrapper.cs) (`29` lines) + [`src/SomaFrameQD.Demo/App.xaml`](../src/SomaFrameQD.Demo/App.xaml) (`18` lines)
  6. `src/SomaFrameQD.Tests/Composition/AppBootstrapperOptimizationTests.cs` *(new STA characterization test file)*
* **Non-Negotiable Invariant ([`AGENTS.md`](../AGENTS.md) Invariant #1):**
  * Preserve `AppBootstrapper`'s **Two-Tier MEF Priority Resolution** (`new CompositionContainer(priorityProvider, mainProvider)` where `priorityProvider` wraps `SelectAssemblies()` and `mainProvider` wraps discovered assemblies) and loud failure on missing contracts (`GetInstance` throwing when `!exports.Any()`).

---

## Exact File-by-File Implementation Steps

### Step 2.1 — Exit `.NET 4.6.1` Quirk Mode in Both `app.config` Files (`sku=".NETFramework,Version=v4.8"`)

* **Files:** [`src/SomaFrameQD/app.config`](../src/SomaFrameQD/app.config) (line 22) and [`src/SomaFrameQD.Demo/app.config`](../src/SomaFrameQD.Demo/app.config) (line 9)
* **Engineering Rationale:**
  * Both files currently specify `<supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.6.1"/>`.
  * In `.NET Framework`, `sku` governs **CLR and WPF `AppContext` quirk switches**: leaving `v4.6.1` forces CLR 4.8 to emulate `.NET 4.6.1` legacy quirks—disabling WPF `.NET 4.7–4.8` `Grid` `*`-sizing layout improvements, `ImageSource`/automation peer memory-leak fixes, SHA-256 defaults, and `ServicePointManager` OS-default TLS negotiation (`TLS 1.2` on Win7 SP1+ and `TLS 1.3` on Win11).
  * Update both `<supportedRuntime>` elements to:
    ```xml
    <startup>
        <supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.8"/>
    </startup>
    ```

---

### Step 2.2 — Add Cross-OS `app.manifest` (`Windows 7 SP1 x64` → `Windows 11 x64`, System-DPI `Q2 = 2A`) to `SomaFrameQD.Demo`

* **Files:** Create `C:\SOFTDEV\SomaFrameQD\src\SomaFrameQD.Demo\app.manifest` and add `<ApplicationManifest>app.manifest</ApplicationManifest>` to `<PropertyGroup>` in [`src/SomaFrameQD.Demo/SomaFrameQD.Demo.csproj`](../src/SomaFrameQD.Demo/SomaFrameQD.Demo.csproj).
* **Exact Manifest Specification (`Q2 = 2A` Locked):**
  * Declare `<requestedExecutionLevel level="asInvoker" uiAccess="false" />`.
  * Declare all **four** Microsoft `<supportedOS>` compatibility GUIDs so Windows 8, 8.1, 10, and 11 never place the process into legacy OS-version lie shims (`Environment.OSVersion` reports true OS version across Win7 SP1 → Win11):
    * `{35138b9a-5d96-4fbd-8e2d-a2440225f93a}` — **Windows 7 SP1** / Windows Server 2008 R2 SP1
    * `{4a2f28e3-53b9-4441-ba9c-d69d4a4a6e38}` — **Windows 8** / Windows Server 2012
    * `{1f676c76-80e1-4239-95bb-83d0f6d0da78}` — **Windows 8.1** / Windows Server 2012 R2
    * `{8e0f7a12-bfb3-4fe8-b9a5-48fd50a15a9a}` — **Windows 10 and Windows 11** / Windows Server 2016–2025
  * Declare **System-DPI Awareness Everywhere (`Q2 = 2A`)** inside `<asmv3:application><asmv3:windowsSettings>`:
    * `<dpiAware xmlns="http://schemas.microsoft.com/SMI/2005/WindowsSettings">true</dpiAware>` (read by Windows 7 SP1 through Windows 11)
    * `<dpiAwareness xmlns="http://schemas.microsoft.com/SMI/2016/WindowsSettings">system</dpiAwareness>` (read by Windows 10 `1607+` and Windows 11, guaranteeing identical System-DPI behavior and zero child-HWND / WinForms mixed-DPI airspace tearing across every OS).
  * Enable Win32 Common Controls `6.0.0.0` (`Microsoft.Windows.Common-Controls`, `processorArchitecture="*"`) for modern Win7–Win11 native dialogs.

---

### Step 2.3 — Clean `AppBootstrapper.cs`, Refactor `DemoAppBootstrapper.cs` for `net48` (`Q3 = 3B`), Filter MEF Probing (`Q4 = 4A`), Enable Multi-Core JIT + Fallback `.debug.log` (`Q5 = 5A`) & Add `AssemblyResolve`

* **Files:** [`src/SomaFrameQD/AppBootstrapper.cs`](../src/SomaFrameQD/AppBootstrapper.cs), [`src/SomaFrameQD.Demo/DemoAppBootstrapper.cs`](../src/SomaFrameQD.Demo/DemoAppBootstrapper.cs), and [`src/SomaFrameQD.Demo/App.xaml`](../src/SomaFrameQD.Demo/App.xaml)
* **Exact Optimizations:**

  1. **Clean Removal of `.NET 5+` `PublishSingleFile` Clutter (`Q3 = 3B` Locked):**
     * In `AppBootstrapper.cs`: completely **remove** `IsPublishSingleFileHandled`, `CheckIfSomaFrameQDAppearsPublishedToSingleFile()`, `PublishSingleFileBypassAssemblies`, the single-file check block in `Configure()` (lines 56–87), and the `.netcoreapp3.1` `currentWorkingDir != baseDirectory` dual scan in `PopulateAssemblySource()` (lines 118–130). No `[Obsolete]` dead code remains.
     * Simplify `PopulateAssemblySource()` to probe `AppDomain.CurrentDomain.BaseDirectory` (the canonical `.NET Framework 4.8` executable directory, immune to `Environment.CurrentDirectory` shifts when launched from shortcuts or file associations on Win7–Win11).
     * **Refactor `DemoAppBootstrapper.cs` & `Demo/App.xaml` cleanly for `net48`:**
       * Rewrite `src/SomaFrameQD.Demo/DemoAppBootstrapper.cs` as a clean, concise `.NET Framework 4.8` `AppBootstrapper` subclass (removing all `PublishSingleFile` overrides and documenting `net48` host customization hooks such as `JitProfileDirectory` / `ShouldInspectAssemblyFile`).
       * Update `src/SomaFrameQD.Demo/App.xaml` (lines 11–13) to remove the stale `.NET 5+ PublishSingleFile` comment and wire `<local:DemoAppBootstrapper x:Key="bootstrapper" />` directly so `DemoAppBootstrapper` is actively exercised by `SomaFrameQD.Demo`!

  2. **CLR 4.8 Multi-Core Background JIT (`ProfileOptimization`) + Automatic `.debug.log` Fallback Output (`Q5 = 5A` Locked):**
     * Expose virtual hooks on `AppBootstrapper`:
       * `protected virtual bool EnableMultiCoreJit => true;`
       * `protected virtual string JitProfileDirectory => ...;` (defaulting to `%LocalAppData%\<EntryAssemblyName>\JitProfiles` when `Assembly.GetEntryAssembly()` is non-null).
     * In `PreInitialize()`, call `ConfigureMultiCoreJit()`:
       * Create `JitProfileDirectory`, call `ProfileOptimization.SetProfileRoot(profileDir)`, and call `ProfileOptimization.StartProfile("Startup.net48.jitprofile")`.
       * **Debug File Fallback Logging (User Requirement on `Q5`):** If an exception occurs during directory creation or `ProfileOptimization` setup, catch the exception, degrade gracefully to normal CLR 4.8 JIT without interrupting startup, and invoke `WriteMultiCoreJitFallbackDebugLog(string attemptedProfileDirectory, Exception exception)`:
         * Formats a diagnostic record containing ISO-8601 timestamp (`DateTimeOffset.Now.ToString("O")`), `Environment.OSVersion`, `Environment.Is64BitProcess`, `attemptedProfileDirectory`, and `exception.ToString()`.
         * Attempts to append the diagnostic entry to `%LocalAppData%\<EntryAssemblyName>\Logs\SomaFrameQD.JitFallback.debug.log` (and if `%LocalAppData%` itself is inaccessible/read-only, falls back to `Path.Combine(Path.GetTempPath(), "<EntryAssemblyName>.JitFallback.debug.log")`, plus `System.Diagnostics.Debug.WriteLine`—wrapped in an inner `try/catch` so diagnostic logging can never fault startup).

  3. **Filtered, `IDisposable`-Clean MEF Assembly Discovery (`Q4 = 4A` Locked):**
     * Introduce `protected virtual bool ShouldInspectAssemblyFile(string filePath)` in `AppBootstrapper` that skips known non-MEF BCL/vendor filename prefixes (case-insensitive):
       * `System.`, `Microsoft.`, `netstandard.`, `mscorlib.`, `Caliburn.Micro`, `AvalonDock`, `MahApps.Metro`, `ControlzEx`, `ICSharpCode.AvalonEdit`, `DotNetProjects.Wpf.Extended.Toolkit`, `HelixToolkit`.
     * All `SomaFrameQD*` assemblies, the entry assembly, and any custom third-party plugin `.dll`s (`MyApp.Plugin.dll`, etc.) pass `ShouldInspectAssemblyFile == true` automatically!
     * For each candidate `.dll`/`.exe` in `AppDomain.CurrentDomain.BaseDirectory` not yet in `AssemblySource.Instance`, load/inspect via a properly `using`-disposed `AssemblyCatalog` (catching `BadImageFormatException` if a native C/C++ `.dll` ever sits in `BaseDirectory` so native DLLs never crash startup!) and add assemblies exporting at least one MEF `Parts.Any()` to `AssemblySource.Instance`.

  4. **Dynamic MEF Plugin Strong-Name Unification Guard (`AppDomain.CurrentDomain.AssemblyResolve` — `DRMA.md` Risk `S2-1`):**
     * Register an idempotent static `AppDomain.CurrentDomain.AssemblyResolve` handler in `AppBootstrapper` that unifies version-skewed strong-name requests (`Name` + `PublicKeyToken` match where loaded/base-directory `Version >= requestedVersion` for OOB BCL/UI packages such as `System.Runtime.CompilerServices.Unsafe`, `System.Memory`, `System.Buffers`, `System.Threading.Tasks.Extensions`, `System.Collections.Immutable`, `System.Reflection.Metadata`, and `Microsoft.Xaml.Behaviors`), preventing `FileLoadException` when plugins like `CodeCompiler` are discovered dynamically via MEF.

---

### Step 2.4 — Add Characterization Tests in `SomaFrameQD.Tests`

* **File:** `src/SomaFrameQD.Tests/Composition/AppBootstrapperOptimizationTests.cs` *(new `[STATestClass]`)*
* **Tests to Add:**
  1. `ShouldInspectAssemblyFile_SkipsKnownVendorAndBclAssemblies_AllowsSomaFrameQDAndCustomPlugins`: Verifies `Microsoft.CodeAnalysis.CSharp.dll`, `System.Text.Json.dll`, `AvalonDock.dll`, `MahApps.Metro.dll`, and `ICSharpCode.AvalonEdit.dll` return `false`, whereas `SomaFrameQD.dll`, `SomaFrameQD.Modules.CodeEditor.dll`, and `Contoso.CustomPlugin.dll` return `true`.
  2. `MultiCoreJit_WhenProfileInitializationFails_DegradesCleanlyAndWritesDebugLogFile`: Subclasses `AppBootstrapper` overriding `JitProfileDirectory` with an invalid path (e.g., containing a null/invalid path character or pointing to an existing file instead of a directory) and a test debug-log path, confirming `ConfigureMultiCoreJit()` **does not throw** AND writes the timestamped fallback exception record to the `.debug.log` file!
  3. `AssemblyResolveUnification_ResolvesHigherVersion_WhenLowerStrongNameRequested`: Simulates CLR 4.8 requesting `System.Runtime.CompilerServices.Unsafe, Version=4.0.4.1, PublicKeyToken=b03f5f7f11d50a3a` when `6.0.0.0`+ is present, confirming clean resolution without `FileLoadException`.
  4. `DemoAppManifestAndConfigs_DeclareWin7Sp1ThroughWin11SupportedOsSystemDpiAndNet48Sku`: Parses `src/SomaFrameQD.Demo/app.manifest`, `src/SomaFrameQD/app.config`, and `src/SomaFrameQD.Demo/app.config` and asserts all 4 `<supportedOS>` GUIDs (`Win7`, `Win8`, `Win8.1`, `Win10/11`), `system` DPI awareness, and `sku=".NETFramework,Version=v4.8"`.

---

## Plan #2 Verification Gate (Must Pass Before Starting Plan #3)

```powershell
dotnet build src\SomaFrameQD.sln --configuration Release --no-restore
dotnet test src\SomaFrameQD.Tests\SomaFrameQD.Tests.csproj --configuration Release --framework net48 --no-build --no-restore
dotnet pack src\SomaFrameQD.sln --configuration Release --no-build --no-restore --output build\packages
powershell -NoProfile -ExecutionPolicy Bypass -File .\eng\validate-artifacts.ps1
```

### Gate Pass Criteria
1. Zero `PublishSingleFile` members or comments remain in `AppBootstrapper.cs`, `DemoAppBootstrapper.cs`, or `Demo/App.xaml`; `DemoAppBootstrapper` is cleanly wired in `Demo/App.xaml`.
2. Both `app.config` files specify `sku=".NETFramework,Version=v4.8"` and `SomaFrameQD.Demo` embeds `app.manifest` with Windows 7 SP1 through Windows 11 `<supportedOS>` GUIDs and System-DPI settings.
3. `SomaFrameQD.sln` compiles in `Release` (`x64`) with `0 Warning(s)` and `0 Error(s)`.
4. All existing 148 STA tests **plus** all 4 new `AppBootstrapperOptimizationTests` (including the `.debug.log` Multi-Core JIT fallback test) pass on `[net48|x64]`, and `eng/validate-artifacts.ps1` passes.

Once all 4 criteria pass, proceed to **[Optimization Implementation Plan #3](./Plan_03_Win32_x64_Pointer_ABI_and_HwndHost_Interop_Hardening.md)**.
