# Implementation Plan #2 — Demo, Tests, Solution Topology & Obsolete Multi-Target Artifact Cleanup (`net48` Only)

> **Purpose:** Retarget `SomaFrameQD.Demo` and `SomaFrameQD.Tests` exclusively to `<TargetFramework>net48</TargetFramework>`, remove the `.NET 10`-only `MahApps.Net10DragHost` project from disk and `src/SomaFrameQD.sln`, and clean up obsolete `.NET Core 3.1` / `.NET 5` publish profiles and VS Code debug launch paths so the entire solution builds and passes all **146 STA characterization tests** on `net48`.  
> **Generated Timestamp:** `2026-11-22T19:22:00-05:00` (`2026-11-22`)  
> **Prerequisite:** **[Implementation Plan #1](./Plan_01_Toolchain_BuildProps_and_Library_Family_Retargeting.md) MUST be 100% completed and verified green first.**  
> **Stage Scope:** **Stage 1 (`net48` Retargeting ONLY)** — **ZERO** Stage 2 runtime, `app.config`, `app.manifest`, `AppBootstrapper`, GAC, `x64`, or Win32 changes permitted.  
> **Execution Order:** [Plan #1](./Plan_01_Toolchain_BuildProps_and_Library_Family_Retargeting.md) → **Plan #2 (This Plan)** → [Plan #3](./Plan_03_Package_Contract_CompatibilityHost_and_CI_Pipeline.md) → [Plan #4](./Plan_04_Documentation_Alignment_and_Stage1_Completion_Gate.md).  
> **Reference Documents:** [`TODO.md`](../TODO.md) (`1.1`–`1.2`) | [`DRMA.md`](../DRMA.md) (`S1-1`, Section 4.2) | [`doc_before-net48/08_Test_Suite_and_Characterization_Harness.md`](../doc_before-net48/08_Test_Suite_and_Characterization_Harness.md) | [`AGENTS.md`](../AGENTS.md)

---

## Stage 1 Four-Plan Split Overview (`Implementation-Plans_net48-retargeting-ONLY`)

| Plan | Execution Scope | Files Touched / Context Budget | Verification Gate |
| :--- | :--- | :--- | :--- |
| **[Plan #1](./Plan_01_Toolchain_BuildProps_and_Library_Family_Retargeting.md)** | Toolchain SDK pin ([`global.json`](../global.json)), shared `<LangVersion>` ([`src/Directory.Build.props`](../src/Directory.Build.props)), all **8 packable library `.csproj` files** (`net48` single-target + unwrap `net48` conditions), and `CodeEditor` `.resx` `Version=4.0.0.0` header fix | **11 files** (`~410` lines read; skipping `.resx` data bodies) | `dotnet --version` resolves cleanly; all 8 library projects restore & build in `Release` for `net48` with `0` errors |
| **Plan #2 (This Plan)** | Retarget [`SomaFrameQD.Demo.csproj`](../src/SomaFrameQD.Demo/SomaFrameQD.Demo.csproj) (drop `HelixToolkit.Core.Wpf`) & [`SomaFrameQD.Tests.csproj`](../src/SomaFrameQD.Tests/SomaFrameQD.Tests.csproj); delete [`MahApps.Net10DragHost`](../eng/compatibility/MahApps.Net10DragHost/) & clean [`SomaFrameQD.sln`](../src/SomaFrameQD.sln); delete `.NET Core 3.1`/`.NET 5` `.pubxml` profiles & update [`src/.vscode/launch.json`](../src/.vscode/launch.json) | **8 files** (`~350` lines) | Full 10-project `SomaFrameQD.sln` restores & builds in `Release` (`net48` + HLSL `.ps` shaders); all **146 STA tests** pass on `net48` |
| **[Plan #3](./Plan_03_Package_Contract_CompatibilityHost_and_CI_Pipeline.md)** | Retarget [`Caliburn5PackageHost.csproj`](../eng/compatibility/Caliburn5PackageHost/Caliburn5PackageHost.csproj); update [`eng/package-contract.json`](../eng/package-contract.json) & [`eng/validate-artifacts.ps1`](../eng/validate-artifacts.ps1) (PowerShell 5.1 + 7 & NuGet lockfile compatibility); update [`.github/workflows/ci.yml`](../.github/workflows/ci.yml) to `net48`-only | **4 files** (`~725` lines) | `dotnet pack` emits 8 `.nupkg` + 8 `.snupkg` (`lib/net48` only); `eng/validate-artifacts.ps1` passes 100% including live `Caliburn5PackageHost` run |
| **[Plan #4](./Plan_04_Documentation_Alignment_and_Stage1_Completion_Gate.md)** | Update [`README.md`](../README.md) & [`doc/caliburn-5-migration.md`](../doc/caliburn-5-migration.md); synchronize live [`AGENTS.md`](../AGENTS.md), [`REPO_MAP.md`](../REPO_MAP.md), & [`TODO.md`](../TODO.md); execute clean-room **Stage 1 Mandatory Completion Gate (`1.4`)** | **5 files** (`~500` targeted lines; preserving immutable `doc_before-net48/`) | Clean-room rebuild, 146/146 tests, `validate-artifacts.ps1`, SHA-256 fixture check, and forensic audit confirming zero Stage 2 leakage (**HARD STOP before Stage 2**) |

---

## Strict Stage 1 Guardrails (Non-Negotiable)

> [!WARNING]
> **Critical Stage-Boundary Separations in Plan #2:**
> 1. **Do NOT edit `AppBootstrapper.cs` or `DemoAppBootstrapper.cs` in Stage 1:** Although Step 2.4 deletes the obsolete `.pubxml` publish profile XML files (`FolderProfileNet5.pubxml`, `FolderProfileNetCore31.pubxml`) per `TODO.md` `1.2`, stripping the C# `PublishSingleFile` detection/bypass methods inside `src/SomaFrameQD/AppBootstrapper.cs` and `src/SomaFrameQD.Demo/DemoAppBootstrapper.cs` is explicitly scheduled for **Stage 2 (`Plan 2.2`)** (`TODO.md` line 95). Leave all `.cs` and `.xaml` files in `SomaFrameQD` and `SomaFrameQD.Demo` untouched!
> 2. **Do NOT edit `src/SomaFrameQD/app.config` or `src/SomaFrameQD.Demo/app.config` in Stage 1:** Upgrading `<supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.6.1"/>` to `sku=".NETFramework,Version=v4.8"` and adding `app.manifest` belong strictly to **Stage 2 (`Plan 2.2`)** (`TODO.md` lines 93–94).
> 3. **Do NOT enable `<AutoGenerateBindingRedirects>` or `<PlatformTarget>x64</PlatformTarget>` yet:** Those belong strictly to **Stage 2 (`Plan 2.1`)** (`TODO.md` line 88).
> 4. **NEVER modify, reformat, or regenerate `src/SomaFrameQD.Tests/TestData/*`:** `AvalonDock460/CompleteLayout.xml`, `AvalonDock4741/ApplicationState.bin`, and `AvalonDock501/ApplicationState.bin` (and their `README.md` files) are append-only compatibility fixtures whose exact byte lengths and SHA-256 hashes are asserted by `AvalonDockPersistenceTests` ([`AGENTS.md`](../AGENTS.md) Invariant #7).

---

## Exact File-by-File Implementation Steps

### Step 2.1 — Single-Target `src/SomaFrameQD.Demo/SomaFrameQD.Demo.csproj` & Drop `HelixToolkit.Core.Wpf`

* **File:** [`C:\SOFTDEV\SomaFrameQD\src\SomaFrameQD.Demo\SomaFrameQD.Demo.csproj`](../src/SomaFrameQD.Demo/SomaFrameQD.Demo.csproj)
* **Changes (`TODO.md` `1.1` & `1.2`):**
  1. Replace `<TargetFrameworks>net48;net10.0-windows</TargetFrameworks>` (line 6) with singular `<TargetFramework>net48</TargetFramework>`.
  2. Delete the `.NET Core` / `.NET 10` conditional `<ItemGroup Condition="$(TargetFramework) != 'net48'">` containing `<PackageReference Include="HelixToolkit.Core.Wpf" Version="2.10.0" />` (lines 68–70).
  3. Remove `Condition="$(TargetFramework) == 'net48'"` from `<ItemGroup Condition="$(TargetFramework) == 'net48'">` around `<PackageReference Include="HelixToolkit.Wpf" Version="2.10.0" />` (lines 72–74) so `HelixToolkit.Wpf` `2.10.0` is referenced unconditionally.
  4. **Preserve unchanged:**
     * `<OutputType>WinExe</OutputType>`, `<UseWPF>true</UseWPF>`, `<AllowUnsafeBlocks>true</AllowUnsafeBlocks>`, `<IsPackable>false</IsPackable>`.
     * The `<HLSLShader Include="Modules\FilterDesigner\ShaderEffects\*.fx">` block (lines 58–61) and `<PackageReference Include="JeremyAnsel.HLSL.Targets" Version="1.0.9" />` (line 65) — required by `eng/validate-artifacts.ps1` to emit `AddEffect.ps` and `MultiplyEffect.ps` under `src\SomaFrameQD.Demo\bin\Release\net48\Modules\FilterDesigner\ShaderEffects\`.
     * `<PackageReference Update="Nerdbank.GitVersioning" Version="3.5.109" />` (line 77) and `src/SomaFrameQD.Demo/version.json` (`0.1.0`).

* **Before (`lines 3–10` and `lines 64–79`):**
  ```xml
    <PropertyGroup>
      <RootNamespace>SomaFrameQD.Demo</RootNamespace>
      <OutputType>WinExe</OutputType>
      <TargetFrameworks>net48;net10.0-windows</TargetFrameworks>
      <UseWPF>true</UseWPF>
      <AllowUnsafeBlocks>true</AllowUnsafeBlocks>
      <IsPackable>false</IsPackable>
    </PropertyGroup>
  ```
  ```xml
    <ItemGroup>
      <PackageReference Include="JeremyAnsel.HLSL.Targets" Version="1.0.9" />
    </ItemGroup>

    <ItemGroup Condition="$(TargetFramework) != 'net48'">
      <PackageReference Include="HelixToolkit.Core.Wpf" Version="2.10.0" />
    </ItemGroup>

    <ItemGroup Condition="$(TargetFramework) == 'net48'">
      <PackageReference Include="HelixToolkit.Wpf" Version="2.10.0" />
    </ItemGroup>

    <ItemGroup>
      <PackageReference Update="Nerdbank.GitVersioning" Version="3.5.109" />
    </ItemGroup>
  ```
* **After (`lines 3–10` and `lines 64–75`):**
  ```xml
    <PropertyGroup>
      <RootNamespace>SomaFrameQD.Demo</RootNamespace>
      <OutputType>WinExe</OutputType>
      <TargetFramework>net48</TargetFramework>
      <UseWPF>true</UseWPF>
      <AllowUnsafeBlocks>true</AllowUnsafeBlocks>
      <IsPackable>false</IsPackable>
    </PropertyGroup>
  ```
  ```xml
    <ItemGroup>
      <PackageReference Include="JeremyAnsel.HLSL.Targets" Version="1.0.9" />
    </ItemGroup>

    <ItemGroup>
      <PackageReference Include="HelixToolkit.Wpf" Version="2.10.0" />
    </ItemGroup>

    <ItemGroup>
      <PackageReference Update="Nerdbank.GitVersioning" Version="3.5.109" />
    </ItemGroup>
  ```

---

### Step 2.2 — Single-Target `src/SomaFrameQD.Tests/SomaFrameQD.Tests.csproj`

* **File:** [`C:\SOFTDEV\SomaFrameQD\src\SomaFrameQD.Tests\SomaFrameQD.Tests.csproj`](../src/SomaFrameQD.Tests/SomaFrameQD.Tests.csproj)
* **Changes (`TODO.md` `1.1`):**
  * Replace `<TargetFrameworks>net48;net10.0-windows</TargetFrameworks>` (line 5) with singular `<TargetFramework>net48</TargetFramework>`.
  * **Preserve unchanged:** `<Project Sdk="MSTest.Sdk">`, `<UseWPF>true</UseWPF>`, `<UseSTASynchronizationContext>true</UseSTASynchronizationContext>`, `<IsPackable>false</IsPackable>`, `<Nullable>disable</Nullable>`, `<ImplicitUsings>disable</ImplicitUsings>`, all 4 `<ProjectReference>` items (`CodeEditor`, `Inspector`, `PropertyGrid`, `SomaFrameQD`), and all 5 `TestData\*` `<None Update="..." CopyToOutputDirectory="PreserveNewest" />` entries.

* **Before (`lines 3–11`):**
  ```xml
    <PropertyGroup>
      <RootNamespace>SomaFrameQD.Tests</RootNamespace>
      <TargetFrameworks>net48;net10.0-windows</TargetFrameworks>
      <UseWPF>true</UseWPF>
      <UseSTASynchronizationContext>true</UseSTASynchronizationContext>
      <IsPackable>false</IsPackable>
      <Nullable>disable</Nullable>
      <ImplicitUsings>disable</ImplicitUsings>
    </PropertyGroup>
  ```
* **After (`lines 3–11`):**
  ```xml
    <PropertyGroup>
      <RootNamespace>SomaFrameQD.Tests</RootNamespace>
      <TargetFramework>net48</TargetFramework>
      <UseWPF>true</UseWPF>
      <UseSTASynchronizationContext>true</UseSTASynchronizationContext>
      <IsPackable>false</IsPackable>
      <Nullable>disable</Nullable>
      <ImplicitUsings>disable</ImplicitUsings>
    </PropertyGroup>
  ```

---

### Step 2.3 — Retire `eng/compatibility/MahApps.Net10DragHost` & Clean `src/SomaFrameQD.sln`

* **Engineering Rationale ([`TODO.md`](../TODO.md) `1.1`, [`MahApps.Net10DragHost/README.md`](../eng/compatibility/MahApps.Net10DragHost/README.md) lines 86–87):**
  * `MahApps.Net10DragHost` targets `<TargetFramework>net10.0-windows</TargetFramework>` exclusively to record the `.NET 10`-specific `Window.CriticalHandle` reflection fix in `MahApps.Metro` `2.4.11` (issue `#4554`). With `.NET 10` removed from the repository, `MahApps.Net10DragHost` cannot build and has fulfilled its lifecycle. Per its own removal instructions: *"Removal should delete this directory and its `Compatibility` solution entry together."*
  * *(Note: Do **NOT** delete `eng/compatibility/Caliburn5PackageHost/`! `Caliburn5PackageHost` is the external packed-NuGet verification host used by `eng/validate-artifacts.ps1` and is retargeted to `net48` in [Plan #3](./Plan_03_Package_Contract_CompatibilityHost_and_CI_Pipeline.md).)*

1. **Delete directory `C:\SOFTDEV\SomaFrameQD\eng\compatibility\MahApps.Net10DragHost\`** (removing `MahApps.Net10DragHost.csproj`, `Program.cs`, and `README.md`).
2. **Edit [`C:\SOFTDEV\SomaFrameQD\src\SomaFrameQD.sln`](../src/SomaFrameQD.sln):**
   * Remove lines `35–38` (the `Compatibility` solution folder `{80B06549-982F-5858-B5D8-F248B6FD4422}` and `MahApps.Net10DragHost` project `{DE3AD73A-59B9-48F3-AEEF-1BD431C8FE71}`):
     ```text
     Project("{2150E333-8FDC-42A3-9474-1A3956D46DE8}") = "Compatibility", "Compatibility", "{80B06549-982F-5858-B5D8-F248B6FD4422}"
     EndProject
     Project("{9A19103F-16F7-4668-BE54-9A1E7A4F7556}") = "MahApps.Net10DragHost", "..\eng\compatibility\MahApps.Net10DragHost\MahApps.Net10DragHost.csproj", "{DE3AD73A-59B9-48F3-AEEF-1BD431C8FE71}"
     EndProject
     ```
   * Remove lines `83–86` in `GlobalSection(ProjectConfigurationPlatforms)`:
     ```text
     		{DE3AD73A-59B9-48F3-AEEF-1BD431C8FE71}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
     		{DE3AD73A-59B9-48F3-AEEF-1BD431C8FE71}.Debug|Any CPU.Build.0 = Debug|Any CPU
     		{DE3AD73A-59B9-48F3-AEEF-1BD431C8FE71}.Release|Any CPU.ActiveCfg = Release|Any CPU
     		{DE3AD73A-59B9-48F3-AEEF-1BD431C8FE71}.Release|Any CPU.Build.0 = Release|Any CPU
     ```
   * Remove line `97` in `GlobalSection(NestedProjects)`:
     ```text
     		{DE3AD73A-59B9-48F3-AEEF-1BD431C8FE71} = {80B06549-982F-5858-B5D8-F248B6FD4422}
     ```
   * Verify `src/SomaFrameQD.sln` retains **10 C# projects** (`SomaFrameQD`, 7 `SomaFrameQD.Modules.*`, `SomaFrameQD.Demo`, `SomaFrameQD.Tests`) plus the `Solution Items` and `Demos` solution folders with CRLF line endings preserved.

---

### Step 2.4 — Delete Obsolete `.NET Core 3.1` / `.NET 5` Publish Profiles & Update `src/.vscode/launch.json`

* **Engineering Rationale ([`TODO.md`](../TODO.md) `1.2`):**
  * `src/SomaFrameQD.Demo/Properties/PublishProfiles/` contains only two legacy single-file `.NET Core 3.1` / `.NET 5` publish profiles (`FolderProfileNetCore31.pubxml` targeting `netcoreapp3.1` and `FolderProfileNet5.pubxml` targeting `net5.0-windows`). Neither is valid on `net48`.
  * `src/.vscode/launch.json` still hardcodes `"type": "coreclr"` launch configurations pointing to `SomaFrameQD.Demo/bin/Debug/netcoreapp3.1/publish/SomaFrameQD.Demo.exe` and `SomaFrameQD.Demo/bin/Debug/net5.0-windows/publish/SomaFrameQD.Demo.exe`.

1. **Delete obsolete publish profile files and empty directory:**
   * Delete `C:\SOFTDEV\SomaFrameQD\src\SomaFrameQD.Demo\Properties\PublishProfiles\FolderProfileNet5.pubxml`
   * Delete `C:\SOFTDEV\SomaFrameQD\src\SomaFrameQD.Demo\Properties\PublishProfiles\FolderProfileNetCore31.pubxml`
   * Remove the now-empty `C:\SOFTDEV\SomaFrameQD\src\SomaFrameQD.Demo\Properties\PublishProfiles\` directory.
2. **Update [`C:\SOFTDEV\SomaFrameQD\src\.vscode\launch.json`](../src/.vscode/launch.json) to `.NET Framework 4.8` (`clr`) paths:**
   ```json
   {
       "version": "0.2.0",
       "configurations": [
           {
               "name": "Launch SomaFrameQD.Demo (.NET Framework 4.8)",
               "type": "clr",
               "request": "launch",
               "program": "${workspaceFolder}/SomaFrameQD.Demo/bin/Debug/net48/SomaFrameQD.Demo.exe",
               "args": [],
               "cwd": "${workspaceFolder}/SomaFrameQD.Demo/bin/Debug/net48/",
               "console": "internalConsole",
               "stopAtEntry": false
           },
           {
               "name": ".NET Framework Attach",
               "type": "clr",
               "request": "attach",
               "processId": "${command:pickProcess}"
           }
       ]
   }
   ```

---

## Plan #2 Verification Gate (Must Pass Before Starting Plan #3)

Run the following verification commands from `C:\SOFTDEV\SomaFrameQD`:

```powershell
# 1. Verify obsolete directories/files are completely removed and solution contains zero DragHost references
if (Test-Path 'eng\compatibility\MahApps.Net10DragHost') { throw 'MahApps.Net10DragHost directory still exists!' }
if (Test-Path 'src\SomaFrameQD.Demo\Properties\PublishProfiles') { throw 'PublishProfiles directory still exists!' }
Select-String -Path 'src\SomaFrameQD.sln','src\.vscode\launch.json','src\SomaFrameQD.Demo\SomaFrameQD.Demo.csproj','src\SomaFrameQD.Tests\SomaFrameQD.Tests.csproj' `
    -Pattern 'net10|net5|netcoreapp|Net10DragHost|DE3AD73A|80B06549|HelixToolkit\.Core\.Wpf|TargetFrameworks'

# 2. Deterministic NuGet restore of the full 10-project solution
dotnet restore src\SomaFrameQD.sln --configfile src\NuGet.config
if ($LASTEXITCODE -ne 0) { throw "Solution restore failed with exit code $LASTEXITCODE" }

# 3. Build the entire 10-project solution in Release configuration
dotnet build src\SomaFrameQD.sln --configuration Release --no-restore
if ($LASTEXITCODE -ne 0) { throw "Solution Release build failed with exit code $LASTEXITCODE" }

# 4. Verify compiled Demo HLSL pixel shaders exist under bin\Release\net48\
foreach ($shader in @('AddEffect.ps', 'MultiplyEffect.ps')) {
    $p = "src\SomaFrameQD.Demo\bin\Release\net48\Modules\FilterDesigner\ShaderEffects\$shader"
    if (-not (Test-Path $p) -or (Get-Item $p).Length -le 0) { throw "Missing or empty HLSL shader: $p" }
}

# 5. Execute the full 146-test STA characterization suite on net48
dotnet test src\SomaFrameQD.Tests\SomaFrameQD.Tests.csproj --configuration Release --framework net48 --no-build --no-restore
if ($LASTEXITCODE -ne 0) { throw "SomaFrameQD.Tests failed on net48 with exit code $LASTEXITCODE" }
```

### Gate Pass Criteria
1. `MahApps.Net10DragHost` and `PublishProfiles` are deleted from disk, and `Select-String` returns **empty** across `SomaFrameQD.sln`, `launch.json`, `SomaFrameQD.Demo.csproj`, and `SomaFrameQD.Tests.csproj`.
2. `dotnet restore src\SomaFrameQD.sln --configfile src\NuGet.config` and `dotnet build src\SomaFrameQD.sln --configuration Release --no-restore` complete with **`0 Error(s)`**.
3. Both compiled HLSL pixel shaders (`AddEffect.ps` and `MultiplyEffect.ps`) exist and are non-empty under `src\SomaFrameQD.Demo\bin\Release\net48\Modules\FilterDesigner\ShaderEffects\`.
4. All **146 STA characterization tests** in `SomaFrameQD.Tests` pass on `net48` (`0` failed, `0` skipped), and `TestData/` fixtures remain 100% unmodified.

Once all 4 criteria pass, proceed to **[Implementation Plan #3](./Plan_03_Package_Contract_CompatibilityHost_and_CI_Pipeline.md)**.
