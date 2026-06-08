# Id Generator Input Formats

This page documents how `IdGenerator/Logic.cs` parses input and generates class-based IDs.

![Id Generator example](https://github.com/user-attachments/assets/2fae17d8-7d15-4262-9b8e-ab5c30f4f2a1)

## Input rules

Each non-empty line defines IDs for one Early Bound class/table.

- The first token is always the Early Bound class name (for example, `Account`, `Contact`, `Acme_Project`).
- Tokens are split by spaces/new lines first.
- Comma syntax is interpreted inside a token, so comma-based values should be written without spaces after commas.

### Space-separated format

`<EntityType> [Count]`

- `Count` is optional.
- If omitted, one ID is generated.
- If `Count` is greater than `1`, a collection is generated with auto names `A`, `B`, `C`, ...

Examples:

- `Account`
- `Account 2`

### Comma-separated format

`<EntityType>,<NameOrCollection>[,<IdName1>,<IdName2>...]`

- If there are exactly 2 comma-separated values, the second value is treated as either:
  - a single ID/property name, or
  - a collection name (if followed by a count later, for example `Contact,Employees 2`).
- If there are 3+ comma-separated values:
  - the second value is the collection name,
  - remaining values are explicit ID names.

Examples:

- `Acme_ProjectTask,Task` (single ID named `Task`)
- `Contact,Partners,Jim,Bob` (collection `Partners` with ID names `Bob`, `Jim` in output ordering)

## Example input

```text
Acme_Project 2
Acme_ProjectTask,Task
Contact,Partners,Jim,Bob
Account 2
```

## Example output (class IDs enabled)

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

## Notes matching current logic

- Output entries are ordered by container name; single IDs (no container) are emitted first.
- Names inside each generated collection are ordered alphabetically.
- Entity names with `_` use the part after `_` when generating default property names (for example `Acme_Project` -> `Project`, `Projects`).
