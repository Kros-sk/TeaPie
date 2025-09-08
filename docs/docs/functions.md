# Functions

TeaPie provides a lightweight function system for generating dynamic values in scripts and `.http` files.

## Function Naming Rules

- Must start with `$`
- Allowed characters: letters, digits, `_`, `.`, `$`, `-`
- Pattern used by parser: `^\$[a-zA-Z0-9_.$-]*$`
- Function names are case-sensitive.
- Function notation in `.http` files follows: `{{(\$.*)}}`

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

>The first match is executed.

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
- Function names are case-sensitive.
- Internally, arguments are tokenized using a command-line parser and converted to the target parameter types.

Using together with Variables:

- You can pass Variables as function arguments by embedding variable notation inside the function call.
- Variable notation must follow variable rules (name pattern, `{{variableName}}`).
- Examples:
  - `{{$add {{MyNumber}} 2}}`            // passes variable MyNumber as first argument
  - `{{$upper "{{FullName}}"}}`        // if the variable value may contain spaces, wrap it in quotes
- You can also mix standalone variables and functions in the same line, e.g.:
  - `X-Trace-Id: {{RequestId}}-{{$guid}}`
  - `X-Date: {{$now "yyyyMMdd"}}-{{EnvironmentName}}`

If a function is not found, TeaPie throws an error during resolution.

---

## Built-in Functions

The following functions are available by default:

| Name | Signature | Description | Example |
|------|-----------|-------------|---------|
| `$guid` | `Guid $guid()` | Generates a new GUID. | `{{$guid}}` |
| `$now` | `string $now(string? format)` | Current local time formatted via `DateTime.ToString(format)`. If `format` is omitted, default formatting is used. | `{{$now "yyyy-MM-dd"}}` |
| `$rand` | `double $rand()` | Random double in the range [0, 1). | `{{$rand}}` |
| `$randomInt` | `int $randomInt(int min, int max)` | Random integer in the range [min, max). | `{{$randomInt 1 100}}` |

Notes:

- `$now` uses `DateTime.Now` and the same formatting behavior as `DateTime.ToString(string?)`.

Example:

``` http
# Generate values with functions
POST https://example.com/api/items
Content-Type: application/json

{
  "id": "{{$guid}}",
  "createdAt": "{{$now "yyyy-MM-dd'T'HH:mm:ss"}}",
  "score": {{$randomInt 10 20}},
  "ratio": {{$rand}}
}
```

---

## Working with Functions in C\#

Access functions through the `TeaPie` instance (`tp`).

### **2️⃣ Registering via TeaPie**

TeaPie supports registering functions with 0–2 parameters (maximum two arguments).

``` cs
// 0-arg
tp.RegisterFunction("$buildNumber", () => Environment.GetEnvironmentVariable("BUILD_NUMBER") ?? "local");

// 1-arg
tp.RegisterFunction("$upper", (string s) => s.ToUpperInvariant());

// 2-arg (maximum)
tp.RegisterFunction("$add", (int a, int b) => a + b);
```

### **1️⃣ Use them in `.http` files**

``` http
{{$upper "hello"}}
{{$add 40 2}}
```

### **3️⃣ Executing via TeaPie**

```cs
var id = tp.ExecFunction<Guid>("$guid");
var timeStamp = tp.ExecFunction<string>("$now", "yyyy-MM-dd");
var sum = tp.ExecFunction<int>("$add", 2, 3);
```

>If a function is not found or execution fails, TeaPie throws an error during resolution.

---

## Argument Conversion

- Arguments from `.http` files are strings
- TeaPie converts them to expected parameter types using .NET conversion (`Convert.ChangeType(...)`)
  - Works for common primitives (int, double, bool, DateTime, etc.)
  - Uses the current culture during parsing
- If conversion fails, execution throws an exception

>Tip: Prefer culture-invariant formats in `.http` files for dates and decimals when needed.
