---
name: teapie
description: Expert knowledge on TeaPie framework for API integration testing. Use when working with TeaPie projects, API testing, .http files, C# test scripts (.csx), test collections, directives, variables, functions, authentication, retrying, or when users need guidance on TeaPie CLI commands, test structure, or framework capabilities.
---

# TeaPie Framework

## Overview

TeaPie (TEsting API Extension) is a lightweight CLI tool for API testing that combines `.http` files with C# scripts for comprehensive integration testing. This skill provides expert knowledge on all TeaPie capabilities, syntax, and best practices.

## Quick Reference

### Test Case Structure

Each test case consists of:

- **`<name>-req.http`** (required) - HTTP request file
- **`<name>-init.csx`** (optional) - Pre-request script for setup
- **`<name>-test.csx`** (optional) - Post-response script for validation

### Naming Convention

Use zero-padded numeric prefixes for ordering:

- `001-Add-Customer-req.http`
- `002-Edit-Customer-req.http`
- `003-Delete-Customer-req.http`

### CLI Commands

See [CLI Commands Reference](references/cli-commands.md) for complete command documentation.

**Common commands:**

```bash
teapie test [path]              # Run tests (default command)
teapie generate <name> [-i] [-t] # Scaffold new test case
teapie init                      # Initialize .teapie folder
teapie explore [path]            # Explore collection structure
```

### Directives

**In `.http` files:**

- `## AUTH-PROVIDER: <name>` - Set authentication provider
- `## RETRY-STRATEGY: <name>` - Select retry strategy
- `## RETRY-UNTIL-TEST-PASS: <test_name>` - Retry until all test assertions pass
- `## RETRY-UNTIL-STATUS: [200, 201]` - Retry until status codes
- `## TEST-EXPECT-STATUS: [200, 201]` - Assert status code
- `## TEST-HAS-BODY` - Assert response has body
- `## TEST-HAS-HEADER: <name>` - Assert header exists

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

## Running Tests

Execute tests with:

```bash
teapie test [path] [-e <env>] [-r <report-file>]
```

Tests run in alphabetical order (ensured by numeric prefixes).

## Test Structure

See [Test Structure Reference](references/test-structure.md) for collection organization, hierarchical structure, and file naming conventions.

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

- [CLI Commands Reference](references/cli-commands.md) - Complete command documentation
- [Directives Reference](references/directives.md) - All directives with examples
- [Variables & Functions Reference](references/variables-functions.md) - Variable system and functions
- [Test Structure Reference](references/test-structure.md) - Test case and collection structure
