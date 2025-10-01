# XrmUnitTest - Copilot Instructions

## Overview

Unit testing framework for Microsoft Dynamics CRM/XRM and Dataverse with in-memory fake CRM server. ~324 MB, ~3,262 C# files, 14 projects. Multi-targeting: .NET Framework 4.6.2/4.7.2/4.8, .NET 6.0/8.0. Uses MSTest, dotnet CLI, AppVeyor CI.

## Build & Test (CRITICAL: Use `-p:EnableWindowsTargeting=true` on Linux/macOS)

### Essential Commands
```bash
# Restore (20-25s)
dotnet restore XrmUnitTest.sln -p:EnableWindowsTargeting=true

# Build (30-35s, expects ~531 nullability warnings)
dotnet build XrmUnitTest.sln -p:EnableWindowsTargeting=true --configuration Debug

# Test (IdGeneratorTests will fail on Linux - expected, requires Windows Desktop)
dotnet test XrmUnitTest.sln -p:EnableWindowsTargeting=true --configuration Debug --no-build

# Clean (2s)
dotnet clean XrmUnitTest.sln -p:EnableWindowsTargeting=true
```

### Critical Requirements
- **Test projects MUST have App.config** or tests fail with: "Unit Test Project Must Contain an App.Config file..."
- **Config files** (DO NOT commit credentials): `UnitTestSettings.config`, `UnitTestSettings.user.config` (gitignored), `ExampleSecrets.json` (Dataverse)
- **Known issues**: NU1701 warnings on net6.0 (ignore), IdGeneratorTests fail on Linux (expected)

## Project Structure

### Key Directories
- `2013/`, `2015/`, `2016/`, `09/` - CRM version-specific projects (XrmUnitTest.YYYY)
- `Dataverse/` - Modern Dataverse projects (net8.0)
- `DLaB.Xrm.*.Base/` - Shared projects (.shproj) for code reuse across frameworks
  - `Test.Base` - Core testing framework (TestBase.cs, AssertCrm.cs, FakeIOrganizationService.cs)
  - `LocalCrm.Base` - In-memory fake CRM implementation
  - `Client.Base` - CRM client operations
  - `NMemory.Base` - In-memory database engine
- `DLaB.Xrm.Entities/` - Auto-generated entity classes (net462)
- `DLaB.Xrm.Entities.Net/` - Auto-generated entity classes (net6.0)
- `IdGenerator/` - Windows-only ID generator tool (net9.0)
- `Code Snippets/` - VS code snippets (.snippet files)

### Solution Structure
- **XrmUnitTest.sln** - Main solution (14 projects, 205 lines)
- **Shared projects** (.shproj) import code into multiple target frameworks via `<Import Project="..."/>`
- **Multi-targeting** - Most projects target multiple frameworks (e.g., net462;net472;net48)

### Core Components
- **DLaB.Xrm.Test.Base/** - Main test framework with TestBase, AssertCrm, Assumptions/, Builders/, Settings/
- **Test projects** - Named `*.Tests`, use MSTest (MSTest.TestAdapter, MSTest.TestFramework)
- **Config files** - Assembly versions in `Properties/AssemblyInfo.cs`, NuGet in `*.nuspec`

## Making Changes

### Before Changes
1. Restore: `dotnet restore XrmUnitTest.sln -p:EnableWindowsTargeting=true`
2. Build baseline: `dotnet build XrmUnitTest.sln -p:EnableWindowsTargeting=true --configuration Debug`
3. Note existing test state

### After Changes
1. Build incrementally and test specific projects when possible
2. Verify no new errors (compare to ~531 pre-existing warnings)
3. Ensure App.config exists if adding test projects

### Adding Test Projects
1. Include App.config (copy from existing test project)
2. Reference MSTest packages + XrmUnitTest.MSTest/DataverseUnitTest.MSTest
3. Consider multi-targeting for multiple .NET versions

### Version Updates
- Assembly versions: `Properties/AssemblyInfo.cs` (e.g., 2.4.0.19 for 2013/2015/2016, 3.4.0.19 for 09)
- NuGet versions: `*.nuspec` files
- Keep versions synchronized across related projects

## CI/CD
- **AppVeyor**: Primary CI (configured in UI, badge in README)
- **GitHub Actions**: `.github/workflows/IdGeneratorRelease.yml` for IdGenerator only (windows-latest, .NET 9.0.x)

## Key Dependencies
- Microsoft.CrmSdk.CoreAssemblies, DLaB.Xrm 5.1.0.11, System.Text.Json 9.0.0
- Test: MSTest 3.6.4-3.10.2, coverlet.collector

## Trust These Instructions
Commands validated through execution. If issues arise: (1) Verify exact command syntax including `-p:EnableWindowsTargeting=true`, (2) Check .NET SDK 9.0.305+, (3) Verify App.config in test projects, (4) Only then explore further.
