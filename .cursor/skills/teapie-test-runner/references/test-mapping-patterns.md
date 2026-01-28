# Test Mapping Patterns

Patterns and strategies for mapping API endpoints to TeaPie test cases.

## Endpoint Identification Patterns

### 1. Folder-Based Mapping

Tests are often organized by API resource in folder structure:

```
Tests/
├── 001-Customers/          → /customers endpoint
├── 002-Cars/               → /cars endpoint
└── 003-Car-Rentals/        → /rental endpoint
```

**Pattern:** Folder name (without numeric prefix) maps to API resource.

### 2. Variable-Based Mapping

Endpoints are defined using environment variables:

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

### 3. HTTP Method + Path Pattern

Extract endpoint from HTTP request line:

```http
POST {{ApiBaseUrl}}{{ApiCarsSection}}
GET {{ApiBaseUrl}}{{ApiCarsSection}}/{{id}}
PUT {{ApiBaseUrl}}{{ApiCarsSection}}/{{AddCarRequest.response.body.$.Id}}
DELETE {{ApiBaseUrl}}{{ApiCarsSection}}/{{CarId}}
```

**Patterns:**
- `POST {{ApiBaseUrl}}{{ApiCarsSection}}` → `POST /cars`
- `GET {{ApiBaseUrl}}{{ApiCarsSection}}/{{id}}` → `GET /cars/{id}`
- `PUT {{ApiBaseUrl}}{{ApiCarsSection}}/{{id}}` → `PUT /cars/{id}`
- `DELETE {{ApiBaseUrl}}{{ApiCarsSection}}/{{id}}` → `DELETE /cars/{id}`

### 4. Named Request Pattern

Named requests help identify specific operations:

```http
# @name AddCarRequest
POST {{ApiBaseUrl}}{{ApiCarsSection}}

# @name GetCarRequest
GET {{ApiBaseUrl}}{{ApiCarsSection}}/{{id}}

# @name EditCarRequest
PUT {{ApiBaseUrl}}{{ApiCarsSection}}/{{id}}
```

**Pattern:** Request name indicates operation type (Add, Get, Edit, Delete).

## Mapping Strategy

### Step 1: Parse HTTP Files

Extract HTTP method and URI from each request:

```python
# Pseudo-code
for line in http_file:
    if line matches HTTP_METHOD_PATTERN:
        method = extract_method(line)
        uri = extract_uri(line)
        # Resolve variables if possible
        resolved_uri = resolve_variables(uri, env_file)
```

### Step 2: Resolve Variables

Resolve endpoint variables using environment files:

```python
# Pseudo-code
def resolve_endpoint(uri_template, env_file):
    # Replace {{ApiBaseUrl}} with actual value
    # Replace {{ApiCarsSection}} with actual value
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
# Match tests with: PUT {{ApiBaseUrl}}{{ApiCarsSection}}/{{id}}
```

## Example Mappings

### Example 1: Simple CRUD

**API:** `/cars` endpoint

**Tests:**
- `Tests/002-Cars/001-Add-Car-req.http` → `POST /cars`
- `Tests/002-Cars/002-Edit-Car-req.http` → `PUT /cars/{id}`
- `Tests/002-Cars/003-Check-Car-req.http` → `GET /cars/{id}`

**Mapping:** Folder `002-Cars/` contains all tests for `/cars` endpoint.

### Example 2: Nested Resources

**API:** `/cars/{id}/rentals` endpoint

**Tests:**
- `Tests/003-Car-Rentals/001-Rent-Car-req.http` → `POST /rental` (may reference car ID)

**Mapping:** Check request body or variables for car ID references.

### Example 3: Multiple Requests in One File

**File:** `001-Add-Car-req.http`

```http
### Add New Car
# @name AddCarRequest
POST {{ApiBaseUrl}}{{ApiCarsSection}}

### Get New Car
# @name GetNewCarRequest
GET {{ApiBaseUrl}}{{ApiCarsSection}}/{{AddCarRequest.response.body.$.Id}}
```

**Mapping:** Single file contains multiple endpoints:
- `POST /cars` (AddCarRequest)
- `GET /cars/{id}` (GetNewCarRequest)

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
   # Find all HTTP files with PUT method
   grep -r "PUT" Tests/**/*.http
   
   # Find files referencing ApiCarsSection
   grep -r "ApiCarsSection" Tests/**/*.http
   
   # Check folder structure
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
