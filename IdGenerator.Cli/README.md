# DataverseUnitTest Id Generator CLI

Generate `Id<T>` definitions for DataverseUnitTest from entity input or existing C#.

## Install from NuGet.org

```powershell
dotnet tool install -g DataverseUnitTest.IdGenerator.Cli
```

```bash
dotnet tool install -g DataverseUnitTest.IdGenerator.Cli
```

**NuGet package:** [DataverseUnitTest.IdGenerator.Cli](https://www.nuget.org/packages/DataverseUnitTest.IdGenerator.Cli)

**Requires:** .NET 10 SDK or runtime (or newer)

Pin a version:

```powershell
dotnet tool install -g DataverseUnitTest.IdGenerator.Cli --version 1.0.0.1
```

Update or uninstall:

```powershell
dotnet tool update -g DataverseUnitTest.IdGenerator.Cli
dotnet tool uninstall -g DataverseUnitTest.IdGenerator.Cli
```

## Quick start

```powershell
idgen --input "Account 2|Contact"
idgen --help
```

## Usage

```text
idgen [input] [options]

Input:
  [input]                   Entity definitions. Use | instead of newlines on the command line.
  -i, --input <text>        Entity input text.
  -f, --input-file <path>   Read entity input from a file.

Modes:
  --from-csharp <container-name> <path|->
                            Parse Id<T> definitions from the given class/struct container
                            and output entity input text. IDs are not regenerated. Use - for stdin.

Options:
  --seed <int>              Use deterministic GUIDs (default seed: 1 when flag is used alone).
  --settings-file <path>    Load IdGeneratorSettings.json from the given path.
  --use-class-ids           Generate class-based IDs (default).
  --use-struct-ids          Generate struct-based IDs.
  --use-target-typed-new    Use target-typed new expressions (default).
  --use-explicit-new        Use explicit new Id<T>(...) expressions.
  -h, --help                Show help.

Examples:
  idgen "Account 2|Contact,Partners,Jim,Bob"
  idgen --seed 42 "Account|Contact 2"
  idgen --from-csharp TestExample.TestMethodNameClass.TestIds MyTest.cs
  idgen --from-csharp TestExample.TestMethodNameClass.TestIds - < existing-ids.cs
```

See the [Id Generator wiki](https://github.com/daryllabar/XrmUnitTest/wiki/Id-Generator) for input format details and the [Id Generator CLI wiki](https://github.com/daryllabar/XrmUnitTest/wiki/Id-Generator-CLI) for full documentation.

## Build from source (contributors)

From the repository root:

```powershell
dotnet run --project IdGenerator.Cli -- "Account 2|Contact"
```

Or install from a local pack:

```powershell
dotnet pack IdGenerator.Cli/IdGenerator.Cli.csproj -c Release
dotnet tool install -g --add-source IdGenerator.Cli/bin/Release DataverseUnitTest.IdGenerator.Cli
```
