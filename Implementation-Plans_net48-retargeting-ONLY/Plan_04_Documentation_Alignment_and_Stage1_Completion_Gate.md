# Implementation Plan #4 — Documentation Alignment, Live Agent Guidance Sync & Stage 1 Mandatory Completion Gate (Hard Stop Before Stage 2)

> **Purpose:** Update `README.md` and `doc/caliburn-5-migration.md` to reflect `.NET Framework 4.8` (`net48`)-only targeting (`TODO.md` `1.3`), synchronize the live root AI navigation files (`AGENTS.md`, `REPO_MAP.md`, and `TODO.md`) while keeping `doc_before-net48/` strictly immutable, and execute the clean-room **Stage 1 Mandatory Completion Gate (`TODO.md` `1.4`)** as a hard stop before any Stage 2 optimization work begins.  
> **Generated Timestamp:** `2026-11-22T19:22:00-05:00` (`2026-11-22`)  
> **Prerequisite:** **[Plan #1](./Plan_01_Toolchain_BuildProps_and_Library_Family_Retargeting.md)**, **[Plan #2](./Plan_02_Demo_Tests_Solution_and_Obsolete_MultiTarget_Cleanup.md)**, and **[Plan #3](./Plan_03_Package_Contract_CompatibilityHost_and_CI_Pipeline.md) MUST all be 100% completed and verified green first.**  
> **Stage Scope:** **Stage 1 (`net48` Retargeting ONLY)** — Final documentation alignment and clean-room verification gate (`1.4`).  
> **Execution Order:** [Plan #1](./Plan_01_Toolchain_BuildProps_and_Library_Family_Retargeting.md) → [Plan #2](./Plan_02_Demo_Tests_Solution_and_Obsolete_MultiTarget_Cleanup.md) → [Plan #3](./Plan_03_Package_Contract_CompatibilityHost_and_CI_Pipeline.md) → **Plan #4 (This Plan — Final Stage 1 Gate)**.  
> **Reference Documents:** [`TODO.md`](../TODO.md) (`1.3`–`1.4`) | [`README.md`](../README.md) | [`doc/caliburn-5-migration.md`](../doc/caliburn-5-migration.md) | [`AGENTS.md`](../AGENTS.md) | [`REPO_MAP.md`](../REPO_MAP.md)

---

## Stage 1 Four-Plan Split Overview (`Implementation-Plans_net48-retargeting-ONLY`)

| Plan | Execution Scope | Files Touched / Context Budget | Verification Gate |
| :--- | :--- | :--- | :--- |
| **[Plan #1](./Plan_01_Toolchain_BuildProps_and_Library_Family_Retargeting.md)** | Toolchain SDK pin ([`global.json`](../global.json)), shared `<LangVersion>` ([`src/Directory.Build.props`](../src/Directory.Build.props)), all **8 packable library `.csproj` files** (`net48` single-target + unwrap `net48` conditions), and `CodeEditor` `.resx` `Version=4.0.0.0` header fix | **11 files** (`~410` lines read; skipping `.resx` data bodies) | `dotnet --version` resolves cleanly; all 8 library projects restore & build in `Release` for `net48` with `0` errors |
| **[Plan #2](./Plan_02_Demo_Tests_Solution_and_Obsolete_MultiTarget_Cleanup.md)** | Retarget [`SomaFrameQD.Demo.csproj`](../src/SomaFrameQD.Demo/SomaFrameQD.Demo.csproj) (drop `HelixToolkit.Core.Wpf`) & [`SomaFrameQD.Tests.csproj`](../src/SomaFrameQD.Tests/SomaFrameQD.Tests.csproj); delete [`MahApps.Net10DragHost`](../eng/compatibility/MahApps.Net10DragHost/) & clean [`SomaFrameQD.sln`](../src/SomaFrameQD.sln); delete `.NET Core 3.1`/`.NET 5` `.pubxml` profiles & update [`src/.vscode/launch.json`](../src/.vscode/launch.json) | **8 files** (`~350` lines) | Full 10-project `SomaFrameQD.sln` restores & builds in `Release` (`net48` + HLSL `.ps` shaders); all **146 STA tests** pass on `net48` |
| **[Plan #3](./Plan_03_Package_Contract_CompatibilityHost_and_CI_Pipeline.md)** | Retarget [`Caliburn5PackageHost.csproj`](../eng/compatibility/Caliburn5PackageHost/Caliburn5PackageHost.csproj); update [`eng/package-contract.json`](../eng/package-contract.json) & [`eng/validate-artifacts.ps1`](../eng/validate-artifacts.ps1) (PowerShell 5.1 + 7 & NuGet lockfile compatibility); update [`.github/workflows/ci.yml`](../.github/workflows/ci.yml) to `net48`-only | **4 files** (`~725` lines) | `dotnet pack` emits 8 `.nupkg` + 8 `.snupkg` (`lib/net48` only); `eng/validate-artifacts.ps1` passes 100% including live `Caliburn5PackageHost` run |
| **Plan #4 (This Plan)** | Update [`README.md`](../README.md) & [`doc/caliburn-5-migration.md`](../doc/caliburn-5-migration.md); synchronize live [`AGENTS.md`](../AGENTS.md), [`REPO_MAP.md`](../REPO_MAP.md), & [`TODO.md`](../TODO.md); execute clean-room **Stage 1 Mandatory Completion Gate (`1.4`)** | **5 files** (`~500` targeted lines; preserving immutable `doc_before-net48/`) | Clean-room rebuild, 146/146 tests, `validate-artifacts.ps1`, SHA-256 fixture check, and forensic audit confirming zero Stage 2 leakage (**HARD STOP before Stage 2**) |

---

## Strict Documentation & Snapshot Immutability Rules

> [!CAUTION]
> **Immutable Baseline Directories — DO NOT MODIFY:**
> 1. **`C:\SOFTDEV\SomaFrameQD\doc_before-net48\*`** (`00_INDEX.md` and `01`–`09`): Per [`AGENTS.md`](../AGENTS.md) Section 2 Rule #4, `doc_before-net48/` is the permanent *"Before `.NET 4.8` Retargeting & Optimization"* architectural snapshot. **Never edit any file inside `doc_before-net48/`.**
> 2. **`C:\SOFTDEV\SomaFrameQD\Implementation-Plans_doc_before-net48\*`**, **`LOC-statistics_plus_net48-retargeting\*`**, **`VNIR.md`**, and **`DRMA.md`**: Historical baseline reports and risk analyses — leave completely untouched.
> 3. **`C:\SOFTDEV\SomaFrameQD\src\SomaFrameQD.Tests\TestData\*`** (including `AvalonDock501/README.md`, which records historical fixture provenance): Leave completely untouched.
> 4. **Append-Only Migration History in `doc/caliburn-5-migration.md`:** Per lines 50–52 of `doc/caliburn-5-migration.md` (*"Dependency changes are recorded as migrations rather than a mutable inventory. Add a new subsection for each later transition so consumers can follow every upgrade path without rewriting earlier guidance"*), preserve the existing `### AvalonDock 4.60.0 to 4.74.1` and `### AvalonDock 4.74.1 to 5.0.1` subsections verbatim and append a new subsection recording the `.NET Framework 4.8` (`net48`) single-target consolidation.

---

## Exact File-by-File Implementation Steps

### Step 4.1 — Update `README.md` for `.NET Framework 4.8` (`net48`) Targeting

* **File:** [`C:\SOFTDEV\SomaFrameQD\README.md`](../README.md) *(read lines `1`–`80` only)*
* **Changes (`TODO.md` `1.3`):**
  1. On line 5, update the parenthetical description of Dirkster99's AvalonDock fork from `(specifically Dirkster99's fork, because it supports .NET Core)` to `(Dirkster.AvalonDock 5.0.1)` (or retain neutral phrasing without implying `SomaFrameQD` targets `.NET Core`).
  2. In `## Continuous builds` (`lines 62–76`), update `for the net48 and net10.0-windows targets` to state that GitHub Actions builds, tests, packs, and validates every push and pull request exclusively for the **`.NET Framework 4.8` (`net48`)** target:

* **Before (`README.md` lines 62–72):**
  ```markdown
  ## Continuous builds

  GitHub Actions builds every push and pull request for the `net48` and
  `net10.0-windows` targets. Successful runs provide the validated NuGet and
  symbol packages as a downloadable workflow artifact. The project files and
  [`eng/package-contract.json`](eng/package-contract.json) are the authoritative
  dependency and asset inventory; CI rejects packages that drift from that
  contract. Net6/net7 remain available from the legacy package line. Publication
  remains restricted to approved upstream releases. See
  [the CI workflow](.github/workflows/ci.yml) for the exact build and release
  contract.
  ```
* **After (`README.md` lines 62–72):**
  ```markdown
  ## Continuous builds

  GitHub Actions builds, tests, packs, and validates every push and pull request
  exclusively for the `.NET Framework 4.8` (`net48`) target. Successful runs
  provide the validated NuGet and symbol packages as a downloadable workflow
  artifact. The project files and
  [`eng/package-contract.json`](eng/package-contract.json) are the authoritative
  dependency and asset inventory; CI rejects packages that drift from that
  contract. Earlier multi-target assets remain available only from historical
  package lines. Publication remains restricted to approved upstream releases.
  See [the CI workflow](.github/workflows/ci.yml) for the exact build and release
  contract.
  ```

---

### Step 4.2 — Update `doc/caliburn-5-migration.md` with Single-Target `net48` Consolidation Record

* **File:** [`C:\SOFTDEV\SomaFrameQD\doc\caliburn-5-migration.md`](../doc/caliburn-5-migration.md) *(read lines `1`–`95` only)*
* **Changes (`TODO.md` `1.3`):**
  1. **Update opening summary (`lines 3–18`)** to reflect that SomaFrameQD's Caliburn Micro 5 (`0.9.0`) package family now targets **`net48` exclusively** (removing `and net10.0-windows` as a currently shipped target while noting that applications requiring `net462`, `net6`, `net7`, or modern `.NET` targets must remain on an earlier historical line):
     ```markdown
     SomaFrameQD's Caliburn Micro 5 package line targets `.NET Framework 4.8`
     (`net48`) exclusively. Applications that require previous `net462`, `net6`,
     `net7`, or `net10.0-windows` assets must remain on an earlier SomaFrameQD
     package revision.
     ```
  2. **Append a new migration subsection under `## Related dependency migration records` (immediately after line 90, following `### AvalonDock 4.74.1 to 5.0.1` without altering lines 54–90):**
     ```markdown
     ### Consolidation to .NET Framework 4.8 (`net48`) only

     This transition removes `net10.0-windows` multi-targeting across `SomaFrameQDWpf`,
     all seven `SomaFrameQD.Modules.*` packages, `SomaFrameQD.Demo`, `SomaFrameQD.Tests`,
     and `Caliburn5PackageHost`, consolidating the `0.9.0` package family on
     `.NET Framework 4.8` (`net48`). Every published `.nupkg` and `.snupkg` archive
     now emits only `lib/net48` assets and a single `.NETFramework4.8` dependency
     group validated by `eng/package-contract.json` and `eng/validate-artifacts.ps1`.
     ```

---

### Step 4.3 — Synchronize Live Agent Guidance (`AGENTS.md` & `REPO_MAP.md`)

Because AI coding agents entering **Stage 2 (`Plan 2.1` → `Plan 2.6`)** read [`AGENTS.md`](../AGENTS.md) and [`REPO_MAP.md`](../REPO_MAP.md) first, update **only** the live repository-state and CLI command blocks in those two root files so Stage 2 sessions do not execute deleted `--framework net10.0-windows` or `.NET SDK 10`-only `dotnet test --project` commands:

1. **In [`C:\SOFTDEV\SomaFrameQD\AGENTS.md`](../AGENTS.md):**
   * **Section 1 (`Current Baseline State vs. Planned .NET Framework 4.8 Roadmap`, lines 12–19):** Update `Current On-Disk State` to state that **Stage 1 (Simple Retargeting to `net48`) is complete** — all 10 solution projects plus `Caliburn5PackageHost` single-target `<TargetFramework>net48</TargetFramework>` under `.NET SDK 8.0.400` (`rollForward: "latestPatch"` in [`global.json`](../global.json)) with `<LangVersion>12.0</LangVersion>`, and **Stage 2 (`Plan 2.1` → `Plan 2.6`)** is ready to execute sequentially one plan at a time.
   * **Section 3 (`Build, Test, Pack & Validation Commands`, lines 44–62):** Update step 3 (`dotnet test`) and step 5 (`validate-artifacts.ps1`) to the exact post-Stage-1 commands:
     ```powershell
     # 1. Deterministic NuGet restore (ignores machine-wide feeds via src\NuGet.config)
     dotnet restore src\SomaFrameQD.sln --configfile src\NuGet.config

     # 2. Build entire solution in Release (net48)
     dotnet build src\SomaFrameQD.sln --configuration Release --no-restore

     # 3. Run MSTest characterization suite (146 STA tests on net48)
     dotnet test src\SomaFrameQD.Tests\SomaFrameQD.Tests.csproj --configuration Release --framework net48 --no-build --no-restore

     # 4. Pack all 8 NuGet libraries (.nupkg + .snupkg)
     dotnet pack src\SomaFrameQD.sln --configuration Release --no-build --no-restore --output build\packages

     # 5. Run declarative artifact, .nuspec, shader, lockfile & consumer smoke validation
     #    (Compatible with both Windows PowerShell 5.1 locally and pwsh 7 in CI)
     powershell -NoProfile -ExecutionPolicy Bypass -File .\eng\validate-artifacts.ps1
     ```

2. **In [`C:\SOFTDEV\SomaFrameQD\REPO_MAP.md`](../REPO_MAP.md):**
   * **Section 1 Directory Tree (`lines 24, 51–59`):** Update `global.json` comment (`Pins .NET SDK 8.0.400 LTS + MSTest.Sdk 4.1.0`), add `Implementation-Plans_net48-retargeting-ONLY/`, update `Caliburn5PackageHost` comment to `(net48)`, remove deleted `MahApps.Net10DragHost`, and update `src/SomaFrameQD.sln` comment from `11-project` to `10-project`.
   * **Section 2 Mermaid Graph & Project Matrix (`lines 98, 129–144`):** Remove the `DragHost` node from the Mermaid graph and update the Target Framework column across the remaining 11 `.csproj` rows from `net48;net10.0-windows` to `net48` (updating `Inspector` and `Demo` notes to reflect unconditional `System.Drawing.Common` `6.0.0` and `HelixToolkit.Wpf` `2.10.0` post-Stage-1).

---

### Step 4.4 — Execute Clean-Room Stage 1 Mandatory Completion Gate (`TODO.md` `1.4`) & Check Off Stage 1 in `TODO.md`

Before marking Stage 1 complete in [`TODO.md`](../TODO.md), execute the full **clean-room verification and forensic boundary audit** from `C:\SOFTDEV\SomaFrameQD`:

```powershell
# ==============================================================================
# PART A: Clean-Room Purge of All Build & Package Outputs
# ==============================================================================
if (Test-Path 'build\packages') { Remove-Item -Recurse -Force 'build\packages' }
Get-ChildItem -Path 'src','eng\compatibility\Caliburn5PackageHost' -Directory -Recurse -Include 'bin','obj' |
    Remove-Item -Recurse -Force

# ==============================================================================
# PART B: Full Stage 1 Build, Test, Pack & Artifact Contract Gate (net48 Only)
# ==============================================================================
dotnet restore src\SomaFrameQD.sln --configfile src\NuGet.config
if ($LASTEXITCODE -ne 0) { throw "GATE FAILURE: dotnet restore failed ($LASTEXITCODE)" }

dotnet build src\SomaFrameQD.sln --configuration Release --no-restore
if ($LASTEXITCODE -ne 0) { throw "GATE FAILURE: dotnet build failed ($LASTEXITCODE)" }

dotnet test src\SomaFrameQD.Tests\SomaFrameQD.Tests.csproj --configuration Release --framework net48 --no-build --no-restore
if ($LASTEXITCODE -ne 0) { throw "GATE FAILURE: SomaFrameQD.Tests failed ($LASTEXITCODE)" }

dotnet pack src\SomaFrameQD.sln --configuration Release --no-build --no-restore --output build\packages
if ($LASTEXITCODE -ne 0) { throw "GATE FAILURE: dotnet pack failed ($LASTEXITCODE)" }

powershell -NoProfile -ExecutionPolicy Bypass -File .\eng\validate-artifacts.ps1
if ($LASTEXITCODE -ne 0) { throw "GATE FAILURE: validate-artifacts.ps1 failed ($LASTEXITCODE)" }

# ==============================================================================
# PART C: Forensic Stage-Boundary & Immutability Audit
# ==============================================================================
# 1. Confirm all 11 remaining .csproj files single-target <TargetFramework>net48</TargetFramework>
$csprojFiles = Get-ChildItem -Path 'src','eng' -Recurse -Filter '*.csproj'
if ($csprojFiles.Count -ne 11) { throw "Expected 11 .csproj files after DragHost removal, found $($csprojFiles.Count)" }
foreach ($p in $csprojFiles) {
    $xml = [xml](Get-Content -LiteralPath $p.FullName -Raw)
    $tf = @($xml.Project.PropertyGroup | ForEach-Object { $_.TargetFramework } | Where-Object { $_ })
    $tfs = @($xml.Project.PropertyGroup | ForEach-Object { $_.TargetFrameworks } | Where-Object { $_ })
    if ($tf.Count -ne 1 -or $tf[0] -cne 'net48' -or $tfs.Count -ne 0) {
        throw "Invalid TFM configuration in $($p.FullName)"
    }
}

# 2. Verify AvalonDock501 & AvalonDock4741 binary fixture integrity (Invariant #7)
$ad501Hash = (Get-FileHash 'src\SomaFrameQD.Tests\TestData\AvalonDock501\ApplicationState.bin' -Algorithm SHA256).Hash
if ($ad501Hash -ne '74E90837D7111A2CCE107C4468606B210F59C05C6A4BC211665E64265D3DD096') {
    throw "AvalonDock501 ApplicationState.bin hash mismatch: $ad501Hash"
}

# 3. Verify ZERO Stage 2 Optimization leakage into Stage 1
Select-String -Path 'src\SomaFrameQD\SomaFrameQD.csproj' -Pattern 'PackageReference Include="System\.ComponentModel\.Composition" Version="6\.0\.0"' -Quiet |
    ForEach-Object { if (-not $_) { throw 'Stage 2 leak: System.ComponentModel.Composition 6.0.0 was prematurely removed in Stage 1!' } }
Select-String -Path 'src\SomaFrameQD.Modules.Inspector\SomaFrameQD.Modules.Inspector.csproj' -Pattern 'PackageReference Include="System\.Drawing\.Common" Version="6\.0\.0"' -Quiet |
    ForEach-Object { if (-not $_) { throw 'Stage 2 leak: System.Drawing.Common 6.0.0 was prematurely removed in Stage 1!' } }
Select-String -Path 'src\SomaFrameQD\app.config','src\SomaFrameQD.Demo\app.config' -Pattern 'Version=v4\.6\.1' |
    Measure-Object | ForEach-Object { if ($_.Count -ne 2) { throw 'Stage 2 leak: app.config supportedRuntime SKU was prematurely modified in Stage 1!' } }

Write-Host "STAGE 1 MANDATORY COMPLETION GATE PASSED 100%."
```

* **Only AFTER Part A, Part B, and Part C above succeed with `STAGE 1 MANDATORY COMPLETION GATE PASSED 100%`:**
  * Update [`C:\SOFTDEV\SomaFrameQD\TODO.md`](../TODO.md) lines `40`–`70` to mark `[x]` on **`1.1`**, **`1.2`**, **`1.3`**, and **`1.4`** (leaving all Stage 2 items `2.1`–`2.6` unchecked `[ ]`).

---

## HARD STOP AFTER PLAN #4 (Mandatory Phase Gate)

> [!IMPORTANT]
> **STOP & VERIFY BEFORE STAGE 2:**  
> Once `1.1`–`1.4` in [`TODO.md`](../TODO.md) are checked off and verified green:
> - **STOP EXECUTION.** Do **not** begin any Stage 2 Optimization work (`Plan 2.1` → `Plan 2.6`) in the same session.
> - Stage 2 Optimization plans belong in `C:\SOFTDEV\SomaFrameQD\Implementation-Plans_net48-optimizations\` and must be reviewed and executed strictly one plan per session starting with `Plan 2.1` (Native `net48` GAC References, `x64` Target Configuration & Binding Redirects).
