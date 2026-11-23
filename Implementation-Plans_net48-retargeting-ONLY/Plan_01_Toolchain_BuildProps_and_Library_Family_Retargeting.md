# Implementation Plan #1 — Toolchain Pin, Shared MSBuild Props & Packable Library Family Retargeting (`net48` Only)

> **Purpose:** Establish a deterministic `.NET Framework 4.8`-capable build toolchain (`global.json` + `src/Directory.Build.props`) and single-target all **8 packable NuGet library projects** (`SomaFrameQDWpf` + the 7 `SomaFrameQD.Modules.*` libraries) exclusively to `<TargetFramework>net48</TargetFramework>` as the first quarter of **Stage 1: Simple Retargeting** in [`TODO.md`](../TODO.md).  
> **Generated Timestamp:** `2026-11-22T19:22:00-05:00` (`2026-11-22`)  
> **Stage Scope:** **Stage 1 (`net48` Retargeting ONLY)** — **ZERO** Stage 2 runtime, GAC, `x64`, `app.config`, Win32, or WPF optimizations permitted.  
> **Execution Order:** **Execute Plan #1 FIRST**, then proceed sequentially to [Plan #2](./Plan_02_Demo_Tests_Solution_and_Obsolete_MultiTarget_Cleanup.md) → [Plan #3](./Plan_03_Package_Contract_CompatibilityHost_and_CI_Pipeline.md) → [Plan #4](./Plan_04_Documentation_Alignment_and_Stage1_Completion_Gate.md).  
> **Reference Documents:** [`TODO.md`](../TODO.md) (`1.1`–`1.2`) | [`DRMA.md`](../DRMA.md) (`S1-1`, `S1-3`) | [`Simple_Retargeting_vs_Optimization_NET48_Win7_x64_2026-11-22_17-36-26.md`](../LOC-statistics_plus_net48-retargeting/Simple_Retargeting_vs_Optimization_NET48_Win7_x64_2026-11-22_17-36-26.md) | [`AGENTS.md`](../AGENTS.md)

---

## Stage 1 Four-Plan Split Overview (`Implementation-Plans_net48-retargeting-ONLY`)

To keep context-window usage small, isolate failure modes, and verify every step with live compiler/test gates, **Stage 1 (`TODO.md` `1.1`–`1.4`)** is divided into **4 sequential Implementation Plans**:

