# Directives Reference

Complete reference for all TeaPie directives.

## Directives in `.csx` Scripts

### `#load` Directive

**Syntax:** `#load "<path-to-script>"`

**Purpose:** Reference another `.csx` script to use its functionality.

**Parameters:**

- `path-to-script` - Absolute or relative path (relative to parent directory of the script)

**Example:**

```csharp
#load "./Definitions/GenerateNewCar.csx"
#load "$teapie/Definitions/Helper.csx"
```

**Important:** Referenced script is automatically executed. Encapsulate logic in methods to prevent unwanted execution.

### `#nuget` Directive

**Syntax:** `#nuget "<package-name>, <version>"`

**Purpose:** Install a NuGet package for use in scripts.

**Parameters:**

- `package-name` - The NuGet package ID
- `version` - Version of NuGet package to be installed

**Example:**

```csharp
#nuget "AutoBogus, 2.13.1"
```

**Important:** NuGet packages are installed globally across all scripts, but you must use the `using` directive to access them in code.

## Directives in `.http` Files

### Authentication Directives

#### `## AUTH-PROVIDER`

**Syntax:** `## AUTH-PROVIDER: [provider-name]`

**Purpose:** Specify the authentication provider for a request.

**Parameters:**

- `provider-name` - Name of authentication provider. Use `"None"` to disable authentication. `OAuth2` is supported natively but requires prior configuration.

**Example:**

```http
## AUTH-PROVIDER: MyAuth
## AUTH-PROVIDER: OAuth2
## AUTH-PROVIDER: None
```

### Retrying Directives

Retrying directives apply only to the current request and do not alter registered retry strategies.

#### `## RETRY-STRATEGY`

**Syntax:** `## RETRY-STRATEGY: <strategy-name>`

**Purpose:** Select a predefined retry strategy by name.

**Parameters:**

- `strategy-name` - Name of a previously registered retry strategy

**Example:**

```http
## RETRY-STRATEGY: Default retry
```

#### `## RETRY-UNTIL-STATUS`

**Syntax:** `## RETRY-UNTIL-STATUS: <status-codes>`

**Purpose:** Retry until one of the specified HTTP status codes is received.

**Parameters:**

- `status-codes` - List of acceptable status codes (e.g., `[200, 201]`)

**Example:**

```http
## RETRY-UNTIL-STATUS: [200, 201]
```

#### `## RETRY-MAX-ATTEMPTS`

**Syntax:** `## RETRY-MAX-ATTEMPTS: <number>`

**Purpose:** Set the maximum number of retry attempts.

**Parameters:**

- `number` - Maximum number of retries allowed

**Example:**

```http
## RETRY-MAX-ATTEMPTS: 5
```

#### `## RETRY-BACKOFF-TYPE`

**Syntax:** `## RETRY-BACKOFF-TYPE: <type>`

**Purpose:** Define the backoff strategy applied between retries.

**Parameters:**

- `type` - Can be `Constant`, `Linear`, `Exponential`, or another strategy supported by Polly.Core

**Example:**

```http
## RETRY-BACKOFF-TYPE: Linear
```

#### `## RETRY-MAX-DELAY`

**Syntax:** `## RETRY-MAX-DELAY: <hh:mm:ss.fff>`

**Purpose:** Set the maximum allowed delay between retries.

**Parameters:**

- `hh:mm:ss.fff` - Maximum delay time before retrying

**Example:**

```http
## RETRY-MAX-DELAY: 00:00:03
```

#### `## RETRY-UNTIL-TEST-PASS`

**Syntax:** `## RETRY-UNTIL-TEST-PASS: <test-name>`

**Purpose:** Retry until the defined test passes.

**Parameters:**

- `test-name` - Name of test defined in post-response `.csx` script (matches `tp.Test("test-name", ...)`)

**Example:**

```http
## RETRY-UNTIL-TEST-PASS: Identifier should be a positive integer
```

### Testing Directives

#### `## TEST-EXPECT-STATUS`

**Syntax:** `## TEST-EXPECT-STATUS: [status-codes]`

**Purpose:** Assert that response status code matches any value in the provided array.

**Parameters:**

- `status-codes` - List of expected HTTP status codes (as integers)

**Example:**

```http
## TEST-EXPECT-STATUS: [200, 201]
```

#### `## TEST-HAS-BODY`

**Syntax:** `## TEST-HAS-BODY` or `## TEST-HAS-BODY: <should-have-body>`

**Purpose:** Check if the response contains a body.

**Parameters:**

- `should-have-body` - Optional boolean parameter. Defaults to `true` if omitted.

**Example:**

```http
## TEST-HAS-BODY
## TEST-HAS-BODY: True
## TEST-HAS-BODY: False
```

#### `## TEST-HAS-HEADER`

**Syntax:** `## TEST-HAS-HEADER: <header-name>`

**Purpose:** Verify that the specified header is present in the response.

**Parameters:**

- `header-name` - Name of the HTTP header to check

**Example:**

```http
## TEST-HAS-HEADER: Content-Type
```

#### Custom Testing Directives

**Syntax:** `## TEST-<directive-name>: [parameter1]; [parameter2]; ...`

**Purpose:** Define custom testing directives with unique names and optional parameters.

**Parameters:**

- `directive-name` - Custom directive name (appended after `TEST-` prefix)
- `parameter[index]` - Optional parameters, delimited by `;`, supporting multiple data types

**Example:**

```http
## TEST-SUCCESSFUL-STATUS: True
## TEST-JSON-HAS-ID-PROPERTY: VariableName
```

**Registration:** Custom test directives must be registered in `init.csx` before first use using `tp.RegisterTestDirective()`.
