# XrmUnitTest Id Generator CLI

Generate `Id<T>` definitions for XrmUnitTest from entity input or existing C#.

## Install

From a local build:

```powershell
dotnet pack IdGenerator.Cli/IdGenerator.Cli.csproj -c Release
dotnet tool install -g --add-source IdGenerator.Cli/bin/Release XrmUnitTest.IdGenerator.Cli
```

From the repository root:

```powershell
dotnet run --project IdGenerator.Cli -- "Account 2|Contact"
```

## Usage

```text
idgen [input] [options]

Input:
  [input]                   Entity definitions. Use | instead of newlines on the command line.
  -i, --input <text>        Entity input text.
  -f, --input-file <path>   Read entity input from a file.

Modes:
  --from-csharp <path|->    Parse existing C# Id definitions and regenerate GUIDs.
                            Use - for stdin.

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
  idgen --from-csharp MyTest.cs
  idgen --from-csharp - < existing-ids.cs
```

See the [Id Generator wiki](https://github.com/daryllabar/XrmUnitTest/wiki/Id-Generator) for input format details.
