# 05 — Themes, Custom Controls, Pixel Shaders & Win32 Interop (Before `.NET 4.8` Retargeting)

> **Baseline Snapshot:** Pre-`.NET Framework 4.8`-only retargeting and optimization (`2026-11-22`).  
> **Scope:** `src/SomaFrameQD/Framework/Themes/*`, `Themes/*`, `Framework/Controls/*`, `Framework/ShaderEffects/*`, `Framework/Win32/NativeMethods.cs`, and `src/SomaFrameQD/app.config`.

---

## 1. Theme Architecture & AvalonDock 5 Resource Bridging

### 1.1 `ITheme` & Built-In Themes (`Framework/Themes/*`)

Themes are discovered via MEF `[Export(typeof(ITheme))]`:
- `string Name`: Localized human-readable display name shown in **Tools → Options → Environment → General**.
- `IEnumerable<Uri> ApplicationResources`: Pack URIs merged at the `Application.Current.Resources` level.
- `IEnumerable<Uri> MainWindowResources`: Pack URIs merged into `MainWindowView`'s `<ResourceDictionary x:Name="ThemeDictionary" />` (`mainWindow.Resources.MergedDictionaries[0]`).

Core `SomaFrameQD` ships three built-in `ITheme` implementations (adapted originally from the MIT-licensed *Wide* framework):

| Theme Class (`GetType().Name`) | Localized `Name` Resource | `ApplicationResources` Pack URI | `MainWindowResources` |
| :--- | :--- | :--- | :--- |
| `LightTheme` *(default)* | `ThemeLightName` | `pack://application:,,,/SomaFrameQD;component/Themes/VS2013/LightTheme.xaml` | *(empty)* |
| `BlueTheme` | `ThemeBlueName` | `pack://application:,,,/SomaFrameQD;component/Themes/VS2013/BlueTheme.xaml` | *(empty)* |
| `DarkTheme` | `ThemeDarkName` | `pack://application:,,,/SomaFrameQD;component/Themes/VS2013/DarkTheme.xaml` | *(empty)* |

> [!IMPORTANT]
> **Theme Lookup Key Rule (`ThemeManager.SetCurrentTheme`):**  
> `ThemeManager.SetCurrentTheme(string name)` matches themes by **CLR type name** (`x.GetType().Name == name`, e.g., `"LightTheme"`, `"BlueTheme"`, `"DarkTheme"`), **not** by the localized `ITheme.Name` property! Persisted `Properties.Settings.Default.ThemeName` always stores `SelectedTheme.GetType().Name`.

### 1.2 `ThemeManager` & Transactional Dictionary Swapping (`ThemeManager.cs`)

`ThemeManager` (`[Export(typeof(IThemeManager))]`) manages active theme state and live switching:
1. **Live Settings Subscription:** Subscribes via weak `SettingsPropertyChangedEventManager` to `Properties.Settings.Default.ThemeName`, immediately invoking `SetCurrentTheme(value)` when user settings are saved.
2. **No-Op Repaint Guard:** If `CurrentTheme == theme`, `SetCurrentTheme` returns `true` immediately without touching resource dictionaries.
3. **AvalonDock 5 Generated Dictionary Bridging (`BuiltInThemeResources.cs`):**
   - AvalonDock `5.0.1` replaced legacy URI-addressable VS2013 dictionaries with code-constructed `DictionaryTheme` classes (`Vs2013LightTheme`, `Vs2013BlueTheme`, `Vs2013DarkTheme`).
   - For the three built-in theme types, `CreateApplicationResources(theme)` prepends `avalonDockTheme.ThemeResourceDictionary` **before** `theme.ApplicationResources`. Consequently, SomaFrameQD's own `Themes/VS2013/*.xaml` resources sit later in `MergedDictionaries` and **override** AvalonDock defaults wherever keys overlap.
   - Custom third-party `ITheme` implementations return `null` from `BuiltInThemeResources.CreateAvalonDockDictionary`, loading strictly the `Uri`s they declare.
4. **Transactional Rollback:** `SetCurrentTheme` snapshots existing `Application` and `MainWindow` merged dictionaries before calling `ReplaceMergedDictionaries` (`BeginInit()` / `Clear()` / `Add()` / `EndInit()`). If any resource URI throws during XAML loading, `catch` restores both previous dictionary arrays (and removes `_applicationResourceDictionary` if newly created) before rethrowing.

