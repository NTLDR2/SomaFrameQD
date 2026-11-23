# 08 — Test Suite & Characterization Harness (`SomaFrameQD.Tests`, Before `.NET 4.8` Retargeting)

> **Baseline Snapshot:** Pre-`.NET Framework 4.8`-only retargeting and optimization (`2026-11-22`).  
> **Scope:** `src/SomaFrameQD.Tests/*` (`23` C# files, `7,333` Core SLOC / `8,483` core lines; `27` total text files / `8,584` lines + `2` binary state fixtures; **`146` test methods**).  
> **Target Frameworks in Snapshot:** `<TargetFrameworks>net48;net10.0-windows</TargetFrameworks>` (`MSTest.Sdk` `4.1.0`, `Microsoft.Testing.Platform` runner).

---

## 1. Test Project & Runner Configuration

`SomaFrameQD.Tests` (`src/SomaFrameQD.Tests/SomaFrameQD.Tests.csproj`) acts as an automated **characterization harness** locking down MEF composition conventions, Caliburn.Micro 5 async screen/coroutine semantics, `ShellViewModel` transition serialization, AvalonDock 5 docking/persistence/themes, and third-party WPF control contracts across both `net48` and `net10.0-windows`.

### Key Project & SDK Settings (`SomaFrameQD.Tests.csproj` & `global.json`)

| Setting | Baseline Value | Architectural Purpose |
| :--- | :--- | :--- |
| **MSBuild SDK** | `MSTest.Sdk` (`4.1.0` in `global.json`) | Unified MSTest v4 build + runner integration without separate `Microsoft.NET.Test.Sdk` package clutter. |
| **Test Runner** | `Microsoft.Testing.Platform` (`global.json`) | Uses modern `dotnet test --project <path>` CLI flags rather than legacy VSTest positional arguments. |
| **Target Frameworks** | `net48;net10.0-windows` | Executes the identical 146-test suite against .NET Framework 4.8 and .NET 10 WPF runtimes. |
| **`UseWPF`** | `true` | Enables real WPF `Application`, `Window`, `DockingManager`, `TextEditor`, and XAML resource instantiation. |
| **`UseSTASynchronizationContext`** | `true` | Installs an STA `DispatcherSynchronizationContext` on `[STATestClass]` / `[STATestMethod]` threads so `await` continuations resume on the UI thread rather than `ThreadPool` worker threads. |
| **Project References** | `SomaFrameQD`, `CodeEditor`, `Inspector`, `PropertyGrid` | Covers core shell/framework plus the three UI-heavy standalone modules with third-party control dependencies. |

### Exact CLI Execution Commands (Pre-Retargeting Baseline)

Because `global.json` sets `"runner": "Microsoft.Testing.Platform"`, always pass `--project`:

```powershell
dotnet test --project src\SomaFrameQD.Tests\SomaFrameQD.Tests.csproj --configuration Release --framework net48
dotnet test --project src\SomaFrameQD.Tests\SomaFrameQD.Tests.csproj --configuration Release --framework net10.0-windows
```

---

## 2. STA Threading, Isolation & Test Infrastructure

### 2.1 Why `[STATestClass]`, `[STATestMethod]` & `[DoNotParallelize]` Are Mandatory
1. **WPF STA Thread Affinity:** Instantiating any `Visual`, `FrameworkElement`, `Window`, `DockingManager`, `AvalonEdit.TextEditor`, or `Extended.Wpf.Toolkit` control throws `InvalidOperationException` unless the executing thread is `ApartmentState.STA`.
2. **Async Continuation Affinity (`<UseSTASynchronizationContext>true</UseSTASynchronizationContext>`):** Caliburn.Micro 5's `ActivateAsync`, `DeactivateAsync`, `ShellViewModel`'s `TransitionCoordinator`, and `EditorOpeningService` rely on `await` continuations remaining on the STA dispatcher.
3. **Process-Global Singleton State (`[DoNotParallelize]`):** Thirteen test classes mutate process-wide singletons—Caliburn's static `IoC` delegates (`IoC.GetInstance`, `IoC.GetAllInstances`, `IoC.BuildUp`), `Application.Current.Resources`, `Properties.Settings.Default`, or focus/window activation—and are marked `[DoNotParallelize]` so MSTest never interleaves them concurrently.

### 2.2 Shared Helpers (`TestInfrastructure/*` & `Framework/Results/ResultTestDoubles.cs`)

| Helper File | Lines | Key Types & Responsibilities |
| :--- | ---: | :--- |
| `TestInfrastructure/StaDispatcher.cs` | 15 | `StaDispatcher.DrainAsync()` — awaits `Dispatcher.CurrentDispatcher.InvokeAsync(delegate { }, DispatcherPriority.Background).Task` so queued WPF layout passes, data bindings, and AvalonDock focus updates settle deterministically before assertions run. |
| `TestInfrastructure/IoCOverrideScope.cs` | 46 | `sealed class IoCOverrideScope : IDisposable` — RAII guard capturing `IoC.GetInstance`, `IoC.GetAllInstances`, and `IoC.BuildUp` on entry and restoring the exact prior delegates in `Dispose()`, preventing IoC stub leaks across tests. |
| `Framework/Results/ResultTestDoubles.cs` | 325 | Coroutine test harness providing `QueuedSynchronizationContext` (verifies `OpenResultBase` marshals off-thread completions back onto the captured context), `CompletionCapture` (records exact completion count and `ResultCompletionEventArgs`), `ResultTestComposition` (minimal MEF container), `ControlledShell` & `ControlledWindowManager` (`TaskCompletionSource`-driven `IShell` and `IWindowManager` doubles), `CompletionTestDocument`/`Tool`/`Window`, and `ResultTestLifecycle`. |

---

## 3. Complete Inventory of Test Source Files (`23` Files, `146` Tests)

| Source File (`src/SomaFrameQD.Tests/...`) | Lines | Tests | Class Attributes | Primary Subsystem & Invariants Covered |
| :--- | ---: | ---: | :--- | :--- |
| `TestInfrastructure/StaDispatcher.cs` | 15 | 0 | *(helper)* | Background-priority WPF STA `Dispatcher` queue draining. |
| `TestInfrastructure/IoCOverrideScope.cs` | 46 | 0 | *(helper)* | Scoped save/restore of static `Caliburn.Micro.IoC` delegates. |
| `Composition/CompositionConventionTests.cs` | 94 | 3 | `[TestClass]` | MEF `priorityProvider` override precedence, `CreationPolicy.NonShared` documents, loud failure (`ImportCardinalityMismatchException`) on missing imports. |
| `Framework/DocumentCloseTests.cs` | 240 | 6 | `[STATestClass]` | `PersistedDocument` dirty close guard (`Yes`/`No`/`Cancel`), canceled Save-As veto on `IsNew` files, `UndoRedoManager` preservation on vetoed/faulted save vs. disposal on real close. |
| `Framework/ToolCloseTests.cs` | 125 | 3 | `[STATestClass]` | `Tool.TryCloseAsync` routing through `IShell.CloseToolAsync`, `CanCloseAsync` veto, hiding (`IsVisible = false`) without removing from `Shell.Tools`, and `ShowToolAsync` reopening. |
| `Framework/WindowBaseLifecycleTests.cs` | 120 | 2 | `[STATestClass]` | Caliburn 5 `WindowBase` async initialization/activation ordering and `AsyncEventHandler<DeactivationEventArgs>` firing. |
| `Framework/Results/ResultTestDoubles.cs` | 325 | 0 | *(helper)* | Deterministic `TaskCompletionSource` shell/window doubles and `QueuedSynchronizationContext` for `IResult` tests. |
| `Framework/Results/ResultCompletionTests.cs` | 432 | 4 | `[TestClass]`, `[DoNotParallelize]` | `LambdaResult` completion/exceptions, path-based `OpenDocumentResult` via `IEditorOpeningService`, synchronous failure paths (`1` `[TestMethod]` + `3` `[STATestMethod]`). |
| `Framework/Results/OpenDocumentResultCompletionTests.cs` | 245 | 8 | `[TestClass]` | `OpenDocumentResult` completes immediately after open (`8` `[STATestMethod]`), detaches close listener on fault/cancel, invokes `OnShutDown` once on later document close, and propagates `OnShutDown` exceptions. |
| `Framework/Results/ShowToolResultCompletionTests.cs` | 241 | 8 | `[TestClass]` | `ShowToolResult<TTool>` (`8` `[STATestMethod]`) waits for `WasClosed` deactivation, completes immediately if `ShowToolAsync` faults/cancels, enforces single terminal outcome under races. |
| `Framework/Results/ShowWindowResultCompletionTests.cs` | 244 | 8 | `[TestClass]` | `ShowWindowResult<TWindow>` (`8` `[STATestMethod]`) waits for window close via `IWindowManager.ShowWindowAsync`, handles fault/cancel/background thread marshaling. |
| `Framework/Results/ShowDialogResultCompletionTests.cs` | 267 | 9 | `[TestClass]` | `ShowDialogResult<TWindow>` (`9` `[STATestMethod]`) maps `true` → success, `false`/`null`/canceled task → `WasCancelled = true`, faulted task → `Error`, and `OnShutDown` errors. |
| `Modules/CodeEditor/AvalonEditCompatibilityTests.cs` | 208 | 3 | `[STATestClass]`, `[DoNotParallelize]` | `AvalonEdit` `6.3.1.120` editor options sync, 16 built-in languages + MEF `ILanguageDefinition` override, `TextDocument` undo/redo & document replacement. |
| `Modules/CodeEditor/CodeEditorSearchPanelTests.cs` | 274 | 4 | `[STATestClass]`, `[DoNotParallelize]` | Single `SearchPanel` ownership (`OwnedSearchPanel`), `WeakReference` GC verification on `TextDocument` replacement, host close cleanup. |
| `Modules/Inspector/InspectorActionTests.cs` | 249 | 4 | `[STATestClass]`, `[DoNotParallelize]` | `EditorBase.CanReset` side-effect freedom, `Reset()` single undo action, `SelectiveUndoEditorBase` idempotent begin/end with `null` original value, `ColorEditorViewModel` grouped undo. |
| `Modules/Settings/SettingsViewModelTests.cs` | 200 | 2 | `[STATestClass]`, `[DoNotParallelize]` | Merged `ISettingsEditor` + `ISettingsEditorAsync` tree hierarchy, first-leaf auto-selection, and fresh-instance requirement after faulted `OnInitializedAsync`. |
| `Modules/Shell/AvalonDockDockingTests.cs` | 476 | 16 | `[STATestClass]`, `[DoNotParallelize]` | `LayoutInitializer` `LeftPane`/`RightPane`/`BottomPane` placement, first-tool preferred width/height vs. subsequent tool preservation, tabbing/floating/redocking content identity. |
| `Modules/Shell/AvalonDockPersistenceTests.cs` | 1,160 | 14 | `[STATestClass]`, `[DoNotParallelize]` | SHA-256 verification of historical fixtures (`4.60`, `4.74.1`, `5.0.1`), forward migration deserialization, multi-cycle round-trips, unresolved `ContentId` cancellation, corrupt XML handling. |
| `Modules/Shell/AvalonDockThemeTests.cs` | 523 | 5 | `[STATestClass]`, `[DoNotParallelize]` | AvalonDock 5 generated `DictionaryTheme` ordering before SomaFrameQD XAML URIs, custom URI-only `ITheme` behavior, transactional rollback on broken XAML URI, Light/Blue/Dark resource resolution. |
| `Modules/Shell/EditorOpeningServiceTests.cs` | 582 | 10 | `[STATestClass]` | Delayed vs. synchronous view readiness before `provider.Open`/`New`, missing/unsupported provider `NotSupportedException`, fault/cancel propagation & listener cleanup, `NewFileCommandHandler` untitled numbering. |
| `Modules/Shell/LayoutItemStatePersisterTests.cs` | 836 | 13 | `[STATestClass]`, `[DoNotParallelize]` | Full `NotFound`/`Success`/`Partial`/`Failed`/`Corrupt` matrix, `IAsyncLayoutItemStateRestorer` ordering & failure removal, equal-but-distinct document references, duplicate `ContentId`, `.tmp`/`.bak` atomic replacement. |
| `Modules/Shell/ShellViewModelTests.cs` | 1,122 | 20 | `[STATestClass]`, `[DoNotParallelize]` | `TransitionCoordinator` FIFO serialization, nested re-entrant transition deadlock freedom, `ActiveLayoutItem` echo suppression, `OnDeactivateAsync(close: false)` vs. `close: true` `_closing` latch/reset, `InitializationTask` startup phases. |
| `Modules/Toolkit/ExtendedWpfToolkitCompatibilityTests.cs` | 459 | 4 | `[STATestClass]`, `[DoNotParallelize]` | `DotNetProjects.Wpf.Extended.Toolkit` `5.0.129.0` assembly identity (`PublicKeyToken=3e4669d2f30244f4`), template part contracts on `PropertyGrid`, `ColorPicker`, `ColorCanvas`, `CollectionControl`, and `Inspector` `ColorEditorView` integration. |

---

## 4. High-Value Characterization Suites Explained

### 4.1 Shell Concurrency & Startup (`ShellViewModelTests.cs` — 20 Tests)
Locks down the subtle interaction between Caliburn.Micro 5's async `Conductor` and AvalonDock 5's synchronous `ActiveContent` bindings:
- **FIFO Serialization & Fault Isolation:** Concurrent `OpenDocumentAsync`, `CloseDocumentAsync`, `ShowToolAsync`, and `CloseToolAsync` calls execute strictly in queue order; a throwing or canceled transition faults only its own `Task` while allowing queued follow-ups to run.
- **Nested Transition Re-Entrancy:** When a document's `OnActivateAsync` or `CanCloseAsync` awaits another shell operation (such as `ShowToolAsync`), `TransitionCoordinator`'s `AsyncLocal` flow check runs the nested transition inline rather than deadlocking behind itself.
- **AvalonDock Echo Suppression:** Verifies synchronous two-way `ActiveLayoutItem` write-backs during transitions do not spawn spurious re-activation transitions, while genuine later clicks re-activate normally.
- **Non-Closing Deactivation vs. Real Shutdown:** Proves `DeactivateAsync(close: false)` does **not** write `ApplicationState.bin` or permanently latch `_closing`, whereas `DeactivateAsync(close: true)` saves layout state once and resets `_closing = false` if close fails so retry succeeds.

### 4.2 Binary + XML State Persistence & Recovery (`LayoutItemStatePersisterTests.cs` & `AvalonDockPersistenceTests.cs` — 27 Tests)
Together, these two suites (`1,996` lines) verify every failure mode of `ApplicationState.bin`:
- **Transactional Write Safety:** Confirms writes stage through `<StateFile>.tmp` and `<StateFile>.bak` (`File.Replace`), leaving the prior valid state file intact if serialization fails mid-write and cleaning up `.tmp` artifacts.
- **Per-Record Fault Isolation (`Partial` vs. `Corrupt`):** Missing plugin types, exceptions inside `LoadState(BinaryReader)`, duplicate `ContentId`s, or throwing `IAsyncLayoutItemStateRestorer.RestoreStateAsync()` callbacks skip only the offending item (`LayoutItemStateLoadStatus.Partial`) and preserve remaining layout panes; structurally invalid headers, stream overruns past `stateEndPosition`, or malformed AvalonDock XML return `Corrupt` without deleting the corrupt file on disk.
- **Reference Equality over Value Equality:** Confirms two open documents whose overridden `Equals()` returns `true` (e.g., two untitled buffers with matching names) are still persisted and restored as distinct instances (`ReferenceEqualityComparer` semantics).

### 4.3 Coroutine `IResult` State Machines (`Framework/Results/*` — 37 Tests)
Tests every combination of synchronous/asynchronous presentation, user cancellation, task faults, `OnConfigure` / `OnShutDown` exceptions, re-execution, and background-thread completions across `OpenDocumentResult`, `ShowToolResult`, `ShowWindowResult`, `ShowDialogResult`, and `LambdaResult` using `QueuedSynchronizationContext` and `CompletionCapture`.

---

## 5. Immutable Historical Layout Fixtures (`src/SomaFrameQD.Tests/TestData/*`)

`AvalonDockPersistenceTests.cs` verifies forward-compatibility deserialization against real captured layout artifacts from three generations of AvalonDock. **Their SHA-256 digests are asserted directly in tests:**

| Fixture Path (`src/SomaFrameQD.Tests/...`) | Size | SHA-256 Digest | Origin & Test Role |
| :--- | ---: | :--- | :--- |
| `TestData/AvalonDock460/CompleteLayout.xml` | `2,109` B | `0F9177CC5005790F7EF980D6DDE413C8FF3A99F55C99DCF8A0120D4E2E4D5E4A` | Raw AvalonDock `4.60` XML layout verifying legacy docking tree element/attribute compatibility. |
| `TestData/AvalonDock4741/ApplicationState.bin` | `4,031` B | `0FF5F1CD3C57BC6AA05E3583A54AB6B42CE976D6F9C0E3BFBA1C3AEFFA1FC9A9` | Full SomaFrameQD binary envelope + AvalonDock `4.74.1` XML (`HomeViewModel` + `Inspector` + `Output` panes; documented in `TestData/AvalonDock4741/README.md`). |
| `TestData/AvalonDock501/ApplicationState.bin` | `3,749` B | `74E90837D7111A2CCE107C4468606B210F59C05C6A4BC211665E64265D3DD096` | Full SomaFrameQD binary envelope + AvalonDock `5.0.1` XML baseline (`TestData/AvalonDock501/README.md`). |

> [!CAUTION]
> **Append-Only Fixture Rule:**  
> Never overwrite, re-serialize, normalize line endings on, or regenerate `TestData/AvalonDock460/*`, `TestData/AvalonDock4741/*`, or `TestData/AvalonDock501/*`. If a future AvalonDock or persistence format revision is introduced, add a **new** sibling fixture folder and keep all historical fixtures byte-identical so forward migration remains proven.
