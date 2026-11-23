# 05 — Themes, Custom Controls, Pixel Shaders & Win32 `x64` Interop (`.NET Framework 4.8` `x64`)

> **Documentation Status:** **Authoritative Post-Conversion Reference (`2026-11-23`)** — Verified against frozen `SomaFrameQD` `0.9.0` sources after Stage 1 Retargeting and Stage 2 Optimization.  
> **Scope:** [`src/SomaFrameQD/Framework/Themes/*`](../src/SomaFrameQD/Framework/Themes/), [`Themes/*`](../src/SomaFrameQD/Themes/), [`Framework/Controls/*`](../src/SomaFrameQD/Framework/Controls/), [`Framework/ShaderEffects/*`](../src/SomaFrameQD/Framework/ShaderEffects/), and [`Framework/Win32/NativeMethods.cs`](../src/SomaFrameQD/Framework/Win32/NativeMethods.cs).  
> **Companion Guides:** [01 — Architecture & Bootstrapper Overview](./01_Architecture_and_Bootstrapper_Overview.md) | [02 — Core Framework Primitives](./02_Core_Framework_Primitives_and_Coroutines.md) | [04 — Shell, AvalonDock 5 Persistence & Built-In Modules](./04_Shell_Docking_Persistence_and_BuiltIn_Modules.md)

---

## 1. Frozen VS2013 Theme Architecture & Cross-OS Window Chrome

