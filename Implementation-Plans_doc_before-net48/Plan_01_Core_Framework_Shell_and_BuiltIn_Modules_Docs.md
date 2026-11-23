# Implementation Plan #1 — Core Framework, Shell & Built-In Modules Documentation (`doc_before-net48/01`–`05`)

> **Purpose:** Capture a comprehensive, code-accurate baseline of the core `SomaFrameQD` (`SomaFrameQDWpf`) library in its **pre-`net48`-conversion state** (`net48;net10.0-windows`) inside `C:\SOFTDEV\SomaFrameQD\doc_before-net48\`, prior to executing any retargeting or optimization in [TODO.md](../TODO.md).  
> **Generated Timestamp:** `2026-11-22T18:01:29-05:00` (`2026-11-22 18:01:29 -05:00`)  
> **Execution Order:** **Execute Plan #1 FIRST**, then proceed to [Plan #2](./Plan_02_Standalone_Modules_Demo_Tests_Build_and_Root_AGENTS_REPO_MAP.md).  
> **LOC Baseline Reference:** [BEFORE_NET48_LOC_Statistics_2026-11-22_17-23-51.md](../LOC-statistics_plus_net48-retargeting/BEFORE_NET48_LOC_Statistics_2026-11-22_17-23-51.md)

---

## Context-Window Split Overview (Plan #1 vs. Plan #2)

Per the LOC baseline, the repository contains **486 core code files (`35,860` lines / `29,538` SLOC)** plus build/CI scripts and configs (`46,373` total text lines):

| Plan | Source Inspection Scope | Context Budget | Output Deliverables |
| :--- | :--- | :--- | :--- |
| **Plan #1 (This Plan)** | Core `src/SomaFrameQD` (`Framework/*`, `Modules/*`, `Themes/*`, `AppBootstrapper.cs`) + [doc/caliburn-5-migration.md](../doc/caliburn-5-migration.md) | `~12,365` Core SLOC (skipping `.resx` XML; sampling XAML styles selectively) | **5 Core Architecture & Shell Guides** (`01`–`05`) in `C:\SOFTDEV\SomaFrameQD\doc_before-net48\` |
| **[Plan #2](./Plan_02_Standalone_Modules_Demo_Tests_Build_and_Root_AGENTS_REPO_MAP.md)** | 7 `src/SomaFrameQD.Modules.*`, `src/SomaFrameQD.Demo`, `src/SomaFrameQD.Tests`, `eng/*`, [.github/workflows/ci.yml](../.github/workflows/ci.yml), [VNIR.md](../VNIR.md) | `~17,173` Core SLOC + `~1,200` build/CI SLOC (skipping `.resx` & large test boilerplate where summarized) | **4 Module/Demo/Test/Build Guides** (`06`–`09`) + `00_INDEX.md` in `C:\SOFTDEV\SomaFrameQD\doc_before-net48\` + root **`REPO_MAP.md`** & **`AGENTS.md`** |

---

## Goal & Strict Constraints

Capture a thorough, code-accurate baseline of the core `SomaFrameQD` (`SomaFrameQDWpf`) library in its current **dual-target (`net48;net10.0-windows`) pre-conversion state**, harvesting upstream XML doc comments, MEF composition contracts, and Caliburn.Micro 5 / AvalonDock 5 behavioral rules.

* **Hard Constraint — DO NOT EDIT CODE:** Read-only on all source code, `.csproj`, config, and existing build files. Only create new Markdown documentation files inside `C:\SOFTDEV\SomaFrameQD\doc_before-net48\`.
* **Context-Window Guardrails:**
  * Do **not** read `.resx` localization files (`6,838` lines across the repo) or auto-generated `.Designer.cs` files.
  * Do **not** read `src/Gemini*` or `doc/gemini-*` git-deleted paths (always read on-disk `src/SomaFrameQD*` files).
  * Inspect `src/SomaFrameQD/Themes/VS2013/Controls/*.xaml` via structure and resource keys rather than reading all 16 XAML control templates line-by-line.

---

## Deliverables to Create in `C:\SOFTDEV\SomaFrameQD\doc_before-net48\`

### 1. `01_Architecture_and_Bootstrapper_Overview.md`
* **Sources to inspect:**
  * `src/SomaFrameQD/AppBootstrapper.cs`
  * `src/SomaFrameQD/SomaFrameQD.csproj`
  * `src/SomaFrameQD/Framework/IModule.cs`, `src/SomaFrameQD/Framework/ModuleBase.cs`
  * `src/SomaFrameQD/Framework/Services/ServiceProvider.cs`, `src/SomaFrameQD/Framework/Services/ExtensionMethods.cs`
  * `README.md`, `doc/caliburn-5-migration.md`
* **Required Contents:**
  * High-level architectural overview and Mermaid component diagram (WPF + Caliburn.Micro `5.0.258` MVVM + MEF `CompositionContainer` + AvalonDock `5.0.1` + MahApps.Metro `2.4.11`).
  * `AppBootstrapper` startup & MEF catalog pipeline (`PriorityAssemblies` override catalog vs. `mainCatalog`, `DirectoryCatalog` assembly discovery, and current `.NET Core 3.1` / `.NET 5+` `PublishSingleFile` detection & `PublishSingleFileBypassAssemblies` hooks).
  * Startup readiness sequence: `OnStartup` → `DisplayRootViewForAsync<IMainWindow>()` → `IMainWindow.ShellActivationTask` → `IShell.InitializationTask` → `HandleFatalStartupException`.
  * `IModule` / `ModuleBase` lifecycle (`GlobalResourceDictionaries`, `DefaultDocuments`, `DefaultTools`, `PreInitialize()`, `Initialize()`, `PostInitializeAsync()`).

### 2. `02_Core_Framework_Primitives_and_Coroutines.md`
* **Sources to inspect:**
  * `src/SomaFrameQD/Framework/IWindow.cs`, `WindowBase.cs`, `ILayoutItem.cs`, `LayoutItemBase.cs`, `IDocument.cs`, `Document.cs`, `IPersistedDocument.cs`, `PersistedDocument.cs`, `ITool.cs`, `Tool.cs`, `IAsyncLayoutItemStateRestorer.cs`
  * `src/SomaFrameQD/Framework/AsyncCommand.cs`, `RelayCommand.cs`, `VisualTreeUtility.cs`
  * `src/SomaFrameQD/Framework/Results/*` (`Show.cs`, `IOpenResult.cs`, `OpenResultBase.cs`, `OpenDocumentResult.cs`, `ShowToolResult.cs`, `ShowWindowResult.cs`, `ShowDialogResult.cs`, `ShowCommonDialogResult.cs`, `LambdaResult.cs`)
  * `src/SomaFrameQD/Framework/Services/IEditorProvider.cs`, `EditorFileType.cs`, `IEditorOpeningService.cs`, `IResourceManager.cs`, `ResourceManager.cs`
  * `src/SomaFrameQD/Framework/Behaviors/*`, `src/SomaFrameQD/Framework/Markup/TranslateExtension.cs`, `src/SomaFrameQD/Framework/Threading/TaskUtility.cs`, `src/SomaFrameQD/Framework/Utils/ItemsControlUtility.cs`
* **Required Contents:**
  * Screen/Docking class hierarchy (`WindowBase`, `LayoutItemBase`, `Document`, `PersistedDocument` [`DoNew`/`DoLoad`/`DoSave`/dirty-guard flow], `Tool` [`PreferredLocation`, width/height, shell-routed `TryCloseAsync`]).
  * Editor file-opening pipeline (`IEditorProvider`, `EditorFileType`, `IEditorOpeningService` truthfulness contract, `[PartCreationPolicy(CreationPolicy.NonShared)]` requirement).
  * Caliburn.Micro 5 `IResult` coroutine system (`Show` static helper, `OpenDocumentResult`, `ShowToolResult`, `ShowWindowResult`, `ShowDialogResult`, `ShowCommonDialogResult`, `LambdaResult`) and terminal completion/cancellation/fault rules.
  * Localization (`TranslateExtension`), WPF attached behaviors (`WindowOptionsBehavior`, `BindableTreeViewSelectedItemBehavior`, `KeyboardFocusBehavior`), and threading/UI helpers (`TaskUtility`, `ItemsControlUtility`, `VisualTreeUtility`).

### 3. `03_Commands_Menus_ToolBars_and_KeyGestures.md`
* **Sources to inspect:**
  * `src/SomaFrameQD/Framework/Commands/*` (19 files)
  * `src/SomaFrameQD/Framework/Menus/*` (10 files)
  * `src/SomaFrameQD/Framework/ToolBars/*` (7 files)
  * `src/SomaFrameQD/Framework/Services/IInputManager.cs`, `InputManager.cs`, `InputBindingTrigger.cs`
* **Required Contents:**
  * SomaFrameQD's declarative command architecture vs. raw WPF `ICommand`: `[CommandDefinition]` (`CommandDefinition`, `CommandListDefinition`), `Command`, `TargetableCommand`.
  * Command routing & dynamic UI updates: `[CommandHandler]`, global `ICommandHandler<T>` vs. active-document contextual `ICommandRerouter`, `ICommandListHandler<T>` dynamic list population (`Populate`), `CommandRouter`, and `CommandService`.
  * Menu & Toolbar composition: `MenuBarDefinition` → `MenuDefinition` → `MenuItemGroupDefinition` → `CommandMenuItemDefinition` / `TextMenuItemDefinition`, `ToolBarDefinition` → `ToolBarItemGroupDefinition` → `CommandToolBarItemDefinition`, plus static `[Export]` registration and `Exclude*` / dynamic exclusion predicates.
  * Keyboard shortcuts: `CommandKeyboardShortcut`, `ExcludeCommandKeyboardShortcut`, `CommandKeyGestureService`, and `InputManager`.

### 4. `04_Shell_Docking_Persistence_and_BuiltIn_Modules.md`
* **Sources to inspect:**
  * All 8 built-in module folders under `src/SomaFrameQD/Modules/*`:
    * `Shell` (33 files, including `IShell.cs` in `Framework/Services`, `ShellViewModel.cs`, `TransitionCoordinator.cs`, `LayoutItemStatePersister.cs`, `EditorOpeningService.cs`, `LayoutInitializer.cs`, `PanesStyleSelector.cs`, `PanesTemplateSelector.cs`, `LayoutUtility.cs`)
    * `MainWindow` (3 files + `IMainWindow.cs`)
    * `MainMenu` (21 files)
    * `ToolBars` (20 files)
    * `StatusBar` (6 files)
    * `Toolbox` (14 files)
    * `UndoRedo` (16 files)
    * `Settings` (11 files)
* **Required Contents:**
  * `Shell` deep dive: `IShell` / `ShellViewModel`, `TransitionCoordinator` serialized FIFO async transition queue, document/tool close guards & shutdown latching, AvalonDock 5 `DockingManager` integration (`LayoutInitializer`, `PanesStyleSelector`, `PanesTemplateSelector`, `LayoutUtility`).
  * Binary + XML Layout State Persistence: `LayoutItemStatePersister`, binary envelope format + trailing AvalonDock XML layout, transactional `.tmp` / `.bak` writes, `LayoutLoadResult` (`NotFound`, `Partial`, `Corrupt`, `Failed` recovery rules), and `IAsyncLayoutItemStateRestorer`.
  * Built-in UI & service modules: `MainWindow` (MahApps `MetroWindow` host & fullscreen), `MainMenu` (`MenuBuilder`, `MenuEx`, auto-hide & language settings), `ToolBars` (`ToolBarBuilder`, main tray vs. `ToolPaneToolBar`), `StatusBar` (`IStatusBar`), `UndoRedo` (`IUndoRedoManager` per-document history & `IUndoableAction` + `IHistoryTool`), `Toolbox` (`[ToolboxItem]`, `IToolboxService`, drag-and-drop), and `Settings` (`ISettingsEditor` / `ISettingsEditorAsync` dialog hierarchy & `SettingsPropertyChangedEventManager`).

### 5. `05_Themes_Rendering_and_Win32_Interop.md`
* **Sources to inspect:**
  * `src/SomaFrameQD/Framework/Themes/*` (`ITheme.cs`, `IThemeManager.cs`, `ThemeManager.cs`, `BuiltInThemeResources.cs`, `LightTheme.cs`, `BlueTheme.cs`, `DarkTheme.cs`)
  * `src/SomaFrameQD/Themes/Generic.xaml`, `src/SomaFrameQD/Themes/VS2013/*`
  * `src/SomaFrameQD/Framework/Controls/*` (`HwndWrapper.cs`, `ClippingHwndHost.cs`, `HwndMouse*.cs`, `DynamicStyle.cs`, `ExpanderEx.cs`, `SliderEx.cs`)
  * `src/SomaFrameQD/Framework/ShaderEffects/*` (`ShaderEffectBase.cs`, `ShaderEffectUtility.cs`, `GrayscaleEffect.cs`)
  * `src/SomaFrameQD/Framework/Win32/NativeMethods.cs`
  * `src/SomaFrameQD/app.config`
* **Required Contents:**
  * Theme system (`ITheme`, `IThemeManager`, `LightTheme`, `BlueTheme`, `DarkTheme`), AvalonDock 5 generated VS2013 dictionary bridging (`BuiltInThemeResources`), MahApps.Metro resource order, and `Themes/VS2013/Controls/*.xaml` overrides.
  * Custom WPF controls (`Generic.xaml`, `ExpanderEx`, `SliderEx`, `DynamicStyle`) and pixel shader pipeline (`ShaderEffectBase`, compiled `GrayscaleEffect.ps`).
  * Native Win32 & `HwndHost` interop (`HwndWrapper`, `ClippingHwndHost`, `HwndMouseState`, `NativeMethods.cs`) — documenting exact pre-conversion behavior, render-loop wiring (`CompositionTarget.Rendering`), and current P/Invoke / `IntPtr` signatures prior to `.NET 4.8` `x64` optimization.

---

## Plan #1 Verification Gate

1. Confirm all 5 Markdown files (`01_Architecture_and_Bootstrapper_Overview.md` through `05_Themes_Rendering_and_Win32_Interop.md`) exist in `C:\SOFTDEV\SomaFrameQD\doc_before-net48\`.
2. Verify all referenced class, interface, method, and file names match existing on-disk `src/SomaFrameQD` sources.
3. Confirm via `git status` / `git diff` that **zero** source code, `.csproj`, config, or existing build files were modified.
4. Only after Plan #1 passes verification, proceed to [Implementation Plan #2](./Plan_02_Standalone_Modules_Demo_Tests_Build_and_Root_AGENTS_REPO_MAP.md).
