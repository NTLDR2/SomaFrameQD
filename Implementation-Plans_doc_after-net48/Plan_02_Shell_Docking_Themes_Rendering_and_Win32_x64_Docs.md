# Post-`net48` Documentation Plan #2 — Shell, AvalonDock 5 Persistence, Themes, Shaders & Win32 `x64` Interop (`doc_after-net48/04`–`05`)

> **Purpose:** Author the core shell, docking persistence, built-in modules, frozen VS2013 theme system, `ps_2_0` pixel shader, and hardened 64-bit (`x64`) Win32 / `HwndHost` interop guides (`04`–`05`) inside `C:\SOFTDEV\SomaFrameQD\doc_after-net48\`, verified 100% against the frozen `0.9.0` source tree.  
> **Generated Timestamp:** `2026-11-23T01:53:00-05:00` (`2026-11-23`)  
> **Prerequisite:** **[Post-`net48` Documentation Plan #1](./Plan_01_Architecture_Bootstrapper_Primitives_and_Commands_Docs.md) MUST be 100% completed and verified first.**  
> **Execution Order:** [Plan #1](./Plan_01_Architecture_Bootstrapper_Primitives_and_Commands_Docs.md) → **Plan #2 (This Plan)** → [Plan #3](./Plan_03_Standalone_Modules_and_Demo_Application_Docs.md) → [Plan #4](./Plan_04_Tests_Build_CI_Master_Index_and_Agent_Routing_Sync.md).  
> **Authoritative Post-Conversion References:** [`changelogs/20261123_CHANGELOG.md`](../changelogs/20261123_CHANGELOG.md) | [`doc/net48-win7-to-win11-x64-deployment-guide.md`](../doc/net48-win7-to-win11-x64-deployment-guide.md) | [`AGENTS.md`](../AGENTS.md)

---

## Four-Plan Context-Window Safety Split (`Implementation-Plans_doc_after-net48`)

| Plan | Source Inspection Scope (Read-Only) | Context Budget | Output Deliverables |
| :--- | :--- | :--- | :--- |
| **[Plan #1](./Plan_01_Architecture_Bootstrapper_Primitives_and_Commands_Docs.md)** | `AppBootstrapper.cs`, `DemoAppBootstrapper.cs`, `app.config` (x2), `app.manifest`, `src/Directory.Build.props`, `Framework/*` (Primitives, `Results/*`, `Services/*`, `Behaviors/*`, `Commands/*`, `Menus/*`, `ToolBars/*`) | `~4,800` targeted SLOC read | **`doc_after-net48/01`–`03`** |
| **Plan #2 (This Plan)** | `Modules/*` (all 8 built-in core modules), `Framework/Themes/*`, `Themes/VS2013/*` (sampled), `Framework/ShaderEffects/*`, `Framework/Controls/*` (`HwndWrapper`, `ClippingHwndHost`), `Framework/Win32/NativeMethods.cs` | `~5,200` targeted SLOC read (sampling VS2013 control XAML) | **`doc_after-net48/04`–`05`** (`2` shell/docking/persistence & themes/shaders/Win32 `x64` guides) |
| **[Plan #3](./Plan_03_Standalone_Modules_and_Demo_Application_Docs.md)** | All 7 `src/SomaFrameQD.Modules.*` projects + `src/SomaFrameQD.Demo` (`FastBitmap`, `HelixViewModel`, `ShaderEffectUtility`, 6 demo modules; skipping `DesignTimeImages.cs`) | `~5,800` targeted SLOC read | **`doc_after-net48/06`–`07`** |
| **[Plan #4](./Plan_04_Tests_Build_CI_Master_Index_and_Agent_Routing_Sync.md)** | `src/SomaFrameQD.Tests` (27 `.cs` files / 174 `[net48\|x64]` executions), `global.json`, `version.json` (x2), `SomaFrameQD.sln`, `eng/*`, `.github/workflows/ci.yml`, `changelogs/*`, `releasenotes/*` | `~3,500` targeted SLOC + scripts/configs | **`doc_after-net48/08`–`09` + `00_INDEX.md`** + **`AGENTS.md`** & **`REPO_MAP.md`** sync + full audit |

---

## Strict Guardrails

1. **ALL C# & SOURCE CODE IS STRICTLY FROZEN:** Read-only on all `.cs`, `.xaml`, `.csproj`, and build/config files.
2. **NEVER Modify `doc_before-net48/`:** Keep [`doc_before-net48/`](../doc_before-net48/00_INDEX.md) 100% untouched.
3. **Context-Window Discipline:** Do **not** bulk-read all 15 XAML files inside `src/SomaFrameQD/Themes/VS2013/Controls/*.xaml` (`~2,600+` lines) or any `.resx` files; read only `Themes/VS2013/Controls/Window.xaml`, the headers of `LightTheme.xaml`/`BlueTheme.xaml`/`DarkTheme.xaml`, and the modified C# files (`MainWindowView*`, `LayoutItemStatePersister.cs`, `BuiltInThemeResources.cs`, `ShaderEffectUtility.cs`, `GrayscaleEffect.cs`, `NativeMethods.cs`, `HwndWrapper.cs`, `ClippingHwndHost.cs`).

---

## Exact Deliverables in `C:\SOFTDEV\SomaFrameQD\doc_after-net48\`

### 1. `04_Shell_Docking_Persistence_and_BuiltIn_Modules.md`
* **Frozen Sources to Verify Against:**
  * [`src/SomaFrameQD/Modules/MainWindow/`](../src/SomaFrameQD/Modules/MainWindow/) ([`MainWindowView.xaml`](../src/SomaFrameQD/Modules/MainWindow/Views/MainWindowView.xaml), [`MainWindowView.xaml.cs`](../src/SomaFrameQD/Modules/MainWindow/Views/MainWindowView.xaml.cs), [`MainWindowViewModel.cs`](../src/SomaFrameQD/Modules/MainWindow/ViewModels/MainWindowViewModel.cs))
  * [`src/SomaFrameQD/Modules/Shell/`](../src/SomaFrameQD/Modules/Shell/) ([`ShellViewModel.cs`](../src/SomaFrameQD/Modules/Shell/ViewModels/ShellViewModel.cs), [`TransitionCoordinator.cs`](../src/SomaFrameQD/Modules/Shell/ViewModels/TransitionCoordinator.cs), [`Services/LayoutItemStatePersister.cs`](../src/SomaFrameQD/Modules/Shell/Services/LayoutItemStatePersister.cs), `LayoutInitializer.cs`, `PanesStyleSelector.cs`, `PanesTemplateSelector.cs`, `LayoutUtility.cs`)
  * Built-in modules `MainMenu`, `ToolBars`, `StatusBar`, `Toolbox`, `UndoRedo`, `Settings` (`ISettingsEditor` / `ISettingsEditorAsync`)
  * Baseline reference: [`doc_before-net48/04_Shell_Docking_Persistence_and_BuiltIn_Modules.md`](../doc_before-net48/04_Shell_Docking_Persistence_and_BuiltIn_Modules.md)
* **Required Technical Coverage:**
  * **`ShellViewModel` & `TransitionCoordinator`:** Serialized FIFO transition queue, `AsyncLocal` causal nested re-entrancy, `_activeLayoutItemEcho` suppression, shutdown guard latching, and `Dirkster.AvalonDock` `5.0.1` (`AvalonDock.Serializer.Xml`) integration.
  * **Transactional `ApplicationState.bin` Persistence & Buffered I/O (Stage 2 Plan #5):** Exact binary envelope wire format (item count, `ContentId`, `SomaFrameQD` `0.9.0.0` `AssemblyQualifiedName`, length-prefixed per-item payload + trailing AvalonDock XML), `16,384`-byte (`16 KB`) buffered `FileStream` I/O with `FileOptions.SequentialScan` on load, `.tmp` + `.bak` `File.Replace` transactional writes, `LayoutRestoreOutcome` (`NotFound`, `Partial`, `Corrupt`, `Failed`), and `IAsyncLayoutItemStateRestorer` post-stream restoration.
  * **Cross-OS `MainWindow` Adaptation (`Win7 SP1` → `Win11` `x64`, Stage 2 Plan #5):** MahApps `MetroWindow` host, `SnapsToDevicePixels="True"` (`MainWindowView.xaml`), dynamic System-DPI ClearType tuning via `ApplySystemDpiTextRenderingOptions()` on `Loaded` and `OnDpiChanged` (`VisualTreeHelper.GetDpi`: `TextFormattingMode.Display` at `<= 1.05x` / `96 DPI` vs. `TextFormattingMode.Ideal` at `> 1.05x` High-DPI), and `MainWindowViewModel` startup `Width`/`Height` clamping against `SystemParameters.WorkArea` (`Math.Max(640, workArea.Width - 24)` × `Math.Max(480, workArea.Height - 40)`).
  * **Remaining 6 Built-In Core Modules:** `MainMenu` (auto-hide & language selector), `ToolBars` (global tray vs. `ToolPaneToolBar`), `StatusBar`, `Toolbox` (`[ToolboxItem]` drag-and-drop), `UndoRedo` (per-document `IUndoRedoManager` & `IHistoryTool`), and `Settings` (`CreationPolicy.NonShared` `SettingsViewModel` + `ISettingsEditor`/`ISettingsEditorAsync` tree).

### 2. `05_Themes_Rendering_and_Win32_Interop.md`
* **Frozen Sources to Verify Against:**
  * [`src/SomaFrameQD/Framework/Themes/`](../src/SomaFrameQD/Framework/Themes/) (`ThemeManager.cs`, [`BuiltInThemeResources.cs`](../src/SomaFrameQD/Framework/Themes/BuiltInThemeResources.cs), `LightTheme.cs`, `BlueTheme.cs`, `DarkTheme.cs`)
  * [`src/SomaFrameQD/Themes/VS2013/`](../src/SomaFrameQD/Themes/VS2013/) (`LightTheme.xaml`, `BlueTheme.xaml`, `DarkTheme.xaml`, [`Controls/Window.xaml`](../src/SomaFrameQD/Themes/VS2013/Controls/Window.xaml)) & [`Themes/Generic.xaml`](../src/SomaFrameQD/Themes/Generic.xaml)
  * [`src/SomaFrameQD/Framework/ShaderEffects/`](../src/SomaFrameQD/Framework/ShaderEffects/) ([`ShaderEffectUtility.cs`](../src/SomaFrameQD/Framework/ShaderEffects/ShaderEffectUtility.cs), [`GrayscaleEffect.cs`](../src/SomaFrameQD/Framework/ShaderEffects/GrayscaleEffect.cs), `GrayscaleEffect.ps`)
  * [`src/SomaFrameQD/Framework/Win32/NativeMethods.cs`](../src/SomaFrameQD/Framework/Win32/NativeMethods.cs)
  * [`src/SomaFrameQD/Framework/Controls/HwndWrapper.cs`](../src/SomaFrameQD/Framework/Controls/HwndWrapper.cs) & [`ClippingHwndHost.cs`](../src/SomaFrameQD/Framework/Controls/ClippingHwndHost.cs)
  * Baseline reference: [`doc_before-net48/05_Themes_Rendering_and_Win32_Interop.md`](../doc_before-net48/05_Themes_Rendering_and_Win32_Interop.md)
* **Required Technical Coverage:**
  * **Frozen VS2013 Theme System & Cross-OS Window Chrome (Stage 2 Plans #4 & #5):**
    * `ThemeManager.SetCurrentTheme` (`theme.GetType().Name` key rule: `"LightTheme"`, `"BlueTheme"`, `"DarkTheme"`), atomic `Application` + `MainWindow` dictionary swap & rollback.
    * `BuiltInThemeResources`: prepending AvalonDock 5's generated `DictionaryTheme` ahead of `SomaFrameQD` XAML URIs + recursive `FreezeFreezables(ResourceDictionary)`.
    * **562** `po:Freeze="True"` (`xmlns:po="http://schemas.microsoft.com/winfx/2006/xaml/presentation/options"`) `SolidColorBrush` declarations (`186` Light + `186` Blue + `186` Dark + `4` `Window.xaml`).
    * Explicit `BorderThickness="1"` + `MainWindowActiveBorder`/`MainWindowInactiveBorder` triggers in `Controls/Window.xaml` providing crisp `1px` window borders on Windows 7 Classic / Aero Basic / non-DWM RDP while preserving MahApps `GlowBrush` on Aero / Win8.1 / Win10 / Win11.
  * **Custom Controls & DirectX `ps_2_0` Pixel Shaders (Stage 2 Plan #4):**
    * `ExpanderEx`, `SliderEx`, `DynamicStyle`, and `GrayscaleEffect` (`GrayscaleEffect.ps` verified `ps_2_0` header `0xFFFF0200`).
    * `ShaderEffectUtility` thread-safe `ConcurrentDictionary<string, PixelShader>` cache with `ShaderRenderMode.Auto` (Direct3D9Ex hardware on DWM + automatic WPF SSE2 CPU software fallback on Win7 Basic/RDP) and `PixelShader.Freeze()`.
  * **Hardened 64-Bit (`x64`) Win32 & `HwndHost` Interop (Stage 2 Plans #3 & #4):**
    * Architectural role of `HwndWrapper` (native Win32 child `HWND` for Direct3D/OpenGL swap-chains inside WPF airspace) and `ClippingHwndHost` (`WS_CHILD | WS_VISIBLE | WS_CLIPCHILDREN` preventing native viewport bleed over AvalonDock splitters).
    * `NativeMethods.cs` 64-bit pointer helpers (`UncheckedInt32(IntPtr)`, `GetLoWord`, `GetHiWord`, `GetXLParam`, `GetYLParam`, `GetWheelDeltaWParam`), `GetWindowLongPtr`/`SetWindowLongPtr` (`*W` Unicode entry points), explicit `CharSet.Unicode` on `WNDCLASSEX`/`RegisterClassEx`/`UnregisterClass`/`CreateWindowEx`/`DefWindowProc`, and `IntPtr` `SetFocus`.
    * `HwndWrapper.cs` overflow-proof `WM_MOUSEMOVE`, `WM_MOUSEWHEEL`, and `WM_XBUTTONDOWN`/`WM_XBUTTONUP` (`HIWORD` button inspection) handling on `x64`, `>= 1px` `NaN` dimension clamping, `IsLoaded && IsVisible && _hWnd != IntPtr.Zero` `CompositionTarget.Rendering` VSync lifecycle gating (`IsRenderingSubscribed` via `[InternalsVisibleTo("SomaFrameQD.Tests")]`), and clean `DestroyWindowCore` teardown.

---

## Plan #2 Verification Gate

1. Confirm `04_Shell_Docking_Persistence_and_BuiltIn_Modules.md` and `05_Themes_Rendering_and_Win32_Interop.md` exist in `C:\SOFTDEV\SomaFrameQD\doc_after-net48\`.
2. Run a PowerShell verification script confirming:
   * Strict UTF-8 + CRLF formatting;
   * Every relative link to `src/`, `doc/`, and `01`–`05` resolves cleanly on disk;
   * Zero stale pre-Stage-2 claims (e.g., describing 32-bit `GetWindowLong`/`wParam.ToInt32()`, unfrozen brushes, or `sku="v4.6.1"` as current);
   * Zero modifications to any `.cs`/source file or to `doc_before-net48/*`.
3. Proceed to **[Post-`net48` Documentation Plan #3](./Plan_03_Standalone_Modules_and_Demo_Application_Docs.md)** only after Plan #2 passes verification.
