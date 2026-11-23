# Implementation Plan #3 — Lockstep NuGet Package Contract, External Compatibility Host & GitHub Actions CI Retargeting (`net48` Only)

> **Purpose:** Retarget the external package-consumer host (`Caliburn5PackageHost`), update `eng/package-contract.json` and `eng/validate-artifacts.ps1` in lockstep to validate `net48`-only `.nupkg`/`.snupkg` artifacts across both Windows PowerShell 5.1 (`powershell.exe`) and PowerShell 7 (`pwsh`), and update `.github/workflows/ci.yml` to build, test, pack, and validate exclusively for `.NET Framework 4.8` (`net48`) without requiring `.NET SDK 10.0.400`.  
> **Generated Timestamp:** `2026-11-22T19:22:00-05:00` (`2026-11-22`)  
> **Prerequisite:** **[Implementation Plan #1](./Plan_01_Toolchain_BuildProps_and_Library_Family_Retargeting.md)** and **[Implementation Plan #2](./Plan_02_Demo_Tests_Solution_and_Obsolete_MultiTarget_Cleanup.md) MUST be 100% completed and verified green first.**  
> **Stage Scope:** **Stage 1 (`net48` Retargeting ONLY)** — **ZERO** Stage 2 GAC package replacements (`System.ComponentModel.Composition` / `System.Drawing.Common`), `x64`, `app.config`, or runtime optimizations permitted.  
> **Execution Order:** [Plan #1](./Plan_01_Toolchain_BuildProps_and_Library_Family_Retargeting.md) → [Plan #2](./Plan_02_Demo_Tests_Solution_and_Obsolete_MultiTarget_Cleanup.md) → **Plan #3 (This Plan)** → [Plan #4](./Plan_04_Documentation_Alignment_and_Stage1_Completion_Gate.md).  
> **Reference Documents:** [`TODO.md`](../TODO.md) (`1.1`, `1.3`) | [`DRMA.md`](../DRMA.md) (`S1-1`, `S1-2`) | [`doc_before-net48/09_Build_Versioning_Packaging_and_CI_Before_NET48.md`](../doc_before-net48/09_Build_Versioning_Packaging_and_CI_Before_NET48.md) | [`AGENTS.md`](../AGENTS.md)

---

## Stage 1 Four-Plan Split Overview (`Implementation-Plans_net48-retargeting-ONLY`)

| Plan | Execution Scope | Files Touched / Context Budget | Verification Gate |
| :--- | :--- | :--- | :--- |
| **[Plan #1](./Plan_01_Toolchain_BuildProps_and_Library_Family_Retargeting.md)** | Toolchain SDK pin ([`global.json`](../global.json)), shared `<LangVersion>` ([`src/Directory.Build.props`](../src/Directory.Build.props)), all **8 packable library `.csproj` files** (`net48` single-target + unwrap `net48` conditions), and `CodeEditor` `.resx` `Version=4.0.0.0` header fix | **11 files** (`~410` lines read; skipping `.resx` data bodies) | `dotnet --version` resolves cleanly; all 8 library projects restore & build in `Release` for `net48` with `0` errors |
| **[Plan #2](./Plan_02_Demo_Tests_Solution_and_Obsolete_MultiTarget_Cleanup.md)** | Retarget [`SomaFrameQD.Demo.csproj`](../src/SomaFrameQD.Demo/SomaFrameQD.Demo.csproj) (drop `HelixToolkit.Core.Wpf`) & [`SomaFrameQD.Tests.csproj`](../src/SomaFrameQD.Tests/SomaFrameQD.Tests.csproj); delete [`MahApps.Net10DragHost`](../eng/compatibility/MahApps.Net10DragHost/) & clean [`SomaFrameQD.sln`](../src/SomaFrameQD.sln); delete `.NET Core 3.1`/`.NET 5` `.pubxml` profiles & update [`src/.vscode/launch.json`](../src/.vscode/launch.json) | **8 files** (`~350` lines) | Full 10-project `SomaFrameQD.sln` restores & builds in `Release` (`net48` + HLSL `.ps` shaders); all **146 STA tests** pass on `net48` |
| **Plan #3 (This Plan)** | Retarget [`Caliburn5PackageHost.csproj`](../eng/compatibility/Caliburn5PackageHost/Caliburn5PackageHost.csproj); update [`eng/package-contract.json`](../eng/package-contract.json) & [`eng/validate-artifacts.ps1`](../eng/validate-artifacts.ps1) (PowerShell 5.1 + 7 & NuGet lockfile compatibility); update [`.github/workflows/ci.yml`](../.github/workflows/ci.yml) to `net48`-only | **4 files** (`~725` lines) | `dotnet pack` emits 8 `.nupkg` + 8 `.snupkg` (`lib/net48` only); `eng/validate-artifacts.ps1` passes 100% including live `Caliburn5PackageHost` run |
| **[Plan #4](./Plan_04_Documentation_Alignment_and_Stage1_Completion_Gate.md)** | Update [`README.md`](../README.md) & [`doc/caliburn-5-migration.md`](../doc/caliburn-5-migration.md); synchronize live [`AGENTS.md`](../AGENTS.md), [`REPO_MAP.md`](../REPO_MAP.md), & [`TODO.md`](../TODO.md); execute clean-room **Stage 1 Mandatory Completion Gate (`1.4`)** | **5 files** (`~500` targeted lines; preserving immutable `doc_before-net48/`) | Clean-room rebuild, 146/146 tests, `validate-artifacts.ps1`, SHA-256 fixture check, and forensic audit confirming zero Stage 2 leakage (**HARD STOP before Stage 2**) |

---

## Strict Stage 1 Guardrails (Non-Negotiable)

> [!IMPORTANT]
> **Packaging, Compatibility Host & CI Supply-Chain Invariants:**
> 1. **Keep `Caliburn5PackageHost` OUTSIDE `src/SomaFrameQD.sln`:** `Caliburn5PackageHost` consumes packed `.nupkg` files (`SomaFrameQDWpf` and `SomaFrameQD.Modules.CodeEditor`) rather than `<ProjectReference>` items. Never add it to `SomaFrameQD.sln`, and do **not** edit `eng/compatibility/Caliburn5PackageHost/Program.cs`.
> 2. **Keep Stage 1 External Dependencies Intact in `eng/package-contract.json`:** Do **not** remove `"System.ComponentModel.Composition": "6.0.0"` from `SomaFrameQDWpf` or `"System.Drawing.Common": "6.0.0"` from `SomaFrameQD.Modules.Inspector` in Stage 1! Those NuGet-to-GAC swaps occur exclusively in **Stage 2 (`Plan 2.1`)**.
> 3. **Preserve Supply-Chain Hardening in `.github/workflows/ci.yml`:** Do **not** alter any of the 40-character immutable GitHub Action commit SHAs (`actions/checkout`, `actions/setup-dotnet`, `actions/upload-artifact`, `actions/download-artifact`), `permissions: contents: read`, `persist-credentials: false`, `fetch-depth: 0` (required by `Nerdbank.GitVersioning`), or the isolated OIDC `publish` job (`NuGet/login@v1`).

---

## Exact File-by-File Implementation Steps

### Step 3.1 — Single-Target `eng/compatibility/Caliburn5PackageHost/Caliburn5PackageHost.csproj`

* **File:** [`C:\SOFTDEV\SomaFrameQD\eng\compatibility\Caliburn5PackageHost\Caliburn5PackageHost.csproj`](../eng/compatibility/Caliburn5PackageHost/Caliburn5PackageHost.csproj)
* **Changes (`TODO.md` `1.1` & `1.3`):**
  * Replace `<TargetFrameworks>net48;net10.0-windows</TargetFrameworks>` (line 5) with singular `<TargetFramework>net48</TargetFramework>`.
  * Leave all other properties and `<PackageReference>` items (`SomaFrameQD.Modules.CodeEditor` and `SomaFrameQDWpf` at `$(SomaFrameQDPackageVersion)`) untouched.

* **Complete Post-Edit File (`Caliburn5PackageHost.csproj`):**
  ```xml
  <Project Sdk="Microsoft.NET.Sdk.WindowsDesktop">

    <PropertyGroup>
      <OutputType>Exe</OutputType>
      <TargetFramework>net48</TargetFramework>
      <UseWPF>true</UseWPF>
      <IsPackable>false</IsPackable>
      <SomaFrameQDPackageVersion Condition="'$(SomaFrameQDPackageVersion)' == ''">0.0.0-local</SomaFrameQDPackageVersion>
    </PropertyGroup>

    <ItemGroup>
      <PackageReference Include="SomaFrameQD.Modules.CodeEditor" Version="$(SomaFrameQDPackageVersion)" />
      <PackageReference Include="SomaFrameQDWpf" Version="$(SomaFrameQDPackageVersion)" />
    </ItemGroup>

  </Project>
  ```

---

### Step 3.2 — Update `eng/package-contract.json` in Lockstep for `net48`-Only

* **File:** [`C:\SOFTDEV\SomaFrameQD\eng\package-contract.json`](../eng/package-contract.json)
* **Engineering Rationale ([`TODO.md`](../TODO.md) `1.3`, [`DRMA.md`](../DRMA.md) Risk `S1-2`):**
  1. **Remove the `net10.0-windows7.0` framework block (`lines 22–37`):** With `net10.0-windows` dropped from all 8 packable `.csproj` files, `dotnet pack` emits only `lib/net48/<Assembly>.dll` + `.pdb` and a single `.nuspec` `<group targetFramework=".NETFramework4.8">`. Leaving `net10.0-windows7.0` in `"frameworks"` causes `validate-artifacts.ps1` to fail on missing HLSL shaders, missing `lib/net10.0-windows7.0/*.dll` ZIP entries, and missing `.nuspec` groups.
  2. **Remove `"externalDependenciesByFramework": { "net10.0-windows7.0": {} }` from `SomaFrameQD.Modules.Inspector` (`lines 116–118`):** `validate-artifacts.ps1` (lines 189–195) explicitly rejects any `externalDependenciesByFramework` key that is not in `$expectedNuspecFrameworks`. On `net48`-only, `SomaFrameQD.Modules.Inspector` unconditionally declares `"System.Drawing.Common": "6.0.0"` in `"externalDependencies"` (until Stage 2 `Plan 2.1` swaps it to GAC `System.Drawing`).

* **Complete Post-Edit File (`eng/package-contract.json`):**
  ```json
  {
    "frameworks": [
      {
        "nuspec": ".NETFramework4.8",
        "asset": "net48",
        "build": "net48",
        "lockFileTarget": ".NETFramework,Version=v4.8",
        "resolvedDependencies": {
          "System.Text.Json": "10.0.12"
        },
        "resolvedAssets": {
          "AvalonEdit": "lib/net462/ICSharpCode.AvalonEdit.dll",
          "Dirkster.AvalonDock": "lib/net48/AvalonDock.dll",
          "Dirkster.AvalonDock.Core": "lib/netstandard2.0/AvalonDock.Core.dll",
          "Dirkster.AvalonDock.Serializer.Xml": "lib/netstandard2.0/AvalonDock.Serializer.Xml.dll",
          "Dirkster.AvalonDock.Themes.VS": "lib/net48/AvalonDock.Themes.VS.dll",
          "Dirkster.AvalonDock.Themes.VS2013": "lib/net48/AvalonDock.Themes.VS2013.dll",
          "DotNetProjects.Extended.Wpf.Toolkit": "lib/net40/DotNetProjects.Wpf.Extended.Toolkit.dll",
          "System.Text.Json": "lib/net462/System.Text.Json.dll"
        }
      }
    ],
    "resolvedDependencies": {
      "AvalonEdit": "6.3.1.120",
      "Caliburn.Micro": "5.0.258",
      "Caliburn.Micro.Core": "5.0.258",
      "Microsoft.Xaml.Behaviors.Wpf": "1.1.135",
      "Dirkster.AvalonDock": "5.0.1",
      "Dirkster.AvalonDock.Core": "5.0.1",
      "Dirkster.AvalonDock.Serializer.Xml": "5.0.1",
      "Dirkster.AvalonDock.Themes.VS": "5.0.1",
      "Dirkster.AvalonDock.Themes.VS2013": "5.0.1",
      "DotNetProjects.Extended.Wpf.Toolkit": "5.0.129"
    },
    "shaders": [
      "AddEffect.ps",
      "MultiplyEffect.ps"
    ],
    "packages": [
      {
        "id": "SomaFrameQDWpf",
        "assembly": "SomaFrameQD",
        "internalDependencies": [],
        "externalDependencies": {
          "Caliburn.Micro": "5.0.258",
          "Dirkster.AvalonDock.Serializer.Xml": "5.0.1",
          "Dirkster.AvalonDock.Themes.VS2013": "5.0.1",
          "DotNetProjects.Extended.Wpf.Toolkit": "5.0.129",
          "MahApps.Metro": "2.4.11",
          "System.ComponentModel.Composition": "6.0.0"
        }
      },
      {
        "id": "SomaFrameQD.Modules.CodeCompiler",
        "assembly": "SomaFrameQD.Modules.CodeCompiler",
        "internalDependencies": [
          "SomaFrameQD.Modules.ErrorList",
          "SomaFrameQD.Modules.Output",
          "SomaFrameQDWpf"
        ],
        "externalDependencies": {
          "Microsoft.CodeAnalysis.CSharp": "3.4.0"
        }
      },
      {
        "id": "SomaFrameQD.Modules.CodeEditor",
        "assembly": "SomaFrameQD.Modules.CodeEditor",
        "internalDependencies": [
          "SomaFrameQDWpf"
        ],
        "externalDependencies": {
          "AvalonEdit": "6.3.1.120"
        }
      },
      {
        "id": "SomaFrameQD.Modules.ErrorList",
        "assembly": "SomaFrameQD.Modules.ErrorList",
        "internalDependencies": [
          "SomaFrameQDWpf"
        ],
        "externalDependencies": {}
      },
      {
        "id": "SomaFrameQD.Modules.GraphEditor",
        "assembly": "SomaFrameQD.Modules.GraphEditor",
        "internalDependencies": [
          "SomaFrameQDWpf"
        ],
        "externalDependencies": {}
      },
      {
        "id": "SomaFrameQD.Modules.Inspector",
        "assembly": "SomaFrameQD.Modules.Inspector",
        "internalDependencies": [
          "SomaFrameQDWpf"
        ],
        "externalDependencies": {
          "System.Drawing.Common": "6.0.0"
        }
      },
      {
        "id": "SomaFrameQD.Modules.Output",
        "assembly": "SomaFrameQD.Modules.Output",
        "internalDependencies": [
          "SomaFrameQDWpf"
        ],
        "externalDependencies": {}
      },
      {
        "id": "SomaFrameQD.Modules.PropertyGrid",
        "assembly": "SomaFrameQD.Modules.PropertyGrid",
        "internalDependencies": [
          "SomaFrameQDWpf"
        ],
        "externalDependencies": {}
      }
    ]
  }
  ```

---

### Step 3.3 — Harden `eng/validate-artifacts.ps1` for `net48`-Only, PowerShell 5.1/7 & NuGet Lockfile Schema Compatibility

* **File:** [`C:\SOFTDEV\SomaFrameQD\eng\validate-artifacts.ps1`](../eng/validate-artifacts.ps1)
* **Engineering Rationale ([`TODO.md`](../TODO.md) `1.3`):**
  Three targeted resilience fixes in `eng/validate-artifacts.ps1` ensure the artifact gate runs cleanly across **both** local Windows 7–11 developer machines (running `powershell.exe` 5.1 + `.NET SDK 8.0.4xx`) **and** GitHub Actions `windows-2025` CI (`pwsh` 7 + `actions/setup-dotnet`):
  1. **Windows PowerShell 5.1 `ZipFile` Assembly Loading (`line 212`):**
     * On PowerShell 7 (`pwsh`), `Add-Type -AssemblyName System.IO.Compression` exposes `[System.IO.Compression.ZipFile]`. On Windows PowerShell 5.1 (`powershell.exe` running on `.NET Framework 4.8` — the native shell on this workstation and on Windows 7 SP1 `x64`), `ZipFile` lives in `System.IO.Compression.FileSystem.dll`; calling `[System.IO.Compression.ZipFile]::OpenRead(...)` without `Add-Type -AssemblyName System.IO.Compression.FileSystem` throws `Unable to find type [System.IO.Compression.ZipFile]`. Adding `Add-Type -AssemblyName System.IO.Compression.FileSystem` works identically in both PowerShell 5.1 and PowerShell 7.
  2. **Strict Single-TFM Archive Entry Check (`lines 253–257` & `lines 320–324`):**
     * Assert that every `.nupkg` contains **only** the expected `lib/<asset>/<Assembly>.dll` entries across `lib/*` (and every `.snupkg` contains **only** `lib/<asset>/<Assembly>.pdb` across `lib/*`), guaranteeing stale `lib/net10.0-windows7.0/*` binaries can never pass silently inside a multi-targeted archive.
  3. **NuGet `6.11` (`.NET SDK 8.0`) vs. NuGet `7.0` (`.NET SDK 10.0`) `project.assets.json` `"targets"` Key & `StrictMode` Empty-Dependency Guard (`lines 290–297` & `lines 384–390`):**
     * NuGet `6.11.x` (bundled in `.NET SDK 8.0.4xx`) keys `.NET Framework 4.8` inside `project.assets.json` `"targets"` as `".NETFramework,Version=v4.8"`, whereas NuGet `7.0` (`.NET SDK 10.0`) keys it by alias `"net48"`.
     * Assert `@($assets.targets.PSObject.Properties).Count -eq $expectedFrameworks.Count` (rejecting any unexpected TFM in `project.assets.json`), and resolve `$targetProperty` against `$framework.lockFileTarget` with fallback to `$framework.build` (`"net48"`), so the validator enforces the exact single `net48` target deterministically under both NuGet client versions.
     * Guard `$group.PSObject.Properties['dependency']` under `Set-StrictMode -Version Latest` (line 291) so `.nuspec` groups with zero dependencies never trigger `PropertyNotFoundException`.

* **Exact Edits in `eng/validate-artifacts.ps1`:**
  1. **Around line 212 (`System.IO.Compression.FileSystem`):**
     ```powershell
     Add-Type -AssemblyName System.IO.Compression
     Add-Type -AssemblyName System.IO.Compression.FileSystem
     ```
  2. **Around lines 253–257 (enforce exact `lib/*` DLL set in `.nupkg`):**
     ```powershell
     $expectedLibFrameworks = @($expectedFrameworks | ForEach-Object asset)
     $actualLibFrameworks = @(
         $entryNames |
             Where-Object { $_ -match '^lib/[^/]+/' } |
             ForEach-Object { ($_ -split '/')[1] } |
             Sort-Object -Unique
     )
     Assert-SetEqual `
         -Expected $expectedLibFrameworks `
         -Actual $actualLibFrameworks `
         -Description "$packageId lib target frameworks"

     $expectedLibEntries = @(
         foreach ($framework in $expectedFrameworks) {
             "lib/$($framework.asset)/$($definition.assembly).dll"
         }
     )
     $actualLibDllEntries = @($entryNames | Where-Object { $_ -match '^lib/[^/]+/[^/]+\.dll$' })
     Assert-SetEqual `
         -Expected $expectedLibEntries `
         -Actual $actualLibDllEntries `
         -Description "$packageId lib assemblies"
     ```
  3. **Around lines 290–297 (`Set-StrictMode` safe `.nuspec` `<dependency>` enumeration):**
     ```powershell
     $actualDependencies = @{}
     $dependencyProperty = $group.PSObject.Properties['dependency']
     $groupDependencies = if ($null -ne $dependencyProperty) { @($dependencyProperty.Value) } else { @() }
     foreach ($dependency in $groupDependencies) {
         $dependencyId = [string] $dependency.id
         Assert-Condition -Condition (-not $actualDependencies.ContainsKey($dependencyId)) `
             -Message "$packageId has duplicate dependency '$dependencyId' in $($group.targetFramework)."
         $actualDependencies[$dependencyId] = [string] $dependency.version
     }
     ```
  4. **Around lines 320–324 (enforce exact `lib/*` PDB set in `.snupkg`):**
     ```powershell
     $expectedPdbEntries = @(
         foreach ($framework in $expectedFrameworks) {
             "lib/$($framework.asset)/$($definition.assembly).pdb"
         }
     )
     $actualLibPdbEntries = @($symbolEntries | Where-Object { $_ -like 'lib/*.pdb' })
     Assert-SetEqual `
         -Expected $expectedPdbEntries `
         -Actual $actualLibPdbEntries `
         -Description "$packageId symbol PDBs"
     ```
  5. **Around lines 384–390 (exact target count + NuGet 6.11 / 7.0 `lockFileTarget` resolution):**
     ```powershell
     $assets = Get-Content -LiteralPath $assetsPath -Raw | ConvertFrom-Json
     $actualAssetTargets = @($assets.targets.PSObject.Properties.Name | Where-Object { $_ -notlike '*/*' })
     Assert-Condition -Condition ($actualAssetTargets.Count -eq $expectedFrameworks.Count) `
         -Message "Expected $($expectedFrameworks.Count) RID-less target(s) in $assetsPath, found $($actualAssetTargets.Count): [$($actualAssetTargets -join ', ')]."

     foreach ($framework in $expectedFrameworks) {
         $targetProperty = $assets.targets.PSObject.Properties[$framework.lockFileTarget]
         if ($null -eq $targetProperty -and $framework.lockFileTarget -ne $framework.build) {
             $targetProperty = $assets.targets.PSObject.Properties[$framework.build]
         }
         Assert-Condition -Condition ($null -ne $targetProperty) `
             -Message "Compatibility assets are missing target '$($framework.lockFileTarget)' (available: [$($actualAssetTargets -join ', ')])."
     ```

---

### Step 3.4 — Update GitHub Actions CI Workflow (`.github/workflows/ci.yml`) for `net48`-Only

* **File:** [`C:\SOFTDEV\SomaFrameQD\.github\workflows\ci.yml`](../.github/workflows/ci.yml)
* **Engineering Rationale ([`TODO.md`](../TODO.md) `1.3`, [`DRMA.md`](../DRMA.md) Risk `S1-1`):**
  * `actions/setup-dotnet@a98b56852c35b8e3190ac28c8c2271da59106c68` (`v6.0.0`) already reads `global-json-file: global.json` (updated in [Plan #1](./Plan_01_Toolchain_BuildProps_and_Library_Family_Retargeting.md) to `.NET SDK 8.0.400` with `rollForward: "latestPatch"`), so CI automatically provisions `.NET 8.0 LTS` instead of `.NET SDK 10.0.400` and forwards `steps.setup-dotnet.outputs.dotnet-version` to the `publish` job.
  * In the `Test characterization harness` step (lines 50–54):
    1. Remove the `net10.0-windows` test command completely.
    2. Replace `.NET 10`-only `dotnet test --project src\SomaFrameQD.Tests\SomaFrameQD.Tests.csproj ...` with `.NET 8 LTS`-compatible positional syntax `dotnet test src\SomaFrameQD.Tests\SomaFrameQD.Tests.csproj --configuration Release --framework net48 --no-build --no-restore`.

* **Before (`.github/workflows/ci.yml` lines 50–54):**
  ```yaml
        - name: Test characterization harness
          run: |
            dotnet test --project src\SomaFrameQD.Tests\SomaFrameQD.Tests.csproj --configuration Release --framework net48 --no-build --no-restore
            dotnet test --project src\SomaFrameQD.Tests\SomaFrameQD.Tests.csproj --configuration Release --framework net10.0-windows --no-build --no-restore
  ```
* **After (`.github/workflows/ci.yml` lines 50–52):**
  ```yaml
        - name: Test characterization harness
          run: dotnet test src\SomaFrameQD.Tests\SomaFrameQD.Tests.csproj --configuration Release --framework net48 --no-build --no-restore
  ```
  *(Leave all 40-char Action commit SHAs, `fetch-depth: 0`, `Validate build and packages`, `Upload packages`, and the entire OIDC `publish` job completely unchanged.)*

---

## Plan #3 Verification Gate (Must Pass Before Starting Plan #4)

Run the following verification commands from `C:\SOFTDEV\SomaFrameQD`:

```powershell
# 1. Confirm zero net10 references remain in packaging contract, compatibility host, and CI workflow
Select-String -Path 'eng\package-contract.json','eng\compatibility\Caliburn5PackageHost\Caliburn5PackageHost.csproj','.github\workflows\ci.yml' `
    -Pattern 'net10|TargetFrameworks|--project src\\SomaFrameQD\.Tests'

# 2. Clear any prior package outputs so stale artifacts cannot mask pack regressions
if (Test-Path 'build\packages') { Remove-Item -Recurse -Force 'build\packages' }

# 3. Pack all 8 NuGet libraries (.nupkg + .snupkg) in Release
dotnet pack src\SomaFrameQD.sln --configuration Release --no-build --no-restore --output build\packages
if ($LASTEXITCODE -ne 0) { throw "dotnet pack failed with exit code $LASTEXITCODE" }

# 4. Run the full declarative artifact, shader, .nuspec, lockfile, and Caliburn5PackageHost live runtime gate
#    (Works in Windows PowerShell 5.1 locally AND pwsh 7 in GitHub Actions CI)
powershell -NoProfile -ExecutionPolicy Bypass -File .\eng\validate-artifacts.ps1
if ($LASTEXITCODE -ne 0) { throw "validate-artifacts.ps1 failed with exit code $LASTEXITCODE" }
```

### Gate Pass Criteria
1. `Select-String` returns **empty** across `eng/package-contract.json`, `Caliburn5PackageHost.csproj`, and `.github/workflows/ci.yml`.
2. `dotnet pack` emits **exactly 8 `.nupkg` files and 8 `.snupkg` files** in `build\packages` at a single coherent `0.9.0` NBGV package version.
3. `eng/validate-artifacts.ps1` completes with exit code `0` and prints:
   `Validated 2 compiled shaders, 8 packages with symbols, exact resolved assets, and the package-only compatibility host at version ...`
4. Every `.nupkg` contains `lib/net48/<Assembly>.dll` (and **zero** `lib/net10.0-windows7.0/*` entries), and `Caliburn5PackageHost` restores, builds, and executes cleanly on `net48`.

Once all 4 criteria pass, proceed to **[Implementation Plan #4](./Plan_04_Documentation_Alignment_and_Stage1_Completion_Gate.md)**.
