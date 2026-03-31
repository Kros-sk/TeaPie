# Variables & Functions Reference

Complete reference for TeaPie's variable system and functions.

## Variables

### Variable Levels

TeaPie supports four levels of variables (priority order):

1. **Global** - From `$shared` environment, available across all test cases
2. **Environment** - Collection-specific environment, available during collection run
3. **Collection** - Available during whole collection run
4. **Test Case** - Available only within specific test case (deleted after test case ends)

### Variable Resolution

Variables are resolved from highest level to lowest. A variable defined at a higher level can be overridden at a lower level.

### Variable Naming Rules

- Allowed characters: letters, digits, underscores (`_`), dollar signs (`$`), periods (`.`), hyphens (`-`)
- Pattern: `[a-zA-Z0-9_.$-]+`

### Working with Variables

**In `.http` files:**

```http
{{variableName}}
{{ApiBaseUrl}}{{ApiCarsSection}}
```

**In `.csx` scripts:**

```csharp
// Set variable (defaults to collection level)
tp.SetVariable("MyVariable", "value");

// Get variable (searches all levels)
var value = tp.GetVariable<string>("MyVariable");

// Check existence
if (tp.ContainsVariable("MyVariable")) { ... }

// Remove variable (from all levels)
tp.RemoveVariable("MyVariable");

// Remove by tag
tp.RemoveVariablesWithTag("temp");
```

**Accessing at specific levels:**

```csharp
var globalVar = tp.GlobalVariables.Get<int>("MyGlobalVariable");
var envVar = tp.EnvironmentVariables.Get<string>("MyEnvVariable");
var collVar = tp.CollectionVariables.Get<bool>("MyCollectionVariable");
var testVar = tp.TestCaseVariables.Get<DateTime>("MyTestCaseVariable");
```

### Variable Tagging

Tag variables for organization and bulk operations:

```csharp
tp.SetVariable("PersonalNumber", 777, "test");
tp.SetVariable("Password", "2444666666", "test", "secret");
tp.SetVariable("Country", "Slovakia", "production");

tp.RemoveVariablesWithTag("test"); // Removes PersonalNumber and Password
```

**Built-in tags:**

- `secret` - Prevents caching to file
- `no-cache` - Prevents caching to file

OAuth2 access tokens are automatically tagged with both `secret` and `no-cache`.

### Request Variables

Request variables enable data sharing across multiple requests within the same `.http` file.

**Syntax:**

```plaintext
{{requestName.(request|response).(body|headers).(*|JPath|XPath)}}
```

**Examples:**

```http
# @name AddCarRequest
POST {{ApiBaseUrl}}{{ApiCarsSection}}
Content-Type: application/json

{
    "Id": 6,
    "Brand": "Toyota"
}

### Get Car
GET {{ApiBaseUrl}}{{ApiCarsSection}}/{{AddCarRequest.response.body.$.Id}}

### Edit Car
PUT {{ApiBaseUrl}}{{ApiCarsSection}}/{{AddCarRequest.response.body.$.Id}}
Content-Type: {{AddCarRequest.request.headers.Content-Type}}

{
    "Id": {{AddCarRequest.response.body.$.Id}},
    "Brand": "Honda"
}
```

**Request variable selectors:**

- `request` - Access request data
- `response` - Access response data
- `body` - Access body content
- `headers` - Access headers
- `*` - Whole body content
- `$.property.path` - JSONPath syntax for nested properties
- `/element/path` - XPath syntax for XML

### Variable Caching

Variables are cached after each run and loaded at the start of the next run. To disable caching, use `--no-cache-variables` or `--no-cache-vars` option.

## Functions

### Function Naming Rules

- Must start with `$`
- Allowed characters: letters, digits, `_`, `.`, `$`, `-`
- Pattern: `^\$[a-zA-Z0-9_.$-]*$`
- Case-sensitive

### Function Levels

1. **Default** - Built-in functions available to all test cases
2. **Custom** - User-registered functions (live for current collection run)

Functions are resolved in order: Default → Custom (first match wins).

### Using Functions

**In `.http` files:**

```http
{{$functionName}}
{{$functionName arg1}}
{{$functionName arg1 arg2}}
```

**Notes:**

- Arguments are whitespace-separated (maximum two per function)
- Use quotes for values with spaces: `{{$now "yyyy-MM-dd HH:mm"}}`
- Both single and double quotes are supported
- Can mix with variables: `{{$add {{MyNumber}} 2}}`

### Built-in Functions

| Name | Signature | Description | Example |
|------|-----------|-------------|---------|
| `$guid` | `Guid $guid()` | Generates a new GUID | `{{$guid}}` |
| `$now` | `string $now(string? format)` | Current local time formatted via `DateTime.ToString(format)`. Default formatting if format omitted. | `{{$now "yyyy-MM-dd"}}` |
| `$rand` | `double $rand()` | Random double in range [0, 1) | `{{$rand}}` |
| `$randomInt` | `int $randomInt(int min, int max)` | Random integer in range [min, max) | `{{$randomInt 1 100}}` |

**Example:**

```http
POST https://example.com/api/items
Content-Type: application/json

{
  "id": "{{$guid}}",
  "createdAt": "{{$now "yyyy-MM-dd'T'HH:mm:ss"}}",
  "score": {{$randomInt 10 20}},
  "ratio": {{$rand}}
}
```

### Custom Functions

**Registering functions (0-2 parameters):**

```csharp
// 0-arg
tp.RegisterFunction("$buildNumber", () => Environment.GetEnvironmentVariable("BUILD_NUMBER") ?? "local");

// 1-arg
tp.RegisterFunction("$upper", (string s) => s.ToUpperInvariant());

// 2-arg (maximum)
tp.RegisterFunction("$add", (int a, int b) => a + b);
```

**Using in `.http` files:**

```http
{{$upper "hello"}}
{{$add 40 2}}
```

**Executing in scripts:**

```csharp
var id = tp.ExecFunction<Guid>("$guid");
var timeStamp = tp.ExecFunction<string>("$now", "yyyy-MM-dd");
var sum = tp.ExecFunction<int>("$add", 2, 3);
```

### Argument Conversion

- Arguments from `.http` files are strings
- TeaPie converts them to expected parameter types using .NET conversion (`Convert.ChangeType(...)`)
- Works for common primitives (int, double, bool, DateTime, etc.)
- Uses current culture during parsing
- If conversion fails, execution throws an exception

**Tip:** Prefer culture-invariant formats in `.http` files for dates and decimals when needed.
