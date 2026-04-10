---
name: teapie
description: Comprehensive TeaPie framework expertise for API integration testing. Use when: (1) Working with TeaPie projects, API testing, .http files, .tp files, C# test scripts (.csx), test collections, directives, variables, functions, authentication, retrying, (2) Creating and scaffolding test cases using `teapie generate`, renumbering tests, initializing TeaPie projects, (3) Running tests with `teapie test`, finding tests for API endpoints, debugging test failures, generating reports, (4) Analyzing .teapie folder structure, discovering custom functions/directives/auth providers registered in init.csx, understanding project-specific configurations, or (5) When users need guidance on TeaPie CLI commands, test structure, framework capabilities, or test organization.
---

# TeaPie Framework

## Overview

TeaPie (TEsting API Extension) is a lightweight CLI tool for API testing that combines `.http` files with C# scripts or uses `.tp` files that cover both types of files within one for comprehensive integration testing. This skill provides expert knowledge on all TeaPie capabilities, syntax, and best practices.

## Quick Reference

### Test Case Structure

TeaPie supports two equivalent formats. Choose based on script complexity and structure preferences; both can coexist in the same collection.

**Multi-file format:**

- **`<name>-req.http`** (required) - HTTP request file (one or more requests)
- **`<name>-init.csx`** (optional) - Pre-request script for setup
- **`<name>-test.csx`** (optional) - Post-response script for validation

**Single-file format (`.tp`):**

- **`<name>.tp`** - All sections in one file using markers: `--- TESTCASE`, `--- INIT`, `--- HTTP`, `--- TEST`, `--- END`

### Naming Convention

Use zero-padded numeric prefixes for ordering:

- `001-Add-Customer-req.http`
- `002-Edit-Customer-req.http`
- `003-Delete-Customer-req.http`

### Directory Organization for Scenarios

Nested directories can organize different test scenarios within a collection:

```text
007-Search/
├── 001-Seed/
│   └── 001-seed-data-req.http
├── 002-Internal-Purchaser-index/
│   ├── 001-by-registration-id-req.http
│   └── 002-by-name-req.http
└── 003-Internal-partner-index/
    └── 001-by-name-req.http
```

Gap scripts (`insert-gap.py`, `renumber-tests.py`) support both test case files and numbered directories.

### CLI Commands

See [CLI Commands Reference](references/cli-commands.md) for complete command documentation.

**Common commands:**

```bash
teapie test [path]                 # Run tests (default command)
teapie generate <name> [-i] [-t]   # Scaffold new test case
teapie init                        # Initialize .teapie folder
teapie explore [path]              # Explore collection structure
teapie compile|comp|c <path>       # Compile C# script (.csx) or test case (.tp) at the specified path to check for compile-time errors
```

### Directives

**In `.http` files:**
*(The following are built-in directives. You can also define custom directives; see below.)*

- `## AUTH-PROVIDER: <name>` - Set authentication provider
- `## RETRY-STRATEGY: <name>` - Select retry strategy
- `## RETRY-UNTIL-TEST-PASS: <test_name>` - Retry until all test assertions pass
- `## RETRY-UNTIL-STATUS: [200, 201]` - Retry until specified status codes are received
- `## TEST-EXPECT-STATUS: [200, 201]` - Assert status code
- `## TEST-HAS-BODY` - Assert response has body
- `## TEST-HAS-HEADER: <name>` - Assert header exists

*You may define your own custom directives in your project to support additional behaviors.*

**In `.csx` scripts:**

- `#load "<path>"` - Load another script
- `#nuget "<package>, <version>"` - Install NuGet package

See [Directives Reference](references/directives.md) for complete directive documentation.

### Variables

Multi-level variable system (priority order):

1. Global (`$shared` environment)
2. Environment (collection-specific)
3. Collection (during collection run)
4. Test Case (deleted after test case ends)

**Syntax:** `{{variableName}}`

**Request variables:** `{{requestName.response.body.$.property}}` or `{{requestName.request.headers.HeaderName}}`

See [Variables & Functions Reference](references/variables-functions.md) for details.

### Functions

Built-in functions:

