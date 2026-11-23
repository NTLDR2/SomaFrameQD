# Optimization Implementation Plan #1 — Locked Architectural Decisions (`Q1`–`Q7`), Native `net48` GAC References, `x64` Target Configuration & Binding Redirects

> **Purpose:**  
> 1. Record the **7 Locked Architectural Decisions (`Q1`–`Q7`)** approved by the project owner governing how `SomaFrameQD` optimizes for **`.NET Framework 4.8` (`net48`) across the entire 64-bit OS continuum from Windows 7 SP1 `x64` through Windows 8.1, Windows 10, and Windows 11 `x64`**.  
> 2. Replace `.NET Core` out-of-band compatibility NuGets (`System.ComponentModel.Composition` `6.0.0` and `System.Drawing.Common` `6.0.0`) with built-in, pre-NGen'd `.NET Framework 4.8` GAC references, enforce 64-bit (`[net48|x64]`) execution across all executables and test runners, and enable automatic `<bindingRedirect>` generation.  
> **Generated / Updated Timestamp:** `2026-11-22T20:58:30-05:00` (`2026-11-22` — Updated with Locked User Decisions `Q1`–`Q7`)  
> **Prerequisites:** **Stage 1 (`net48` Retargeting ONLY) 100% Complete & Verified** ([`TODO.md`](../TODO.md) `1.1`–`1.4` green) **AND Open Questions `Q1`–`Q7` Resolved & Locked (Complete).**  
> **Target Platform Continuum:** `.NET Framework 4.8` (`net48`), **Windows 7 SP1 `x64` → Windows 8.1 `x64` → Windows 10 `x64` → Windows 11 `x64`**.  
> **Execution Order:** **Plan #1 (This Plan — Ready for Execution)** → [Plan #2](./Plan_02_CLR48_Runtime_SKU_AppManifest_Startup_JIT_and_MEF_Probing.md) → [Plan #3](./Plan_03_Win32_x64_Pointer_ABI_and_HwndHost_Interop_Hardening.md) → [Plan #4](./Plan_04_WPF_RenderLoop_Lifecycle_and_VS2013_Theme_Resource_Freezing.md) → [Plan #5](./Plan_05_WindowChrome_Docking_DPI_and_Win7_to_Win11_OS_Adaptation.md) → [Plan #6](./Plan_06_Standalone_Modules_Part1_CodeCompiler_CodeEditor_ErrorList_Output.md) → [Plan #7](./Plan_07_Standalone_Modules_Part2_Inspector_GraphEditor_PropertyGrid_and_Demo.md) → [Plan #8](./Plan_08_End_to_End_x64_Win7_to_Win11_Verification_and_After_LOC_Report.md).  
> **Reference Documents:** [`TODO.md`](../TODO.md) (`Plan 2.1`) | [`DRMA.md`](../DRMA.md) (`S2-1`, `S2-2`) | [`Simple_Retargeting_vs_Optimization_NET48_Win7_x64_2026-11-22_17-36-26.md`](../LOC-statistics_plus_net48-retargeting/Simple_Retargeting_vs_Optimization_NET48_Win7_x64_2026-11-22_17-36-26.md) | [`AGENTS.md`](../AGENTS.md)

---

## Stage 2 Eight-Plan Sequential Roadmap (`Implementation-Plans_net48-optimizations`)

All 8 plans below have been synchronized with the **Locked Architectural Decisions (`Q1`–`Q7`)** and execute strictly one plan per session:

