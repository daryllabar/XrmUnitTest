# XrmUnitTest Repository - Copilot Coding Agent Instructions

## Repository Summary

**XrmUnitTest** is a comprehensive unit testing framework for Microsoft Dynamics CRM/XRM and Dataverse (formerly CDS). It provides an in-memory fake CRM/Dataverse server for testing plugins and workflows without deploying to an actual CRM instance.

### Technology Stack
- **Languages**: C# (~3,262 files)
- **Size**: ~324 MB
- **Frameworks**: .NET Framework (4.6.2, 4.7.2, 4.8), .NET 6.0, .NET 8.0
- **Project Types**: Multi-targeting SDK-style and shared projects (.shproj)
- **Test Framework**: MSTest
- **Build Tool**: dotnet CLI
- **CI/CD**: AppVeyor (primary) + GitHub Actions (IdGenerator only)

### Supported CRM/Dataverse Versions
- **2013**: XrmUnitTest.2013 (net462)
- **2015**: XrmUnitTest.2015 (net462)
- **2016**: XrmUnitTest.2016 (net462)
- **09** (v9.x): XrmUnitTest.09 (net462, net472, net48)
- **Dataverse**: DataverseUnitTest (net8.0)

## Build & Test Instructions

### Prerequisites
- .NET SDK 9.0+ (currently using 9.0.305)
- Important: Building on Linux/macOS requires the `EnableWindowsTargeting=true` property

### Build Commands

**ALWAYS use `-p:EnableWindowsTargeting=true` when building on non-Windows systems.**

#### Restore Dependencies
```bash
dotnet restore XrmUnitTest.sln -p:EnableWindowsTargeting=true
```
- **Time**: ~20-25 seconds
- **Known Warnings**: 
  - NU1701 for Microsoft.PowerPlatform.Dataverse.Client on net6.0 (expected, can be ignored)
  - System.Text.Json, System.IO.Pipelines, Microsoft.Bcl.AsyncInterfaces targeting warnings on net6.0 (expected)

#### Build Solution
```bash
# Debug build
dotnet build XrmUnitTest.sln -p:EnableWindowsTargeting=true --configuration Debug

# Release build  
dotnet build XrmUnitTest.sln -p:EnableWindowsTargeting=true --configuration Release
```
- **Time**: ~30-35 seconds
- **Expected Output**: ~531 warnings (mostly nullability warnings), 0 errors
- **Note**: Warnings are pre-existing and not a sign of build failure

#### Clean Solution
```bash
dotnet clean XrmUnitTest.sln -p:EnableWindowsTargeting=true
```
- **Time**: ~2 seconds

### Running Tests

#### Run All Tests
```bash
dotnet test XrmUnitTest.sln -p:EnableWindowsTargeting=true --configuration Debug --no-build
```
- **Time**: Variable (depends on test count and configuration)
- **Expected Failures on Linux**:
  - IdGeneratorTests will fail (requires Microsoft.WindowsDesktop.App which is Windows-only)
  - Main XrmUnitTest tests should pass if properly configured

#### Run Tests for Specific Project
```bash
# Dataverse tests
dotnet test Dataverse/DataverseUnitTest.Tests/DataverseUnitTest.Tests.csproj -p:EnableWindowsTargeting=true --no-build

# XrmUnitTest 09 tests
dotnet test 09/XrmUnitTest.09.Tests/XrmUnitTest.09.Tests.csproj -p:EnableWindowsTargeting=true --no-build
```

### Test Configuration Requirements

**CRITICAL**: Unit test projects MUST have an `App.config` file. Tests will fail with an error if App.config is missing:
```
"Unit Test Project Must Contain an App.Config file to be able to Load User Settings into!"
```

**Test Configuration Files** (do NOT commit with real credentials):
- `UnitTestSettings.config` - Legacy configuration file (XML-based)
- `UnitTestSettings.user.config` - User-specific overrides (ignored by .gitignore)
- `ExampleSecrets.json` - Example secrets file for Dataverse projects (see `Dataverse/DataverseUnitTest.Tests/ExampleSecrets.json`)

### Common Build Issues & Workarounds

**Issue 1**: Build fails with "NETSDK1100: To build a project targeting Windows..."
- **Solution**: Always add `-p:EnableWindowsTargeting=true` to all dotnet commands

**Issue 2**: IdGeneratorTests fail on Linux
- **Expected**: IdGenerator targets net8.0-windows and requires Windows Desktop App
- **Workaround**: Skip IdGenerator tests on Linux, or exclude from test runs

**Issue 3**: Test failures about missing App.config
- **Solution**: Ensure test projects have an App.config file (can copy from UnitTestSettings.config)

