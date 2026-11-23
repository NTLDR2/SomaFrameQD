# SomaFrameQD — "BEFORE" .NET Framework 4.8 Conversion Lines of Code (LOC) Statistics

> **Purpose:** Baseline ("Before" Conversion) Lines of Code inventory prior to retargeting SomaFrameQD exclusively to **.NET Framework 4.8 (`net48`)** with **Windows 7 SP1 64-bit (`x64`) and higher** compatibility, sized for safe context-window management during refactoring.

## Report Metadata & Counting Methodology

| Property | Value |
| :--- | :--- |
| **Generated Timestamp (ISO-8601)** | `2026-11-22T17:23:51-05:00` |
| **Generated Local Time** | `2026-11-22 17:23:51 -05:00` |
| **Report Filename** | `BEFORE_NET48_LOC_Statistics_2026-11-22_17-23-51.md` |
| **Repository Root** | ``C:\SOFTDEV\SomaFrameQD`` |
| **Target Platform (Upcoming)** | ``.NET Framework 4.8 (net48)`` + ``Windows 7 SP1 x64+`` |
| **Total On-Disk Files Scanned** | 616 files (575 text files + 41 binary assets) |
| **Excluded Directories** | ``.git/``, ``.vs/``, ``bin/``, ``obj/``, ``artifacts/``, ``packages/``, ``TestResults/``, ``publish/``, ``LOCstatistics/`` |
| **Excluded Legacy Git-Deleted Paths** | All staged/unstaged deleted ``src/Gemini*`` and ``doc/gemini-*`` entries (on-disk files only are counted) |
| **Binary Assets Excluded from LOC** | ``.png`` (images), ``.bin`` (serialized test fixtures), ``.ps`` (compiled DirectX pixel shader ``GrayscaleEffect.ps``) |
| **Line Classification Identity** | ``Total Lines = Code Lines (SLOC) + Comment Lines + Blank Lines`` (inline code+comment lines count as Code) |

---

## 1. Executive Summary & Grand Totals

### Key Headline Metrics

- **Core Source & UI Code (`.cs` + `.xaml` + `.fx`):** **486 files**, **29,538 SLOC**, **1,937 comment lines**, **4,385 blank lines**, **35,860 total lines**.
  - **Handwritten C# (`*.cs` excluding `*.Designer.cs`):** 410 files / **24,409 SLOC** (29,663 total lines; includes 6 `AssemblyInfo.cs` files = 18 SLOC / 26 total lines).
  - **Designer-Generated C# (`*.Designer.cs`):** 17 files / **945 SLOC** (1,696 total lines).
- **All Repository Text Files (Categories A–D Combined):** **575 files**, **36,671 SLOC**, **4,168 comment lines**, **5,534 blank lines**, **46,373 total lines**.
- **Binary Assets (Not Line-Counted):** **41 files** (797.5 KB total).

### Breakdown by High-Level Category

| Category | Files | Code Lines (SLOC) | Comment Lines | Blank Lines | Total Lines | % of Total Lines |
| :--- | ---: | ---: | ---: | ---: | ---: | ---: |
| A1: Core C# Handwritten (*.cs) | 410 | 24,409 | 1,250 | 4,004 | 29,663 | 64.0% |
| A2: Core C# Designer-Generated (*.Designer.cs) | 17 | 945 | 547 | 204 | 1,696 | 3.7% |
| A3: XAML UI & Themes (*.xaml) | 57 | 4,168 | 140 | 175 | 4,483 | 9.7% |
| A4: HLSL Shader Source (*.fx) | 2 | 16 | 0 | 2 | 18 | 0.0% |
| **Subtotal — Category A: Core Source & UI Code** | **486** | **29,538** | **1,937** | **4,385** | **35,860** | **77.3%** |
| B: Build, Project, Packaging, CI & Config | 35 | 1,497 | 165 | 204 | 1,866 | 4.0% |
| C: Localization Resources & XML Fixtures | 44 | 4,337 | 2,066 | 473 | 6,876 | 14.8% |
| D: Documentation & License | 10 | 1,299 | 0 | 472 | 1,771 | 3.8% |
| **Grand Total (All Text Files, Categories A–D)** | **575** | **36,671** | **4,168** | **5,534** | **46,373** | **100.0%** |

