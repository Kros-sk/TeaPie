---
name: teapie-scaffolding
description: Test case creation, renumbering, and organization for TeaPie framework. Use when: (1) Creating new test cases using scaffolding, (2) Renumbering test cases to maintain workflow order, (3) Inserting gaps in numbering for new tests, (4) Creating complete test cases from OpenAPI specifications, (5) Reorganizing test collections, or (6) When users need help with test case structure and organization.
---

# TeaPie Scaffolding

## Overview

Test case creation, renumbering, and organization for TeaPie framework. Helps maintain proper test case ordering and create complete test cases from API specifications.

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

**Basic test case (in current directory):**

```bash
teapie generate MyTestCase
```

**With pre-request script:**

```bash
teapie generate MyTestCase -i
```

**With post-response script:**

```bash
teapie generate MyTestCase -t
```

**With both scripts:**

```bash
teapie generate MyTestCase -i -t
```

**In specific directory (existing or new):**

```bash
# Existing directory
teapie generate MyTestCase ./Tests/002-Cars

# New directory (will be created automatically)
teapie generate 001-Create-Car ./Tests/004-Cars-Basic

# With prefix in name and scripts
teapie generate 001-Create-Car ./Tests/004-Cars-Basic -i -t
```

**Creating multiple test cases in a new collection:**

```bash
# 1. Create collection directory first (or let generate create it)
mkdir -p ./Tests/004-Cars-Basic

# 2. Generate test cases with proper prefixes
cd ./Tests/004-Cars-Basic
teapie generate 001-Create-Car -i -t
teapie generate 002-Get-Car -t
teapie generate 003-Delete-Car -t

# Or generate from parent directory
teapie generate 001-Create-Car ./Tests/004-Cars-Basic -i -t
teapie generate 002-Get-Car ./Tests/004-Cars-Basic -t
teapie generate 003-Delete-Car ./Tests/004-Cars-Basic -t
```

### Generated Files

The `generate` command creates:

- `<name>-req.http` - Request file (always generated)
- `<name>-init.csx` - Pre-request script (if `-i` is set)
- `<name>-test.csx` - Post-response script (if `-t` is set)

### Naming Convention

**Always use zero-padded numeric prefixes:**

- ✅ `001-Add-Customer-req.http`
- ✅ `002-Edit-Customer-req.http`
- ❌ `1-Add-Customer-req.http` (incorrect sorting)

**Pattern:** `<prefix>-<descriptive-name>-<suffix>.<ext>`

## Renumbering Test Cases

Maintain proper workflow order by renumbering test cases when inserting new ones.

### Understanding Hierarchy

Test cases are organized hierarchically:

- Collections can be nested (subdirectories)
- Each level maintains its own numbering sequence
- Files are executed in alphabetical order

**Example structure:**
```
Tests/
├── 001-Customers/
│   ├── 001-Add-Customer-req.http
│   └── 002-Edit-Customer-req.http
├── 002-Cars/
│   ├── 001-Add-Car-req.http
│   ├── 002-Edit-Car-req.http
│   └── 003-Check-Car-req.http
└── 003-Car-Rentals/
    └── 001-Rent-Car-req.http
```

### Renumbering Workflow

**Scenario 1: Insert test case between existing ones**

If you need to insert a test case between `002-Edit-Car-req.http` and `003-Check-Car-req.http`:

1. Renumber `003-Check-Car-req.http` → `004-Check-Car-req.http`
2. Create new test case as `003-New-Test-req.http`

**Use script:** `scripts/renumber-tests.py` to automate this process.

**Scenario 2: Create gap for future test**

Create a gap in numbering for a test case to be added later:

```bash
python scripts/insert-gap.py --directory ./Tests/002-Cars --after 002 --gap-size 1
```

This renumbers `003-*` files to `004-*`, creating space for `003-*`.

### Renumbering Scripts

Use `scripts/renumber-tests.py` to renumber test cases:

```bash
# Renumber all test cases in directory
python scripts/renumber-tests.py --directory ./Tests/002-Cars

# Renumber starting from specific number
python scripts/renumber-tests.py --directory ./Tests/002-Cars --start 001

# Insert test case at specific position
python scripts/renumber-tests.py --directory ./Tests/002-Cars --insert 003 MyNewTest
```

The script maintains:

- Proper zero-padding
- All related files (`-req.http`, `-init.csx`, `-test.csx`)
- Hierarchical structure

## Creating Tests from OpenAPI

TeaPie doesn't have built-in OpenAPI support. Create complete test cases manually from OpenAPI specifications.

### Workflow

1. **Read OpenAPI specification:**
   - Extract endpoint paths, methods, request/response schemas
   - Identify path parameters, query parameters, request bodies

2. **Create request file (`-req.http`):**
   - Use proper HTTP method and endpoint
   - Include headers (Content-Type, Authorization if needed)
   - Add request body with realistic example data
   - Add testing directives (e.g., `## TEST-EXPECT-STATUS: [201]`) for status code validation

3. **Create pre-request script (`-init.csx`) if needed:**
   - Set up test data
   - Generate variables for dynamic values
   - Prepare authentication tokens

4. **Create post-response script (`-test.csx`):**
   - Assert response body structure and content
   - Check response headers (if not covered by directives)
   - Extract values for subsequent requests
   - **Note:** Status code validation should be done via `## TEST-EXPECT-STATUS` directive in `.http` file, not in C# script

See [OpenAPI to Tests Guide](references/openapi-to-tests.md) for detailed patterns and examples.

### Example: Creating POST /cars Test

**Generated `001-Add-Car-req.http`:**

```http
### Add New Car
# @name AddCarRequest
## TEST-EXPECT-STATUS: [201]
POST {{ApiBaseUrl}}{{ApiCarsSection}}
Content-Type: application/json

{
    "Brand": "Toyota",
    "Model": "RAV4",
    "Year": 2022
}
```

**Generated `001-Add-Car-test.csx`:**

```csharp
await tp.Test("Response should contain car ID.", async () =>
{
    dynamic responseJson = await tp.Responses["AddCarRequest"].GetBodyAsExpandoAsync();
    NotNull(responseJson.Id);
    tp.SetVariable("NewCarId", responseJson.Id, "cars");
});
```

## Best Practices

### Numbering Strategy

1. **Use zero-padding:** Always use `001-`, `002-`, `010-` (not `1-`, `2-`, `10-`)
2. **Group related tests:** Keep related operations together (e.g., all CRUD operations for a resource)

### File Organization

1. **Descriptive names:** Use clear names like `001-Add-Customer-req.http` not `001-Test1-req.http`
2. **Consistent structure:** Follow the same pattern across all test cases

### Code Style

1. **Avoid unnecessary comments:** Do not include verbose explanatory comments in test files. Demo collections contain educational comments (e.g., `// Use 'load' directive...`, `// Name the request...`), but production test code should be clean and self-documenting.
2. **Keep code readable:** Use descriptive variable names and clear test descriptions instead of comments.

### Workflow Order

Order test cases to reflect typical API usage:

1. Create resource (`001-Add-*`)
2. Read resource (`002-Get-*`)
3. Update resource (`003-Edit-*`)
4. Delete resource (`004-Delete-*`)

## Resources

- [OpenAPI to Tests Guide](references/openapi-to-tests.md) - Detailed guide for creating tests from OpenAPI specs
- Scripts: `renumber-tests.py`, `insert-gap.py` - Utilities for renumbering and organizing tests
- Templates: `templates/` - Request file templates
