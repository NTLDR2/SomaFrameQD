# Version Numbers Inventory Report (VNIR)

> **Report Date:** 2026-11-22

---

## 1. Key Architectural Finding: How Project Versions Work in `SomaFrameQD`

Contrary to traditional Visual Studio solutions where each `.csproj` or `Properties\AssemblyInfo.cs` carries its own `[assembly: AssemblyVersion(...)]` or `<Version>`, **none of the 12 `.csproj` files or 6 `AssemblyInfo.cs` files hardcode an individual project version**.

Instead, version numbering uses a **centralized + split `Nerdbank.GitVersioning` scheme** across the solution:
1. **[version.json](version.json#L1-L20)** defines the centralized base version (`"0.9.0"`, SemVer 2.0) with `"pathFilters": [".", ":^src/SomaFrameQD.Demo"]` governing `SomaFrameQD`, all 7 `SomaFrameQD.Modules.*` libraries, and `SomaFrameQD.Tests`.
2. **[src/SomaFrameQD.Demo/version.json](src/SomaFrameQD.Demo/version.json#L1-L19)** defines an independent standalone version (`"0.1.0"`, SemVer 2.0) with `"pathFilters": ["."]` governing `SomaFrameQD.Demo` exclusively (without `"inherit": true`, so root exclusions do not leak into the child config).
3. **[src/Directory.Build.props](src/Directory.Build.props#L13-L15)** injects **`Nerdbank.GitVersioning` (`3.5.109`)** into every project under `src/`. At build time, NBGV walks upward to the nearest `version.json` and automatically computes and stamps `AssemblyVersion` (`0.9.0.0` for core/modules/tests; `0.1.0.0` for `SomaFrameQD.Demo`), `AssemblyFileVersion` (`0.9.0.<height>` / `0.1.0.<height>`), `AssemblyInformationalVersion`, and NuGet `PackageVersion`.
4. **[eng/validate-artifacts.ps1](eng/validate-artifacts.ps1#L336-L338)** enforces that all 8 packed NuGet libraries emit **one identical version** (`SomaFrameQD.Demo` has `<IsPackable>false</IsPackable>`, so its `0.1.0` version stays cleanly decoupled).

---

## 2. Complete Inventory of Version Numbers

### A. Product / Solution Version Control

| Location | Property / Key | Current Value | Notes |
| :--- | :--- | :--- | :--- |
| [version.json](version.json#L3-L9) | `version` / `nugetPackageVersion.semVer` | `0.9.0` / `2` | **Master product/assembly/package version** for `SomaFrameQD`, all 7 modules, and `SomaFrameQD.Tests` (`pathFilters` excludes `src/SomaFrameQD.Demo`) |
| [src/SomaFrameQD.Demo/version.json](src/SomaFrameQD.Demo/version.json#L3-L8) | `version` / `nugetPackageVersion.semVer` | `0.1.0` / `2` | **Independent alpha version** for `SomaFrameQD.Demo` only (`pathFilters: ["."]`) |
| [src/Directory.Build.props](src/Directory.Build.props#L14) | `Nerdbank.GitVersioning` | `3.5.109` | Stamps version onto all projects under `src/` |
| [src/SomaFrameQD.Demo/SomaFrameQD.Demo.csproj](src/SomaFrameQD.Demo/SomaFrameQD.Demo.csproj#L77) | `Nerdbank.GitVersioning` (`Update`) | `3.5.109` | Redundant explicit pin matching `Directory.Build.props` |
| [Caliburn5PackageHost.csproj](eng/compatibility/Caliburn5PackageHost/Caliburn5PackageHost.csproj#L8) | `SomaFrameQDPackageVersion` | `0.0.0-local` | Default fallback version when not passed via `-p:` by `validate-artifacts.ps1` |
| [AvalonDock4741/ApplicationState.bin](src/SomaFrameQD.Tests/TestData/AvalonDock4741/ApplicationState.bin) | Serialized `SomaFrameQD.Tests` `AssemblyQualifiedName` | `Version=0.9.0.0` | Binary test state fixture (8 serialized test item headers) |
| [AvalonDock501/ApplicationState.bin](src/SomaFrameQD.Tests/TestData/AvalonDock501/ApplicationState.bin) | Serialized `SomaFrameQD.Tests` `AssemblyQualifiedName` | `Version=0.9.0.0` | Binary test state fixture (8 serialized test item headers) |

### B. Per-Project (`.csproj` & `AssemblyInfo.cs`) Version & Target Inventory

All 6 `AssemblyInfo.cs` files contain only `[assembly: ThemeInfo(...)]` and `[assembly: XmlnsDefinition(...)]` — **zero** `AssemblyVersion` or `AssemblyFileVersion` attributes.

| Project File | Project SDK | `<TargetFrameworks>` | Explicit `<Version>` in `.csproj`? | Packable? |
| :--- | :--- | :--- | :--- | :--- |
| [SomaFrameQD.csproj](src/SomaFrameQD/SomaFrameQD.csproj#L1-L11) | `Microsoft.NET.Sdk.WindowsDesktop` | `net48;net10.0-windows` | None (uses root `version.json` -> `0.9.0`) | Yes (`SomaFrameQDWpf`) |
| [SomaFrameQD.Demo.csproj](src/SomaFrameQD.Demo/SomaFrameQD.Demo.csproj#L1-L10) | `Microsoft.NET.Sdk.WindowsDesktop` | `net48;net10.0-windows` | None (uses `src/SomaFrameQD.Demo/version.json` -> `0.1.0`) | No |
| [SomaFrameQD.Modules.CodeCompiler.csproj](src/SomaFrameQD.Modules.CodeCompiler/SomaFrameQD.Modules.CodeCompiler.csproj#L1-L8) | `Microsoft.NET.Sdk` | `net48;net10.0-windows` | None (uses root `version.json` -> `0.9.0`) | Yes |
| [SomaFrameQD.Modules.CodeEditor.csproj](src/SomaFrameQD.Modules.CodeEditor/SomaFrameQD.Modules.CodeEditor.csproj#L1-L9) | `Microsoft.NET.Sdk.WindowsDesktop` | `net48;net10.0-windows` | None (uses root `version.json` -> `0.9.0`) | Yes |
| [SomaFrameQD.Modules.ErrorList.csproj](src/SomaFrameQD.Modules.ErrorList/SomaFrameQD.Modules.ErrorList.csproj#L1-L9) | `Microsoft.NET.Sdk.WindowsDesktop` | `net48;net10.0-windows` | None (uses root `version.json` -> `0.9.0`) | Yes |
| [SomaFrameQD.Modules.GraphEditor.csproj](src/SomaFrameQD.Modules.GraphEditor/SomaFrameQD.Modules.GraphEditor.csproj#L1-L9) | `Microsoft.NET.Sdk.WindowsDesktop` | `net48;net10.0-windows` | None (uses root `version.json` -> `0.9.0`) | Yes |
| [SomaFrameQD.Modules.Inspector.csproj](src/SomaFrameQD.Modules.Inspector/SomaFrameQD.Modules.Inspector.csproj#L1-L9) | `Microsoft.NET.Sdk.WindowsDesktop` | `net48;net10.0-windows` | None (uses root `version.json` -> `0.9.0`) | Yes |
| [SomaFrameQD.Modules.Output.csproj](src/SomaFrameQD.Modules.Output/SomaFrameQD.Modules.Output.csproj#L1-L9) | `Microsoft.NET.Sdk.WindowsDesktop` | `net48;net10.0-windows` | None (uses root `version.json` -> `0.9.0`) | Yes |
| [SomaFrameQD.Modules.PropertyGrid.csproj](src/SomaFrameQD.Modules.PropertyGrid/SomaFrameQD.Modules.PropertyGrid.csproj#L1-L9) | `Microsoft.NET.Sdk.WindowsDesktop` | `net48;net10.0-windows` | None (uses root `version.json` -> `0.9.0`) | Yes |
| [SomaFrameQD.Tests.csproj](src/SomaFrameQD.Tests/SomaFrameQD.Tests.csproj#L1-L11) | `MSTest.Sdk` (`4.1.0`) | `net48;net10.0-windows` | None (uses root `version.json` -> `0.9.0`) | No |
| [Caliburn5PackageHost.csproj](eng/compatibility/Caliburn5PackageHost/Caliburn5PackageHost.csproj#L1-L10) | `Microsoft.NET.Sdk.WindowsDesktop` | `net48;net10.0-windows` | None (outside `src/`, defaults `1.0.0`) | No |
| [MahApps.Net10DragHost.csproj](eng/compatibility/MahApps.Net10DragHost/MahApps.Net10DragHost.csproj#L1-L11) | `Microsoft.NET.Sdk` | `net10.0-windows` | None (outside `src/`, defaults `1.0.0`) | No |

### C. SDK, Toolchain, Runtime Config, & Publish Profile Versions

| File | Item | Version | Notes |
| :--- | :--- | :--- | :--- |
| [global.json](global.json#L3-L8) | `.NET SDK` / `MSTest.Sdk` | `10.0.400` / `4.1.0` | Blocks local CLI builds unless `.NET 10` SDK is installed or `global.json` is changed |
| [src/SomaFrameQD.sln](src/SomaFrameQD.sln#L2-L5) | Solution Format / VS / Min VS | `12.00` / `16.0.29519.87` / `10.0.40219.1` | Visual Studio 2019 header |
| [src/SomaFrameQD/app.config](src/SomaFrameQD/app.config#L4-L22) | `supportedRuntime` / `System` | `v4.0` (`sku=".NETFramework,Version=v4.6.1"`) / `4.0.0.0` | **Stale:** still says `v4.6.1` despite `net48` retarget |
| [src/SomaFrameQD.Demo/app.config](src/SomaFrameQD.Demo/app.config#L4-L9) | `supportedRuntime` / `System` | `v4.0` (`sku=".NETFramework,Version=v4.6.1"`) / `4.0.0.0` | **Stale:** still says `v4.6.1` despite `net48` retarget |
| [SomaFrameQD.Demo.csproj](src/SomaFrameQD.Demo/SomaFrameQD.Demo.csproj#L59) | `ShaderProfile` | `ps_2_0` | DirectX Pixel Shader 2.0 profile for `.fx` shaders |
| [FolderProfileNet5.pubxml](src/SomaFrameQD.Demo/Properties/PublishProfiles/FolderProfileNet5.pubxml#L5-L12) | `ToolsVersion` / `TargetFramework` | `4.0` / `net5.0-windows` | **Obsolete:** targets removed `.NET 5` |
| [FolderProfileNetCore31.pubxml](src/SomaFrameQD.Demo/Properties/PublishProfiles/FolderProfileNetCore31.pubxml#L5-L12) | `ToolsVersion` / `TargetFramework` | `4.0` / `netcoreapp3.1` | **Obsolete:** targets removed `.NET Core 3.1` |
| [src/.vscode/launch.json](src/.vscode/launch.json#L2-L34) | Schema / Debug paths | `0.2.0` / `netcoreapp3.1` & `net5.0-windows` | **Obsolete** debug launch paths |
| [src/NuGet.config](src/NuGet.config#L6) | `protocolVersion` | `3` | NuGet v3 feed |
| [.github/workflows/ci.yml](.github/workflows/ci.yml#L23-L100) | Runner & GitHub Actions | `windows-2025`, `checkout` `v7.0.1`, `setup-dotnet` `v6.0.0`, `upload-artifact` `v7.0.1`, `download-artifact` `v8.0.1`, `NuGet/login@v1` | Pinned by commit SHA in CI |

### D. NuGet Dependency Versions & Hardcoded Test Version Assertions

| Dependency / Assertion | Version | Referenced In |
| :--- | :--- | :--- |
| `Caliburn.Micro` (and `.Core`) | `5.0.258` | [SomaFrameQD.csproj](src/SomaFrameQD/SomaFrameQD.csproj#L56), [package-contract.json](eng/package-contract.json#L41-L42) |
| `Microsoft.Xaml.Behaviors.Wpf` (transitive) | `1.1.135` | [package-contract.json](eng/package-contract.json#L43) |
| `Dirkster.AvalonDock.*` (`Core`, `Serializer.Xml`, `Themes.VS`, `Themes.VS2013`) | `5.0.1` (`5.0.1.0` assembly) | [SomaFrameQD.csproj](src/SomaFrameQD/SomaFrameQD.csproj#L57-L58), [package-contract.json](eng/package-contract.json#L44-L48), hardcoded `new Version(5, 0, 1, 0)` in [Caliburn5PackageHost/Program.cs](eng/compatibility/Caliburn5PackageHost/Program.cs#L41) |
| `DotNetProjects.Extended.Wpf.Toolkit` | `5.0.129` (`5.0.129.0` assembly) | [SomaFrameQD.csproj](src/SomaFrameQD/SomaFrameQD.csproj#L59), [package-contract.json](eng/package-contract.json#L49), hardcoded `new Version(5, 0, 129, 0)` in [Caliburn5PackageHost/Program.cs](eng/compatibility/Caliburn5PackageHost/Program.cs#L43) & [ExtendedWpfToolkitCompatibilityTests.cs](src/SomaFrameQD.Tests/Modules/Toolkit/ExtendedWpfToolkitCompatibilityTests.cs#L24-L25) |
| `AvalonEdit` | `6.3.1.120` (`6.3.1.120` assembly) | [SomaFrameQD.Modules.CodeEditor.csproj](src/SomaFrameQD.Modules.CodeEditor/SomaFrameQD.Modules.CodeEditor.csproj#L37), [package-contract.json](eng/package-contract.json#L40), hardcoded `new Version(6, 3, 1, 120)` in [Caliburn5PackageHost/Program.cs](eng/compatibility/Caliburn5PackageHost/Program.cs#L42) |
| `MahApps.Metro` | `2.4.11` | [SomaFrameQD.csproj](src/SomaFrameQD/SomaFrameQD.csproj#L60), [MahApps.Net10DragHost.csproj](eng/compatibility/MahApps.Net10DragHost/MahApps.Net10DragHost.csproj#L10), [package-contract.json](eng/package-contract.json#L65) |
| `System.ComponentModel.Composition` | `6.0.0` | [SomaFrameQD.csproj](src/SomaFrameQD/SomaFrameQD.csproj#L61), [package-contract.json](eng/package-contract.json#L66) |
| `System.Drawing.Common` | `6.0.0` | [SomaFrameQD.Modules.Inspector.csproj](src/SomaFrameQD.Modules.Inspector/SomaFrameQD.Modules.Inspector.csproj#L36) (`< net10.0`), [package-contract.json](eng/package-contract.json#L114) |
| `Microsoft.CodeAnalysis.CSharp` | `3.4.0` | [SomaFrameQD.Modules.CodeCompiler.csproj](src/SomaFrameQD.Modules.CodeCompiler/SomaFrameQD.Modules.CodeCompiler.csproj#L17), [package-contract.json](eng/package-contract.json#L78) |
| `System.Text.Json` (transitive on `net48`) | `10.0.12` | [package-contract.json](eng/package-contract.json#L9) |
| `JeremyAnsel.HLSL.Targets` | `1.0.9` | [SomaFrameQD.Demo.csproj](src/SomaFrameQD.Demo/SomaFrameQD.Demo.csproj#L65) |
| `HelixToolkit.Wpf` / `HelixToolkit.Core.Wpf` | `2.10.0` | [SomaFrameQD.Demo.csproj](src/SomaFrameQD.Demo/SomaFrameQD.Demo.csproj#L69-L73) |

### E. Resource / Designer Metadata & Documentation Versions

- **`.resx` Resource Headers (43 files):** ResX schema `2.0` and `System.Windows.Forms, Version=4.0.0.0` across 42 files — **except** [CodeEditor/Properties/Resources.resx](src/SomaFrameQD.Modules.CodeEditor/Properties/Resources.resx#L115-L118), which references `System.Windows.Forms, Version=5.0.0.0` (worth noting when retargeting strictly to `.NET Framework 4.8`).
- **`.Designer.cs` Attributes:** `StronglyTypedResourceBuilder` (`16.0.0.0` / `17.0.0.0`), `SettingsSingleFileGenerator` (`16.4.0.0` / `16.10.0.0`), runtime comment `4.0.30319.42000`.
- **Documentation & Changelog:** [CHANGELOG.md](CHANGELOG.md#L3-L96) stops at `0.6.0` (`2015-09-11`), covering `0.2.0`–`0.6.0`. [caliburn-5-migration.md](doc/caliburn-5-migration.md) documents the legacy upstream `1.0 beta` line, reset `0.9.0` / Demo `0.1.0` release policy, AvalonDock `4.60.0` -> `4.74.1` -> `5.0.1`, and MahApps `2.4.9` -> `2.4.11`. [LICENCE.txt](LICENCE.txt#L3-L8) specifies Apache License `Version 2.0`.

---

## 3. Upstream (`tgjones/gemini`) Age Audit of `SomaFrameQD.Demo` & Samples

Checked against both the local Git commit graph and live upstream `https://github.com/tgjones/gemini` (upstream `master` HEAD is `41daf3f` on **2026-10-18**, identical to the base of this fork):

### Executive Verdict: **The Demo & Samples Are 11–15 Years Old in Substance (With 7-Week-Old Compile Glue)**

1. **Why GitHub's folder timestamp says "~7 weeks ago" (`2026-10-03`):**
   Between Sept 20 and Oct 3, 2026, contributor `kornman00` (merged by Tim Jones) ran a package-modernization pass (Caliburn.Micro 5, AvalonDock 5, `.NET 4.8` + `.NET 10`) that touched **only 3 files** in `Demo` purely to keep the build green:
   - `Demo.csproj` (`2026-10-03`, commit `185774e`): changed `<TargetFrameworks>` to `net48;net10.0-windows`.
   - `Modules/Shell/ViewModels/ShellViewModel.cs` (`2026-09-30`, commit `5a6e49d`): updated close-guard overrides for Caliburn.Micro 5's async lifecycle.
   - `Modules/FilterDesigner/Sample.cs` (`2026-09-30`, commit `7a4fe5c`): changed 2 lines from `ShowTool(...)` to `await ShowToolAsync(...)`.
2. **When the Original Creator (Tim Jones) Last Updated `Demo`:**
   - **Last commit of any kind by Tim Jones in `Demo`:** **2020-01-03 (~6 years, 10 months ago)** — mechanical `.NET Core 3.1` project/NuGet/shader build migration.
   - **Last actual feature/sample work by Tim Jones:** **2011 to July 2015 (11 to 15 years ago!)**. Furthermore, Tim Jones deleted the separate `SharpDX`, `MonoGame`, and `XNA` sample projects on **2019-11-26** (`fb5401c`), though stale links to `Demo.FilterDesigner` remain in `README.md`.

### Module-by-Module Age Breakdown of [src/SomaFrameQD.Demo](src/SomaFrameQD.Demo)

| Demo / Sample Area | Core Code Written | Last Touch by Original Creator (Tim Jones) | Last Touch by Anyone (& What Changed) | Effective Age of Sample Logic |
| :--- | :--- | :--- | :--- | :--- |
| **`Startup`** ([Module.cs](src/SomaFrameQD.Demo/Modules/Startup/Module.cs)) | 2013–2014 | **2014-12-04** (warning cleanup) | **2014-12-04** | **~12 years old** |
| **`SampleBrowser`** ([SampleBrowserViewModel.cs](src/SomaFrameQD.Demo/Modules/SampleBrowser/ViewModels/SampleBrowserViewModel.cs), Views) | 2014 | **2014-12-25** (Views/VM) / **2020-01-02** (`ISample` async sig) | **2020-01-02** (Caliburn 4 alpha signature) | **~12 years old** |
| **`FilterDesigner`** (29 files: graph nodes, HLSL `.fx`, `FastBitmap`, UI) | 2014 | **2014-12-25** on 24 of 29 files (`2020-01-03` shader URI helper) | **2026-09-30** (2 lines in `Sample.cs`); **2022-08-23** (`MenuDefinitions.cs`); **2020-02-29** (selectable graph lines) | **~12 years old** |
| **`Home` & `Helix` 3D C# Scripting** ([HelixViewModel.cs](src/SomaFrameQD.Demo/Modules/Home/ViewModels/HelixViewModel.cs), [HomeViewModel.cs](src/SomaFrameQD.Demo/Modules/Home/ViewModels/HomeViewModel.cs)) | 2011–2014 | **2013-06-11** (`IDemoScript`, `HelixView.xaml.cs`), **2014-12-08** (`HomeViewModel`), **2020-01-02** (`HelixViewModel` Roslyn refs) | **2022-10-10** (`readonly` fix) & **2022-08-23** (`kornman00` added `DynamicExclusionPredicate` menu demo) | **~12–13 years old** (1 menu demo added 4 yrs ago) |
| **`TextEditor`** ([EditorViewModel.cs](src/SomaFrameQD.Demo/Modules/TextEditor/ViewModels/EditorViewModel.cs), `EditorProvider.cs`) | 2011–2015 | **2015-07-15** (`PersistedDocument` load) | **2020-09-13** (`CanCreateNew` property) | **~11 years old** |
| **`Shell` (Demo Settings & Shell override)** | 2013–2014 | **2020-01-02** (`ShellViewModel`); never on `DemoApplicationSettings*` | **2026-09-30** (`ShellViewModel` close guard); **2016-06-19** (settings views) | **~10–12 years old** (lifecycle adapter updated Sept 2026) |
| **App Root & Publish Profiles** (`App.xaml.cs`, `DemoAppBootstrapper.cs`, `.pubxml`) | 2011 / 2021 | **2011-09-29** (`App.xaml.cs`), **2019-11-26** (`App.xaml`) | **2021-04-25** (`PublishSingleFile` for `.NET Core 3.1` & `.NET 5`) | **5.5 to 15 years old** |