### 1.3 VS2013 XAML Resource Structure (`src/SomaFrameQD/Themes/*`)

- **`Themes/Generic.xaml`:** Assembly-level default theme dictionary (`[assembly: ThemeInfo(ResourceDictionaryLocation.None, ResourceDictionaryLocation.SourceAssembly)]`) containing the default `Style` and `ExpanderToggleButton` template for `controls:ExpanderEx`.
- **`Themes/VS2013/LightTheme.xaml`, `BlueTheme.xaml`, `DarkTheme.xaml` (245 lines each):** Each merges `Controls/Merged.xaml` first and then defines the complete semantic `SolidColorBrush` palette (`Menu*`, `Toolbar*`, `MainWindow*`, `Environment*`, `History*`, `ExpanderEx*`, `StatusBar*`, `TreeView*`, `Button*`, `OptionMark*`, `TextBox*`, `SliderThumb*`, `ColorCanvas*`, `ColorPicker*`, `ScrollBar*`, `ComboBox*`, `Label*`, `FocusStyle.Brush`).
- **`Themes/VS2013/Controls/Merged.xaml`:** Merges 15 control template dictionaries:
  - Standard WPF & Toolkit controls: `Button.xaml`, `CheckBox.xaml`, `ColorCanvas.xaml`, `ComboBox.xaml`, `Focus.xaml`, `Label.xaml`, `Menu.xaml`, `ScrollBar.xaml`, `Slider.xaml`, `TextBox.xaml`, `Toolbar.xaml`, `Tooltip.xaml`, `TreeView.xaml` (assisted by `Converters/TreeViewIndentConverter.cs`).
  - MahApps window chrome overrides: `Window.xaml` (defines `MainWindowStyle` based on `MahApps.Metro.Controls.MetroWindow`, `WindowTitleFontSize = 16`, and `HeaderFontFamily = "Segoe UI, Segoe UI Light, Lucida Sans Unicode, Verdana"`) and `WindowCommands.xaml` (styles `MahApps.Metro.Controls.WindowButtonCommands` min/max/close caption buttons).

---

## 2. Custom WPF Controls (`Framework/Controls/*`)

