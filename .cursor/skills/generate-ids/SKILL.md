---
name: generate-xrm-ids
description: Generate XrmUnitTest Id<T> definitions using the Id Generator CLI. Use when writing or updating unit tests that need Id<>, nested *Ids classes, or test entity GUIDs for Dynamics/Dataverse.
---

# Generate XrmUnitTest IDs

Use the Id Generator CLI instead of hand-writing GUIDs or guessing naming rules.

Documentation: [Id Generator wiki](https://github.com/daryllabar/XrmUnitTest/wiki/Id-Generator)

## When to use

- Adding or extending test classes with `Id<T>` properties
- Creating nested `*Ids` helper classes for test data
- Regenerating GUIDs while preserving existing structure

## Preferred command

From the repository root:

```powershell
dotnet run --project IdGenerator.Cli -- --settings-file .cursor/IdGeneratorSettings.json "Account 2|Contact"
```

If the global tool is installed:

```powershell
idgen --settings-file .cursor/IdGeneratorSettings.json "Account 2|Contact"
```

## Input format

Each non-empty line defines IDs for one Early Bound entity type. On the command line, use `|` instead of a newline.

Examples:

```text
Account
Account 2
Acme_ProjectTask,Task
Contact,Partners,Jim,Bob
Account 2|Contact,Partners,Jim,Bob
```

Rules:

- First token is the entity type (`Account`, `Contact`, `Acme_Project`, etc.)
- Space format: `<EntityType> [Count]`
- Comma format: `<EntityType>,<NameOrCollection>[,<IdName1>,...]`
- Do not put spaces after commas inside comma-based tokens

## Modes

### Generate from entity input

```powershell
dotnet run --project IdGenerator.Cli -- --settings-file .cursor/IdGeneratorSettings.json "Account|Contact 2"
```

### Regenerate from existing C#

Parse an existing test ID block and regenerate GUIDs with the same shape:

```powershell
dotnet run --project IdGenerator.Cli -- --settings-file .cursor/IdGeneratorSettings.json --from-csharp path/to/TestFile.cs
```

Or from stdin:

```powershell
Get-Content path/to/snippet.cs | dotnet run --project IdGenerator.Cli -- --settings-file .cursor/IdGeneratorSettings.json --from-csharp -
```

### Deterministic output

Use `--seed` when reproducible GUIDs are useful (docs, examples, snapshots):

```powershell
dotnet run --project IdGenerator.Cli -- --seed 42 --settings-file .cursor/IdGeneratorSettings.json "Account 2"
```

Without `--seed`, GUIDs are random (normal test usage).

## Settings

Default project settings live at `.cursor/IdGeneratorSettings.json`:

- `useClassIds: true` — nested `*Ids` classes (current test style)
- `useTargetTypedNew: true` — `new("GUID")` syntax

Override on the command line when needed:

- `--use-struct-ids`
- `--use-explicit-new`

## Workflow

1. Identify entities needed for the test.
2. Build entity input (use `|` on one line for CLI convenience).
3. Run the CLI and capture stdout.
4. Paste the generated properties/classes into the test's nested IDs helper class.
5. Do not invent GUIDs or naming conventions manually.

Match existing test patterns such as nested `TestIdsClass` / `*Ids` containers in the test project.

## Install the global tool (optional)

```powershell
dotnet pack IdGenerator.Cli/IdGenerator.Cli.csproj -c Release
dotnet tool install -g --add-source IdGenerator.Cli/bin/Release XrmUnitTest.IdGenerator.Cli
```

Then use `idgen` anywhere.
