# Optimization Implementation Plan #6 — Standalone Modules Part 1: `CodeCompiler` (+ `///` Roslyn `AppDomain` Docs), `Output`, `ErrorList` & `CodeEditor` (`net48` `x64`)

> **Purpose:** Optimize the four text, diagnostic, and Roslyn C# compilation standalone modules (`SomaFrameQD.Modules.CodeCompiler`, `Output`, `ErrorList`, and `CodeEditor`) for `.NET Framework 4.8` (`x64`) across Windows 7 SP1 through Windows 11—eliminating UI-thread Gen-2 `GC.Collect()` stalls, **documenting Roslyn PE emission and `.NET Framework 4.8` default-`AppDomain` assembly retention directly in code via `///` XML comments (`Q7 = 7A` Locked)**, handling hidden/metadata Roslyn diagnostics safely, fixing `OutputViewModel` multi-thread `StringBuilder` races and O(N²) UI text-flush overhead, short-circuiting unfiltered `ErrorList` queries, hardening `.xshd` base-directory discovery, and verifying `AvalonDock` (`System.Text.Json` `10.0.12`) × `Roslyn` (`3.4.0`) strong-name coexistence in a single `x64` process.  
> **Generated / Updated Timestamp:** `2026-11-22T20:58:30-05:00` (`2026-11-22` — Updated with Locked User Decision `Q7 = 7A` + Mandatory `///` Roslyn `AppDomain` Code Comments)  
> **Prerequisite:** **[Plans #1](./Plan_01_Open_Questions_Native_GAC_References_x64_and_Binding_Redirects.md)–[#5](./Plan_05_WindowChrome_Docking_DPI_and_Win7_to_Win11_OS_Adaptation.md) MUST be 100% completed and verified green first.**  
> **Target Platform Continuum:** `.NET Framework 4.8` (`net48`), **Windows 7 SP1 `x64` → Windows 8.1 `x64` → Windows 10 `x64` → Windows 11 `x64`**.  
> **Execution Order:** [Plan #1](./Plan_01_Open_Questions_Native_GAC_References_x64_and_Binding_Redirects.md) → [Plan #2](./Plan_02_CLR48_Runtime_SKU_AppManifest_Startup_JIT_and_MEF_Probing.md) → [Plan #3](./Plan_03_Win32_x64_Pointer_ABI_and_HwndHost_Interop_Hardening.md) → [Plan #4](./Plan_04_WPF_RenderLoop_Lifecycle_and_VS2013_Theme_Resource_Freezing.md) → [Plan #5](./Plan_05_WindowChrome_Docking_DPI_and_Win7_to_Win11_OS_Adaptation.md) → **Plan #6 (This Plan)** → [Plan #7](./Plan_07_Standalone_Modules_Part2_Inspector_GraphEditor_PropertyGrid_and_Demo.md) → [Plan #8](./Plan_08_End_to_End_x64_Win7_to_Win11_Verification_and_After_LOC_Report.md).  
> **Reference Documents:** [`TODO.md`](../TODO.md) (`Plan 2.5` Part A) | [`DRMA.md`](../DRMA.md) (`S2-1`, `S2-5`) | [`doc_before-net48/06_Standalone_Modules_Reference.md`](../doc_before-net48/06_Standalone_Modules_Reference.md) | [`AGENTS.md`](../AGENTS.md)

---

## Stage 2 Eight-Plan Sequential Roadmap (`Implementation-Plans_net48-optimizations`)

| Plan | Optimization Focus (Win7 SP1 `x64` → Win11 `x64`) | Context Budget | Gate Verification |
| :--- | :--- | :--- | :--- |
| **[Plan #1](./Plan_01_Open_Questions_Native_GAC_References_x64_and_Binding_Redirects.md)** | Locked Decisions (`Q1`–`Q7`) + Native `net48` GAC references (`System.ComponentModel.Composition`, `System.Drawing`), `x64` target config (`1A`), `portable` PDBs (`6A`), automatic binding redirects, & lockstep `package-contract.json` | `~15` files (`~650` lines) | Clean `x64` build; `SomaFrameQD.Tests` runs as `[net48\|x64]`; `validate-artifacts.ps1` passes |
| **[Plan #2](./Plan_02_CLR48_Runtime_SKU_AppManifest_Startup_JIT_and_MEF_Probing.md)** | CLR 4.8 `sku="v4.8"` (`app.config`), `app.manifest` (`<supportedOS>` Win7 SP1 → Win10/11 + System-DPI `2A`), **clean removal** of `PublishSingleFile` APIs + clean `net48` `DemoAppBootstrapper.cs` (`3B`), filtered MEF probing (`4A`), Multi-Core JIT + `.debug.log` fallback logger (`5A`), & `AssemblyResolve` unification | `~7` files (`~650` lines) | Clean `x64` build + startup/MEF/JIT-fallback-log/assembly-unification tests pass |
| **[Plan #3](./Plan_03_Win32_x64_Pointer_ABI_and_HwndHost_Interop_Hardening.md)** | 64-bit (`x64`) Win32 P/Invoke & `HwndHost` hardening (`NativeMethods.cs`, `HwndWrapper.cs`, `ClippingHwndHost.cs`, `WindowOptionsBehavior.cs`) | `~6` files (`~950` lines) | Clean `x64` build + dedicated 64-bit pointer/message overflow unit tests pass |
| **[Plan #4](./Plan_04_WPF_RenderLoop_Lifecycle_and_VS2013_Theme_Resource_Freezing.md)** | `HwndWrapper` `CompositionTarget.Rendering` visibility/loaded gating, `po:Freeze="True"` across ~340 VS2013 `SolidColorBrush` resources, & `ps_2_0` `GrayscaleEffect` check | `~8` files (`~1,050` lines) | Clean `x64` build + render-loop gating & frozen theme dictionary tests pass |
| **[Plan #5](./Plan_05_WindowChrome_Docking_DPI_and_Win7_to_Win11_OS_Adaptation.md)** | Cross-OS Window/Docking adaptation (`Win7 SP1` Aero & Classic/RDP ↔ `Win10`/`Win11` Snap + System-DPI `2A`), work-area clamping, TLS 1.2/1.3 safety, & buffered `ApplicationState.bin` I/O | `~8` files (`~750` lines) | Clean `x64` build + persistence/chrome/bounds/TLS characterization tests pass |
| **Plan #6 (This Plan)** | Standalone Modules Part 1 (`CodeCompiler` + `///` Roslyn/`AppDomain` docs `7A`, `Output`, `ErrorList`, `CodeEditor`) | `~9` files (`~650` lines) | Clean `x64` build + combined `AvalonDock` (`System.Text.Json` `10.0.12`) × `CodeCompiler` (`Roslyn` `3.4.0`) `x64` test passes |
| **[Plan #7](./Plan_07_Standalone_Modules_Part2_Inspector_GraphEditor_PropertyGrid_and_Demo.md)** | Standalone Modules Part 2 (`Inspector`, `GraphEditor`, `PropertyGrid`) & `SomaFrameQD.Demo` (`FastBitmap`, `HelixViewModel` + `///` docs `7A`, `ScreenColorPicker`) | `~10` files (`~900` lines) | Clean `x64` build + `Inspector`, `GraphEditor`, `PropertyGrid`, & `FastBitmap` tests pass |
| **[Plan #8](./Plan_08_End_to_End_x64_Win7_to_Win11_Verification_and_After_LOC_Report.md)** | Clean-room end-to-end `x64` verification, Win7 SP1 → Win11 QA runbook, live doc sync, & timestamped `AFTER_NET48_LOC_Statistics` report | Scripts + root docs (`~500` lines) | All tests & `validate-artifacts.ps1` green on `x64`; `AFTER` LOC delta report generated |

---

## Context-Window Guardrails & Dependency Freeze Rule

* **Context Budget (`~9` targeted C# files, `~650` lines):** Skip all `.resx` and `.Designer.cs` files across `CodeEditor`, `ErrorList`, and `Output`.
* **Dependency Freeze ([`DRMA.md`](../DRMA.md) Section 4.2):** Keep `Microsoft.CodeAnalysis.CSharp` frozen at **`3.4.0`** and `AvalonEdit` frozen at **`6.3.1.120`**. Never upgrade Roslyn to `4.x+` (which drops clean `.NET Framework` / Win7 host guarantees).
* **MEF Lifetime Invariant ([`AGENTS.md`](../AGENTS.md) Invariant #2):** Preserve `[PartCreationPolicy(CreationPolicy.NonShared)]` on `CodeEditorViewModel` and `CodeEditorSettingsViewModel`.

---

## Exact Module-by-Module Implementation Steps

### Step 6.1 — Optimize `SomaFrameQD.Modules.CodeCompiler` & Add `///` Roslyn `AppDomain` Documentation (`Q7 = 7A` Locked)

* **Files:**
  * [`C:\SOFTDEV\SomaFrameQD\src\SomaFrameQD.Modules.CodeCompiler\ICodeCompiler.cs`](../src/SomaFrameQD.Modules.CodeCompiler/ICodeCompiler.cs) (`14` lines)
  * [`C:\SOFTDEV\SomaFrameQD\src\SomaFrameQD.Modules.CodeCompiler\CodeCompiler.cs`](../src/SomaFrameQD.Modules.CodeCompiler/CodeCompiler.cs) (`98` lines)
* **Defects & Bottlenecks Found:**
  1. **Line `37` (`GC.Collect();`):** Forces a full synchronous Generation-2 Stop-The-World garbage collection on the calling (UI) thread before **every** compilation! On `.NET Framework 4.8` (`x64`) with a populated WPF visual tree, a Gen-2 blocking GC stalls UI responsiveness by 20–100+ ms without reclaiming any default-`AppDomain` dynamic assemblies (`DRMA.md` Risk `S2-5`).
  2. **Lines `63–66` (`if (!diagnostic.Location.IsInSource) throw new NotSupportedException(...)`):** Any Roslyn assembly-level or metadata reference diagnostic (e.g., `CS1701` version mismatch or missing reference) has `Location == Location.None` (`IsInSource == false`), crashing `Compile(...)` with an unhandled `NotSupportedException` instead of reporting the error in `IErrorList`!
  3. **Line `85` (`DiagnosticSeverity.Hidden`):** Roslyn emits `DiagnosticSeverity.Hidden` diagnostics (`CS8019` unnecessary `using`, etc.) alongside failures; `GetItemType(DiagnosticSeverity.Hidden)` hits `default:` and **throws `ArgumentOutOfRangeException("severity")`**!
  4. **Lines `54–55`:** Redundant `ms.Seek(0, SeekOrigin.Begin);` before `ms.ToArray()`, plus un-sized `new MemoryStream()` (pre-sizing `new MemoryStream(16384)` avoids 6 intermediate buffer resize copies per compile).
* **Exact Changes (`Q7 = 7A` + User `///` Documentation Requirement):**
  * **Add thorough `/// <summary>` and `/// <remarks>` XML documentation comments directly in both `ICodeCompiler.cs` and `CodeCompiler.cs`** explicitly explaining:
    1. How `CodeCompiler` compiles C# `SyntaxTree`s via Roslyn (`Microsoft.CodeAnalysis.CSharp` `3.4.0`), emits the PE image into an in-memory `MemoryStream`, and loads it directly into `AppDomain.CurrentDomain` via `Assembly.Load(byte[])` so compiled script types can interact with host WPF ViewModels/controls without cross-domain `MarshalByRefObject` serialization overhead.
    2. How `.NET Framework 4.8` (`clr.dll`) handles Roslyn dynamic assemblies: unlike `.NET Core`'s collectible `AssemblyLoadContext`, assemblies loaded via `Assembly.Load(byte[])` into the default `AppDomain` remain pinned in loader memory until process exit (`GC.Collect()` cannot unload them and is intentionally omitted to avoid UI-thread Gen-2 GC pauses).
    3. Best practices for callers on `.NET Framework 4.8`: cache `MetadataReference` instances across compilations, debounce live keystroke-triggered script editors, and skip recompiling identical source text.
  * Remove `GC.Collect();` on line 37, pre-size `using (var ms = new MemoryStream(16384))`, and remove redundant `ms.Seek`.
  * In `ProcessResult`: skip `DiagnosticSeverity.Hidden`; when `!diagnostic.Location.IsInSource` or `string.IsNullOrWhiteSpace(lineSpan.Path)`, report the diagnostic cleanly in `_errorList` with `path: null, line: null, column: null, onClick: null` instead of throwing `NotSupportedException`.

---

### Step 6.2 — Make `SomaFrameQD.Modules.Output` Thread-Safe & Coalesce UI Flushes (`OutputViewModel.cs`)

* **File:** [`C:\SOFTDEV\SomaFrameQD\src\SomaFrameQD.Modules.Output\ViewModels\OutputViewModel.cs`](../src/SomaFrameQD.Modules.Output/ViewModels/OutputViewModel.cs) (`68` lines)
* **Defects & Bottlenecks Found:**
  1. **Data Race on `_stringBuilder` (lines `41, 51, 58, 64`):** `OutputWriter` (`TextWriter`) allows background threads to call `Append`/`AppendLine`/`Clear` concurrently while the UI thread executes `_stringBuilder.ToString()`, corrupting `StringBuilder` chunks on multi-core `x64` CPUs!
  2. **O(N²) UI-Thread String Allocation Flood (lines `57–58`):** Calling `AppendLine` 200 times synchronously invokes `Execute.OnUIThread(() => _view.SetText(_stringBuilder.ToString()))` 200 times.
* **Changes:**
  * Guard all `_stringBuilder` reads/writes (`Append`, `AppendLine`, `Clear`, snapshot `GetSnapshotText()`) with `private readonly object _syncRoot = new object();`.
  * Coalesce background UI refreshes via an atomic `_uiRefreshPending` flag (`Interlocked.CompareExchange`) posted via `Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, ...)` when called off-thread or during burst writes (while preserving synchronous fallback when `Application.Current == null` in unit tests).

---

### Step 6.3 — Optimize `SomaFrameQD.Modules.ErrorList` Notification Churn (`ErrorListViewModel.cs`)

* **File:** [`C:\SOFTDEV\SomaFrameQD\src\SomaFrameQD.Modules.ErrorList\ViewModels\ErrorListViewModel.cs`](../src/SomaFrameQD.Modules.ErrorList/ViewModels/ErrorListViewModel.cs) (`103` lines)
* **Optimizations:**
  * Short-circuit `FilteredItems`: when `ShowErrors && ShowWarnings && ShowMessages` are all `true`, return `_items` directly without allocating three chained LINQ `.Where(...)` iterators on every WPF binding read.

---

### Step 6.4 — Harden `SomaFrameQD.Modules.CodeEditor` `.xshd` Path Discovery & File I/O (`LanguageDefinitionManager.cs`, `CodeEditorViewModel.cs`)

* **Files:** [`LanguageDefinitionManager.cs`](../src/SomaFrameQD.Modules.CodeEditor/LanguageDefinitionManager.cs) and [`CodeEditorViewModel.cs`](../src/SomaFrameQD.Modules.CodeEditor/ViewModels/CodeEditorViewModel.cs)
* **Optimizations:**
  * Probe `Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SyntaxHighlighting")` first (with fallback to `Directory.GetCurrentDirectory()` if distinct and present) so `.xshd` custom grammars load reliably when launched from Start Menu shortcuts, taskbar pins, or Explorer file associations across Windows 7–11.
  * Audit `CodeEditorViewModel.DoLoad` / `DoSave` buffered I/O while preserving all 7 existing `AvalonEditCompatibilityTests` and `CodeEditorSearchPanelTests`.

---

### Step 6.5 — Add `x64` Roslyn × AvalonDock (`System.Text.Json 10.0.12`) Coexistence & Module Tests

* **Files:** Add `<ProjectReference>` to `SomaFrameQD.Modules.CodeCompiler`, `ErrorList`, and `Output` in [`src/SomaFrameQD.Tests/SomaFrameQD.Tests.csproj`](../src/SomaFrameQD.Tests/SomaFrameQD.Tests.csproj) and create `src/SomaFrameQD.Tests/Modules/CodeCompiler/CodeCompilerAndOutputTests.cs`.
* **Tests to Add (`DRMA.md` Risk `S2-1` Mandatory Test):**
  1. `CodeCompiler_CoexistsWithAvalonDockVs2013ThemeAndSystemTextJson_In64BitNet48Process`: Loads `Vs2013DarkTheme` / `DarkTheme` (`System.Text.Json` `10.0.12` + `Unsafe` `6.1.2` + `System.Memory` `4.6.3`) **and** compiles + executes an in-memory C# class via `CodeCompiler.Compile(...)` (`Microsoft.CodeAnalysis.CSharp` `3.4.0`) in the same `[net48|x64]` process, asserting zero `FileLoadException`s!
  2. `CodeCompiler_HiddenAndMetadataDiagnostics_ReportCleanlyWithoutThrowing`: Verifies hidden diagnostics (`CS8019`) and non-source diagnostics report cleanly without throwing `ArgumentOutOfRangeException` or `NotSupportedException`.
  3. `OutputViewModel_ConcurrentMultiThreadAppends_PreserveCompleteTextWithoutCorruption`: Spawns 8 parallel `Task.Run` writers appending 250 lines each through `IOutput.Writer` and verifies all 2,000 lines arrive intact.

---

## Plan #6 Verification Gate (Must Pass Before Starting Plan #7)

```powershell
dotnet restore src\SomaFrameQD.sln --configfile src\NuGet.config
dotnet build src\SomaFrameQD.sln --configuration Release --no-restore
dotnet test src\SomaFrameQD.Tests\SomaFrameQD.Tests.csproj --configuration Release --framework net48 --no-build --no-restore
dotnet pack src\SomaFrameQD.sln --configuration Release --no-build --no-restore --output build\packages
powershell -NoProfile -ExecutionPolicy Bypass -File .\eng\validate-artifacts.ps1
```

### Gate Pass Criteria
1. Clean `Release` (`x64`) build completes with `0 Warning(s)` and `0 Error(s)`, with full `///` XML documentation comments present on `ICodeCompiler` and `CodeCompiler`.
2. Combined `AvalonDock` (`System.Text.Json` `10.0.12`) × `CodeCompiler` (`Roslyn` `3.4.0`) `x64` test, concurrent `OutputViewModel` stress test, and all prior STA characterization tests pass on `[net48|x64]`.
3. `eng/validate-artifacts.ps1` passes 100%.

Once all 3 criteria pass, proceed to **[Optimization Implementation Plan #7](./Plan_07_Standalone_Modules_Part2_Inspector_GraphEditor_PropertyGrid_and_Demo.md)**.
