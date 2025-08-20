# Functions

TeaPie provides a lightweight function system for generating dynamic values in scripts and `.http` files.
Function names must start with a `$` and can contain letters, digits, underscores (`_`), dollar signs (`$`), periods (`.`), and hyphens (`-`).

## Function Levels

TeaPie supports two levels of functions, determining visibility and resolution order:

| Level | Scope & Behavior |
|------|-------------------|
| Default | Built-in functions available to all test cases and collections. |
| Custom | User-registered functions that live for the current collection run. |

### Function Resolution Order

Functions are resolved in this order:
1. Default Functions
2. Custom Functions

The first match is executed.

---

## Using Functions in HTTP Files

Syntax (no parentheses, no commas; max 2 arguments):
- `{{$functionName}}`
- `{{$functionName arg1}}`
- `{{$functionName arg1 arg2}}`

Notes:
- Arguments are whitespace-separated tokens (maximum two per function).
- Use quotes for values with spaces: `{{$now "yyyy-MM-dd HH:mm"}}`
- Both single and double quotes are supported.
- Internally, arguments are tokenized using a command-line parser and converted to the target parameter types.

Example:

```
# Generate values with functions
POST https://example.com/api/items
Content-Type: application/json

{
  "id": "{{$guid}}",
  "createdAt": "{{$now "yyyy-MM-dd'T'HH:mm:ss"}}"
}
```

If a function is not found, TeaPie throws an error during resolution.

---

## Built-in Functions

The following functions are available by default:

| Name | Signature | Description | Example |
|------|-----------|-------------|---------|
| `$guid` | `Guid $guid()` | Generates a new GUID. | `{{$guid}}` |
| `$now` | `string $now(string? format)` | Current local time formatted via `DateTime.ToString(format)`. If `format` is omitted, default formatting is used. | `{{$now "yyyy-MM-dd"}}` |

Notes:
- `$guid` renders as a string when placed in an HTTP file.
- `$now` uses `DateTime.Now` and the same formatting behavior as `DateTime.ToString(string?)`.

---

## Working with Functions in C#

Access functions through the `TeaPie` instance (`tp`).

### Registering

TeaPie supports registering functions with 0–2 parameters (maximum two arguments).

```
// 0-arg
tp.RegisterFunction("$buildNumber", () => Environment.GetEnvironmentVariable("BUILD_NUMBER") ?? "local");

// 1-arg
tp.RegisterFunction("$upper", (string s) => s.ToUpperInvariant());

// 2-arg (maximum)
tp.RegisterFunction("$add", (int a, int b) => a + b);
```

Use them in `.http` files:

```
{{$upper "hello"}}
{{$add 40 2}}
```

### Executing in Code

```
var id = tp.ExecFunction<Guid>("$guid");
var when = tp.ExecFunction<string>("$now", "yyyy-MM-dd");
var sum = tp.ExecFunction<int>("$add", 2, 3);
```

If a function is not found or execution fails, `ExecFunction<T>` returns `default`.

---

## Function Naming Rules

- Must start with `$`
- Allowed characters: letters, digits, `_`, `.`, `$`, `-`
- Pattern used by parser: `^\$[a-zA-Z0-9_.$-]*$`
- Function notation in `.http` files follows: `{{(\$.*)}}`

---

## Argument Conversion

- Arguments from `.http` files are strings
- TeaPie converts them to expected parameter types using .NET conversion (`Convert.ChangeType(...)`)
  - Works for common primitives (int, double, bool, DateTime, etc.)
  - Uses the current culture during parsing
- If conversion fails, execution throws an exception

Tip: Prefer culture-invariant formats in `.http` files for dates and decimals when needed.

---

## Managing Functions

You can manage functions via collections:

```
// Access
var defaults = tp.DefaultFunctions;
var custom = tp.CustomFunctions;

// Check and remove
bool hasGuid = defaults.Contains("$guid");
bool removed = custom.Remove("$add");

// Enumerate
foreach (var f in custom) {
    tp.Logger.LogInformation("Function: {Name}", f.Name);
}
```

For convenience methods, see:
- [TeaPie](xref:TeaPie.TeaPie)
- [TeaPie.Functions.FunctionsCollection](xref:TeaPie.Functions.FunctionsCollection)
