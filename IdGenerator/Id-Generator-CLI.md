# Id Generator CLI

Use the Id Generator CLI (`idgen`) to generate `Id<T>` definitions for DataverseUnitTest from the command line or from AI coding assistants such as Cursor.

This complements the [Id Generator](Id-Generator) WinForms tool and reuses the same parsing and output logic.

## Overview

| Tool | Best for |
|------|----------|
| **WinForms app** (`IdGenerator/`) | Interactive use, visual settings, parse/regenerate from output |
| **CLI** (`IdGenerator.Cli/`) | Scripts, CI, terminal workflows, AI assistants |
| **Cursor skill** (`.cursor/skills/generate-ids/`) | In-repo guidance for Cursor agents |

The CLI and WinForms app share the same core library (`IdGenerator.Core/`).

---

## Quick start

### Install from NuGet.org (recommended)

Install the global `idgen` tool:

```powershell
dotnet tool install -g DataverseUnitTest.IdGenerator.Cli
```

```bash
dotnet tool install -g DataverseUnitTest.IdGenerator.Cli
```

Then run:

```powershell
idgen --input "Account 2|Contact"
```

- **NuGet package:** [DataverseUnitTest.IdGenerator.Cli](https://www.nuget.org/packages/DataverseUnitTest.IdGenerator.Cli)
- **Requires:** .NET 10 SDK or runtime (or newer)

Pin a specific version:

```powershell
dotnet tool install -g DataverseUnitTest.IdGenerator.Cli --version 1.0.0.1
```

Update or uninstall:

```powershell
dotnet tool update -g DataverseUnitTest.IdGenerator.Cli
dotnet tool uninstall -g DataverseUnitTest.IdGenerator.Cli
```

### Run from the repository (contributors)

If you are working in the source repo and have not installed the global tool, from the repository root:

```powershell
dotnet run --project IdGenerator.Cli -- --settings-file .cursor/IdGeneratorSettings.json --input "Account 2|Contact"
```

```bash
dotnet run --project IdGenerator.Cli -- --settings-file .cursor/IdGeneratorSettings.json --input "Account 2|Contact"
```

### Install from a local build (contributors)

```powershell
dotnet pack IdGenerator.Cli/IdGenerator.Cli.csproj -c Release
dotnet tool install -g --add-source IdGenerator.Cli/bin/Release DataverseUnitTest.IdGenerator.Cli
```

GitHub releases may also include a pre-built tool package (`IdGenerator.Cli.zip`).

---

## Command reference

```text
idgen [input] [options]
```

### Input options

| Option | Description |
|--------|-------------|
| `[input]` | Entity definitions as a positional argument. Use `\|` instead of newlines on the command line. |
| `-i`, `--input <text>` | Entity input text. Preferred on PowerShell when quoting complex input. |
| `-f`, `--input-file <path>` | Read entity input from a file. |

If no input option is provided and stdin is redirected, entity input is read from stdin.

### Modes

| Option | Description |
|--------|-------------|
| `--from-csharp <path\|->` | Parse existing C# `Id<T>` definitions and output entity input text. IDs are not regenerated. Use `-` for stdin. |

### Generation options

| Option | Description |
|--------|-------------|
| `--seed [int]` | Use deterministic GUIDs. Default seed is `1` when the flag is used without a value. |
| `--settings-file <path>` | Load options from `IdGeneratorSettings.json`. |
| `--use-class-ids` | Generate nested `*Ids` classes (default). |
| `--use-struct-ids` | Generate struct-based IDs. |
| `--use-target-typed-new` | Use `new("GUID")` syntax (default). |
| `--use-explicit-new` | Use `new Id<T>("GUID")` syntax. |
| `-h`, `--help` | Show help. |

### Exit codes

| Code | Meaning |
|------|---------|
| `0` | Success |
| `1` | Error (invalid input, parse failure, etc.) |
| `2` | Help displayed |

---

## Entity input format

Each non-empty line defines IDs for one Early Bound entity/table. See [Id Generator](Id-Generator) for full background on the WinForms tool.

### Pipe character (`|`)

On the command line, `|` is treated the same as a newline. This makes multi-entity input easier to type:

```text
Account 2|Contact,Partners,Jim,Bob
```

is equivalent to:

```text
Account 2
Contact,Partners,Jim,Bob
```

In input files (`--input-file`), use normal newlines. Pipes are optional.

### Space-separated format

```text
<EntityType> [Count]
```

- `Count` is optional; if omitted, one ID is generated.
- If `Count` is greater than `1`, a collection is generated with auto names `A`, `B`, `C`, ...

Examples:

```text
Account
Account 2
```

### Comma-separated format

```text
<EntityType>,<NameOrCollection>[,<IdName1>,<IdName2>...]
```

- If there are exactly 2 comma-separated values, the second value is either a single ID/property name or a collection name (if followed by a count, e.g. `Contact,Employees 2`).
- If there are 3+ comma-separated values, the second value is the collection name and remaining values are explicit ID names.
- Do not put spaces after commas inside comma-based tokens.

Examples:

```text
Acme_ProjectTask,Task
Contact,Partners,Jim,Bob
Contact,Employees 2
```

### Example input

```text
Acme_Project 2
Acme_ProjectTask,Task
Contact,Partners,Jim,Bob
Account 2
```

Or on one CLI line:

```text
Acme_Project 2|Acme_ProjectTask,Task|Contact,Partners,Jim,Bob|Account 2
```

### Example output (class IDs enabled)

```csharp
public Id<Acme_ProjectTask> Task { get; } = new("C9E8361B-AC23-4134-B242-C52BDFD56A9F");
public AccountIds Accounts { get; } = new();
public PartnerIds Partners { get; } = new();
public ProjectIds Projects { get; } = new();

public class AccountIds
{
    public Id<Account> A { get; } = new("9B046FDF-94AF-4F4A-85C4-CD7F7F984367");
    public Id<Account> B { get; } = new("4CA2375F-5287-494D-8E30-4DFD1CF2F9E7");
}

public class PartnerIds
{
    public Id<Contact> Bob { get; } = new("C46E99ED-F28E-48FF-9EFB-6B7516CDB99D");
    public Id<Contact> Jim { get; } = new("C7F251B1-6D2A-4A28-BA07-8D3F781A1D6A");
}

public class ProjectIds
{
    public Id<Acme_Project> A { get; } = new("4D67154A-0A50-4C3E-8D55-0FE93BE4F3DC");
    public Id<Acme_Project> B { get; } = new("E804C340-1C9D-41D7-930F-64B730B3373E");
}
```

### Output rules

- Output entries are ordered by container name; single IDs (no container) are emitted first.
- Names inside each generated collection are ordered alphabetically.
- Entity names with `_` use the part after `_` when generating default property names (e.g. `Acme_Project` → `Project`, `Projects`).

---

## Common workflows

### Generate IDs for a new test

1. Identify the Early Bound entity types needed.
2. Build entity input (one line with `|` is fine for CLI use).
3. Run the CLI and capture stdout.
4. Paste the generated properties/classes into a nested helper class in your test.

```powershell
idgen --settings-file .cursor/IdGeneratorSettings.json --input "Account|Contact 2|SystemUser"
```

### Extract entity input from existing C#

Use this when you want to convert existing `Id<T>` definitions back into entity input text:

```powershell
idgen --settings-file .cursor/IdGeneratorSettings.json --from-csharp path/to/MyTest.cs
```

From stdin (PowerShell):

```powershell
Get-Content path/to/snippet.cs | idgen --settings-file .cursor/IdGeneratorSettings.json --from-csharp -
```

From stdin (bash):

```bash
idgen --settings-file .cursor/IdGeneratorSettings.json --from-csharp - < snippet.cs
```

### Deterministic output for docs or examples

Use `--seed` when you need reproducible GUIDs:

```powershell
idgen --seed 42 --settings-file .cursor/IdGeneratorSettings.json --input "Account 2"
```

Without `--seed`, GUIDs are random (normal test usage).

### Read input from a file

```powershell
idgen --settings-file .cursor/IdGeneratorSettings.json --input-file entities.txt
```

Example `entities.txt`:

```text
Account 2
Contact,Partners,Jim,Bob
SystemUser
```

---

## Settings

Settings can be loaded from a JSON file:

```json
{
  "useTargetTypedNew": true,
  "useClassIds": true
}
```

The repository includes a default file at `.cursor/IdGeneratorSettings.json`.

| Setting | Default | Description |
|---------|---------|-------------|
| `useClassIds` | `true` | Generate class-based nested `*Ids` types instead of structs. |
| `useTargetTypedNew` | `true` | Use target-typed `new("GUID")` instead of `new Id<T>("GUID")`. |
| `entities` | (sample) | Used by the WinForms app; ignored by CLI unless you reuse the same file. |

CLI flags override settings file values for the current run.

---

## AI assistant usage (Cursor and similar)

AI assistants should **run the CLI** rather than inventing GUIDs or naming rules by hand.

### Recommended command

If the global tool is installed from NuGet:

```powershell
idgen --settings-file .cursor/IdGeneratorSettings.json --input "Account 2|Contact"
```

When working in the source repository without the global tool installed:

```powershell
dotnet run --project IdGenerator.Cli -- --settings-file .cursor/IdGeneratorSettings.json --input "Account 2|Contact"
```

### Cursor project skill

This repository includes a Cursor skill at:

```text
.cursor/skills/generate-ids/SKILL.md
```

That skill tells Cursor agents:

- When to generate test IDs
- Which command to run
- How to format entity input
- How to use `--from-csharp` and `--seed`

Clone or copy that skill into other projects if needed.

### AI workflow checklist

1. Determine required entity types and counts from the test being written.
2. Build entity input using the formats above.
3. Run `idgen` (install from NuGet with `dotnet tool install -g DataverseUnitTest.IdGenerator.Cli`, or use `dotnet run --project IdGenerator.Cli` when working in the source repo).
4. Paste stdout into the test's nested IDs helper class.
5. Do **not** manually create GUIDs or guess container/property naming.

Match patterns used elsewhere in the test project, such as nested `TestIdsClass` / `*Ids` containers.

---

## Examples

### Basic collection

Input:

```text
Contact 4
```

Output:

```csharp
public ContactIds Contacts { get; } = new();

public class ContactIds
{
    public Id<Contact> A { get; } = new("...");
    public Id<Contact> B { get; } = new("...");
    public Id<Contact> C { get; } = new("...");
    public Id<Contact> D { get; } = new("...");
}
```

### Mixed entities on one CLI line

```powershell
idgen --input "Account|Contact 4|SystemUser 2"
```

### Struct output

```powershell
idgen --use-struct-ids --input "Contact 2"
```

### Explicit `new Id<T>(...)` syntax

```powershell
idgen --use-explicit-new --input "Account"
```

---

## Troubleshooting

### PowerShell and the pipe character

Inside double quotes, `|` is a literal character. Prefer `--input` for clarity:

```powershell
idgen --input "Account 2|Contact"
```

Avoid unquoted input containing `|`, which PowerShell may interpret as the pipeline operator.

### `Entity input is required`

Provide one of:

- positional input
- `--input`
- `--input-file`
- `--from-csharp`
- redirected stdin

### `No Id<T> definitions were found in the C# input`

The `--from-csharp` input must contain parseable `Id<T>` properties or fields in the supported formats (class properties, nested classes, structs, or top-level properties).

When parsing succeeds, the command outputs normalized entity input text rather than regenerated IDs.

### WinForms vs CLI settings

The WinForms app saves settings next to its executable (`IdGeneratorSettings.json`). The CLI uses the path from `--settings-file`, or defaults next to the CLI executable if no file is specified.

---

## Related pages

- [Id Generator](Id-Generator) — WinForms tool and input format reference
- [DataverseUnitTest.IdGenerator.Cli on NuGet.org](https://www.nuget.org/packages/DataverseUnitTest.IdGenerator.Cli)
- Repository: `IdGenerator/`, `IdGenerator.Core/`, `IdGenerator.Cli/`
- GitHub releases — WinForms zip and CLI tool package