**Issue 4**: Assumption entity not found errors in tests
- **Cause**: Tests expect entity JSON files in `Assumptions/Entity Json/` directory
- **Solution**: Review test setup and ensure required assumption files exist at expected paths

## Project Layout & Architecture

### Root Directory Structure
```
/
├── .github/workflows/          # GitHub Actions (IdGeneratorRelease.yml only)
├── 2013/                       # CRM 2013 version projects
├── 2015/                       # CRM 2015 version projects
├── 2016/                       # CRM 2016 version projects
├── 09/                         # CRM v9.x (Dynamics 365) version projects
│   ├── XrmUnitTest.09/         # Main v9 library
│   └── XrmUnitTest.09.Tests/   # Tests for v9
├── Code Snippets/              # Visual Studio code snippets (.snippet files)
├── Dataverse/                  # Dataverse (modern) version projects
│   ├── DataverseUnitTest/      # Main Dataverse library (net8.0)
│   └── DataverseUnitTest.Tests/ # Tests for Dataverse
├── DLaB.Xrm.Client.Base/       # Shared project - CRM client code
├── DLaB.Xrm.Entities/          # Early-bound entity classes (net462)
├── DLaB.Xrm.Entities.Net/      # Early-bound entity classes (net6.0)
├── DLaB.Xrm.LocalCrm.Base/     # Shared project - In-memory fake CRM implementation
├── DLaB.Xrm.LocalCrm.Tests/    # Tests for LocalCrm
├── DLaB.Xrm.LocalCrm.Tests.Base/ # Shared test code
├── DLaB.Xrm.Test.Base/         # Shared project - Core testing framework
├── DLaB.Xrm.Test.Tests/        # Tests for test framework
├── DLaB.Xrm.Test.Tests.Base/   # Shared test code
├── DLaB.Xrm.Tests.Base/        # Additional shared test utilities
├── IdGenerator/                # Standalone ID generator tool (net9.0, Windows-only)
├── IdGeneratorTests/           # Tests for IdGenerator (net8.0-windows)
├── NMemory.Base/               # Shared project - In-memory database
├── References/                 # Contains NuGetContentInstaller.exe
├── XrmUnitTest.Test.Base/      # Shared project - Additional test utilities
├── XrmUnitTest.sln             # Main solution file (205 lines, 14 projects)
└── README.md                   # Project documentation
```

### Key Configuration Files
- **Solution**: `XrmUnitTest.sln` - Multi-project solution targeting multiple frameworks
- **NuGet**: `*.nuspec` files in version-specific folders (2013, 2015, 2016, 09, Dataverse)
- **Assembly Versions**: Defined in `Properties/AssemblyInfo.cs` files
  - Current versions: 2.4.0.19 (2013/2015/2016), 3.4.0.19 (09), varies for others
- **Strong Naming**: Projects use `.snk` key files (e.g., `XrmUnitTest.09.Snk`)

### Shared Projects (.shproj)
The solution heavily uses shared projects to share code across multiple target frameworks:
- `DLaB.Xrm.Client.Base` - Client-side CRM operations
- `DLaB.Xrm.LocalCrm.Base` - Fake/in-memory CRM database
- `DLaB.Xrm.Test.Base` - Core test infrastructure
- `NMemory.Base` - In-memory database engine
- `DLaB.Xrm.LocalCrm.Tests.Base` - Shared test code for LocalCrm
- `DLaB.Xrm.Test.Tests.Base` - Shared test code for test framework
- `DLaB.Xrm.Tests.Base` - Additional shared utilities
- `XrmUnitTest.Test.Base` - Additional test utilities

### Core Components

**DLaB.Xrm.Test.Base** (Main Testing Framework):
- `TestBase.cs` - Base class for unit tests
- `AssertCrm.cs` - CRM-specific assertions
- `FakeIOrganizationService.cs` - Fake service implementation
- `FakeExecutionContext.cs` - Fake plugin context
- `Assumptions/` - Entity data assumption system
- `Builders/` - Builder pattern for test data
- `Settings/` - Configuration and settings management

**DLaB.Xrm.LocalCrm.Base** (In-Memory CRM):
- Implements a complete in-memory CRM database for unit testing
- No actual CRM deployment needed for plugin testing
- Located in `DLaB.Xrm.LocalCrm.Base/`

**Test Projects**:
- Follow naming: `*.Tests` or `*Tests`
- Use MSTest framework (MSTest.TestAdapter, MSTest.TestFramework)
- Additional packages: XrmUnitTest.MSTest or DataverseUnitTest.MSTest

### CI/CD Pipelines

