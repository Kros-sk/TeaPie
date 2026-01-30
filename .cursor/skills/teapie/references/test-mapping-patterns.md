# Test Mapping Patterns

Patterns and strategies for mapping API endpoints to TeaPie test cases.

TeaPie supports multiple approaches to defining endpoints in HTTP files. Before creating or modifying tests, always check the existing project style and maintain consistency.

## Detecting Project Style

**Before creating new tests, check existing project style:**

1. Examine `.teapie/env.json` - does it contain endpoint path variables (e.g., `ApiCarsSection`, `ApiCustomersSection`)?
2. Check existing HTTP files - do they use `{{ApiBaseUrl}}{{ApiXxxSection}}` or direct URLs like `{{ApiBaseUrl}}/cars`?

**Match the existing approach:**
- If the project uses environment variables for endpoint paths, use them consistently
- If the project uses direct URLs (only `ApiBaseUrl` in env), use direct URLs
- If the project doesn't exist yet, choose based on project needs (see "Choosing an Approach" section) or ask for user.

## Endpoint Identification Patterns

### 1. Folder-Based Mapping

Tests are often organized by API resource in folder structure:

```text
Tests/
├── 001-Customers/          → /customers endpoint
├── 002-Cars/               → /cars endpoint
└── 003-Car-Rentals/        → /rental endpoint
```

**Pattern:** Folder name (without numeric prefix) maps to API resource.

### 2. Direct URL Mapping

Endpoints can be defined with direct paths in HTTP files:

**Environment file (`env.json`):**
```json
{
    "$shared": {
        "ApiBaseUrl": "http://api.example.com"
    }
}
```

**HTTP file:**
```http
POST {{ApiBaseUrl}}/cars
GET {{ApiBaseUrl}}/cars/{{id}}
PUT {{ApiBaseUrl}}/cars/{{CarId}}
DELETE {{ApiBaseUrl}}/cars/{{CarId}}
```

**Pattern:** `{{ApiBaseUrl}}/<path>` - base URL from environment, path directly in HTTP file.

### 3. Variable-Based Mapping (Optional)

Endpoints may also be defined using environment variables for both base URL and paths:

**Environment file (`env.json`):**
```json
{
    "$shared": {
        "ApiBaseUrl": "http://api.example.com",
        "ApiCustomersSection": "/customers",
        "ApiCarsSection": "/cars",
        "ApiCarRentalSection": "/rental"
    }
}
```

**HTTP file:**
```http
POST {{ApiBaseUrl}}{{ApiCarsSection}}
```

**Resolved endpoint:** `POST http://api.example.com/cars`

**Pattern:** `{{ApiBaseUrl}}{{Api<Resource>Section}}` maps to `/resource` endpoint.

**Note:** This approach is useful for multi-environment setups where paths might differ between environments.

### 4. HTTP Method + Path Pattern

Extract endpoint from HTTP request line. Both approaches work:

**Direct URL approach:**
```http
POST {{ApiBaseUrl}}/cars
GET {{ApiBaseUrl}}/cars/{{id}}
PUT {{ApiBaseUrl}}/cars/{{AddCarRequest.response.body.$.Id}}
DELETE {{ApiBaseUrl}}/cars/{{CarId}}
```

**Variable-based approach:**
```http
POST {{ApiBaseUrl}}{{ApiCarsSection}}
GET {{ApiBaseUrl}}{{ApiCarsSection}}/{{id}}
PUT {{ApiBaseUrl}}{{ApiCarsSection}}/{{AddCarRequest.response.body.$.Id}}
DELETE {{ApiBaseUrl}}{{ApiCarsSection}}/{{CarId}}
```

**Patterns (both resolve to):**
- `POST .../cars` → `POST /cars`
- `GET .../cars/{{id}}` → `GET /cars/{id}`
- `PUT .../cars/{{id}}` → `PUT /cars/{id}`
- `DELETE .../cars/{{id}}` → `DELETE /cars/{id}`

### 5. Named Request Pattern

Named requests help identify specific operations:

**Direct URL approach:**
```http
# @name AddCarRequest
POST {{ApiBaseUrl}}/cars

# @name GetCarRequest
GET {{ApiBaseUrl}}/cars/{{id}}

# @name EditCarRequest
PUT {{ApiBaseUrl}}/cars/{{id}}
```

**Variable-based approach:**
```http
# @name AddCarRequest
POST {{ApiBaseUrl}}{{ApiCarsSection}}

# @name GetCarRequest
GET {{ApiBaseUrl}}{{ApiCarsSection}}/{{id}}

# @name EditCarRequest
PUT {{ApiBaseUrl}}{{ApiCarsSection}}/{{id}}
```

**Pattern:** Request name indicates operation type (Add, Get, Edit, Delete).

## Choosing an Approach

**Priority: Always match existing project style first.**

If creating a new project, consider:

| Approach | When to Use |
|----------|-------------|
| **Direct URL** | Simple projects, single environment, clear/readable tests, quick prototyping |
| **Variable-based** | Multi-environment setups, paths differ between environments, centralized path management |
| **Hybrid** | Base URL in env (`ApiBaseUrl`), paths directly in HTTP files - best of both worlds |

**Hybrid approach (recommended for most projects):**
- Store only `ApiBaseUrl` in `env.json`
- Write paths directly in HTTP files: `{{ApiBaseUrl}}/cars`
- Easy to read, easy to change environments

## Mapping Strategy

### Step 0: Detect Project Style

Before parsing or creating tests, detect the project's endpoint style:

