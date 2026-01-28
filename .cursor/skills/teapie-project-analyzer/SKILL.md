---
name: teapie-project-analyzer
description: Analyze TeaPie project structure and discover custom extensions. Use when: (1) Finding and understanding .teapie folder structure, (2) Discovering custom functions, directives, and auth providers registered in init.csx, (3) Listing available environments from env.json, (4) Finding shared scripts in Definitions folder, (5) Understanding project-specific TeaPie configurations, or (6) When users need to understand what custom extensions are available in a TeaPie project.
---

# TeaPie Project Analyzer

## Overview

Analyze TeaPie project structure to discover custom extensions, configurations, and shared resources. Helps understand project-specific TeaPie setup.

## Finding .teapie Folder

The `.teapie` folder is typically located at the repository root. TeaPie searches upward from the collection path to find it.

**Search strategy:**

1. Check current directory for `.teapie`
2. Walk up parent directories
3. Stop at repository root (where `.git` folder exists)

**Use script:** `scripts/analyze-teapie-project.py` to automatically discover and analyze.

## Analyzing init.csx

The `init.csx` file contains custom extensions registered for the project.

### Custom Functions

**Pattern:** `tp.RegisterFunction("$name", function)`

**Example:**

```csharp
tp.RegisterFunction("$buildNumber", () => Environment.GetEnvironmentVariable("BUILD_NUMBER") ?? "local");
tp.RegisterFunction("$upper", (string s) => s.ToUpperInvariant());
```

**Discovered functions:**

- `$buildNumber` - Returns build number from environment
- `$upper` - Converts string to uppercase

### Custom Test Directives

**Pattern:** `tp.RegisterTestDirective("DIRECTIVE-NAME", ...)`

**Example:**

```csharp
tp.RegisterTestDirective(
    "SUCCESSFUL-STATUS",
    TestDirectivePatternBuilder.Create("SUCCESSFUL-STATUS").AddBooleanParameter("MyBool").Build(),
    (parameters) => $"Response status code should {negation}be successful.",
    async (response, parameters) => { /* test logic */ }
);
```

**Usage in .http files:** `## TEST-SUCCESSFUL-STATUS: True`

### Custom Authentication Providers

**Pattern:** `tp.RegisterAuthProvider("ProviderName", provider)`

**Example:**

```csharp
tp.RegisterAuthProvider(
    "MyAuth",
    new MyAuthProvider(tp.ApplicationContext)
        .ConfigureOptions(new MyAuthProviderOptions { AuthUrl = authUrl })
);
```

**Usage in .http files:** `## AUTH-PROVIDER: MyAuth`

### Retry Strategies

**Pattern:** `tp.RegisterRetryStrategy("StrategyName", options)`

**Example:**

```csharp
tp.RegisterRetryStrategy("Default retry", new RetryStrategyOptions<HttpResponseMessage>
{
    MaxRetryAttempts = 3,
    Delay = TimeSpan.FromMilliseconds(500),
    MaxDelay = TimeSpan.FromSeconds(2),
    BackoffType = DelayBackoffType.Exponential
});
```

**Usage in .http files:** `## RETRY-STRATEGY: Default retry`

### Default Configurations

**Patterns:**

- `tp.SetDefaultAuthProvider("ProviderName")` - Set default auth provider
- `tp.SetOAuth2AsDefaultAuthProvider()` - Set OAuth2 as default
- `tp.SetEnvironment("environmentName")` - Set default environment

## Analyzing env.json

The `env.json` file defines environment variables.

### Structure

```json
{
    "$shared": {
        "ApiBaseUrl": "http://api.example.com",
        "SharedVar": "value"
    },
    "local": {
        "ApiBaseUrl": "http://localhost:3001",
        "DebugMode": true
    },
    "production": {
        "ApiBaseUrl": "https://api.example.com"
    }
}
```

### Discovered Information

- **Environments:** `$shared`, `local`, `production`
- **Variables per environment:** List all variables defined
- **Variable values:** Show values (mask secrets if tagged)

## Analyzing Definitions Folder

The `Definitions/` folder contains shared scripts and class definitions.

### Common Patterns

**Class definitions:**

- `Car.csx` - Domain model classes
- `CarRent.csx` - Entity classes

**Helper scripts:**

- `CarFaker.csx` - Test data generation
- `GenerateNewCar.csx` - Utility functions

**Usage:** Scripts are loaded using `#load "$teapie/Definitions/FileName.csx"`

## Project Analysis Workflow

### Step 1: Locate .teapie Folder

```bash
python scripts/analyze-teapie-project.py --project-root .
```

### Step 2: Analyze Components

The script analyzes:

1. **init.csx** - Custom registrations
2. **env.json** - Environments and variables
3. **Definitions/** - Shared scripts
4. **Structure** - Folder organization

### Step 3: Generate Report

Output includes:

- Custom functions with signatures
- Custom test directives with parameters
- Authentication providers
- Retry strategies
- Available environments
- Shared scripts in Definitions/

## Using Discovered Extensions

### Custom Functions

Use discovered functions in `.http` files:

```http
POST {{ApiBaseUrl}}{{ApiCarsSection}}
X-Build-Number: {{$buildNumber}}
```

### Custom Directives

Use discovered directives in `.http` files:

```http
## TEST-SUCCESSFUL-STATUS: True
## AUTH-PROVIDER: MyAuth
## RETRY-STRATEGY: Default retry
```

### Shared Scripts

Reference shared scripts in test case scripts:

```csharp
#load "$teapie/Definitions/GenerateNewCar.csx"

var car = GenerateCar();
tp.SetVariable("NewCar", car.ToJsonString());
```

## Best Practices

1. **Document custom extensions:** Add comments in `init.csx` explaining purpose
2. **Use consistent naming:** Follow naming conventions for custom functions/directives
3. **Organize Definitions:** Group related scripts in `Definitions/` subfolders
4. **Version control:** Commit `.teapie` folder (exclude `cache/`, `reports/`, `runs/`)

## Resources

- Script: `analyze-teapie-project.py` - Automated project analysis tool