| Plan | Execution Scope | Files Touched / Context Budget | Verification Gate |
| :--- | :--- | :--- | :--- |
| **Plan #1 (This Plan)** | Toolchain SDK pin ([`global.json`](../global.json)), shared `<LangVersion>` ([`src/Directory.Build.props`](../src/Directory.Build.props)), all **8 packable library `.csproj` files** (`net48` single-target + unwrap `net48` conditions), and `CodeEditor` `.resx` `Version=4.0.0.0` header fix | **11 files** (`~410` lines read; skipping `.resx` data bodies) | `dotnet --version` resolves cleanly; all 8 library projects restore & build in `Release` for `net48` with `0` errors |
| **[Plan #2](./Plan_02_Demo_Tests_Solution_and_Obsolete_MultiTarget_Cleanup.md)** | Retarget [`SomaFrameQD.Demo.csproj`](../src/SomaFrameQD.Demo/SomaFrameQD.Demo.csproj) (drop `HelixToolkit.Core.Wpf`) & [`SomaFrameQD.Tests.csproj`](../src/SomaFrameQD.Tests/SomaFrameQD.Tests.csproj); delete [`MahApps.Net10DragHost`](../eng/compatibility/MahApps.Net10DragHost/) & clean [`SomaFrameQD.sln`](../src/SomaFrameQD.sln); delete `.NET Core 3.1`/`.NET 5` `.pubxml` profiles & update [`src/.vscode/launch.json`](../src/.vscode/launch.json) | **8 files** (`~350` lines) | Full 10-project `SomaFrameQD.sln` restores & builds in `Release` (`net48` + HLSL `.ps` shaders); all **146 STA tests** pass on `net48` |
| **[Plan #3](./Plan_03_Package_Contract_CompatibilityHost_and_CI_Pipeline.md)** | Retarget [`Caliburn5PackageHost.csproj`](../eng/compatibility/Caliburn5PackageHost/Caliburn5PackageHost.csproj); update [`eng/package-contract.json`](../eng/package-contract.json) & [`eng/validate-artifacts.ps1`](../eng/validate-artifacts.ps1) (PowerShell 5.1 + 7 & NuGet lockfile compatibility); update [`.github/workflows/ci.yml`](../.github/workflows/ci.yml) to `net48`-only | **4 files** (`~725` lines) | `dotnet pack` emits 8 `.nupkg` + 8 `.snupkg` (`lib/net48` only); `eng/validate-artifacts.ps1` passes 100% including live `Caliburn5PackageHost` run |
| **[Plan #4](./Plan_04_Documentation_Alignment_and_Stage1_Completion_Gate.md)** | Update [`README.md`](../README.md) & [`doc/caliburn-5-migration.md`](../doc/caliburn-5-migration.md); synchronize live [`AGENTS.md`](../AGENTS.md), [`REPO_MAP.md`](../REPO_MAP.md), & [`TODO.md`](../TODO.md); execute clean-room **Stage 1 Mandatory Completion Gate (`1.4`)** | **5 files** (`~500` targeted lines; preserving immutable `doc_before-net48/`) | Clean-room rebuild, 146/146 tests, `validate-artifacts.ps1`, SHA-256 fixture check, and forensic audit confirming zero Stage 2 leakage (**HARD STOP before Stage 2**) |

---

## Strict Stage 1 Guardrails (Non-Negotiable)

> [!IMPORTANT]
> **What MUST NOT Be Changed Anywhere in Plan #1 (or Any Stage 1 Plan):**
> 1. **Do NOT swap NuGet packages to GAC `<Reference>` items yet:** Keep `<PackageReference Include="System.ComponentModel.Composition" Version="6.0.0" />` in `src/SomaFrameQD/SomaFrameQD.csproj` and keep `<PackageReference Include="System.Drawing.Common" Version="6.0.0" />` in `src/SomaFrameQD.Modules.Inspector/SomaFrameQD.Modules.Inspector.csproj`. Swapping those out-of-band NuGets to `.NET Framework 4.8` GAC assemblies belongs strictly to **Stage 2 (`Plan 2.1`)**.
> 2. **Do NOT bump or alter any NuGet package versions:** Keep `Caliburn.Micro` (`5.0.258`), `Dirkster.AvalonDock.*` (`5.0.1`), `DotNetProjects.Extended.Wpf.Toolkit` (`5.0.129`), `MahApps.Metro` (`2.4.11`), `AvalonEdit` (`6.3.1.120`), `Microsoft.CodeAnalysis.CSharp` (`3.4.0`), and `Nerdbank.GitVersioning` (`3.5.109`) frozen at their exact baseline versions ([`DRMA.md`](../DRMA.md) Section 4.2).
> 3. **Do NOT add `x64` `<PlatformTarget>`, `<Prefer32Bit>`, `<AutoGenerateBindingRedirects>`, or PDB format changes in `src/Directory.Build.props`:** Those belong strictly to **Stage 2 (`Plan 2.1`)**.
> 4. **Do NOT touch `version.json` (`0.9.0`), `src/SomaFrameQD.Demo/version.json` (`0.1.0`), `app.config`, `AppBootstrapper.cs`, `NativeMethods.cs`, `HwndWrapper.cs`, or any `.cs`/`.xaml` source files.**
> 5. **Context-Window Discipline:** Do **not** bulk-read `.resx` files (`6,838` lines across 43 files) or `.Designer.cs` files. In Step 4 below, read and edit **only** lines `108`–`120` of `src/SomaFrameQD.Modules.CodeEditor/Properties/Resources.resx`.

---

## Exact File-by-File Implementation Steps

### Step 1.1 — Update Toolchain SDK Pin & Test Runner Coupling in `global.json`

* **File:** [`C:\SOFTDEV\SomaFrameQD\global.json`](../global.json)
* **Engineering Rationale ([`TODO.md`](../TODO.md) `1.2`, [`DRMA.md`](../DRMA.md) Risk `S1-1`):**
  * `global.json` currently pins `"version": "10.0.400"` (`"rollForward": "latestPatch"`) and sets `"test": { "runner": "Microsoft.Testing.Platform" }`.
  * On workstations with `.NET 8.0 LTS` installed (such as `.NET SDK 8.0.417` on this machine, alongside `.NET Framework 4.8` Reference Assemblies in `C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8`), pinning `10.0.400` blocks `dotnet` CLI execution completely. Furthermore, `"test": { "runner": "Microsoft.Testing.Platform" }` activates `.NET SDK 10`'s `dotnet test --project` CLI mode, which `.NET SDK 8.0` rejects as an unrecognized option.
  * Updating `sdk.version` to `"8.0.400"` with `"rollForward": "latestPatch"` resolves cleanly to `.NET SDK 8.0.417` locally **and** instructs GitHub Actions (`actions/setup-dotnet` with `global-json-file: global.json`) to provision the latest `.NET 8.0.4xx` LTS SDK without requiring `.NET SDK 10.0.400`.
  * Retaining `"msbuild-sdks": { "MSTest.Sdk": "4.1.0" }` while removing `"test": { "runner": "Microsoft.Testing.Platform" }` allows `MSTest.Sdk` `4.1.0`'s built-in defaults (`EnableMSTestRunner=true` and `TestingPlatformDotnetTestSupport=true`) to execute `SomaFrameQD.Tests` over `Microsoft.Testing.Platform` on `net48` via standard `dotnet test src\SomaFrameQD.Tests\SomaFrameQD.Tests.csproj ...` (as well as `dotnet run --project ...`).
  * **Why this happens in Plan #1:** Updating `global.json` first unlocks live `dotnet restore` and `dotnet build` verification at the gate of Plan #1 and every subsequent plan.

* **Before (`global.json` lines 1–13):**
  ```json
  {
    "sdk": {
      "version": "10.0.400",
      "rollForward": "latestPatch",
      "allowPrerelease": false
    },
    "msbuild-sdks": {
      "MSTest.Sdk": "4.1.0"
    },
    "test": {
      "runner": "Microsoft.Testing.Platform"
    }
  }
  ```
* **After (`global.json`):**
  ```json
  {
    "sdk": {
      "version": "8.0.400",
      "rollForward": "latestPatch",
      "allowPrerelease": false
    },
    "msbuild-sdks": {
      "MSTest.Sdk": "4.1.0"
    }
  }
  ```

---

### Step 1.2 — Pin `<LangVersion>12.0</LangVersion>` in `src/Directory.Build.props`

* **File:** [`C:\SOFTDEV\SomaFrameQD\src\Directory.Build.props`](../src/Directory.Build.props)
* **Engineering Rationale ([`TODO.md`](../TODO.md) `1.1`, [`DRMA.md`](../DRMA.md) Risk `S1-3`):**
  * When `net10.0-windows` is removed and `<TargetFramework>net48</TargetFramework>` becomes the sole target, the .NET SDK automatically drops the default C# language version for `net48` down to **C# `7.3`**.
  * Dropping to C# `7.3` immediately breaks `src/SomaFrameQD.Tests/SomaFrameQD.Tests.csproj` (which sets `<Nullable>disable</Nullable>`, causing Roslyn `csc` to emit **`error CS8630: Invalid 'nullable' value: 'Disable' for C# 7.3. Please use language version '8.0' or greater`**) and rejects C# 8+ syntax across modern test/infrastructure files.
  * Pinning `<LangVersion>12.0</LangVersion>` is purely a compile-time parser setting supported identically across `.NET SDK 8.0.4xx`, `.NET SDK 9.0`, `.NET SDK 10.0`, and Visual Studio 2022/2026 (`MSBuild 18.0.5`), emitting standard CLR v4.0.30319 IL with **zero** extra runtime dependencies on `.NET Framework 4.8`.

* **Before (`src/Directory.Build.props` lines 1–7):**
  ```xml
  <Project>
      <PropertyGroup>
          <Authors>Tim Jones</Authors>
          <RepositoryUrl>https://github.com/tgjones/SomaFrameQD</RepositoryUrl>
          <PackageLicenseExpression>Apache-2.0</PackageLicenseExpression>
      </PropertyGroup>
  ```
* **After (`src/Directory.Build.props` lines 1–8):**
  ```xml
  <Project>
      <PropertyGroup>
          <Authors>Tim Jones</Authors>
          <RepositoryUrl>https://github.com/tgjones/SomaFrameQD</RepositoryUrl>
          <PackageLicenseExpression>Apache-2.0</PackageLicenseExpression>
          <LangVersion>12.0</LangVersion>
      </PropertyGroup>
  ```
  *(Leave lines 8–16 — `IncludeSymbols`, `SymbolPackageFormat`, and `Nerdbank.GitVersioning` `3.5.109` — completely unchanged.)*

---

### Step 1.3 — Single-Target All 8 Packable Library `.csproj` Files to `<TargetFramework>net48</TargetFramework>`

Apply the exact changes below across all 8 NuGet library projects (`TODO.md` `1.1` & `1.2`):

#### 1. [`src/SomaFrameQD/SomaFrameQD.csproj`](../src/SomaFrameQD/SomaFrameQD.csproj) *(NuGet ID: `SomaFrameQDWpf`, Assembly: `SomaFrameQD`)*
* Change `<TargetFrameworks>net48;net10.0-windows</TargetFrameworks>` (line 5) to singular `<TargetFramework>net48</TargetFramework>`.
* Remove the multi-target `Condition="$(TargetFramework) == 'net48'"` attribute from `<ItemGroup>` around `<Reference Include="PresentationFramework.Aero" />` (lines 13–15) so `<Reference Include="PresentationFramework.Aero" />` is unconditional.
* **Preserve unchanged:** `<PackageId>SomaFrameQDWpf</PackageId>` (line 7) and all 6 `<PackageReference>` items on lines 55–62 (including `<PackageReference Include="System.ComponentModel.Composition" Version="6.0.0" />`).
* **Exact Target Diff (`lines 3–16`):**
  ```xml
    <PropertyGroup>
      <RootNamespace>SomaFrameQD</RootNamespace>
      <TargetFramework>net48</TargetFramework>
      <UseWPF>true</UseWPF>
      <PackageId>SomaFrameQDWpf</PackageId>
      <Title>SomaFrameQD</Title>
      <PackageTags>core WPF MVVM AvalonDock Caliburn Micro Visual Studio IDE Shell</PackageTags>
      <Description>SomaFrameQD is an application shell similar in concept to the Visual Studio Shell. It uses AvalonDock and has an MVVM architecture based on Caliburn Micro.</Description>
    </PropertyGroup>

    <ItemGroup>
      <Reference Include="PresentationFramework.Aero" />
    </ItemGroup>
  ```

#### 2. [`src/SomaFrameQD.Modules.CodeCompiler/SomaFrameQD.Modules.CodeCompiler.csproj`](../src/SomaFrameQD.Modules.CodeCompiler/SomaFrameQD.Modules.CodeCompiler.csproj)
* Replace `<TargetFrameworks>net48;net10.0-windows</TargetFrameworks>` (line 5) with `<TargetFramework>net48</TargetFramework>`.
* Leave `<Project Sdk="Microsoft.NET.Sdk">`, the 3 `<ProjectReference>` items (`ErrorList`, `Output`, `SomaFrameQD`), and `<PackageReference Include="Microsoft.CodeAnalysis.CSharp" Version="3.4.0" />` unchanged.

#### 3. [`src/SomaFrameQD.Modules.CodeEditor/SomaFrameQD.Modules.CodeEditor.csproj`](../src/SomaFrameQD.Modules.CodeEditor/SomaFrameQD.Modules.CodeEditor.csproj)
* Replace `<TargetFrameworks>net48;net10.0-windows</TargetFrameworks>` (line 5) with `<TargetFramework>net48</TargetFramework>`.
* Leave `<PackageReference Include="AvalonEdit" Version="6.3.1.120" />` and all resource/compile items unchanged.

#### 4. [`src/SomaFrameQD.Modules.ErrorList/SomaFrameQD.Modules.ErrorList.csproj`](../src/SomaFrameQD.Modules.ErrorList/SomaFrameQD.Modules.ErrorList.csproj)
* Replace `<TargetFrameworks>net48;net10.0-windows</TargetFrameworks>` (line 5) with `<TargetFramework>net48</TargetFramework>`.

#### 5. [`src/SomaFrameQD.Modules.GraphEditor/SomaFrameQD.Modules.GraphEditor.csproj`](../src/SomaFrameQD.Modules.GraphEditor/SomaFrameQD.Modules.GraphEditor.csproj)
* Replace `<TargetFrameworks>net48;net10.0-windows</TargetFrameworks>` (line 5) with `<TargetFramework>net48</TargetFramework>`.

#### 6. [`src/SomaFrameQD.Modules.Inspector/SomaFrameQD.Modules.Inspector.csproj`](../src/SomaFrameQD.Modules.Inspector/SomaFrameQD.Modules.Inspector.csproj)
* Replace `<TargetFrameworks>net48;net10.0-windows</TargetFrameworks>` (line 5) with `<TargetFramework>net48</TargetFramework>`.
* Remove the multi-target `Condition="!$([MSBuild]::IsTargetFrameworkCompatible('$(TargetFramework)', 'net10.0'))"` attribute from `<ItemGroup>` around `<PackageReference Include="System.Drawing.Common" Version="6.0.0" />` (lines 35–37), keeping `<PackageReference Include="System.Drawing.Common" Version="6.0.0" />` unconditionally for Stage 1:
  ```xml
    <ItemGroup>
      <PackageReference Include="System.Drawing.Common" Version="6.0.0" />
    </ItemGroup>
  ```

#### 7. [`src/SomaFrameQD.Modules.Output/SomaFrameQD.Modules.Output.csproj`](../src/SomaFrameQD.Modules.Output/SomaFrameQD.Modules.Output.csproj)
* Replace `<TargetFrameworks>net48;net10.0-windows</TargetFrameworks>` (line 5) with `<TargetFramework>net48</TargetFramework>`.

#### 8. [`src/SomaFrameQD.Modules.PropertyGrid/SomaFrameQD.Modules.PropertyGrid.csproj`](../src/SomaFrameQD.Modules.PropertyGrid/SomaFrameQD.Modules.PropertyGrid.csproj)
* Replace `<TargetFrameworks>net48;net10.0-windows</TargetFrameworks>` (line 5) with `<TargetFramework>net48</TargetFramework>`.

---

### Step 1.4 — Normalize `.NET 5` WinForms `<resheader>` in `CodeEditor` `Resources.resx`

* **File:** [`C:\SOFTDEV\SomaFrameQD\src\SomaFrameQD.Modules.CodeEditor\Properties\Resources.resx`](../src/SomaFrameQD.Modules.CodeEditor/Properties/Resources.resx) *(read lines `108`–`122` only)*
* **Engineering Rationale ([`TODO.md`](../TODO.md) `1.1`, [`DRMA.md`](../DRMA.md) Risk `S1-3`):**
  * A repository-wide scan across all 43 `.resx` files confirms that `src/SomaFrameQD.Modules.CodeEditor/Properties/Resources.resx` (lines `115` and `118`) is the **only** `.resx` file referencing `.NET 5`'s `System.Windows.Forms, Version=5.0.0.0`. Every other `.resx` file in `SomaFrameQD` uses `.NET Framework 4.x`'s `Version=4.0.0.0`.
  * Normalizing both `<resheader>` entries to `Version=4.0.0.0` guarantees clean `ResGen` / Visual Studio `.NET Framework 4.8` resource generation without touching `Resources.Designer.cs` or any localized string values.

* **Before (`lines 114–119`):**
  ```xml
    <resheader name="reader">
      <value>System.Resources.ResXResourceReader, System.Windows.Forms, Version=5.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value>
    </resheader>
    <resheader name="writer">
      <value>System.Resources.ResXResourceWriter, System.Windows.Forms, Version=5.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value>
    </resheader>
  ```
* **After (`lines 114–119`):**
  ```xml
    <resheader name="reader">
      <value>System.Resources.ResXResourceReader, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value>
    </resheader>
    <resheader name="writer">
      <value>System.Resources.ResXResourceWriter, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value>
    </resheader>
  ```

---

## Plan #1 Verification Gate (Must Pass Before Starting Plan #2)

Run the following verification commands from `C:\SOFTDEV\SomaFrameQD` (note: we build the library roots directly here because `SomaFrameQD.Demo`, `SomaFrameQD.Tests`, and `MahApps.Net10DragHost` inside `SomaFrameQD.sln` are retargeted/removed in [Plan #2](./Plan_02_Demo_Tests_Solution_and_Obsolete_MultiTarget_Cleanup.md)):

```powershell
# 1. Confirm global.json resolves to an installed .NET 8.0.4xx SDK (e.g., 8.0.417)
dotnet --version

# 2. Confirm zero <TargetFrameworks> or multi-target Condition attributes remain in the 8 library .csproj files
$libProjects = @(
    'src\SomaFrameQD\SomaFrameQD.csproj',
    'src\SomaFrameQD.Modules.CodeCompiler\SomaFrameQD.Modules.CodeCompiler.csproj',
    'src\SomaFrameQD.Modules.CodeEditor\SomaFrameQD.Modules.CodeEditor.csproj',
    'src\SomaFrameQD.Modules.ErrorList\SomaFrameQD.Modules.ErrorList.csproj',
    'src\SomaFrameQD.Modules.GraphEditor\SomaFrameQD.Modules.GraphEditor.csproj',
    'src\SomaFrameQD.Modules.Inspector\SomaFrameQD.Modules.Inspector.csproj',
    'src\SomaFrameQD.Modules.Output\SomaFrameQD.Modules.Output.csproj',
    'src\SomaFrameQD.Modules.PropertyGrid\SomaFrameQD.Modules.PropertyGrid.csproj'
)
Select-String -Path $libProjects -Pattern 'TargetFrameworks|net10|Condition='

# 3. Restore and build all 8 packable libraries in Release for net48 (using isolated NuGet.config)
foreach ($proj in @(
    'src\SomaFrameQD.Modules.CodeCompiler\SomaFrameQD.Modules.CodeCompiler.csproj',
    'src\SomaFrameQD.Modules.CodeEditor\SomaFrameQD.Modules.CodeEditor.csproj',
    'src\SomaFrameQD.Modules.GraphEditor\SomaFrameQD.Modules.GraphEditor.csproj',
    'src\SomaFrameQD.Modules.Inspector\SomaFrameQD.Modules.Inspector.csproj',
    'src\SomaFrameQD.Modules.PropertyGrid\SomaFrameQD.Modules.PropertyGrid.csproj'
)) {
    dotnet restore $proj --configfile src\NuGet.config
    if ($LASTEXITCODE -ne 0) { throw "Restore failed for $proj" }
    dotnet build $proj --configuration Release --no-restore
    if ($LASTEXITCODE -ne 0) { throw "Build failed for $proj" }
}

# 4. Confirm all 8 net48 library DLLs exist and zero net10.0-windows outputs were emitted
Get-ChildItem -Path src\SomaFrameQD*\bin\Release\net48\SomaFrameQD*.dll | Select-Object FullName
Get-ChildItem -Path src\SomaFrameQD*\bin\Release\net10* -ErrorAction SilentlyContinue
```

### Gate Pass Criteria
1. `dotnet --version` succeeds (`8.0.417` or latest `8.0.4xx` patch) with zero SDK resolution errors.
2. `Select-String` returns **empty** across all 8 library `.csproj` files (confirming singular `<TargetFramework>net48</TargetFramework>` and zero residual TFM `Condition` attributes).
3. All 8 library projects restore against `src\NuGet.config` and compile cleanly in `Release` configuration with **`0 Error(s)`**, producing `bin\Release\net48\SomaFrameQD*.dll` and **zero** `net10.0-windows` directories.
4. `src\SomaFrameQD.Modules.CodeEditor\Properties\Resources.resx` contains `Version=4.0.0.0` on lines 115 and 118, and zero Stage 2 files were touched.

Once all 4 criteria pass, proceed to **[Implementation Plan #2](./Plan_02_Demo_Tests_Solution_and_Obsolete_MultiTarget_Cleanup.md)**.