```python
# Pseudo-code
def detect_project_style(env_file, http_files):
    # Check env.json for path variables
    env_vars = load_env_file(env_file)
    has_path_vars = any(
        key.startswith('Api') and key.endswith('Section')
        for key in env_vars.keys()
    )

    # Check existing HTTP files for pattern usage
    uses_path_vars = check_http_files_for_pattern(
        http_files, r'\{\{Api\w+Section\}\}'
    )

    if has_path_vars and uses_path_vars:
        return "variable-based"
    else:
        return "direct-url"
```

**Use detected style consistently when creating new tests.**

### Step 1: Parse HTTP Files

Extract HTTP method and URI from each request:

```python
# Pseudo-code
for line in http_file:
    if line matches HTTP_METHOD_PATTERN:
        method = extract_method(line)
        uri = extract_uri(line)
        # Resolve variables if present
        resolved_uri = resolve_variables(uri, env_file)
```

### Step 2: Resolve Variables (if present)

Resolve endpoint variables using environment files when they exist:

```python
# Pseudo-code
def resolve_endpoint(uri_template, env_file):
    # Replace {{ApiBaseUrl}} with actual value
    # Replace {{ApiCarsSection}} with actual value (if used)
    # Direct paths like /cars don't need resolution
    # Handle request variables if needed
    return resolved_uri
```

### Step 3: Extract Endpoint Pattern

Normalize endpoint to pattern:

```python
# Examples:
# "/cars/123" → "/cars/{id}"
# "/cars/abc" → "/cars/{id}"
# "/customers/456/details" → "/customers/{id}/details"
```

### Step 4: Match to Changed API

Compare extracted patterns to changed API:

```python
# Changed API: PUT /cars/{id}
# Match tests with either:
#   - PUT {{ApiBaseUrl}}/cars/{{id}}           (direct URL)
#   - PUT {{ApiBaseUrl}}{{ApiCarsSection}}/{{id}}  (variable-based)
```

## Example Mappings

### Example 1: Simple CRUD (Direct URL)

**API:** `/cars` endpoint

**Tests:**
- `Tests/002-Cars/001-Add-Car-req.http` → `POST /cars`
- `Tests/002-Cars/002-Edit-Car-req.http` → `PUT /cars/{id}`
- `Tests/002-Cars/003-Check-Car-req.http` → `GET /cars/{id}`

**HTTP file content:**
```http
# @name AddCarRequest
POST {{ApiBaseUrl}}/cars
Content-Type: application/json

{"brand": "Toyota", "model": "Camry"}
```

**Mapping:** Folder `002-Cars/` contains all tests for `/cars` endpoint.

### Example 2: Simple CRUD (Variable-Based)

**API:** `/cars` endpoint

**env.json:**
```json
{
    "$shared": {
        "ApiBaseUrl": "http://api.example.com",
        "ApiCarsSection": "/cars"
    }
}
```

**HTTP file content:**
```http
# @name AddCarRequest
POST {{ApiBaseUrl}}{{ApiCarsSection}}
Content-Type: application/json

{"brand": "Toyota", "model": "Camry"}
```

**Mapping:** Same folder structure, different URL style.

### Example 3: Multiple Requests in One File

**File:** `001-Add-Car-req.http`

**Direct URL approach:**
```http
### Add New Car
# @name AddCarRequest
POST {{ApiBaseUrl}}/cars

### Get New Car
# @name GetNewCarRequest
GET {{ApiBaseUrl}}/cars/{{AddCarRequest.response.body.$.Id}}
```

**Mapping:** Single file contains multiple endpoints:
- `POST /cars` (AddCarRequest)
- `GET /cars/{id}` (GetNewCarRequest)

### Example 4: Nested Resources

**API:** `/cars/{id}/rentals` endpoint

**Tests:**
- `Tests/003-Car-Rentals/001-Rent-Car-req.http` → `POST /cars/{id}/rentals`

**HTTP file:**
```http
# @name RentCarRequest
POST {{ApiBaseUrl}}/cars/{{CarId}}/rentals
Content-Type: application/json

{"startDate": "2024-01-15", "endDate": "2024-01-20"}
```

**Mapping:** Check request body or variables for car ID references.

## Implementation Notes

### Variable Resolution

Variables are resolved in this order:
1. Global (`$shared` environment)
2. Environment (collection-specific)
3. Collection (during collection run)
4. Test Case (deleted after test case ends)

### Request Variables

Request variables reference previous requests:
- `{{AddCarRequest.response.body.$.Id}}` - Extract ID from AddCarRequest response
- `{{RequestName.request.headers.Content-Type}}` - Access request headers

When mapping, consider that some endpoints depend on data from previous requests.

### Path Parameters

Path parameters can be:
- Literal values: `/cars/123`
- Variables: `/cars/{{CarId}}`
- Request variables: `/cars/{{AddCarRequest.response.body.$.Id}}`

Normalize to pattern: `/cars/{id}`

## Finding Tests for Changed API

**Workflow:**

1. **Identify changed endpoint:**
   - Method: `PUT`
   - Path: `/cars/{id}`
   - Changes: Request body schema modified

2. **Search for matching tests:**
   ```bash
   # Find all HTTP files with PUT method and /cars path
   grep -r "PUT.*\/cars" Tests/**/*.http

   # Alternative: Find files with PUT method
   grep -r "PUT" Tests/**/*.http

   # If project uses variable-based approach, also search for:
   grep -r "ApiCarsSection" Tests/**/*.http

   # Check folder structure (folder name often matches resource)
   ls Tests/002-Cars/
   ```

3. **Run relevant tests:**
   ```bash
   # Run entire collection
   teapie test Tests/002-Cars

   # Run specific test case
   teapie test Tests/002-Cars/002-Edit-Car-req.http
   ```

4. **Verify coverage:**
   - Check test results
   - Ensure all affected endpoints are tested
   - Update tests if API contract changed