### 1.1 `ITheme` & Built-In Themes ([`Framework/Themes/*`](../src/SomaFrameQD/Framework/Themes/))
Themes export `[Export(typeof(ITheme))]` declaring `Name` (localized display label), `ApplicationResources` (pack URIs merged into `Application.Current.Resources`), and `MainWindowResources` (pack URIs merged into `MainWindowView`'s `ThemeDictionary`). Core `SomaFrameQD` includes three built-in VS2013 themes:

| Theme Class (`GetType().Name`) | Localized `Name` Key | `ApplicationResources` Pack URI | Frozen Brushes |
| :--- | :--- | :--- | :---: |
| [`LightTheme`](../src/SomaFrameQD/Framework/Themes/LightTheme.cs) *(default)* | `ThemeLightName` | `pack://application:,,,/SomaFrameQD;component/Themes/VS2013/LightTheme.xaml` | `186` (`po:Freeze="True"`) |
| [`BlueTheme`](../src/SomaFrameQD/Framework/Themes/BlueTheme.cs) | `ThemeBlueName` | `pack://application:,,,/SomaFrameQD;component/Themes/VS2013/BlueTheme.xaml` | `186` (`po:Freeze="True"`) |
| [`DarkTheme`](../src/SomaFrameQD/Framework/Themes/DarkTheme.cs) | `ThemeDarkName` | `pack://application:,,,/SomaFrameQD;component/Themes/VS2013/DarkTheme.xaml` | `186` (`po:Freeze="True"`) |

> [!IMPORTANT]
> **Theme Lookup Key Invariant (`ThemeManager.SetCurrentTheme`):**  
> [`ThemeManager.cs`](../src/SomaFrameQD/Framework/Themes/ThemeManager.cs) resolves themes strictly by **CLR type name** (`theme.GetType().Name == name`: `"LightTheme"`, `"BlueTheme"`, `"DarkTheme"`), **never** by the localized `ITheme.Name` display string.

### 1.2 `BuiltInThemeResources` Bridging, Recursive Freezing & Atomic Rollback
1. **AvalonDock 5 Dictionary Precedence ([`BuiltInThemeResources.cs`](../src/SomaFrameQD/Framework/Themes/BuiltInThemeResources.cs)):**
   * `Dirkster.AvalonDock.Themes.VS2013` (`5.0.1`) supplies code-constructed `DictionaryTheme` subclasses (`Vs2013LightTheme`, `Vs2013BlueTheme`, `Vs2013DarkTheme`) instead of standalone XAML URIs.
   * For the three built-in themes, `ThemeManager` prepends `BuiltInThemeResources.CreateAvalonDockDictionary(theme)` **before** `theme.ApplicationResources` so SomaFrameQD's own VS2013 XAML resources sit later in `MergedDictionaries` and remain authoritative on overlapping keys. Custom third-party `ITheme` implementations return `null` and load strictly their declared URIs.
2. **562 Frozen XAML Brushes + Recursive AvalonDock `Freezable` Freezing (Stage 2 Plan #4):**
   * [`LightTheme.xaml`](../src/SomaFrameQD/Themes/VS2013/LightTheme.xaml), [`BlueTheme.xaml`](../src/SomaFrameQD/Themes/VS2013/BlueTheme.xaml), [`DarkTheme.xaml`](../src/SomaFrameQD/Themes/VS2013/DarkTheme.xaml), and [`Controls/Window.xaml`](../src/SomaFrameQD/Themes/VS2013/Controls/Window.xaml) declare `xmlns:po="http://schemas.microsoft.com/winfx/2006/xaml/presentation/options"` (`mc:Ignorable="po"`) and apply `po:Freeze="True"` across **562** `SolidColorBrush` resources (`186 × 3` palettes + `4` window-chrome brushes across `Window.xaml`/`Controls`), eliminating per-brush `DependencyObject` change-listener allocations and Gen-2 GC overhead on `.NET Framework 4.8`.
   * `BuiltInThemeResources.FreezeEligibleResources(ResourceDictionary, HashSet<ResourceDictionary>)` recursively traverses AvalonDock 5's generated `ThemeResourceDictionary` graph (with cycle-safe `visited` tracking) and calls `freezable.Freeze()` on every eligible `Freezable` (`SolidColorBrush`, `LinearGradientBrush`, `Pen`, `Geometry`).
3. **No-Op Guard & Transactional Rollback (`ThemeManager.SetCurrentTheme`):**
   * Returns `true` immediately without repainting if `CurrentTheme == theme`.
   * Snapshots both `Application` and `MainWindow` `MergedDictionaries` prior to `BeginInit()`/`Clear()`/`Add()`/`EndInit()`; if any URI throws during load, restores both snapshots atomically before rethrowing.

### 1.3 Cross-OS `MainWindowStyle` — Win7 Classic/Basic/RDP `1px` Border + Win7 Aero → Win11 Glow ([`Controls/Window.xaml`](../src/SomaFrameQD/Themes/VS2013/Controls/Window.xaml))
MahApps.Metro's `MetroWindow` suppresses standard Win32 non-client borders and normally relies on DWM-composited `GlowBrush` windows for edge definition. When DWM composition is disabled (Windows 7 Classic theme, Windows 7 Aero Basic, or non-composited Remote Desktop sessions), glow windows are invisible or omitted—leaving a `BorderThickness="0"` window indistinguishable from background windows.

`MainWindowStyle` in `Themes/VS2013/Controls/Window.xaml` pairs both mechanisms cleanly:
* Sets explicit `BorderThickness="1"` and `BorderBrush="{DynamicResource MainWindowActiveBorder}"` (switching `BorderBrush` to `MainWindowInactiveBorder` in `<Trigger Property="IsActive" Value="False">` alongside `TitleForeground`), guaranteeing a crisp `1px` border on non-DWM Windows 7 / RDP sessions.
* Retains `GlowBrush="{DynamicResource MainWindowActiveBorder}"` and `NonActiveGlowBrush="{DynamicResource MainWindowInactiveBorder}"` for DWM-composited Windows 7 Aero, Windows 8.1, Windows 10, and Windows 11 Snap Layouts.

---

## 2. Custom WPF Controls & DirectX `ps_2_0` Pixel Shaders

### 2.1 Custom Controls ([`Framework/Controls/*`](../src/SomaFrameQD/Framework/Controls/))

| Control / Helper | Base Type | Architectural Purpose |
| :--- | :--- | :--- |
| [`DynamicStyle`](../src/SomaFrameQD/Framework/Controls/DynamicStyle.cs) | Static attached properties | Enables `{DynamicResource}` style inheritance (`BaseStyle` + `DerivedStyle` and `ItemContainerBaseStyle` + `ItemContainerDerivedStyle`) by cloning `Setters` and `Triggers` onto a fresh `Style { BasedOn = baseStyle }`. |
| [`ExpanderEx`](../src/SomaFrameQD/Framework/Controls/ExpanderEx.cs) | `Expander` | Applies the compact tool-pane expander template in [`Themes/Generic.xaml`](../src/SomaFrameQD/Themes/Generic.xaml) (used in `Inspector` groups). |
| [`SliderEx`](../src/SomaFrameQD/Framework/Controls/SliderEx.cs) | `Slider` | Exposes `ThumbDragStarted` and `ThumbDragCompleted` CLR events so `Inspector` brackets interactive slider drags into one undoable action. |
| [`ClippingHwndHost`](../src/SomaFrameQD/Framework/Controls/ClippingHwndHost.cs) | `HwndHost` | Hosts a WPF `Visual` (`Content`) inside a dedicated `WS_VISIBLE \| WS_CHILD` Win32 `HwndSource` (`"SomaFrameQDClippingHwndHost"`) to enforce hard OS-level rectangular clipping against sibling native `HWND` viewports. |

### 2.2 Frozen `ps_2_0` Pixel Shaders ([`ShaderEffectBase.cs`](../src/SomaFrameQD/Framework/ShaderEffects/ShaderEffectBase.cs), [`ShaderEffectUtility.cs`](../src/SomaFrameQD/Framework/ShaderEffects/ShaderEffectUtility.cs), [`GrayscaleEffect.cs`](../src/SomaFrameQD/Framework/ShaderEffects/GrayscaleEffect.cs))
Disabled menu and toolbar icons render through `GrayscaleEffect` (`Framework/ShaderEffects/GrayscaleEffect.ps`, verified 236-byte DirectX **Shader Model `ps_2_0`** bytecode with magic header **`0xFFFF0200`**):
* `ShaderEffectBase<T>` caches one `[ThreadStatic] private static PixelShader _shader` per UI thread, resolved via `ShaderEffectUtility.GetPixelShader(typeof(T).Name)`.
* `ShaderEffectUtility.GetPixelShader` configures **`ShaderRenderMode = ShaderRenderMode.Auto`**—executing on GPU Direct3D9Ex when DWM hardware acceleration is active (Win7 Aero → Win11) and seamlessly falling back to WPF's built-in **SSE2 CPU software rasterizer** (which supports `ps_2_0` exclusively) on Win7 Basic/Classic, headless VMs, and RDP sessions—and calls **`pixelShader.Freeze()`** so static `Freezable.Changed` event roots never retain recycled menu/toolbar `GrayscaleEffect` instances.

---

## 3. Hardened 64-Bit (`x64`) Win32 & `HwndHost` Interop

### 3.1 Why `HwndWrapper` & `ClippingHwndHost` Exist in SomaFrameQD
1. **Dedicated Child `HWND` for External Renderers (`HwndWrapper`):** Standard WPF renders an entire top-level `Window` inside one DirectX-composited Win32 `HWND`, leaving external native engines (Direct3D 9/11/12 swapchains, OpenGL/Vulkan contexts, CAD/scientific viewports) with no sub-window handle to present frames into. [`HwndWrapper`](../src/SomaFrameQD/Framework/Controls/HwndWrapper.cs) subclasses `HwndHost` to allocate a real `WS_CHILD | WS_VISIBLE` child `HWND` inside any dockable AvalonDock document or tool pane.
2. **Bridging Win32 Airspace Mouse, Focus & Capture:** Because Win32 enforces strict rectangular `HWND` airspace isolation, WPF's routed mouse events (`MouseMove`, `MouseWheel`, `MouseDown`/`Up`) do not bubble out of non-WPF child `HWND`s. `HwndWrapper` intercepts raw Win32 `WndProc` messages on the child `HWND`, manages `SetFocus`/`GetFocus` and `SetCapture`/`ReleaseCapture` across `Application.Activated`/`Deactivated` transitions, and raises strongly typed `HwndMouseEventArgs` backed by `HwndMouseState`.
3. **Rectangular Splitter/Pane Clipping (`ClippingHwndHost`):** Both `HwndWrapper.CreateHostWindow` and `ClippingHwndHost.BuildWindowCore` clamp WPF's default `double.NaN` `Width`/`Height` via `GetSafeChildDimension(ActualWidth, Width)` (`>= 0` pixels) before calling `CreateWindowExW` / `HwndSourceParameters`—preventing `(int)double.NaN` (`0x80000000` = `CW_USEDEFAULT`, invalid on `WS_CHILD` windows) on 64-bit Windows.

### 3.2 64-Bit Pointer ABI & Unicode P/Invoke Hardening ([`NativeMethods.cs`](../src/SomaFrameQD/Framework/Win32/NativeMethods.cs))

| Interop Hazard in Legacy 32-Bit Code | Post-Stage-2 `x64` Solution in `NativeMethods.cs` & `HwndWrapper.cs` |
| :--- | :--- |
| **`IntPtr.ToInt32()` `OverflowException` on `WM_MOUSEWHEEL` & Negative Multi-Monitor `WM_MOUSEMOVE`** | On `x64`, `WPARAM` and `LPARAM` are 8-byte `IntPtr`s. Scrolling down (`-120` → `0x00000000FF880000`) or moving onto a left/top secondary monitor (`X = -25, Y = -10` → `0x00000000FFF6FFE7`) sets bit 31 (`> int.MaxValue`), crashing `ToInt32()`. `NativeMethods.UncheckedInt32(IntPtr)` (`unchecked((int)value.ToInt64())`), `GetWheelDeltaWParam(IntPtr)`, `GetXLParam(IntPtr)`, and `GetYLParam(IntPtr)` extract signed 16-bit words without overflow. |
| **Missed `WM_XBUTTONUP` Release Events** | Win32 clears `MK_XBUTTON1`/`MK_XBUTTON2` in `LOWORD(wParam)` on button release and reports the released button in `HIWORD(wParam)` (`XBUTTON1 = 0x0001`, `XBUTTON2 = 0x0002`). `HwndWrapper.IsXButton1`/`IsXButton2` inspects `NativeMethods.HighWord(wParamBits)` alongside `MK_XBUTTON*`. |
| **32-Bit `GetWindowLong` / `SetWindowLong` Pointer Truncation** | Replaced by `NativeMethods.GetWindowLongPtr(IntPtr, int)` and `SetWindowLongPtr(IntPtr, int, IntPtr)`—dispatching on `IntPtr.Size == 8` to `GetWindowLongPtrW`/`SetWindowLongPtrW` (with 32-bit `GetWindowLongW`/`SetWindowLongW` fallback so 32-bit XAML designers loading `AnyCPU` `SomaFrameQD.dll` remain ABI-clean). |
| **ANSI/Unicode Window-Proc Mismatch & GC Delegate Collection** | `WNDCLASSEX`, `RegisterClassExW`, `CreateWindowExW`, `DefWindowProcW`, `SendMessageW`, `GetModuleHandleW`, and `LoadCursorW` use explicit `CharSet.Unicode`; `NativeMethods.DefaultWindowProc` pins the `DefWindowProc` delegate in a `static readonly` field for the lifetime of the 64-bit `AppDomain`, and `RegisterWindowClass()` synchronizes registration once per process (`ERROR_CLASS_ALREADY_EXISTS = 1410`). |
| **Truncated `SetFocus` Return Handle** | `NativeMethods.SetFocus(IntPtr)` returns a full 64-bit `IntPtr` (`_hWndPrev`) instead of a truncated 32-bit `int`. |

### 3.3 Visibility-Gated VSync Render Loop (`HwndWrapper.UpdateRenderingSubscription`)
Previously, `HwndWrapper` subscribed to `CompositionTarget.Rendering` in its constructor and remained subscribed until `Dispose()`, waking WPF's `MediaContext` at `60–240 Hz` even when hosted on an inactive background document tab or hidden tool pane.

In `SomaFrameQD` `0.9.0` (Stage 2 Plan #4):
* `HwndWrapper` hooks `Loaded`, `Unloaded`, `IsVisibleChanged`, `BuildWindowCore`, `DestroyWindowCore`, and `Dispose`, routing all six transitions through `UpdateRenderingSubscription()`:
  ```csharp
  bool shouldSubscribe = !_isDisposed && IsLoaded && IsVisible && _hWnd != IntPtr.Zero;
  ```
* `CompositionTarget.Rendering` attaches **only** while the viewport is actively loaded, visible on screen, and backed by a live child `HWND` (exposed as `internal bool IsRenderingSubscribed` for deterministic STA verification in `Win32X64InteropTests.HwndWrapper_RenderLoop_FiresOnlyWhileLoadedAndVisible`).
* **Testing Note:** Because `CompositionTarget.Rendering` runs at `DispatcherPriority.Render` (`7`), unit tests hosting a live `HwndWrapper` inside an offscreen `Window` should use synchronous `UpdateLayout()` + `Dispose()` rather than awaiting `DispatcherPriority.Background` (`4`) drains prior to disposal.
