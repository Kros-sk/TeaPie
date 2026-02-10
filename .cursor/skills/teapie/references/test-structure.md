# Test Structure Reference

Complete reference for TeaPie test case and collection structure.

## Test Case Structure

### Required Files

- **`<name>-req.http`** - Request file containing HTTP request(s). Must follow Microsoft HTTP file conventions.

### Optional Files

- **`<name>-init.csx`** - Pre-request script for data setup and initialization
- **`<name>-test.csx`** - Post-response script for validation and assertions

### File Naming Convention

**Pattern:** `<prefix>-<descriptive-name>-<suffix>.<ext>`

**Examples:**

- `001-Add-Customer-req.http`
- `001-Add-Customer-init.csx`
- `001-Add-Customer-test.csx`
- `002-Edit-Customer-req.http`

**Naming rules:**

- Use zero-padded numeric prefixes (`001-`, `002-`, `003-`) for consistent alphabetical ordering
- Use kebab-case for descriptive names
- Suffixes: `-req` (request), `-init` (pre-request), `-test` (post-response)

## Collection Structure

### Definition

A collection is a directory containing at least one test case. Collections can be nested (subdirectories).

### Hierarchical Organization

```text
Tests/
├── 001-Customers/              # Collection (numbered)
│   ├── 001-Add-Customer-req.http
│   └── 001-Add-Customer-test.csx
├── 002-Cars/                   # Another collection
│   ├── 001-Add-Car-init.csx
│   ├── 001-Add-Car-req.http
│   ├── 001-Add-Car-test.csx
│   ├── 002-Edit-Car-req.http
│   ├── 002-Edit-Car-test.csx
│   ├── 003-Check-Car-init.csx
│   ├── 003-Check-Car-req.http
│   └── 003-Check-Car-test.csx
└── 003-Car-Rentals/            # Another collection
    ├── 001-Rent-Car-init.csx
    ├── 001-Rent-Car-req.http
    └── 001-Rent-Car-test.csx
```

### Scenario Organization with Nested Directories

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

### Collection-Level Files

Collections can contain optional files:

- **`env.json`** - Environment definitions file (auto-detected)
- **`init.csx`** - Collection initialization script (auto-detected)

### Execution Order

Test cases are executed in alphabetical order (ensured by numeric prefixes):

1. Structure exploration - Scans for test cases and related files
2. Initialization script execution (if present)
3. For each test case (in alphabetical order):
   - Pre-request script execution (`-init.csx`)
   - HTTP request(s) execution (`-req.http`)
   - Post-response script execution (`-test.csx`)

## .teapie Folder Structure

The `.teapie` folder contains shared resources and is typically located at the repository root.

### Structure

```text
.teapie/
├── init.csx                    # Global initialization script (auto-detected)
├── env.json                    # Environment definitions (auto-detected)
├── Definitions/                # Shared class definitions and helper scripts
│   ├── Car.csx
│   ├── CarFaker.csx
│   └── GenerateNewCar.csx
├── cache/                      # Auto-generated: Cached scripts, variables, NuGet packages
├── reports/                    # Auto-generated: Test reports
└── runs/                       # Auto-generated: Request/response artifacts (planned)
```

### Using Shared Scripts

Reference shared scripts using the `$teapie` wildcard:

```csharp
#load "$teapie/Definitions/GenerateNewCar.csx"
#load "$teapie/Definitions/CarFaker.csx"
```

The `$teapie` wildcard resolves to the absolute path of the `.teapie` folder, avoiding relative path issues.

## Test Case Discovery

TeaPie discovers test cases by:

1. Recursively scanning directories (depth-first)
2. Finding all `.http` files ending with `-req.http`
3. Matching associated scripts by name:
   - `{test-case-name}-init.csx` for pre-request
   - `{test-case-name}-test.csx` for post-response
4. Processing files in alphabetical order (ensured by numerical prefixes)

## Best Practices

### Numeric Prefixes

**Always use zero-padded numeric prefixes:**

- ✅ `001-`, `002-`, `003-`, `010-`, `011-`
- ❌ `1-`, `2-`, `3-`, `10-` (incorrect sorting)

This ensures consistent ordering across all environments and prevents incorrect sorting (e.g., `10` before `2`).

### Folder Organization

**Organize by API resource or feature:**

- `001-Customers/` - Tests for `/customers` endpoint
- `002-Cars/` - Tests for `/cars` endpoint
- `003-Car-Rentals/` - Tests for `/rental` endpoint

### Test Case Naming

**Use descriptive names that indicate the operation:**

- `001-Add-Customer-req.http` - Creates a new customer
- `002-Edit-Customer-req.http` - Updates an existing customer
- `003-Delete-Customer-req.http` - Deletes a customer

### Multiple Requests in One File

A single `.http` file can contain multiple requests separated by `###`:

```http
### Add New Car
# @name AddCarRequest
POST {{ApiBaseUrl}}{{ApiCarsSection}}
Content-Type: application/json

{{NewCar}}

### Get New Car
# @name GetNewCarRequest
GET {{ApiBaseUrl}}{{ApiCarsSection}}/{{AddCarRequest.response.body.$.Id}}
```

Use named requests (`# @name RequestName`) to reference data from previous requests.