| Control / Helper | Base Type | Purpose & Key Mechanics |
| :--- | :--- | :--- |
| `DynamicStyle` | Static attached-property helper | Solves WPF's inability to use `{DynamicResource}` inside `<Style BasedOn="...">`. Exposes `BaseStyle` + `DerivedStyle` (for `FrameworkElement.Style`) and `ItemContainerBaseStyle` + `ItemContainerDerivedStyle` (for `ItemsControl.ItemContainerStyle`), cloning `derivedStyle.Setters` and `Triggers` onto a fresh `Style { BasedOn = baseStyle }` whenever either dependency property changes. |
| `ExpanderEx` | `Expander` | Overrides `DefaultStyleKey` to apply the compact tool-pane expander template in `Themes/Generic.xaml` (used in `Inspector` collapsible groups). |
| `SliderEx` | `Slider` | Surfaces protected `OnThumbDragStarted` and `OnThumbDragCompleted` virtuals as public CLR events (`ThumbDragStarted`, `ThumbDragCompleted`) so Caliburn.Micro action messages can bracket interactive slider drags into a single undoable action (used in `Inspector`'s `RangeEditorView`). |
| `ClippingHwndHost` | `HwndHost` | Hosts a WPF `Visual` (`Content` dependency property) inside a dedicated child Win32 `HwndSource` (`"SomaFrameQDClippingHwndHost"`, `WS_VISIBLE \| WS_CHILD`) to isolate clipping/airspace regions, calling `UpdateWindowPos()` in `OnRenderSizeChanged`. |

---

## 3. DirectX Pixel Shader Effects (`Framework/ShaderEffects/*`)

SomaFrameQD includes a lightweight WPF `ShaderEffect` infrastructure used internally to render disabled toolbar and menu icons in grayscale:

- **`ShaderEffectBase<T>` (`ShaderEffect, IDisposable`):** Caches one `[ThreadStatic] private static PixelShader _shader` per UI thread, lazily loaded by convention via `ShaderEffectUtility.GetPixelShader(typeof(T).Name)` from `pack://application:,,,/SomaFrameQD;component/Framework/ShaderEffects/<TypeName>.ps`. `IDisposable.Dispose()` clears `PixelShader = null`.
- **`GrayscaleEffect` (`ShaderEffectBase<GrayscaleEffect>`):** Registers pixel-shader sampler register `s0` (`InputProperty`) and loads the pre-compiled 236-byte DirectX bytecode resource `Framework/ShaderEffects/GrayscaleEffect.ps` embedded in `SomaFrameQD.csproj`.

---

## 4. Native Win32 Interop & `HwndWrapper` (`Framework/Controls/Hwnd*`, `Framework/Win32/NativeMethods.cs`)

### 4.1 `HwndWrapper` Hosted Rendering Base Class (`HwndWrapper.cs`)
`HwndWrapper` (`abstract class HwndWrapper : HwndHost`, originally adapted from Nick Gravelyn's Ms-PL graphics host) enables external rendering APIs (Direct3D, OpenGL, custom GDI/native swapchains) to render inside a dockable WPF document or tool pane:

- **Child HWND Lifecycle:** `BuildWindowCore` registers window class `"GraphicsDeviceControlHostWindowClass"` (`RegisterClassEx` with `DefWindowProc` and `IDC_ARROW`), creates a `WS_CHILD | WS_VISIBLE` child window via `CreateWindowEx`, and destroys it via `DestroyWindow` in `DestroyWindowCore`.
- **Render Loop:** Subscribes to `CompositionTarget.Rendering` in the constructor and calls `protected abstract void Render(IntPtr windowHandle)` whenever `ActualWidth >= 1 && ActualHeight >= 1`.
- **Mouse, Focus & Capture Management:**
  - Subscribes to `Application.Current.Activated` / `Deactivated` (`ResetMouseState()`, releasing buttons and capture when unfocused).
  - Intercepts `WM_MOUSEMOVE`, `WM_MOUSELEAVE` (registered via `TrackMouseEvent` with `TME_LEAVE`), `WM_MOUSEWHEEL`, and left/right/middle/X1/X2 button down/up/double-click messages in `WndProc`, translating them into `HwndMouseEventArgs` backed by `HwndMouseState`.
  - Saves previous focus (`GetFocus`) on mouse-enter and restores it (`SetFocus`) on mouse-leave; provides `CaptureMouse()` / `ReleaseMouseCapture()` (`SetCapture` / `ReleaseCapture`) plus static `HwndMouse` cursor helpers (`GetCursorPosition`, `SetCursorPosition`, `ShowCursor`, `HideCursor`).

### 4.2 Pre-`.NET 4.8` / `x64` Baseline Interop Notes (Recorded for Stage 2 Optimization)

Because this document captures the **untouched pre-conversion baseline**, the following existing characteristics in `src/SomaFrameQD` are recorded verbatim for reference when Stage 2 (`TODO.md` Plans `2.2`–`2.4`) begins:

1. **`app.config` Runtime SKU (`src/SomaFrameQD/app.config`):** Currently specifies `<supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.6.1"/>`.
2. **Win32 Window-Long Signatures (`Framework/Win32/NativeMethods.cs`, lines 102–107):** Declares 32-bit `GetWindowLong(IntPtr hwnd, int index)` and `SetWindowLong(IntPtr hwnd, int index, int value)` (used by `WindowOptionsBehavior`).
3. **64-Bit `WPARAM` / `LPARAM` Casts (`Framework/Controls/HwndWrapper.cs`, lines 356, 394–431):** `WndProc` currently calls `wParam.ToInt32()` on `WM_MOUSEWHEEL` and uses checked `(int)wParam` / `(int)lParam` casts on `WM_XBUTTON*` and `WM_MOUSEMOVE`. On 64-bit (`x64`) processes, `IntPtr.ToInt32()` and checked `(int)IntPtr` throw `OverflowException` when bit 31 is set (such as negative mouse-wheel deltas or signed negative coordinates).
4. **`CompositionTarget.Rendering` Subscription Scope (`HwndWrapper.cs`, lines 171, 181):** Subscribes in `HwndWrapper()` constructor and unsubscribes in `Dispose(bool)`.