- `{{$guid}}` - Generate GUID
- `{{$now "format"}}` - Current time with format
- `{{$rand}}` - Random double [0, 1)
- `{{$randomInt min max}}` - Random integer [min, max)

Custom functions can be registered in `init.csx`.

## C# Scripting (.csx Files)

`.csx` files are full C# in scripting mode - no class wrapper needed, code runs directly at top level.

**Key points:**
- `tp` object provides access to requests, responses, variables, and test methods
- `tp.Test("name", () => { ... })` for sync tests, `await tp.Test("name", async () => { ... })` for async
- xUnit assertions without `Assert.` prefix: `Equal()`, `True()`, `NotNull()`, etc.
- JSON: `tp.Response.GetBodyAsExpandoAsync()` returns `CaseInsensitiveExpandoObject` - **ONLY for JSON objects `{...}`, NOT arrays `[...]`**
- **For JSON arrays**: Use string checks (simple) or `System.Text.Json.JsonSerializer.Deserialize<JsonElement[]>()` (complex)
- **Use `JsonContains()` for comparing JSON objects** - store entire response as JSON string, not individual properties
- Delete temporary variables after use: `tp.RemoveVariable("VariableName")` if only needed between two tests cases

See [C# Scripting Reference](references/csx-scripting.md) for complete documentation.

## Initialization & Setup

### Installing TeaPie

Install the TeaPie CLI tool globally:

```bash
dotnet tool install -g TeaPie.Tool
```

### Initializing TeaPie

**Only initialize if `.teapie` folder doesn't exist in the project root.**

Check if TeaPie is already initialized:
- If `.teapie/` folder exists → skip initialization, TeaPie is already configured
- If `.teapie/` folder doesn't exist → run `teapie init`

Initialize TeaPie configuration:

```bash
teapie init
```

This creates the `.teapie/` folder with default configuration files and updates `.gitignore`.

### Creating Tests Folder

**Ask user for tests folder path.** Standard location is `tests/teapie`, but user may prefer a custom path.

**Standard path:**

```bash
mkdir -p tests/teapie
```

## Creating Test Cases

### Using Generate Command

**Full Syntax:**

```bash
teapie generate <test-case-name> [path] [-i|--init|--pre-request] [-t|--test|--post-response]
```

**Arguments:**
- `<test-case-name>` - Name of test case (required). Can include numeric prefix: `001-Create-Car`
- `[path]` - Target directory path (optional). If omitted, creates in current directory. If path doesn't exist, it will be created automatically.

**Options:**
- `-i`, `--init`, `--pre-request` - Generate pre-request script (`<name>-init.csx`)
- `-t`, `--test`, `--post-response` - Generate post-response script (`<name>-test.csx`)

**Examples:**

```bash
# Basic test case
teapie generate MyTestCase

# With pre-request script
teapie generate MyTestCase -i

# With both scripts
teapie generate MyTestCase -i -t

# In specific directory
teapie generate 001-Create-Car ./Tests/002-Cars -i -t
```

### Renumbering Test Cases

Maintain proper workflow order by renumbering test cases when inserting new ones.

**Use script:** `scripts/renumber-tests.py` to automate renumbering:

```bash
# Renumber all test cases in directory
python scripts/renumber-tests.py --directory ./Tests/002-Cars

# Insert test case at specific position
python scripts/renumber-tests.py --directory ./Tests/002-Cars --insert 003 MyNewTest
```

**Use script:** `scripts/insert-gap.py` to create gaps:

```bash
python scripts/insert-gap.py --directory ./Tests/002-Cars --after 002 --gap-size 1
```

### Creating Tests from OpenAPI

TeaPie doesn't have built-in OpenAPI support. Create complete test cases manually from OpenAPI specifications.

See [OpenAPI to Tests Guide](references/openapi-to-tests.md) for detailed patterns and examples, including how to extract validation rules and status codes from OpenAPI to create edge case test scenarios.

## Running Tests

### Basic Test Execution

Run tests in current directory:

```bash
teapie test
```

Run specific collection or test case:

```bash
teapie test ./Tests/002-Cars
teapie test ./Tests/002-Cars/001-Add-Car-req.http
```

### With Options

```bash
# Run with specific environment
teapie test -e local

# Generate JUnit XML report
teapie test -r report.xml

# Run without variable caching
teapie test --no-cache-vars

# Verbose output (for debugging and investigation)
teapie test -v
```

### Exit Codes

- `0` - Success (all tests passed)
- `1` - Error during execution
- `2` - Some tests failed
- `130` - Premature termination (Ctrl+C)

### Verbose Output for Debugging

Use the `-v` (verbose) flag to get detailed output for investigating test failures:

```bash
teapie test -v
teapie test -v ./Tests/002-Cars/001-Add-Car-req.http
```

**Verbose output includes:**
- Detailed request/response information
- Variable resolution and values
- Script execution details
- HTTP headers and body content
- Error stack traces
- Test assertion details

## Debugging Failed Tests

When a test fails or execution errors occur, follow this escalating diagnostic workflow to understand the root cause, explain it clearly to the user, and suggest a fix.

### Step 1: Re-run with Request/Response Logging

Use `--requests-log-file` to capture full HTTP request and response details into a JSONL file:

```bash
teapie test ./path/to/failing-test -e <environment> --requests-log-file requests.json
```

Then read the `requests.json` file and examine:
- **Request**: method, URL, headers, body — verify the request was constructed correctly
- **Response**: status code, headers, body — check what the server actually returned
- **Timing**: duration in milliseconds — identify timeouts or slow responses
- **Errors**: any errors captured during the request

This reveals mismatches between expected and actual HTTP traffic (wrong URL, missing headers, unexpected response body, auth failures, etc.).

Reference: https://www.teapie.fun/docs/requests-logging.html

### Step 2: Re-run with Debug/Verbose Logging

If request/response data alone is insufficient, enable detailed framework logging:

```bash
# Debug-level logging to console
teapie test ./path/to/failing-test -e <environment> -d

# Maximum verbosity
teapie test ./path/to/failing-test -e <environment> -v

# Write debug logs to a file for analysis
teapie test ./path/to/failing-test -e <environment> --log-file debug.log --log-file-log-level Debug
```

**CLI logging flags:**
- `-d` / `--debug` — Activates detailed logging output
- `-v` / `--verbose` — Most comprehensive logging (includes variable resolution, script execution, full stack traces)
- `-q` / `--quiet` — Suppresses all output
- `--log-level <level>` — Sets minimum log level for console
- `--log-file <path>` — Writes logs to a file
- `--log-file-log-level <level>` — Sets minimum log level for the log file

Debug/verbose output helps diagnose:
- Variable resolution failures (unresolved `{{variables}}`)
- Script compilation or execution errors
- Authentication provider issues
- Pipeline step failures
- Directive processing problems

Reference: https://www.teapie.fun/docs/logging.html

### Step 3: Analyze and Explain

After collecting diagnostic data, you MUST:

1. **Identify the root cause** — Pinpoint exactly what failed and why (e.g., "The server returned 401 because the auth token expired", "Variable `{{baseUrl}}` was not resolved because environment was not set")
2. **Explain clearly to the user** — Provide a concise, non-technical-jargon explanation of what went wrong, referencing the specific request/response data or log entries
3. **Suggest a fix** — If possible, propose concrete changes to the test files, scripts, environment config, or CLI invocation that would resolve the issue

### Combined Diagnostic Command

For maximum diagnostic information in a single run:

```bash
teapie test ./path/to/failing-test -e <environment> --requests-log-file requests.json --log-file debug.log --log-file-log-level Debug -v
```

This produces both the structured request/response log and detailed framework debug output, giving you everything needed for thorough analysis.

## Project Analysis

Analyze TeaPie project structure to discover custom extensions, configurations, and shared resources.

### Finding .teapie Folder

The `.teapie` folder is typically located at the repository root. TeaPie searches upward from the collection path to find it.

**Search strategy:**

1. Check current directory for `.teapie`
2. Walk up parent directories
3. Stop at repository root (where `.git` folder exists)

### Analyzing init.csx

The `init.csx` file contains custom extensions registered for the project:

- **Custom Functions:** `tp.RegisterFunction("$name", function)`
- **Custom Test Directives:** `tp.RegisterTestDirective("DIRECTIVE-NAME", ...)`
- **Custom Auth Providers:** `tp.RegisterAuthProvider("ProviderName", provider)`
- **Retry Strategies:** `tp.RegisterRetryStrategy("StrategyName", options)`

### Analyzing env.json

The `env.json` file defines environment variables:

```json
{
    "$shared": {
        "ApiBaseUrl": "http://api.example.com"
    },
    "local": {
        "ApiBaseUrl": "http://localhost:3001"
    }
}
```

### Analyzing Definitions Folder

The `Definitions/` folder contains shared scripts and class definitions. Scripts are loaded using `#load "$teapie/Definitions/FileName.csx"`.

See [Project Analysis Reference](references/project-analysis.md) for complete guide.

## Finding Tests for API Endpoints

When an API endpoint changes, identify which tests cover it:

1. **Parse HTTP files** to extract endpoints
2. **Resolve variables** from environment files
3. **Match endpoint patterns** to changed APIs
4. **Find corresponding test files**

### Using Scripts

Use `scripts/find-tests-for-api.py` to find tests covering a specific endpoint:

```bash
python scripts/find-tests-for-api.py --endpoint "/cars" --collection ./Tests
python scripts/find-tests-for-api.py --method POST --endpoint "/customers" --collection ./Tests
```

Use `scripts/parse-http-file.py` to extract endpoints from HTTP files:

```bash
python scripts/parse-http-file.py ./Tests/002-Cars/001-Add-Car-req.http
```

See [Test Mapping Patterns](references/test-mapping-patterns.md) for detailed patterns and examples.

## .teapie Folder

The `.teapie` folder contains shared resources:

- `init.csx` - Global initialization script (auto-detected)
- `env.json` - Environment definitions (auto-detected)
- `Definitions/` - Shared class definitions and helper scripts
- `cache/` - Cached scripts, variables, NuGet packages. TeaPie system folder.
- `reports/` - Test reports. TeaPie system folder.

Use `$teapie` wildcard in `#load` directives: `#load "$teapie/Definitions/Helper.csx"`.

## Initialization Script

The `init.csx` script runs before the first test case. Use it to:

- Configure OAuth2: `tp.ConfigureOAuth2Provider(...)`
- Register auth providers: `tp.RegisterAuthProvider(...)`
- Register retry strategies: `tp.RegisterRetryStrategy(...)`
- Register custom functions: `tp.RegisterFunction(...)`
- Register custom test directives: `tp.RegisterTestDirective(...)`
- Register reporters: `tp.RegisterReporter(...)`

## Resources

### Reference Documentation

- [CLI Commands Reference](references/cli-commands.md) - Complete command documentation
- [Directives Reference](references/directives.md) - All directives with examples
- [Variables & Functions Reference](references/variables-functions.md) - Variable system and functions
- [Test Structure Reference](references/test-structure.md) - Test case and collection structure
- [OpenAPI to Tests Guide](references/openapi-to-tests.md) - Creating tests from OpenAPI specs
- [Test Mapping Patterns](references/test-mapping-patterns.md) - API-to-test mapping strategies
- [Project Analysis Reference](references/project-analysis.md) - Analyzing project structure and extensions
- [C# Scripting Reference](references/csx-scripting.md) - tp object, tests, assertions, JSON handling
- [Test Design Best Practices](references/test-design.md) - Writing meaningful, maintainable tests

### Scripts

- `scripts/renumber-tests.py` - Renumber test cases in a directory
- `scripts/insert-gap.py` - Insert gaps in test case numbering
- `scripts/find-tests-for-api.py` - Find tests covering specific API endpoints
- `scripts/parse-http-file.py` - Parse HTTP files and extract endpoint information

### Templates

- `templates/basic-post.http` - POST request template
- `templates/basic-get.http` - GET request template
- `templates/basic-put.http` - PUT request template
- `templates/basic-delete.http` - DELETE request template
- `templates/with-auth.http` - Request with authentication
- `templates/with-retry.http` - Request with retry strategy
