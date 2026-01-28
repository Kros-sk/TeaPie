---
name: teapie-test-runner
description: Intelligent test execution and API-to-test mapping for TeaPie framework. Use when: (1) Running TeaPie tests after API changes, (2) Finding which tests cover specific API endpoints, (3) Detecting API changes and identifying affected tests, (4) Executing specific test cases or collections, (5) Generating test reports, or (6) When users request test execution or need to understand test coverage for APIs.
---

# TeaPie Test Runner

## Overview

Intelligent test execution and API-to-test mapping for TeaPie framework. Helps identify which tests to run when APIs change and executes tests efficiently.

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

## Finding Tests for API Endpoints

When an API endpoint changes, identify which tests cover it:

1. **Parse HTTP files** to extract endpoints
2. **Resolve variables** from environment files
3. **Match endpoint patterns** to changed APIs
4. **Find corresponding test files**

### Endpoint Extraction Patterns

Tests map to APIs through:

**Folder structure:**

- `001-Customers/` → `/customers` endpoint
- `002-Cars/` → `/cars` endpoint

**Variable patterns:**

- `{{ApiBaseUrl}}{{ApiCarsSection}}` → `/cars`
- `{{ApiBaseUrl}}{{ApiCarsSection}}/{{id}}` → `/cars/{id}`

**HTTP method + path:**

- `POST {{ApiBaseUrl}}{{ApiCarsSection}}` → `POST /cars`
- `GET {{ApiBaseUrl}}{{ApiCarsSection}}/{{id}}` → `GET /cars/{id}`

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

## API Change Detection Workflow

When an API is changed or created:

1. **Identify changed endpoint:**
   - Parse changed API code/files
   - Extract endpoint path and HTTP method
   - Note any path parameter changes

2. **Find affected tests:**
   - Search for endpoint patterns in `.http` files
   - Check folder structure for resource-based organization
   - Match variable patterns (e.g., `{{ApiCarsSection}}`)

3. **Run relevant tests:**

   ```bash
   # Run specific collection
   teapie test ./Tests/002-Cars

   # Run specific test case
   teapie test ./Tests/002-Cars/001-Add-Car-req.http

   # Run with report
   teapie test ./Tests/002-Cars -r cars-report.xml
   ```

4. **Verify results:**
   - Check exit code (0 = success, 2 = tests failed)
   - Review console output
   - Analyze JUnit XML report if generated

## Test Execution Strategies

### Run All Tests

```bash
teapie test
```

### Run Specific Collection

```bash
teapie test ./Tests/002-Cars
```

### Run Single Test Case

```bash
teapie test ./Tests/002-Cars/001-Add-Car-req.http
```

### Run with Environment

```bash
teapie test -e production
teapie test -e local --env-file ./custom-env.json
```

### Generate Reports

```bash
# JUnit XML report
teapie test -r junit-report.xml

# With logging
teapie test --log-file test.log --log-level Debug
```

## Verbose Output for Debugging

Use the `-v` (verbose) flag to get detailed output for investigating test failures and understanding test execution:

```bash
# Verbose output for better investigation
teapie test -v

# Verbose output for specific test case
teapie test -v ./Tests/002-Cars/001-Add-Car-req.http

# Verbose output with report
teapie test -v -r report.xml
```

**Verbose output includes:**

- Detailed request/response information
- Variable resolution and values
- Script execution details
- HTTP headers and body content
- Error stack traces
- Test assertion details
- Variable assignments and updates

**When to use verbose mode:**

- Debugging test failures
- Investigating unexpected behavior
- Understanding variable flow between requests
- Verifying request/response content
- Troubleshooting authentication issues
- Analyzing retry attempts

## Exit Codes

- `0` - Success (all tests passed)
- `1` - Error during execution
- `2` - Some tests failed
- `130` - Premature termination (Ctrl+C)

Check exit codes in CI/CD pipelines to determine test status.

## Resources

- [Test Mapping Patterns](references/test-mapping-patterns.md) - Detailed patterns for API-to-test mapping
- Scripts: `find-tests-for-api.py`, `parse-http-file.py` - Utilities for finding and parsing tests