---

## 2. Breakdown by File Extension / Language

### Text Files by Extension

| Extension | Category | Files | Code Lines (SLOC) | Comment Lines | Blank Lines | Total Lines | % of Total Lines |
| :--- | :--- | ---: | ---: | ---: | ---: | ---: | ---: |
| `.cs` | Core Code (C#) | 427 | 25,354 | 1,797 | 4,208 | 31,359 | 67.6% |
| `.resx` | Resources | 43 | 4,301 | 2,064 | 473 | 6,838 | 14.7% |
| `.xaml` | Core Code (XAML) | 57 | 4,168 | 140 | 175 | 4,483 | 9.7% |
| `.md` | Docs | 9 | 1,161 | 0 | 435 | 1,596 | 3.4% |
| `.csproj` | Build / Config | 12 | 413 | 0 | 61 | 474 | 1.0% |
| `.ps1` | Build / Config | 1 | 330 | 58 | 73 | 461 | 1.0% |
| `.gitignore` | Build / Config | 1 | 146 | 67 | 48 | 261 | 0.6% |
| `.json` | Build / Config | 5 | 220 | 14 | 0 | 234 | 0.5% |
| `.txt` | License | 1 | 138 | 0 | 37 | 175 | 0.4% |
| `.yml` | Build / Config | 2 | 81 | 13 | 17 | 111 | 0.2% |
| `.sln` | Build / Config | 1 | 101 | 0 | 1 | 102 | 0.2% |
| `.settings` | Build / Config | 5 | 59 | 0 | 0 | 59 | 0.1% |
| `.config` | Build / Config | 3 | 49 | 1 | 0 | 50 | 0.1% |
| `.editorconfig` | Build / Config | 1 | 45 | 0 | 2 | 47 | 0.1% |
| `.pubxml` | Build / Config | 2 | 31 | 12 | 0 | 43 | 0.1% |
| `.xml` | XML Test Data | 1 | 36 | 2 | 0 | 38 | 0.1% |
| `.fx` | Core Code (HLSL) | 2 | 16 | 0 | 2 | 18 | 0.0% |
| `.props` | Build / Config | 1 | 14 | 0 | 2 | 16 | 0.0% |
| `.code-workspace` | Build / Config | 1 | 8 | 0 | 0 | 8 | 0.0% |
| **Grand Total (Text)** | **All** | **575** | **36,671** | **4,168** | **5,534** | **46,373** | **100.0%** |

### Binary Assets Summary (Excluded from Text LOC)

| Extension | Asset Type | Files | Total Size (Bytes) | Total Size (KB) |
| :--- | :--- | ---: | ---: | ---: |
| `.png` | PNG icons, screenshots & UI assets | 38 | 808,608 | 789.66 KB |
| `.bin` | Binary AvalonDock serialized state fixtures | 2 | 7,780 | 7.60 KB |
| `.ps` | Compiled WPF DirectX pixel shader (GrayscaleEffect.ps) | 1 | 236 | 0.23 KB |
| **Total Binary Assets** | — | **41** | **816,624** | **797.48 KB** |

---

## 3. Breakdown by Solution Project & Top-Level Area

This table separates **Core Code (`.cs` + `.xaml` + `.fx`)** from **All Text Files** (which also includes `.resx` localization XML, `.csproj`, configs, scripts, and docs) so localization XML volume does not obscure true module code size.

| Project / Top-Level Area | Core Files | Core SLOC | Core Total Lines | All Text Files | All Text SLOC | All Comments | All Blanks | All Total Lines | % of Total Lines |
| :--- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| `src/SomaFrameQD` | 250 | 12,365 | 15,097 | 259 | 13,840 | 1,176 | 1,915 | 16,931 | 36.5% |
| `src/SomaFrameQD.Demo` | 64 | 3,368 | 3,778 | 76 | 3,933 | 379 | 406 | 4,718 | 10.2% |
| `src/SomaFrameQD.Modules.CodeCompiler` | 2 | 104 | 112 | 3 | 120 | 0 | 12 | 132 | 0.3% |
| `src/SomaFrameQD.Modules.CodeEditor` | 21 | 886 | 1,127 | 29 | 1,437 | 370 | 231 | 2,038 | 4.4% |
| `src/SomaFrameQD.Modules.ErrorList` | 19 | 669 | 841 | 27 | 1,227 | 359 | 172 | 1,758 | 3.8% |
| `src/SomaFrameQD.Modules.GraphEditor` | 23 | 1,548 | 2,400 | 26 | 1,645 | 526 | 389 | 2,560 | 5.5% |
| `src/SomaFrameQD.Modules.Inspector` | 63 | 2,536 | 3,124 | 70 | 3,180 | 519 | 430 | 4,129 | 8.9% |
| `src/SomaFrameQD.Modules.Output` | 11 | 259 | 334 | 18 | 713 | 323 | 111 | 1,147 | 2.5% |
| `src/SomaFrameQD.Modules.PropertyGrid` | 8 | 181 | 247 | 15 | 626 | 325 | 101 | 1,052 | 2.3% |
| `src/SomaFrameQD.Tests` | 23 | 7,333 | 8,483 | 27 | 7,421 | 38 | 1,125 | 8,584 | 18.5% |
| `src/ Root & IDE Config` | 0 | 0 | 0 | 5 | 198 | 15 | 5 | 218 | 0.5% |
| **Subtotal — `src/` Solution Tree** | **484** | **29,249** | **35,543** | **555** | **34,340** | **4,030** | **4,897** | **43,267** | **93.3%** |
| `eng/compatibility/Caliburn5PackageHost` | 1 | 163 | 178 | 3 | 197 | 0 | 22 | 219 | 0.5% |
| `eng/compatibility/MahApps.Net10DragHost` | 1 | 126 | 139 | 3 | 212 | 0 | 36 | 248 | 0.5% |
| `eng/ Packaging & Validation` | 0 | 0 | 0 | 2 | 467 | 58 | 73 | 598 | 1.3% |
| `.github/ CI & Workflows` | 0 | 0 | 0 | 2 | 81 | 13 | 17 | 111 | 0.2% |
| `doc/ Documentation` | 0 | 0 | 0 | 1 | 176 | 0 | 43 | 219 | 0.5% |
| Repository Root Files | 0 | 0 | 0 | 9 | 1,198 | 67 | 446 | 1,711 | 3.7% |
| **Grand Total (All Repository Text Files)** | **486** | **29,538** | **35,860** | **575** | **36,671** | **4,168** | **5,534** | **46,373** | **100.0%** |

---

## 4. Granular Subfolder & Module Breakdown

### 4.1 Core `src/SomaFrameQD` (Framework, Built-In Modules & Themes)

Note: `src/SomaFrameQD/Resources/` contains 30 binary `.png` files and 0 text files, and `src/SomaFrameQD/Framework/ShaderEffects/` also contains 1 compiled binary pixel shader (`GrayscaleEffect.ps`).

| Subfolder / Module | Core Files | Core SLOC | Core Total Lines | All Text Files | All Text SLOC | All Comments | All Blanks | All Total Lines |
| :--- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| `(Project Root)` | 1 | 160 | 236 | 3 | 243 | 37 | 44 | 324 |
| `Framework/(root)` | 16 | 625 | 772 | 16 | 625 | 24 | 123 | 772 |
| `Framework/Behaviors` | 3 | 246 | 320 | 3 | 246 | 21 | 53 | 320 |
| `Framework/Commands` | 19 | 589 | 701 | 19 | 589 | 12 | 100 | 701 |
| `Framework/Controls` | 8 | 664 | 1,027 | 8 | 664 | 195 | 168 | 1,027 |
| `Framework/Markup` | 1 | 11 | 13 | 1 | 11 | 0 | 2 | 13 |
| `Framework/Menus` | 10 | 174 | 222 | 10 | 174 | 10 | 38 | 222 |
| `Framework/Results` | 9 | 517 | 613 | 9 | 517 | 10 | 86 | 613 |
| `Framework/Services` | 14 | 328 | 423 | 14 | 328 | 37 | 58 | 423 |
| `Framework/ShaderEffects` | 3 | 56 | 64 | 3 | 56 | 0 | 8 | 64 |
| `Framework/Themes` | 7 | 243 | 374 | 7 | 243 | 89 | 42 | 374 |
| `Framework/Threading` | 1 | 8 | 9 | 1 | 8 | 0 | 1 | 9 |
| `Framework/ToolBars` | 7 | 161 | 183 | 7 | 161 | 0 | 22 | 183 |
| `Framework/Utils` | 1 | 43 | 50 | 1 | 43 | 1 | 6 | 50 |
| `Framework/Win32` | 1 | 151 | 203 | 1 | 151 | 5 | 47 | 203 |
| `Modules/MainMenu` | 21 | 778 | 928 | 21 | 778 | 6 | 144 | 928 |
| `Modules/MainWindow` | 3 | 127 | 139 | 3 | 127 | 0 | 12 | 139 |
| `Modules/Settings` | 11 | 386 | 443 | 11 | 386 | 3 | 54 | 443 |
| `Modules/Shell` | 33 | 2,240 | 2,668 | 33 | 2,240 | 64 | 364 | 2,668 |
| `Modules/StatusBar` | 6 | 172 | 196 | 6 | 172 | 3 | 21 | 196 |
| `Modules/ToolBars` | 20 | 512 | 607 | 20 | 512 | 22 | 73 | 607 |
| `Modules/Toolbox` | 14 | 360 | 413 | 14 | 360 | 4 | 49 | 413 |
| `Modules/UndoRedo` | 16 | 623 | 742 | 16 | 623 | 8 | 111 | 742 |
| `Themes/(root)` | 1 | 71 | 75 | 1 | 71 | 0 | 4 | 75 |
| `Themes/VS2013/(root)` | 3 | 579 | 735 | 3 | 579 | 78 | 78 | 735 |
| `Themes/VS2013/Controls` | 16 | 2,131 | 2,242 | 16 | 2,131 | 53 | 58 | 2,242 |
| `Themes/VS2013/Controls/Converters` | 1 | 37 | 44 | 1 | 37 | 0 | 7 | 44 |
| `Properties` | 4 | 373 | 655 | 11 | 1,765 | 494 | 142 | 2,401 |
| **Subtotal — `src/SomaFrameQD`** | **250** | **12,365** | **15,097** | **259** | **13,840** | **1,176** | **1,915** | **16,931** |

### 4.2 Standalone `src/SomaFrameQD.Modules.*` Projects

| Module Project & Subfolder | Core Files | Core SLOC | Core Total Lines | All Text Files | All Text SLOC | All Comments | All Blanks | All Total Lines |
| :--- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| `SomaFrameQD.Modules.CodeCompiler / (Project Root)` | 2 | 104 | 112 | 3 | 120 | 0 | 12 | 132 |
| **Subtotal — `SomaFrameQD.Modules.CodeCompiler`** | **2** | **104** | **112** | **3** | **120** | **0** | **12** | **132** |
| `SomaFrameQD.Modules.CodeEditor / (Project Root)` | 5 | 245 | 298 | 6 | 283 | 6 | 53 | 342 |
| `SomaFrameQD.Modules.CodeEditor / Commands` | 5 | 75 | 100 | 5 | 75 | 0 | 25 | 100 |
| `SomaFrameQD.Modules.CodeEditor / Controls` | 1 | 38 | 42 | 1 | 38 | 0 | 4 | 42 |
| `SomaFrameQD.Modules.CodeEditor / ViewModels` | 2 | 260 | 303 | 2 | 260 | 2 | 41 | 303 |
| `SomaFrameQD.Modules.CodeEditor / Views` | 5 | 102 | 115 | 5 | 102 | 3 | 10 | 115 |
| `SomaFrameQD.Modules.CodeEditor / Properties` | 3 | 166 | 269 | 10 | 679 | 359 | 98 | 1,136 |
| **Subtotal — `SomaFrameQD.Modules.CodeEditor`** | **21** | **886** | **1,127** | **29** | **1,437** | **370** | **231** | **2,038** |
| `SomaFrameQD.Modules.ErrorList / (Project Root)` | 5 | 150 | 172 | 6 | 185 | 0 | 27 | 212 |
| `SomaFrameQD.Modules.ErrorList / Commands` | 6 | 194 | 229 | 6 | 194 | 0 | 35 | 229 |
| `SomaFrameQD.Modules.ErrorList / Converters` | 1 | 31 | 34 | 1 | 31 | 0 | 3 | 34 |
| `SomaFrameQD.Modules.ErrorList / Design` | 1 | 34 | 35 | 1 | 34 | 0 | 1 | 35 |
| `SomaFrameQD.Modules.ErrorList / ViewModels` | 1 | 92 | 103 | 1 | 92 | 0 | 11 | 103 |
| `SomaFrameQD.Modules.ErrorList / Views` | 2 | 62 | 68 | 2 | 62 | 3 | 3 | 68 |
| `SomaFrameQD.Modules.ErrorList / Properties` | 3 | 106 | 200 | 10 | 629 | 356 | 92 | 1,077 |
| **Subtotal — `SomaFrameQD.Modules.ErrorList`** | **19** | **669** | **841** | **27** | **1,227** | **359** | **172** | **1,758** |
| `SomaFrameQD.Modules.GraphEditor / (Project Root)` | 1 | 9 | 11 | 2 | 41 | 0 | 6 | 47 |
| `SomaFrameQD.Modules.GraphEditor / Controls` | 17 | 1,381 | 2,168 | 17 | 1,381 | 439 | 348 | 2,168 |
| `SomaFrameQD.Modules.GraphEditor / Converters` | 1 | 18 | 20 | 1 | 18 | 0 | 2 | 20 |
| `SomaFrameQD.Modules.GraphEditor / Themes` | 1 | 94 | 109 | 1 | 94 | 7 | 8 | 109 |
| `SomaFrameQD.Modules.GraphEditor / Properties` | 3 | 46 | 92 | 5 | 111 | 80 | 25 | 216 |
| **Subtotal — `SomaFrameQD.Modules.GraphEditor`** | **23** | **1,548** | **2,400** | **26** | **1,645** | **526** | **389** | **2,560** |
| `SomaFrameQD.Modules.Inspector / (Project Root)` | 7 | 199 | 230 | 8 | 239 | 2 | 36 | 277 |
| `SomaFrameQD.Modules.Inspector / Commands` | 2 | 43 | 50 | 2 | 43 | 0 | 7 | 50 |
| `SomaFrameQD.Modules.Inspector / Controls` | 5 | 873 | 1,150 | 5 | 873 | 129 | 148 | 1,150 |
| `SomaFrameQD.Modules.Inspector / Conventions` | 5 | 122 | 141 | 5 | 122 | 0 | 19 | 141 |
| `SomaFrameQD.Modules.Inspector / Inspectors` | 33 | 917 | 1,055 | 33 | 917 | 32 | 106 | 1,055 |
| `SomaFrameQD.Modules.Inspector / Resources` | 1 | 50 | 51 | 1 | 50 | 0 | 1 | 51 |
| `SomaFrameQD.Modules.Inspector / Themes` | 1 | 49 | 51 | 1 | 49 | 0 | 2 | 51 |
| `SomaFrameQD.Modules.Inspector / Util` | 2 | 49 | 57 | 2 | 49 | 0 | 8 | 57 |
| `SomaFrameQD.Modules.Inspector / ViewModels` | 1 | 66 | 76 | 1 | 66 | 0 | 10 | 76 |
| `SomaFrameQD.Modules.Inspector / Views` | 2 | 44 | 48 | 2 | 44 | 3 | 1 | 48 |
| `SomaFrameQD.Modules.Inspector / Win32` | 1 | 18 | 20 | 1 | 18 | 0 | 2 | 20 |
| `SomaFrameQD.Modules.Inspector / Properties` | 3 | 106 | 195 | 9 | 710 | 353 | 90 | 1,153 |
| **Subtotal — `SomaFrameQD.Modules.Inspector`** | **63** | **2,536** | **3,124** | **70** | **3,180** | **519** | **430** | **4,129** |
| `SomaFrameQD.Modules.Output / (Project Root)` | 2 | 24 | 26 | 3 | 58 | 0 | 7 | 65 |
| `SomaFrameQD.Modules.Output / Commands` | 2 | 43 | 50 | 2 | 43 | 0 | 7 | 50 |
| `SomaFrameQD.Modules.Output / ViewModels` | 2 | 88 | 103 | 2 | 88 | 0 | 15 | 103 |
| `SomaFrameQD.Modules.Output / Views` | 3 | 57 | 65 | 3 | 57 | 3 | 5 | 65 |
| `SomaFrameQD.Modules.Output / Properties` | 2 | 47 | 90 | 8 | 467 | 320 | 77 | 864 |
| **Subtotal — `SomaFrameQD.Modules.Output`** | **11** | **259** | **334** | **18** | **713** | **323** | **111** | **1,147** |
| `SomaFrameQD.Modules.PropertyGrid / (Project Root)` | 2 | 20 | 22 | 3 | 48 | 0 | 8 | 56 |
| `SomaFrameQD.Modules.PropertyGrid / Commands` | 2 | 43 | 50 | 2 | 43 | 0 | 7 | 50 |
| `SomaFrameQD.Modules.PropertyGrid / ViewModels` | 1 | 47 | 54 | 1 | 47 | 0 | 7 | 54 |
| `SomaFrameQD.Modules.PropertyGrid / Views` | 2 | 24 | 31 | 2 | 24 | 5 | 2 | 31 |
| `SomaFrameQD.Modules.PropertyGrid / Properties` | 1 | 47 | 90 | 7 | 464 | 320 | 77 | 861 |
| **Subtotal — `SomaFrameQD.Modules.PropertyGrid`** | **8** | **181** | **247** | **15** | **626** | **325** | **101** | **1,052** |
| **Total — All 7 Standalone Modules** | **147** | **6,183** | **8,185** | **188** | **8,948** | **2,422** | **1,446** | **12,816** |

### 4.3 `src/SomaFrameQD.Demo` Application Modules

Note: `src/SomaFrameQD.Demo/Resources/` and `Modules/FilterDesigner/Resources/` also include binary `.png` assets (not in text LOC).

| Subfolder / Module | Core Files | Core SLOC | Core Total Lines | All Text Files | All Text SLOC | All Comments | All Blanks | All Total Lines |
| :--- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| `(Project Root)` | 3 | 46 | 58 | 6 | 154 | 9 | 12 | 175 |
| `Modules/FilterDesigner` | 30 | 2,285 | 2,496 | 30 | 2,285 | 11 | 200 | 2,496 |
| `Modules/Home` | 13 | 540 | 612 | 13 | 540 | 14 | 58 | 612 |
| `Modules/SampleBrowser` | 5 | 110 | 118 | 5 | 110 | 0 | 8 | 118 |
| `Modules/Shell` | 4 | 151 | 173 | 4 | 151 | 3 | 19 | 173 |
| `Modules/Startup` | 1 | 47 | 57 | 1 | 47 | 1 | 9 | 57 |
| `Modules/TextEditor` | 4 | 117 | 133 | 4 | 117 | 0 | 16 | 133 |
| `Properties` | 4 | 72 | 131 | 11 | 498 | 329 | 84 | 911 |
| `Properties/PublishProfiles` | 0 | 0 | 0 | 2 | 31 | 12 | 0 | 43 |
| **Subtotal — `src/SomaFrameQD.Demo`** | **64** | **3,368** | **3,778** | **76** | **3,933** | **379** | **406** | **4,718** |

### 4.4 `src/SomaFrameQD.Tests` Test Suite

Note: `src/SomaFrameQD.Tests/TestData/` contains 1 XML fixture (`CompleteLayout.xml`) and 2 binary `.bin` fixtures (`ApplicationState.bin` under `AvalonDock4741` and `AvalonDock501`).

| Subfolder / Module | Core Files | Core SLOC | Core Total Lines | All Text Files | All Text SLOC | All Comments | All Blanks | All Total Lines |
| :--- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| `(Project Root)` | 0 | 0 | 0 | 1 | 29 | 0 | 4 | 33 |
| `Composition` | 1 | 80 | 94 | 1 | 80 | 0 | 14 | 94 |
| `Framework` | 9 | 1,944 | 2,239 | 9 | 1,944 | 0 | 295 | 2,239 |
| `Modules` | 11 | 5,255 | 6,089 | 11 | 5,255 | 36 | 798 | 6,089 |
| `TestData` | 0 | 0 | 0 | 3 | 59 | 2 | 7 | 68 |
| `TestInfrastructure` | 2 | 54 | 61 | 2 | 54 | 0 | 7 | 61 |
| **Subtotal — `src/SomaFrameQD.Tests`** | **23** | **7,333** | **8,483** | **27** | **7,421** | **38** | **1,125** | **8,584** |

---

## 5. Top 25 Largest Files & Context-Window Planning Guide for `.NET 4.8` Retargeting

### Top 25 Largest Code, Build, Script & Doc Files (Excluding `.resx` Boilerplate)

| Rank | File Path | Category | Code Lines (SLOC) | Comment Lines | Blank Lines | Total Lines |
| ---: | :--- | :--- | ---: | ---: | ---: | ---: |
| 1 | `src/SomaFrameQD.Tests/Modules/Shell/AvalonDockPersistenceTests.cs` | Core C# | 1,021 | 14 | 125 | 1,160 |
| 2 | `src/SomaFrameQD.Tests/Modules/Shell/ShellViewModelTests.cs` | Core C# | 950 | 0 | 172 | 1,122 |
| 3 | `src/SomaFrameQD.Demo/Modules/FilterDesigner/Design/DesignTimeImages.cs` | Core C# | 1,015 | 0 | 1 | 1,016 |
| 4 | `README.md` | Docs | 694 | 0 | 284 | 978 |
| 5 | `src/SomaFrameQD.Modules.GraphEditor/Controls/ZoomAndPanControl.cs` | Core C# | 509 | 306 | 144 | 959 |
| 6 | `src/SomaFrameQD.Tests/Modules/Shell/LayoutItemStatePersisterTests.cs` | Core C# | 718 | 1 | 117 | 836 |
| 7 | `src/SomaFrameQD.Modules.Inspector/Controls/SimpleGridSplitter.cs` | Core C# | 592 | 129 | 98 | 819 |
| 8 | `src/SomaFrameQD/Framework/Controls/HwndWrapper.cs` | Core C# | 393 | 116 | 106 | 615 |
| 9 | `src/SomaFrameQD/Properties/Resources.Designer.cs` | Designer C# | 322 | 197 | 66 | 585 |
| 10 | `src/SomaFrameQD.Tests/Modules/Shell/EditorOpeningServiceTests.cs` | Core C# | 484 | 0 | 98 | 582 |
| 11 | `src/SomaFrameQD.Tests/Modules/Shell/AvalonDockThemeTests.cs` | Core C# | 478 | 5 | 40 | 523 |
| 12 | `src/SomaFrameQD/Modules/Shell/ViewModels/ShellViewModel.cs` | Core C# | 384 | 11 | 82 | 477 |
| 13 | `src/SomaFrameQD.Tests/Modules/Shell/AvalonDockDockingTests.cs` | Core C# | 394 | 16 | 66 | 476 |
| 14 | `src/SomaFrameQD/Modules/Shell/Services/LayoutItemStatePersister.cs` | Core C# | 416 | 4 | 55 | 475 |
| 15 | `eng/validate-artifacts.ps1` | Build/Script | 330 | 58 | 73 | 461 |
| 16 | `src/SomaFrameQD.Tests/Modules/Toolkit/ExtendedWpfToolkitCompatibilityTests.cs` | Core C# | 407 | 0 | 52 | 459 |
| 17 | `src/SomaFrameQD.Tests/Framework/Results/ResultCompletionTests.cs` | Core C# | 381 | 0 | 51 | 432 |
| 18 | `src/SomaFrameQD/Themes/VS2013/Controls/Menu.xaml` | XAML | 352 | 20 | 15 | 387 |
| 19 | `src/SomaFrameQD/Themes/VS2013/Controls/Toolbar.xaml` | XAML | 328 | 12 | 21 | 361 |
| 20 | `src/SomaFrameQD.Tests/Framework/Results/ResultTestDoubles.cs` | Core C# | 264 | 0 | 61 | 325 |
| 21 | `src/SomaFrameQD/Themes/VS2013/Controls/ScrollBar.xaml` | XAML | 288 | 0 | 1 | 289 |
| 22 | `src/SomaFrameQD.Modules.GraphEditor/Controls/ZoomAndPanControl_IScrollInfo.cs` | Core C# | 149 | 105 | 33 | 287 |
| 23 | `src/SomaFrameQD.Tests/Modules/CodeEditor/CodeEditorSearchPanelTests.cs` | Core C# | 240 | 0 | 34 | 274 |
| 24 | `src/SomaFrameQD.Tests/Framework/Results/ShowDialogResultCompletionTests.cs` | Core C# | 238 | 0 | 29 | 267 |
| 25 | `src/SomaFrameQD/Modules/Shell/Services/EditorOpeningService.cs` | Core C# | 216 | 4 | 42 | 262 |

> **Localization .resx Note:** There are 43 .resx files totaling 6,838 lines (4,301 XML SLOC). The largest .resx files are: `src/SomaFrameQD/Properties/Resources.fr.resx` (294 lines), `src/SomaFrameQD/Properties/Resources.resx` (294 lines), `src/SomaFrameQD/Properties/Resources.ru.resx` (288 lines), `src/SomaFrameQD/Properties/Resources.zh-Hans.resx` (285 lines), `src/SomaFrameQD/Properties/Resources.ko.resx` (285 lines). These typically do not require manual context-window reads during `.NET 4.8` retargeting unless adding/removing resource keys.

### Recommended Context-Window Batches for `.NET Framework 4.8` + Windows 7 x64 Conversion

Excluding `.resx` localization files keeps the entire active engineering surface to **532 files / 39,535 total lines** (and Core Code is **35,860 total lines**). Splitting the conversion into the 5 focused batches below prevents context-window exhaustion:

| Batch | Scope & Planned `.NET 4.8` Work | Files (excl. `.resx`) | Code Lines (SLOC) | Total Lines |
| :--- | :--- | ---: | ---: | ---: |
| **Batch 1 — SDK, Projects & Compatibility Shims** | `global.json`, `src/Directory.Build.props`, all 12 `.csproj` files, `SomaFrameQD.sln`, `src/.vscode/launch.json`, `eng/compatibility/*` | 22 | 977 | 1,108 |
| **Batch 2 — Core `SomaFrameQD` Shell & Framework** | `AppBootstrapper`, `app.config`, `Framework/*` (incl. `Win32`, `Threading`, `Services`), built-in `Modules/*`, `Themes/*` | 252 | 12,404 | 15,136 |
| **Batch 3 — Standalone `SomaFrameQD.Modules.*`** | 7 module projects (`CodeCompiler`, `CodeEditor`, `ErrorList`, `GraphEditor`, `Inspector`, `Output`, `PropertyGrid`) | 150 | 6,218 | 8,220 |
| **Batch 4 — `SomaFrameQD.Demo` & Publish Profiles** | `DemoAppBootstrapper`, `app.config`, `PublishProfiles/*`, `FilterDesigner`, `TextEditor`, sample modules | 69 | 3,445 | 3,867 |
| **Batch 5 — Tests, Packaging, CI & Docs** | `SomaFrameQD.Tests`, `eng/validate-artifacts.ps1`, `eng/package-contract.json`, `.github/workflows/ci.yml`, `README.md` / docs | 35 | 8,989 | 10,713 |

> **Context-Budget Observation:** Even the two largest single-project batches — **Batch 2 (`SomaFrameQD` Core)** at **15,136 lines** and **Batch 3 (all 7 standalone modules)** at **8,220 lines** — should be further narrowed during actual editing by inspecting only `.NET 10` / `#if NETFRAMEWORK` / `RuntimeInformation` / `PublishSingleFile` / Win32 / WPF target-specific files via targeted search rather than reading all XAML themes and controls wholesale.

---

*End of report — Generated at `2026-11-22T17:23:51-05:00`.*
