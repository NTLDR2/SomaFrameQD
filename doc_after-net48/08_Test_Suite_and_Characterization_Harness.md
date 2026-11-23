# 08 — Test Suite, Characterization & Optimization Harness (`SomaFrameQD.Tests`, `[net48|x64]`)

> **Documentation Status:** **Authoritative Post-Conversion Reference (`2026-11-23`)** — Verified against frozen `SomaFrameQD.Tests` (`0.9.0`) sources after Stage 1 Retargeting and Stage 2 Optimization.  
> **Scope:** [`src/SomaFrameQD.Tests/*`](../src/SomaFrameQD.Tests/) (`27` C# files, `8,422` Core SLOC / `9,895` total text lines + `2` binary `ApplicationState.bin` fixtures + `1` XML layout fixture; **`170` test methods** expanding to **`174` `[net48|x64]` executions**).  
> **Target Framework & Runner:** `<TargetFramework>net48</TargetFramework>`, `<PlatformTarget>x64</PlatformTarget>` (`PE32+`, `<Prefer32Bit>false</Prefer32Bit>`), `MSTest.Sdk` `4.1.0` (`Microsoft.Testing.Platform` runner).  
> **Companion Guides:** [00 — Master Index](./00_INDEX.md) | [01 — Architecture & Bootstrapper](./01_Architecture_and_Bootstrapper_Overview.md) | [04 — Shell & Persistence](./04_Shell_Docking_Persistence_and_BuiltIn_Modules.md) | [05 — Themes & Win32 `x64` Interop](./05_Themes_Rendering_and_Win32_Interop.md) | [06 — Standalone Modules](./06_Standalone_Modules_Reference.md) | [09 — Build, Packaging & CI](./09_Build_Versioning_Packaging_and_CI_After_NET48.md)

---

## 1. `[net48|x64]` Test Project & Runner Configuration

[`SomaFrameQD.Tests.csproj`](../src/SomaFrameQD.Tests/SomaFrameQD.Tests.csproj) combines the **148-execution characterization baseline** (locking down MEF composition, Caliburn.Micro 5 async lifecycles/coroutines, `TransitionCoordinator`, `ApplicationState.bin`, and AvalonDock 5) with **26 Stage 2 `[net48|x64]` optimization executions** verifying 64-bit Win32 `IntPtr` ABI safety, Multi-Core JIT fallback diagnostics, cross-OS TLS/manifest rules, 562 frozen VS2013 theme brushes, `ps_2_0` shader headers, Roslyn × AvalonDock `x64` coexistence, multi-threaded `Output` logging, High-DPI `ScreenColorPicker` sampling, `ConnectorItem` GC leak freedom, and finalizer-safe `FastBitmap` disposal.

| Project / SDK Setting | Active Value | Architectural Purpose |
| :--- | :--- | :--- |
| **MSBuild SDK** | `MSTest.Sdk` (`4.1.0` in [`global.json`](../global.json)) | Unified MSTest v4 build + `Microsoft.Testing.Platform` self-contained test executable (`SomaFrameQD.Tests.exe`). |
| **Target & Architecture** | `<TargetFramework>net48</TargetFramework>`, `<PlatformTarget>x64</PlatformTarget>`, `<Prefer32Bit>false</Prefer32Bit>` | Forces 64-bit (`PE32+`, `IntPtr.Size == 8`) CLR 4.8 execution matching `SomaFrameQD.Demo` and production `x64` hosts. |
| **Binding Redirects** | `<AutoGenerateBindingRedirects>true</AutoGenerateBindingRedirects>` + `<GenerateBindingRedirectsOutputType>true</GenerateBindingRedirectsOutputType>` | Emits `SomaFrameQD.Tests.exe.config` unifying `System.Text.Json` `10.0.12`, Roslyn `3.4.0`, and BCL ValueTuple/Memory/Unsafe assemblies. |
| **WPF & STA Context** | `<UseWPF>true</UseWPF>` + `<UseSTASynchronizationContext>true</UseSTASynchronizationContext>` | Installs a WPF `DispatcherSynchronizationContext` on `[STATestClass]` / `[STATestMethod]` threads so `await` continuations resume on the UI thread. |
| **Project References** | `SomaFrameQD`, `SomaFrameQD.Demo`, + **all 7** `SomaFrameQD.Modules.*` | Exercises every library and sample assembly in one unified 64-bit `AppDomain`. |

### Exact CLI Execution Command

```powershell
dotnet test src\SomaFrameQD.Tests\SomaFrameQD.Tests.csproj --configuration Release --framework net48 --no-build --no-restore
```

---

## 2. Critical STA Threading, Dispatcher & Process-Timeout Rules

1. **Mandatory `[STATestClass]` / `[STATestMethod]` & `[DoNotParallelize]`:**
   * WPF visuals (`Window`, `DockingManager`, `TextEditor`, `HwndWrapper`, `GraphControl`, `PropertyGrid`) require `ApartmentState.STA`, and Caliburn.Micro 5's async lifecycles require `<UseSTASynchronizationContext>true</UseSTASynchronizationContext>`.
   * Seventeen test classes touch process-wide state (`Caliburn.Micro.IoC` static delegates, `Application.Current.Resources`, `Properties.Settings.Default`, or Win32 focus/mouse capture) and declare `[DoNotParallelize]` to prevent cross-test interference.
2. **Explicit `DispatcherFrame` Pumping & Shared Helpers:**
   * [`TestInfrastructure/StaDispatcher.cs`](../src/SomaFrameQD.Tests/TestInfrastructure/StaDispatcher.cs) (`15` lines): `StaDispatcher.DrainAsync()` awaits `Dispatcher.CurrentDispatcher.InvokeAsync(delegate { }, DispatcherPriority.Background).Task` so queued layout passes and data bindings settle before assertions. When waiting on background `Task.Run` producers that post `Dispatcher.BeginInvoke(DispatcherPriority.Background, ...)` callbacks onto the active STA thread, pump a nested `Dispatcher.PushFrame(new DispatcherFrame())` rather than blocking the STA thread synchronously.
   * [`TestInfrastructure/IoCOverrideScope.cs`](../src/SomaFrameQD.Tests/TestInfrastructure/IoCOverrideScope.cs) (`46` lines): `IDisposable` guard saving and restoring `IoC.GetInstance`, `IoC.GetAllInstances`, and `IoC.BuildUp`.
   * [`Framework/Results/ResultTestDoubles.cs`](../src/SomaFrameQD.Tests/Framework/Results/ResultTestDoubles.cs) (`325` lines): Deterministic `TaskCompletionSource` doubles (`ControlledShell`, `ControlledWindowManager`, `QueuedSynchronizationContext`, `CompletionCapture`) for `IResult` state-machine testing.
3. **Offscreen `HwndWrapper` Render-Priority Starvation Caveat:**
   * `CompositionTarget.Rendering` executes at **`DispatcherPriority.Render` (`7`)**, which is higher priority than **`DispatcherPriority.Background` (`4`)**. Never `await StaDispatcher.DrainAsync()` while a live `HwndWrapper` is loaded and visible inside an offscreen `Window`—use synchronous `window.UpdateLayout()` and dispose/collapse the `HwndWrapper` before draining `Background` callbacks (see [`Win32X64InteropTests.cs`](../src/SomaFrameQD.Tests/Framework/Win32/Win32X64InteropTests.cs)).
4. **Hard OS-Level Process Timeout Supervision:**
   * `Microsoft.Testing.Platform`'s built-in `--timeout` switch is cooperative and cannot preempt an unpumped Win32 modal loop or deadlocked STA message pump. Automated wrappers that need hard hang protection should supervise `SomaFrameQD.Tests.exe` with an OS-level process timeout (`Process.WaitForExit(timeoutMs)` + `Process.Kill()`).

---

## 3. Complete Inventory of All `27` Test Source Files (`174` `[net48|x64]` Executions)

| Source File (`src/SomaFrameQD.Tests/...`) | Lines | Methods / Executions | Class Attributes | Primary Subsystem & Verified Invariants |
| :--- | ---: | :---: | :--- | :--- |
| [`TestInfrastructure/StaDispatcher.cs`](../src/SomaFrameQD.Tests/TestInfrastructure/StaDispatcher.cs) | `15` | `0` / `0` | *(helper)* | Background-priority WPF STA `Dispatcher` queue draining (`DrainAsync`). |
| [`TestInfrastructure/IoCOverrideScope.cs`](../src/SomaFrameQD.Tests/TestInfrastructure/IoCOverrideScope.cs) | `46` | `0` / `0` | *(helper)* | Scoped RAII save/restore of static `Caliburn.Micro.IoC` delegates. |
| [`Composition/CompositionConventionTests.cs`](../src/SomaFrameQD.Tests/Composition/CompositionConventionTests.cs) | `94` | `3` / `3` | `[TestClass]` | MEF `priorityProvider` override precedence, `CreationPolicy.NonShared` documents, loud failure (`ImportCardinalityMismatchException`) on missing imports. |
| [`Composition/AppBootstrapperOptimizationTests.cs`](../src/SomaFrameQD.Tests/Composition/AppBootstrapperOptimizationTests.cs) | `262` | `6` / `6` | `[STATestClass]`, `[DoNotParallelize]` | **Stage 2:** `ShouldInspectAssemblyFile` vendor/BCL skip filter, Multi-Core JIT `.debug.log` fallback, `TryUnifyStrongNamedAssembly` version/token rules, `app.manifest` + `app.config` SKU/OS/System-DPI checks, native PE probing resilience, cross-OS `ResolveCrossOsSecurityProtocol` (`Win7` TLS 1.2 vs. `Win8.1`–`Win11` `SystemDefault`). |
| [`Framework/DocumentCloseTests.cs`](../src/SomaFrameQD.Tests/Framework/DocumentCloseTests.cs) | `240` | `6` / `6` | `[STATestClass]` | `PersistedDocument` dirty close guard (`Yes`/`No`/`Cancel`), canceled Save-As veto on `IsNew`, `UndoRedoManager` preservation on vetoed/faulted save vs. disposal on real close. |
| [`Framework/ToolCloseTests.cs`](../src/SomaFrameQD.Tests/Framework/ToolCloseTests.cs) | `125` | `3` / `3` | `[STATestClass]` | `Tool.TryCloseAsync` → `IShell.CloseToolAsync`, `CanCloseAsync` veto, hiding (`IsVisible = false`) while retaining in `Shell.Tools`, and `ShowToolAsync` reopening. |
| [`Framework/WindowBaseLifecycleTests.cs`](../src/SomaFrameQD.Tests/Framework/WindowBaseLifecycleTests.cs) | `120` | `2` / `2` | `[STATestClass]` | Caliburn 5 `WindowBase` async initialization/activation ordering and `AsyncEventHandler<DeactivationEventArgs>` firing. |
| [`Framework/Win32/Win32X64InteropTests.cs`](../src/SomaFrameQD.Tests/Framework/Win32/Win32X64InteropTests.cs) | `270` | `7` / `7` | `[STATestClass]`, `[DoNotParallelize]` | **Stage 2:** 64-bit process assertion (`IntPtr.Size == 8`), `GetWheelDeltaWParam` `-120` high-bit extraction, `GetXLParam`/`GetYLParam` negative multi-monitor coordinates, live `HwndWrapper` `WM_MOUSEMOVE`/`WM_MOUSEWHEEL`/`WM_XBUTTONUP` `SendMessage`, `WindowOptionsBehavior` pre/post-`HWND` `GetWindowLongPtr`/`SetWindowLongPtr`, `ClippingHwndHost` `NaN`-dimension creation, and `IsLoaded && IsVisible` VSync render-loop gating. |
| [`Framework/Results/ResultTestDoubles.cs`](../src/SomaFrameQD.Tests/Framework/Results/ResultTestDoubles.cs) | `325` | `0` / `0` | *(helper)* | Deterministic `TaskCompletionSource` shell/window doubles and `QueuedSynchronizationContext` for coroutine tests. |
| [`Framework/Results/ResultCompletionTests.cs`](../src/SomaFrameQD.Tests/Framework/Results/ResultCompletionTests.cs) | `432` | `4` / `4` | `[TestClass]`, `[DoNotParallelize]` | `LambdaResult` completion/exceptions, path-based `OpenDocumentResult` via `IEditorOpeningService`, synchronous failure paths. |
| [`Framework/Results/OpenDocumentResultCompletionTests.cs`](../src/SomaFrameQD.Tests/Framework/Results/OpenDocumentResultCompletionTests.cs) | `245` | `8` / `8` | `[TestClass]` | `OpenDocumentResult` post-open completion, fault/cancel listener cleanup, single `OnShutDown` invocation on later document close, `OnShutDown` error propagation. |
| [`Framework/Results/ShowToolResultCompletionTests.cs`](../src/SomaFrameQD.Tests/Framework/Results/ShowToolResultCompletionTests.cs) | `241` | `8` / `8` | `[TestClass]` | `ShowToolResult<TTool>` waiting for `WasClosed`, fault/cancel completion, single terminal outcome under races. |
| [`Framework/Results/ShowWindowResultCompletionTests.cs`](../src/SomaFrameQD.Tests/Framework/Results/ShowWindowResultCompletionTests.cs) | `244` | `8` / `8` | `[TestClass]` | `ShowWindowResult<TWindow>` lifecycle via `IWindowManager.ShowWindowAsync`, fault/cancel/off-thread marshaling. |
| [`Framework/Results/ShowDialogResultCompletionTests.cs`](../src/SomaFrameQD.Tests/Framework/Results/ShowDialogResultCompletionTests.cs) | `267` | `9` / `9` | `[TestClass]` | `ShowDialogResult<TWindow>` mapping (`true` → success, `false`/`null`/canceled → `WasCancelled`, faulted → `Error`). |
| [`Modules/CodeCompiler/CodeCompilerAndOutputTests.cs`](../src/SomaFrameQD.Tests/Modules/CodeCompiler/CodeCompilerAndOutputTests.cs) | `316` | `4` / `4` | `[STATestClass]`, `[DoNotParallelize]` | **Stage 2:** In-process Roslyn `3.4.0` × `AvalonDock` `5.0.1` × `System.Text.Json` `10.0.12` `[net48\|x64]` coexistence, `DiagnosticSeverity.Hidden` skip + `Location.None` metadata diagnostic reporting, 8-thread × 250-line `OutputViewModel` concurrent stress, and `ErrorListViewModel.FilteredItems` zero-allocation short-circuiting. |
| [`Modules/CodeEditor/AvalonEditCompatibilityTests.cs`](../src/SomaFrameQD.Tests/Modules/CodeEditor/AvalonEditCompatibilityTests.cs) | `208` | `3` / `3` | `[STATestClass]`, `[DoNotParallelize]` | `AvalonEdit` `6.3.1.120` options sync, 16 built-in languages + MEF `ILanguageDefinition` override, `TextDocument` undo/redo & replacement. |
| [`Modules/CodeEditor/CodeEditorSearchPanelTests.cs`](../src/SomaFrameQD.Tests/Modules/CodeEditor/CodeEditorSearchPanelTests.cs) | `274` | `4` / `4` | `[STATestClass]`, `[DoNotParallelize]` | Single `OwnedSearchPanel` ownership, `WeakReference` GC collection on `TextDocument` swap, host close cleanup. |
| [`Modules/Inspector/InspectorActionTests.cs`](../src/SomaFrameQD.Tests/Modules/Inspector/InspectorActionTests.cs) | `249` | `4` / `4` | `[STATestClass]`, `[DoNotParallelize]` | `EditorBase.CanReset` purity, `Reset()` single undo action, `SelectiveUndoEditorBase` idempotent begin/end with `null` initial value, `ColorEditorViewModel` grouped undo. |
| [`Modules/Settings/SettingsViewModelTests.cs`](../src/SomaFrameQD.Tests/Modules/Settings/SettingsViewModelTests.cs) | `200` | `2` / `2` | `[STATestClass]`, `[DoNotParallelize]` | Merged `ISettingsEditor` + `ISettingsEditorAsync` category tree, first-leaf selection, fresh-instance requirement after faulted initialization. |
| [`Modules/Shell/AvalonDockDockingTests.cs`](../src/SomaFrameQD.Tests/Modules/Shell/AvalonDockDockingTests.cs) | `476` | `16` / `16` | `[STATestClass]`, `[DoNotParallelize]` | `LayoutInitializer` `LeftPane`/`RightPane`/`BottomPane` placement, first-tool preferred dimensions vs. existing pane preservation, tab/float/redock identity. |
| [`Modules/Shell/AvalonDockPersistenceTests.cs`](../src/SomaFrameQD.Tests/Modules/Shell/AvalonDockPersistenceTests.cs) | `1,160` | `14` / `14` | `[STATestClass]`, `[DoNotParallelize]` | SHA-256 verification of historical fixtures (`4.60`, `4.74.1`, `5.0.1`), forward migration deserialization, multi-cycle round-trips, unresolved `ContentId` cancellation, corrupt XML handling. |
| [`Modules/Shell/AvalonDockThemeTests.cs`](../src/SomaFrameQD.Tests/Modules/Shell/AvalonDockThemeTests.cs) | `676` | `8` / `12` | `[STATestClass]`, `[DoNotParallelize]` | AvalonDock 5 `DictionaryTheme` ordering (`3` `[DataRow]`s in baseline + `3` `[DataRow]`s in Stage 2 `BuiltInThemes_AllSolidColorBrushResources_AreFrozenAfterLoad`), custom URI-only `ITheme`, transactional rollback, **Stage 2** `GrayscaleEffect.ps` `0xFFFF0200` `ps_2_0` + frozen `PixelShader`, and `MainWindowStyle` `1px` non-DWM border + System-DPI `TextFormattingMode` + `1024×768` work-area clamping. |
| [`Modules/Shell/EditorOpeningServiceTests.cs`](../src/SomaFrameQD.Tests/Modules/Shell/EditorOpeningServiceTests.cs) | `582` | `10` / `10` | `[STATestClass]` | View readiness (`ViewAttached` + `Loaded`) before `IEditorProvider.Open`/`New`, unsupported extension `NotSupportedException`, fault/cancel cleanup, `NewFileCommandHandler` untitled numbering. |
| [`Modules/Shell/LayoutItemStatePersisterTests.cs`](../src/SomaFrameQD.Tests/Modules/Shell/LayoutItemStatePersisterTests.cs) | `836` | `13` / `13` | `[STATestClass]`, `[DoNotParallelize]` | `NotFound`/`Success`/`Partial`/`Failed`/`Corrupt` matrix, `IAsyncLayoutItemStateRestorer` post-stream execution, reference-distinct document tracking, duplicate `ContentId`, `.tmp`/`.bak` atomic replacement. |
| [`Modules/Shell/ShellViewModelTests.cs`](../src/SomaFrameQD.Tests/Modules/Shell/ShellViewModelTests.cs) | `1,122` | `20` / `20` | `[STATestClass]`, `[DoNotParallelize]` | `TransitionCoordinator` FIFO queue, nested `AsyncLocal` inline re-entrancy, `ActiveLayoutItem` echo suppression, `OnDeactivateAsync(close: false)` vs. `close: true` `_closing` latch/reset, `InitializationTask` ordering. |
| [`Modules/StandaloneAndDemoOptimizationTests.cs`](../src/SomaFrameQD.Tests/Modules/StandaloneAndDemoOptimizationTests.cs) | `301` | `4` / `4` | `[STATestClass]`, `[DoNotParallelize]` | **Stage 2:** `ScreenShotUtility.GetVirtualScreenPhysicalBounds` + negative-origin `(0, 0)` `ScreenColorPicker.TrySamplePixel`, `ConnectorItem` `Unloaded` `WeakReference` GC collection + suppressed no-op `Position` writes, `PropertyGridViewModel` `ICommandRerouter`, and `FastBitmap` 64-bit bounds/indexing + GC finalizer-thread crash immunity. |
| [`Modules/Toolkit/ExtendedWpfToolkitCompatibilityTests.cs`](../src/SomaFrameQD.Tests/Modules/Toolkit/ExtendedWpfToolkitCompatibilityTests.cs) | `459` | `4` / `4` | `[STATestClass]`, `[DoNotParallelize]` | `DotNetProjects.Wpf.Extended.Toolkit` `5.0.129.0` (`PublicKeyToken=3e4669d2f30244f4`) identity, template part contracts (`PropertyGrid`, `ColorPicker`, `ColorCanvas`, `CollectionControl`), and `Inspector` `ColorEditorView` integration. |
| **Total (`SomaFrameQD.Tests`)** | **`9,685` `.cs` lines (`8,422` SLOC)** | **`170` / `174`** | — | **148 characterization baseline executions + 26 Stage 2 `[net48\|x64]` optimization executions = 174 passing executions.** |

---

## 4. High-Value Characterization & Optimization Suites

1. **Shell Concurrency & Startup (`ShellViewModelTests.cs` — `20` Executions):** Proves FIFO serialization across concurrent open/close/show/hide calls, deadlock-free inline execution of nested causal transitions via `AsyncLocal`, `_activeLayoutItemEcho` suppression of synchronous AvalonDock two-way `ActiveContent` write-backs, and safe `_closing` reset if shutdown is vetoed.
2. **Binary + XML State Recovery (`LayoutItemStatePersisterTests.cs` & `AvalonDockPersistenceTests.cs` — `27` Executions):** Verifies `.tmp`/`.bak` `File.Replace` transactional writes, per-record byte-length fault isolation (`Partial` degradation vs. `Corrupt` envelope rejection without deleting user state), reference-equality tracking (`ReferenceEqualityComparer`), and post-stream `IAsyncLayoutItemStateRestorer` execution.
3. **Coroutine `IResult` State Machines (`Framework/Results/*` — `37` Executions):** Exhaustively verifies `OpenDocumentResult`, `ShowToolResult`, `ShowWindowResult`, `ShowDialogResult`, and `LambdaResult` across synchronous/asynchronous presentation, user cancellation, faults, `OnShutDown` errors, and off-thread marshaling.
4. **Stage 2 `.NET Framework 4.8` `x64` & Win7→Win11 Hardening (`26` Executions across 5 Suites):** Directly verifies every runtime, P/Invoke, High-DPI, VSync, theme-freezing, shader, Roslyn, and concurrency invariant introduced during Stage 2 Optimization (`Plans 01`–`08`).

---

## 5. Immutable Historical Layout Fixtures ([`src/SomaFrameQD.Tests/TestData/*`](../src/SomaFrameQD.Tests/TestData/))

[`AvalonDockPersistenceTests.cs`](../src/SomaFrameQD.Tests/Modules/Shell/AvalonDockPersistenceTests.cs) asserts exact SHA-256 digests over real layout artifacts from three AvalonDock generations (with binary envelopes keyed to `SomaFrameQD.Tests, Version=0.9.0.0`):

| Fixture Path (`src/SomaFrameQD.Tests/...`) | Size | On-Disk SHA-256 Digest | Origin & Test Assertion |
| :--- | ---: | :--- | :--- |
| [`TestData/AvalonDock460/CompleteLayout.xml`](../src/SomaFrameQD.Tests/TestData/AvalonDock460/CompleteLayout.xml) | `2,109` B | `0F9177CC5005790F7EF980D6DDE413C8FF3A99F55C99DCF8A0120D4E2E4D5E4A` *(LF-normalized constant in test: `7697172D53DC8F9ADFDE8DF4ADC6F52AE4BA323A7E45DC336ADCFB7AFC2CCA13`)* | Raw AvalonDock `4.60` XML layout verifying legacy docking element/attribute forward compatibility. |
| [`TestData/AvalonDock4741/ApplicationState.bin`](../src/SomaFrameQD.Tests/TestData/AvalonDock4741/ApplicationState.bin) | `4,031` B | `0FF5F1CD3C57BC6AA05E3583A54AB6B42CE976D6F9C0E3BFBA1C3AEFFA1FC9A9` | Full `SomaFrameQD` binary state envelope + AvalonDock `4.74.1` XML (`TestData/AvalonDock4741/README.md`). |
| [`TestData/AvalonDock501/ApplicationState.bin`](../src/SomaFrameQD.Tests/TestData/AvalonDock501/ApplicationState.bin) | `3,749` B | `74E90837D7111A2CCE107C4468606B210F59C05C6A4BC211665E64265D3DD096` | Full `SomaFrameQD` binary state envelope + AvalonDock `5.0.1` XML writer baseline (`TestData/AvalonDock501/README.md`). |

> [!CAUTION]
> **Append-Only Fixture Rule:**  
> Never overwrite, reformat, normalize line endings on, or regenerate `TestData/AvalonDock460/*`, `TestData/AvalonDock4741/*`, or `TestData/AvalonDock501/*`. Future layout format revisions must add a new sibling fixture directory while keeping existing fixtures byte-identical.