**AppVeyor** (Primary CI):
- Badge in README: `https://ci.appveyor.com/api/projects/status/e4x424jxt92vk00a?svg=true`
- No appveyor.yml in repository (likely configured in AppVeyor UI)
- Runs on all branches

**GitHub Actions**:
- `.github/workflows/IdGeneratorRelease.yml` - Builds and releases IdGenerator only
- Triggers: Push to `main` branch affecting `IdGenerator/**` or manual workflow_dispatch
- Runs on: `windows-latest`
- Actions:
  1. Checkout code
  2. Setup .NET 9.0.x
  3. Build IdGenerator in Release
  4. Publish to `./publish`
  5. Zip output
  6. Create GitHub release with zip artifact

### Dependencies & Package References

**Common Dependencies Across Projects**:
- Microsoft.CrmSdk.CoreAssemblies (version varies by CRM version)
- Microsoft.CrmSdk.XrmTooling.CoreAssembly
- DLaB.Xrm (version 5.1.0.11)
- DLaB.Common.Source
- System.Text.Json (9.0.0)
- Microsoft.PowerPlatform.Dataverse.Client (Dataverse projects)

**Test-Specific Dependencies**:
- MSTest (3.6.4 - 3.10.2)
- Microsoft.NET.Test.Sdk
- coverlet.collector (code coverage)
- XrmUnitTest.MSTest or DataverseUnitTest.MSTest

### File Organization Patterns

**Entities** (Auto-generated):
- `DLaB.Xrm.Entities/` - Contains auto-generated entity classes
- Files have `<auto-generated>` headers
- Generated by CrmSvcUtil (version 9.0.0.9369)

**Code Snippets**:
- `Code Snippets/crmplugin.snippet` - Plugin class template
- `Code Snippets/crmplugintest.snippet` - Plugin test template
- `Code Snippets/crmtestmethodclass.snippet` - Test method class template
- `Code Snippets/region.snippet` - Region snippet

**NuGet Packages**:
- Built using Release configuration
- Defined in `*.nuspec` files
- Target multiple frameworks in single package

## Making Changes

### Before Making Code Changes
1. **Always restore first**: `dotnet restore XrmUnitTest.sln -p:EnableWindowsTargeting=true`
2. **Build to verify baseline**: `dotnet build XrmUnitTest.sln -p:EnableWindowsTargeting=true --configuration Debug`
3. **Check existing test state**: Ensure you understand which tests pass/fail before changes

### After Making Code Changes
1. **Build incrementally**: Test your changes in isolation when possible
2. **Run affected tests**: Target specific test projects rather than entire solution
3. **Verify no new errors**: Compare error/warning counts before and after
4. **Check test configuration**: Ensure App.config exists if adding new test projects

### Version Updates
- Assembly versions are in `Properties/AssemblyInfo.cs` files
- NuGet package versions are in `*.nuspec` files  
- Keep versions synchronized across related projects

### Adding New Test Projects
1. Must include `App.config` file (copy from existing test project)
2. Reference MSTest packages (MSTest.TestAdapter, MSTest.TestFramework, Microsoft.NET.Test.Sdk)
3. Add XrmUnitTest.MSTest or DataverseUnitTest.MSTest package reference
4. Consider multi-targeting if supporting multiple .NET versions

## Trust These Instructions

These instructions were created by thoroughly exploring the repository, validating all build and test commands, and documenting actual behavior. If information seems incomplete or incorrect:

1. First, verify you're using the documented commands exactly as written (especially `-p:EnableWindowsTargeting=true`)
2. Check that your .NET SDK version is 9.0.305 or compatible
3. Verify test projects have App.config files
4. Only then should you explore further - the information above is accurate as of repository exploration

## Quick Reference

**Most Common Commands**:
```bash
# Full build from clean state
dotnet restore XrmUnitTest.sln -p:EnableWindowsTargeting=true
dotnet build XrmUnitTest.sln -p:EnableWindowsTargeting=true --configuration Debug

# Build and test
dotnet build XrmUnitTest.sln -p:EnableWindowsTargeting=true --configuration Debug
dotnet test XrmUnitTest.sln -p:EnableWindowsTargeting=true --configuration Debug --no-build

# Clean and rebuild
dotnet clean XrmUnitTest.sln -p:EnableWindowsTargeting=true
dotnet build XrmUnitTest.sln -p:EnableWindowsTargeting=true --configuration Debug
```

**Expected Times** (on typical CI/CD runner):
- Restore: ~20-25 seconds
- Clean: ~2 seconds  
- Build (Debug/Release): ~30-35 seconds
- Full test run: Variable, depends on configuration

**Remember**: Windows-targeting flag is ALWAYS required on Linux/macOS systems for all dotnet commands.