| Plan | Optimization Focus (Win7 SP1 `x64` → Win11 `x64`) | Context Budget | Gate Verification |
| :--- | :--- | :--- | :--- |
| **Plan #1 (This Plan)** | **Locked Decisions (`Q1`–`Q7`)** + Native `net48` GAC references (`System.ComponentModel.Composition`, `System.Drawing`), `x64` executable/test target config (`1A`), deterministic `portable` PDBs (`6A`), automatic binding redirects, & lockstep [`eng/package-contract.json`](../eng/package-contract.json) | `~15` build/contract files (`~650` lines) | Clean `x64` build; `SomaFrameQD.Tests` runs as **`[net48\|x64]`** (148/148 green); `validate-artifacts.ps1` passes |
| **[Plan #2](./Plan_02_CLR48_Runtime_SKU_AppManifest_Startup_JIT_and_MEF_Probing.md)** | CLR 4.8 `sku="v4.8"` (`app.config`), `app.manifest` (`<supportedOS>` Win7 SP1 → Win10/11 + System-DPI `2A`), **clean removal** of `PublishSingleFile` APIs + clean `net48` `DemoAppBootstrapper.cs` (`3B`), filtered MEF probing (`4A`), Multi-Core JIT + `.debug.log` fallback logger (`5A`), & `AssemblyResolve` unification | `~7` files (`~650` lines) | Clean `x64` build + startup/MEF/JIT-fallback-log/assembly-unification tests pass |
| **[Plan #3](./Plan_03_Win32_x64_Pointer_ABI_and_HwndHost_Interop_Hardening.md)** | 64-bit (`x64`) Win32 P/Invoke & `HwndHost` hardening: `GetWindowLongPtr`/`SetWindowLongPtr`, `SetFocus` `IntPtr` fix, overflow-free `WM_MOUSEWHEEL`/`WM_MOUSEMOVE`/`WM_XBUTTON` unpacking in `HwndWrapper`, `NaN` (`CW_USEDEFAULT`) size guards in `HwndWrapper` & `ClippingHwndHost`, `WindowOptionsBehavior` zero-HWND guard | `~6` files (`~950` lines) | Clean `x64` build + dedicated 64-bit pointer/message overflow unit tests pass |
| **[Plan #4](./Plan_04_WPF_RenderLoop_Lifecycle_and_VS2013_Theme_Resource_Freezing.md)** | `HwndWrapper` `CompositionTarget.Rendering` visibility/loaded gating (zero idle 60–144 Hz VSync drain), `po:Freeze="True"` across ~340 immutable VS2013 `SolidColorBrush` resources (`Light`/`Blue`/`Dark`), & `ps_2_0` `GrayscaleEffect` verification | `~8` files (`~1,050` lines) | Clean `x64` build + render-loop gating & frozen theme dictionary tests pass |
| **[Plan #5](./Plan_05_WindowChrome_Docking_DPI_and_Win7_to_Win11_OS_Adaptation.md)** | Cross-OS Window/Docking adaptation (`Win7 SP1` Aero & Classic/Basic/RDP non-DWM ↔ `Win10`/`Win11` Snap + System-DPI `2A`): `MahApps.Metro` `MetroWindow` chrome, AvalonDock floating windows, multi-monitor work-area clamping, Win7–Win11 TLS 1.2/1.3 safety, & buffered `ApplicationState.bin` I/O | `~8` files (`~750` lines) | Clean `x64` build + persistence/chrome/bounds/TLS characterization tests pass |
| **[Plan #6](./Plan_06_Standalone_Modules_Part1_CodeCompiler_CodeEditor_ErrorList_Output.md)** | Standalone Modules Part 1 (`CodeCompiler`, `Output`, `ErrorList`, `CodeEditor`): remove UI-thread `GC.Collect()`, add `///` XML docs explaining Roslyn/`AppDomain` retention (`7A`), handle non-source/hidden diagnostics, thread-safe coalesced `OutputViewModel` logging, & `BaseDirectory` `.xshd` discovery | `~9` files (`~650` lines) | Clean `x64` build + combined `AvalonDock` (`System.Text.Json` `10.0.12`) × `CodeCompiler` (`Roslyn` `3.4.0`) `x64` test passes |
| **[Plan #7](./Plan_07_Standalone_Modules_Part2_Inspector_GraphEditor_PropertyGrid_and_Demo.md)** | Standalone Modules Part 2 (`Inspector`, `GraphEditor`, `PropertyGrid`) & `SomaFrameQD.Demo`: physical-pixel `GetSystemMetrics` fix for High-DPI/multi-monitor `ScreenColorPicker`, `ConnectorItem` layout-churn guard, `FastBitmap` finalizer crash fix & `x64` pointer math, and `HelixViewModel` debounced Roslyn compilation with `///` XML docs (`7A`) | `~10` files (`~900` lines) | Clean `x64` build + `Inspector`, `GraphEditor`, `PropertyGrid`, & `FastBitmap` tests pass |
| **[Plan #8](./Plan_08_End_to_End_x64_Win7_to_Win11_Verification_and_After_LOC_Report.md)** | Clean-room end-to-end `x64` build/test/pack/`validate-artifacts.ps1` verification, PE32+/`CorFlags` & `.exe.config` redirect audit, Win7 SP1 → Win11 QA runbook, live doc sync, & timestamped **`AFTER_NET48_LOC_Statistics`** report | Scripts + root docs (`~500` lines) | All tests & `validate-artifacts.ps1` green on `x64`; `AFTER` vs. `BEFORE` LOC delta report generated |

---

## PART 1 — LOCKED ARCHITECTURAL DECISIONS (`Q1`–`Q7`, User-Approved)

> [!NOTE]
> **All 7 Open Questions Resolved (`2026-11-22`):**  
> The decisions below are locked and propagated across **[Plan #1](./Plan_01_Open_Questions_Native_GAC_References_x64_and_Binding_Redirects.md)** through **[Plan #8](./Plan_08_End_to_End_x64_Win7_to_Win11_Verification_and_After_LOC_Report.md)**.

| ID | Architectural Topic | Locked Decision | Propagated Into |
| :--- | :--- | :--- | :--- |
| **`Q1`** | **`x64` (`PE32+`) Platform Target Policy** | **Option `1A` (Approved):** Enforce `<PlatformTarget>x64</PlatformTarget>` and `<Prefer32Bit>false</Prefer32Bit>` across all executables and test runners (`SomaFrameQD.Demo`, `SomaFrameQD.Tests`, `Caliburn5PackageHost`) plus `x64` solution configurations so every runtime/test process runs strictly as 64-bit `[net48\|x64]`, while keeping the 8 published NuGet library `.dll`s `<PlatformTarget>AnyCPU</PlatformTarget>` (100% `x64`-hardened without breaking Visual Studio XAML designers). | **Plan #1**, **[Plan #3](./Plan_03_Win32_x64_Pointer_ABI_and_HwndHost_Interop_Hardening.md)**, **[Plan #8](./Plan_08_End_to_End_x64_Win7_to_Win11_Verification_and_After_LOC_Report.md)** |
| **`Q2`** | **High-DPI & Multi-Monitor Mode (`Win7 SP1` → `Win11`)** | **Option `2A` (Approved):** **System-DPI Aware Everywhere** (`<dpiAware>true</dpiAware>` + `<dpiAwareness>system</dpiAwareness>` in `app.manifest`) for maximum Win32 child-HWND (`HwndWrapper`, `ClippingHwndHost`), AvalonDock floating window, and transitive WinForms (`AvalonEdit`) airspace stability from Windows 7 SP1 to Windows 11. | **[Plan #2](./Plan_02_CLR48_Runtime_SKU_AppManifest_Startup_JIT_and_MEF_Probing.md)**, **[Plan #5](./Plan_05_WindowChrome_Docking_DPI_and_Win7_to_Win11_OS_Adaptation.md)**, **[Plan #7](./Plan_07_Standalone_Modules_Part2_Inspector_GraphEditor_PropertyGrid_and_Demo.md)**, **[Plan #8](./Plan_08_End_to_End_x64_Win7_to_Win11_Verification_and_After_LOC_Report.md)** |
| **`Q3`** | **`.NET 5+` `PublishSingleFile` Cleanup & `DemoAppBootstrapper.cs`** | **Option `3B` + Clean `net48` `DemoAppBootstrapper.cs` (Approved):** Cleanly **delete** `IsPublishSingleFileHandled`, `CheckIfSomaFrameQDAppearsPublishedToSingleFile()`, and `PublishSingleFileBypassAssemblies` from `AppBootstrapper.cs` (zero `[Obsolete]` clutter), and **refactor `DemoAppBootstrapper.cs`** (wired directly in `Demo/App.xaml`) so it is clean, elegant, and idiomatic for `.NET Framework 4.8`. | **[Plan #2](./Plan_02_CLR48_Runtime_SKU_AppManifest_Startup_JIT_and_MEF_Probing.md)**, **[Plan #7](./Plan_07_Standalone_Modules_Part2_Inspector_GraphEditor_PropertyGrid_and_Demo.md)**, **[Plan #8](./Plan_08_End_to_End_x64_Win7_to_Win11_Verification_and_After_LOC_Report.md)** |
| **`Q4`** | **Startup MEF `DirectoryCatalog` Probing Filter** | **Option `4A` (Approved):** Overridable `protected virtual bool ShouldInspectAssemblyFile(string filePath)` skipping known non-MEF BCL/vendor prefixes (`System.*`, `Microsoft.*`, `netstandard*`, `Caliburn.Micro*`, `AvalonDock*`, `MahApps.Metro*`, `ControlzEx*`, `ICSharpCode.AvalonEdit*`, `DotNetProjects.Wpf.Extended.Toolkit*`, `HelixToolkit*`) while automatically discovering all `SomaFrameQD*` and custom third-party plugin `.dll`s. | **[Plan #2](./Plan_02_CLR48_Runtime_SKU_AppManifest_Startup_JIT_and_MEF_Probing.md)**, **[Plan #8](./Plan_08_End_to_End_x64_Win7_to_Win11_Verification_and_After_LOC_Report.md)** |
| **`Q5`** | **CLR 4.8 Multi-Core Background JIT (`ProfileOptimization`)** | **Option `5A` + Debug File Fallback Logging (Approved):** Enabled by default in `%LocalAppData%\<EntryAssemblyName>\JitProfiles` (`EnableMultiCoreJit => true`). **Plus:** if Multi-Core JIT initialization degrades to normal JIT due to an exception, write a diagnostic entry (timestamp, OS, attempted path, exception) to a debug log file (`%LocalAppData%\<EntryAssemblyName>\Logs\SomaFrameQD.JitFallback.debug.log` with `%TEMP%` fallback). | **[Plan #2](./Plan_02_CLR48_Runtime_SKU_AppManifest_Startup_JIT_and_MEF_Probing.md)**, **[Plan #8](./Plan_08_End_to_End_x64_Win7_to_Win11_Verification_and_After_LOC_Report.md)** |
| **`Q6`** | **Debug Symbol (`.pdb`) Format** | **Option `6A` (Approved):** Explicit `<DebugType>portable</DebugType>` and `<Deterministic>true</Deterministic>` everywhere in `src/Directory.Build.props`. | **Plan #1**, **[Plan #8](./Plan_08_End_to_End_x64_Win7_to_Win11_Verification_and_After_LOC_Report.md)** |
| **`Q7`** | **`CodeCompiler` Roslyn `AppDomain` Policy** | **Option `7A` + Explicit `///` XML Code Documentation (Approved):** Preserve fast in-process `ICodeCompiler.Compile(...)`, remove blocking UI-thread `GC.Collect()`, cache `MetadataReference`s, debounce `HelixViewModel` (`350ms` timer), **and add thorough `///` XML documentation comments directly in `ICodeCompiler.cs`, `CodeCompiler.cs`, and `HelixViewModel.cs`** explaining Roslyn PE emission and `.NET Framework 4.8` default-`AppDomain` assembly retention. | **[Plan #6](./Plan_06_Standalone_Modules_Part1_CodeCompiler_CodeEditor_ErrorList_Output.md)**, **[Plan #7](./Plan_07_Standalone_Modules_Part2_Inspector_GraphEditor_PropertyGrid_and_Demo.md)**, **[Plan #8](./Plan_08_End_to_End_x64_Win7_to_Win11_Verification_and_After_LOC_Report.md)** |

---

## PART 2 — Plan #1 Implementation Steps

### Strict Scope & Guardrails for Plan #1
* **Context Budget:** `~15` build/project/contract files (`~650` lines total). Do **not** read `.resx`, `.Designer.cs`, or XAML control templates.
* **Out of Scope for Plan #1:** Do **not** modify `app.config`, `AppBootstrapper.cs`, `NativeMethods.cs`, `HwndWrapper.cs`, or any `.cs`/`.xaml` application code in Plan #1—isolating GAC reference swaps, `x64` targeting, and binding redirects in Plan #1 guarantees any assembly-resolution or NuGet contract issue is caught immediately before touching runtime code in [Plan #2](./Plan_02_CLR48_Runtime_SKU_AppManifest_Startup_JIT_and_MEF_Probing.md).

---

### Step 1.1 — Swap `System.ComponentModel.Composition` (`6.0.0` NuGet) to Built-In `.NET 4.8` GAC `<Reference>`

* **File:** [`C:\SOFTDEV\SomaFrameQD\src\SomaFrameQD\SomaFrameQD.csproj`](../src/SomaFrameQD/SomaFrameQD.csproj)
* **Engineering Rationale ([`TODO.md`](../TODO.md) `2.1`, [`DRMA.md`](../DRMA.md) `S2-2`):**
  * `System.ComponentModel.Composition` `6.0.0` was required only when targeting `.NET Core` / `.NET 10`. On `.NET Framework 4.8` (Windows 7 SP1 through Windows 11), `System.ComponentModel.Composition` (`4.0.0.0`, `b77a5c561934e089`) ships in the `.NET Framework` GAC with **pre-compiled native `x64` NGen images** maintained by Windows Update.
  * Replacing `<PackageReference Include="System.ComponentModel.Composition" Version="6.0.0" />` in `SomaFrameQD.csproj` with a shared `<Reference Include="System.ComponentModel.Composition" />` in `src/Directory.Build.props` (since MSBuild GAC `<Reference>` items are non-transitive across `<ProjectReference>`, while `dotnet pack` automatically emits `<frameworkAssembly assemblyName="System.ComponentModel.Composition" />` into `.nuspec` for external NuGet consumers) removes the external NuGet dependency and binds all 10 projects directly to the native `x64` NGen GAC image.

* **Exact Edit (`src/SomaFrameQD/SomaFrameQD.csproj`):**
  * In lines 13–15:
    ```xml
      <ItemGroup>
        <Reference Include="PresentationFramework.Aero" />
        <Reference Include="System.ComponentModel.Composition" />
      </ItemGroup>
    ```
  * In lines 55–62: remove `<PackageReference Include="System.ComponentModel.Composition" Version="6.0.0" />` (retaining `Caliburn.Micro` `5.0.258`, `Dirkster.AvalonDock.Serializer.Xml` `5.0.1`, `Dirkster.AvalonDock.Themes.VS2013` `5.0.1`, `DotNetProjects.Extended.Wpf.Toolkit` `5.0.129`, and `MahApps.Metro` `2.4.11`).

---

### Step 1.2 — Swap `System.Drawing.Common` (`6.0.0` NuGet) to Built-In `.NET 4.8` GAC `<Reference Include="System.Drawing" />`

* **File:** [`C:\SOFTDEV\SomaFrameQD\src\SomaFrameQD.Modules.Inspector\SomaFrameQD.Modules.Inspector.csproj`](../src/SomaFrameQD.Modules.Inspector/SomaFrameQD.Modules.Inspector.csproj)
* **Engineering Rationale ([`TODO.md`](../TODO.md) `2.1`, [`DRMA.md`](../DRMA.md) `S2-2`):**
  * `System.Drawing.Common` `6.0.0` copies an out-of-band `lib/net461/System.Drawing.Common.dll` into `bin\`. On `.NET Framework 4.8`, `Bitmap`, `Graphics.FromImage`, `Graphics.CopyFromScreen`, and `PixelFormat` (`ScreenShotUtility.cs` / `ScreenColorPicker.cs`) live natively in the OS-serviced, pre-NGen'd `.NET Framework 4.8` GAC `System.Drawing.dll` (`4.0.0.0`).
  * Replacing `<PackageReference Include="System.Drawing.Common" Version="6.0.0" />` with `<Reference Include="System.Drawing" />` eliminates the out-of-band DLL and removes all external NuGet dependencies from `SomaFrameQD.Modules.Inspector.nuspec`.

* **Exact Edit (`src/SomaFrameQD.Modules.Inspector/SomaFrameQD.Modules.Inspector.csproj` lines 35–37):**
  ```xml
    <ItemGroup>
      <Reference Include="System.Drawing" />
    </ItemGroup>
  ```

---

### Step 1.3 — Configure Deterministic `portable` Symbols (`Q6 = 6A`), Binding Redirects & `x64` Executable/Test Targeting (`Q1 = 1A`)

1. **[`C:\SOFTDEV\SomaFrameQD\src\Directory.Build.props`](../src/Directory.Build.props):**
   * Pin `<Deterministic>true</Deterministic>` and `<DebugType>portable</DebugType>` (`Q6 = 6A`).
   * Enforce `<PlatformTarget>x64</PlatformTarget>`, `<Prefer32Bit>false</Prefer32Bit>`, `<AutoGenerateBindingRedirects>true</AutoGenerateBindingRedirects>`, and `<GenerateBindingRedirectsOutputType>true</GenerateBindingRedirectsOutputType>` on executables (`Q1 = 1A`, `DRMA.md` Risk `S2-1`):
   ```xml
   <Project>
       <PropertyGroup>
           <Authors>Tim Jones</Authors>
           <RepositoryUrl>https://github.com/tgjones/SomaFrameQD</RepositoryUrl>
           <PackageLicenseExpression>Apache-2.0</PackageLicenseExpression>
           <LangVersion>12.0</LangVersion>
           <Deterministic>true</Deterministic>
       </PropertyGroup>

       <PropertyGroup>
           <DebugType>portable</DebugType>
           <IncludeSymbols>true</IncludeSymbols>
           <SymbolPackageFormat>snupkg</SymbolPackageFormat>
       </PropertyGroup>

       <PropertyGroup Condition="'$(OutputType)' == 'Exe' or '$(OutputType)' == 'WinExe'">
           <PlatformTarget>x64</PlatformTarget>
           <Prefer32Bit>false</Prefer32Bit>
           <AutoGenerateBindingRedirects>true</AutoGenerateBindingRedirects>
           <GenerateBindingRedirectsOutputType>true</GenerateBindingRedirectsOutputType>
       </PropertyGroup>

       <ItemGroup>
           <PackageReference Include="Nerdbank.GitVersioning" Version="3.5.109" PrivateAssets="all" />
       </ItemGroup>
   </Project>
   ```
2. **Explicit `x64` + Binding Redirect Properties in Executable & Test `.csproj` Files:**
   * Because `MSTest.Sdk` defines `<OutputType>Exe</OutputType>` inside its SDK `.props` (evaluated *after* `Directory.Build.props`), and `Caliburn5PackageHost` resides under `eng/compatibility/` outside `src/`, explicitly declare:
     ```xml
     <PlatformTarget>x64</PlatformTarget>
     <Prefer32Bit>false</Prefer32Bit>
     <AutoGenerateBindingRedirects>true</AutoGenerateBindingRedirects>
     <GenerateBindingRedirectsOutputType>true</GenerateBindingRedirectsOutputType>
     ```
     inside `<PropertyGroup>` in all three executable/test hosts:
     * [`src/SomaFrameQD.Demo/SomaFrameQD.Demo.csproj`](../src/SomaFrameQD.Demo/SomaFrameQD.Demo.csproj)
     * [`src/SomaFrameQD.Tests/SomaFrameQD.Tests.csproj`](../src/SomaFrameQD.Tests/SomaFrameQD.Tests.csproj)
     * [`eng/compatibility/Caliburn5PackageHost/Caliburn5PackageHost.csproj`](../eng/compatibility/Caliburn5PackageHost/Caliburn5PackageHost.csproj)
   * Also add `Debug|x64` and `Release|x64` solution configuration entries alongside `Debug|Any CPU` and `Release|Any CPU` in [`src/SomaFrameQD.sln`](../src/SomaFrameQD.sln) so Visual Studio developers selecting `x64` in the IDE toolbar have native solution platform support out-of-the-box.

---

### Step 1.4 — Update `eng/package-contract.json` in Lockstep

* **File:** [`C:\SOFTDEV\SomaFrameQD\eng\package-contract.json`](../eng/package-contract.json)
* **Changes:**
  1. Under `packages[id == "SomaFrameQDWpf"].externalDependencies`: remove `"System.ComponentModel.Composition": "6.0.0"` (retaining `Caliburn.Micro` `5.0.258`, `Dirkster.AvalonDock.Serializer.Xml` `5.0.1`, `Dirkster.AvalonDock.Themes.VS2013` `5.0.1`, `DotNetProjects.Extended.Wpf.Toolkit` `5.0.129`, and `MahApps.Metro` `2.4.11`).
  2. Under `packages[id == "SomaFrameQD.Modules.Inspector"].externalDependencies`: remove `"System.Drawing.Common": "6.0.0"` so `"externalDependencies": {}`.

---

## Plan #1 Verification Gate (Must Pass Before Starting Plan #2)

```powershell
# 1. Clean stale outputs so old x86 or System.Drawing.Common.dll artifacts cannot linger
if (Test-Path 'build\packages') { Remove-Item -Recurse -Force 'build\packages' }
Get-ChildItem -Path 'src','eng\compatibility\Caliburn5PackageHost' -Directory -Recurse -Include 'bin','obj' |
    Remove-Item -Recurse -Force

# 2. Restore and build full 10-project solution in Release
dotnet restore src\SomaFrameQD.sln --configfile src\NuGet.config
dotnet build src\SomaFrameQD.sln --configuration Release --no-restore

# 3. Verify SomaFrameQD.Tests runs in 64-BIT ([net48|x64], IntPtr.Size == 8) and passes all 148 tests
dotnet test src\SomaFrameQD.Tests\SomaFrameQD.Tests.csproj --configuration Release --framework net48 --no-build --no-restore

# 4. Verify zero OOB System.Drawing.Common.dll in bin/ and verify generated .exe.config bindingRedirects
if (Get-ChildItem -Path 'src' -Recurse -Filter 'System.Drawing.Common.dll') {
    throw 'Out-of-band System.Drawing.Common.dll still present!'
}
Select-String -Path 'src\SomaFrameQD.Demo\bin\Release\net48\SomaFrameQD.Demo.exe.config' -Pattern 'bindingRedirect'

# 5. Pack all 8 NuGet libraries and run full artifact & Caliburn5PackageHost x64 validation
dotnet pack src\SomaFrameQD.sln --configuration Release --no-build --no-restore --output build\packages
powershell -NoProfile -ExecutionPolicy Bypass -File .\eng\validate-artifacts.ps1
```

### Gate Pass Criteria
1. Clean `Release` build succeeds with **`0 Warning(s)` and `0 Error(s)`**; `System.Drawing.Common.dll` is completely absent from `bin\`; `SomaFrameQD.Demo.exe.config` and `SomaFrameQD.Tests.exe.config` contain generated `<bindingRedirect>` entries unifying `System.Runtime.CompilerServices.Unsafe`, `System.Memory`, `System.Threading.Tasks.Extensions`, and `Microsoft.Xaml.Behaviors`.
2. `SomaFrameQD.Tests.exe` executes strictly as **`[net48|x64]`** (64-bit RyuJIT `PE32+`, **not** `[net48|x86]`) and passes **148/148** tests.
3. `eng/validate-artifacts.ps1` passes 100% against the GAC-backed `.nupkg` contract, with `Caliburn5PackageHost.exe` executing as a 64-bit `net48` process.

Once all 3 criteria pass, proceed to **[Optimization Implementation Plan #2](./Plan_02_CLR48_Runtime_SKU_AppManifest_Startup_JIT_and_MEF_Probing.md)**.
