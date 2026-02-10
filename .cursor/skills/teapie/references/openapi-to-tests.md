# OpenAPI to Tests Guide

Guide for creating complete TeaPie test cases from OpenAPI specifications.

## Overview

TeaPie doesn't have built-in OpenAPI integration. This guide shows how to manually create test cases from OpenAPI specs.

## Finding OpenAPI Specification

**IMPORTANT:** Before creating tests, you need to locate the OpenAPI specification file in the project.

### How to Find OpenAPI Schema

1. **Common locations:**
   - `openapi.json` or `openapi.yaml` in project root
   - `swagger.json` or `swagger.yaml` in project root
   - `docs/` or `documentation/` directories
   - `api/` or `src/api/` directories
   - `openapi` directories
   - `.well-known/` directory
   - Build output directories (`dist/`, `build/`, `out/`)

2. **Search strategies:**
   - Search for files matching patterns: `*openapi*.json`, `*swagger*.json`, `*api*.json`, `{projectname}.json`
   - Check project documentation (README.md, CONTRIBUTING.md)
   - Look for API documentation endpoints (e.g., `/swagger`, `/api-docs`, `/openapi.json`)
   - Check build configuration files for OpenAPI generation settings

3. **Runtime OpenAPI schema (for web services):**
   - Some web services expose OpenAPI schema at runtime via HTTP endpoints
   - **Common runtime endpoints:**
     - `/swagger/v1/swagger.json` (Swashbuckle/ASP.NET Core)
     - `/openapi.json` (OpenAPI standard)
     - `/api-docs` (Swagger UI)
     - `/swagger.json` (generic Swagger)
     - `/v1/api-docs` (versioned APIs)
   - **To get runtime schema:**
     1. Start the web service (if not already running)
     2. Make a GET request to the OpenAPI endpoint
     3. Save the response as JSON file for reference
   - **Example:**
     ```bash
     # Start service, then fetch schema
     curl http://localhost:5000/swagger/v1/swagger.json > openapi.json
     ```
   - Check service configuration files (e.g., `Startup.cs`, `Program.cs`, `appsettings.json`) for Swagger/OpenAPI endpoint configuration

4. **If you cannot locate the OpenAPI specification:**
   - **Ask the user:** "Where can I find the OpenAPI specification file in this project? Is it available as a static file, or do I need to start the service and fetch it from a runtime endpoint?"
   - The user may provide:
     - File path
     - URL to API documentation
     - Runtime endpoint URL (requires service to be running)
     - Instructions on how to generate it
     - Instructions on how to start the service
     - Alternative documentation format

**Note:** This guide uses JSON format for OpenAPI specifications. If you find a YAML file, convert it to JSON first or parse it as YAML.

## Workflow

### Step 1: Analyze OpenAPI Specification

Extract key information:

**Endpoint information:**
- Path: `/cars`
- Method: `POST`
- Path parameters: `{id}` in `/cars/{id}`
- Query parameters: `?filter=active`
- Request body schema
- Response status code (200, 201, 400, etc.)
- Response schemas

**Example OpenAPI:**
```json
{
  "paths": {
    "/cars": {
      "post": {
        "summary": "Add a new car",
        "requestBody": {
          "required": true,
          "content": {
            "application/json": {
              "schema": {
                "type": "object",
                "required": ["Brand", "Model"],
                "properties": {
                  "Brand": {"type": "string"},
                  "Model": {"type": "string"},
                  "Year": {"type": "integer", "minimum": 1900}
                }
              }
            }
          }
        },
        "responses": {
          "201": {
            "description": "Car created",
            "content": {
              "application/json": {
                "schema": {
                  "type": "object",
                  "properties": {
                    "Id": {"type": "integer"},
                    "Brand": {"type": "string"},
                    "Model": {"type": "string"}
                  }
                }
              }
            }
          }
        }
      }
    }
  }
}
```

### Step 2: Create Request File

**File:** `<prefix>-Add-Car-req.http`

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

**Key points:**
- Use named request (`# @name`) for referencing in other requests
- Use environment variables for base URL (`{{ApiBaseUrl}}`)
- Include required headers (`Content-Type`)
- Provide realistic example data matching schema

### Step 3: Create Pre-Request Script (if needed)

**File:** `<prefix>-Add-Car-init.csx`

Use when you need to:
- Generate dynamic test data
- Set up prerequisites
- Prepare authentication tokens

**Example:**
```csharp
#load "$teapie/Definitions/CarFaker.csx"

var car = new CarFaker().Generate();
tp.SetVariable("NewCar", car.ToJsonString(), "cars");
```

### Step 4: Create Post-Response Script

**File:** `<prefix>-Add-Car-test.csx`

Use `JsonContains` for validation and store entire response objects:

```csharp
await tp.Test("Response should contain request data.", async () =>
{
    string requestBody = await tp.Request.Content.ReadAsStringAsync();
    string responseBody = await tp.Response.Content.ReadAsStringAsync();
    
    JsonContains(responseBody, requestBody, "id", "createdAt");
});

await tp.Test("Store created car.", async () =>
{
    string responseBody = await tp.Response.Content.ReadAsStringAsync();
    dynamic response = await tp.Response.GetBodyAsExpandoAsync();
    
    True(response.Id > 0);
    tp.SetVariable("CarId", response.Id);
    tp.SetVariable("CreatedCar", responseBody);
});
```

For GET requests, compare with stored object using `JsonContains`:
```csharp
await tp.Test("Retrieved car should match created car.", async () =>
{
    string responseBody = await tp.Response.Content.ReadAsStringAsync();
    string createdCar = tp.GetVariable<string>("CreatedCar");
    
    JsonContains(responseBody, createdCar, "createdAt");
    tp.RemoveVariable("CreatedCar");
});
```

**Note:** `JsonContains()` requires property names to match exactly. If your API uses camelCase for responses (common with ASP.NET Core) but your requests use PascalCase, `JsonContains` may fail. In such cases, use `CaseInsensitiveExpandoObject` for case-insensitive comparison or JsonElement for precise property-by-property comparison. See [JsonContains Limitations](test-design.md#jsoncontains-limitations) for details.

## Patterns by HTTP Method

### GET Request

**OpenAPI:**
```json
{
  "paths": {
    "/cars/{id}": {
      "get": {
        "parameters": [
          {
            "name": "id",
            "in": "path",
            "required": true,
            "schema": {"type": "integer"}
          }
        ],
        "responses": {
          "200": {
            "description": "Car found"
          }
        }
      }
    }
  }
}
```

**Request file:**
```http
### Get Car
# @name GetCarRequest
## TEST-EXPECT-STATUS: [200]
GET {{ApiBaseUrl}}{{ApiCarsSection}}/{{CarId}}
```

**Test script:**
```csharp
await tp.Test("Response should contain car details.", async () =>
{
    dynamic responseJson = await tp.Responses["GetCarRequest"].GetBodyAsExpandoAsync();
    NotNull(responseJson.Id);
    NotNull(responseJson.Brand);
});
```

### PUT Request

**OpenAPI:**
```json
{
  "paths": {
    "/cars/{id}": {
      "put": {
        "parameters": [
          {
            "name": "id",
            "in": "path",
            "required": true
          }
        ],
        "requestBody": {
          "content": {
            "application/json": {
              "schema": {
                "type": "object",
                "properties": {
                  "Brand": {"type": "string"},
                  "Model": {"type": "string"}
                }
              }
            }
          }
        }
      }
    }
  }
}
```

**Request file:**
```http
### Edit Car
# @name EditCarRequest
## TEST-EXPECT-STATUS: [200]
PUT {{ApiBaseUrl}}{{ApiCarsSection}}/{{CarId}}
Content-Type: application/json

{
    "Brand": "Honda",
    "Model": "Civic"
}
```

**Test script:**
```csharp
await tp.Test("Updated car should reflect changes.", async () =>
{
    dynamic requestJson = await tp.Requests["EditCarRequest"].GetBodyAsExpandoAsync();
    dynamic responseJson = await tp.Responses["EditCarRequest"].GetBodyAsExpandoAsync();

    Equal(requestJson.Brand, responseJson.Brand);
    Equal(requestJson.Model, responseJson.Model);
});
```

### DELETE Request

**OpenAPI:**
```json
{
  "paths": {
    "/cars/{id}": {
      "delete": {
        "parameters": [
          {
            "name": "id",
            "in": "path",
            "required": true
          }
        ],
        "responses": {
          "204": {
            "description": "Car deleted"
          }
        }
      }
    }
  }
}
```

**Request file:**
```http
### Delete Car
# @name DeleteCarRequest
## TEST-EXPECT-STATUS: [204]
DELETE {{ApiBaseUrl}}{{ApiCarsSection}}/{{CarId}}
```

## Handling Path Parameters

**OpenAPI:**
```json
{
  "paths": {
    "/cars/{id}/rentals": {
      "post": {
        "parameters": [
          {
            "name": "id",
            "in": "path",
            "required": true
          }
        ]
      }
    }
  }
}
```

**Request file:**
```http
### Rent Car
# @name RentCarRequest
POST {{ApiBaseUrl}}{{ApiCarsSection}}/{{CarId}}/rentals
Content-Type: application/json

{
    "StartDate": "2024-01-01",
    "EndDate": "2024-01-07"
}
```

Use request variables to reference IDs from previous requests:
```http
### Rent Car
# @name RentCarRequest
POST {{ApiBaseUrl}}{{ApiCarsSection}}/{{AddCarRequest.response.body.$.Id}}/rentals
```

## Handling Query Parameters

**OpenAPI:**
```json
{
  "paths": {
    "/cars": {
      "get": {
        "parameters": [
          {
            "name": "filter",
            "in": "query",
            "schema": {"type": "string"}
          },
          {
            "name": "limit",
            "in": "query",
            "schema": {"type": "integer"}
          }
        ]
      }
    }
  }
}
```

**Request file:**
```http
### Get Cars with Filter
# @name GetCarsFilteredRequest
GET {{ApiBaseUrl}}{{ApiCarsSection}}?filter=active&limit=10
```

Or use variables:
```http
GET {{ApiBaseUrl}}{{ApiCarsSection}}?filter={{FilterValue}}&limit={{Limit}}
```

## Handling Request Bodies

### Simple Object

**OpenAPI:**
```json
{
  "requestBody": {
    "content": {
      "application/json": {
        "schema": {
          "type": "object",
          "properties": {
            "Name": {"type": "string"},
            "Email": {"type": "string"}
          }
        }
      }
    }
  }
}
```

**Request body:**
```json
{
    "Name": "John Doe",
    "Email": "john@example.com"
}
```

### Nested Objects

**OpenAPI:**
```json
{
  "requestBody": {
    "content": {
      "application/json": {
        "schema": {
          "type": "object",
          "properties": {
            "Car": {
              "type": "object",
              "properties": {
                "Brand": {"type": "string"},
                "Model": {"type": "string"}
              }
            }
          }
        }
      }
    }
  }
}
```

**Request body:**
```json
{
    "Car": {
        "Brand": "Toyota",
        "Model": "RAV4"
    }
}
```

### Arrays

**OpenAPI:**
```json
{
  "requestBody": {
    "content": {
      "application/json": {
        "schema": {
          "type": "object",
          "properties": {
            "Items": {
              "type": "array",
              "items": {
                "type": "string"
              }
            }
          }
        }
      }
    }
  }
}
```

**Request body:**
```json
{
    "Items": ["item1", "item2", "item3"]
}
```

## Edge Case Testing from OpenAPI

Extract validation rules and status codes from OpenAPI to create edge case test scenarios.

### Extracting Validation Information

Identify validation rules in OpenAPI schema:
- `required` - Required fields
- `minLength` / `maxLength` - String length constraints
- `minimum` / `maximum` - Numeric range constraints
- `pattern` - Regex validation
- `enum` - Allowed values
- `format` - Special formats (email, uri, date-time, etc.)
- `minItems` / `maxItems` - Array size constraints

### Extracting Status Codes

Review `responses` section for error codes: `400` (Bad Request), `404` (Not Found), `409` (Conflict), `422` (Unprocessable Entity), etc.

### Creating Edge Case Test Scenarios

Based on validations, create tests for:

1. **Missing required fields** - Omit each required field
2. **Invalid values** - Values outside min/max, wrong format, invalid enum
3. **Boundary testing** - Edge values (min-1, max+1, empty string)
4. **Type mismatches** - Wrong data types (string instead of number, etc.)

**Example:**
```http
### Create Product - Missing Required Field
## TEST-EXPECT-STATUS: [400]
POST {{ApiBaseUrl}}/api/products
Content-Type: application/json

{
    "Price": 10.99
}
```

### Testing 400 Responses

When testing 400 responses:
- **Group multiple edge cases** into fewer requests instead of creating separate test files for each scenario
- If OpenAPI shows a standard error response structure, verify response body properties in `-test.csx` file
- Use `## TEST-EXPECT-STATUS: [400]` directive and add assertions to verify error response contains expected properties from OpenAPI

See [Test Design Best Practices](test-design.md#edge-cases-and-error-handling) for details.

## Best Practices

1. **Use realistic data:** Match OpenAPI schema constraints (min/max, patterns, etc.)
2. **Test all status codes:** Create tests for success and error responses
3. **Extract reusable values:** Store IDs and other values in variables for subsequent requests
4. **Validate schemas:** Check response structure matches OpenAPI specification
5. **Use named requests:** Enable request variable references between requests
6. **Follow workflow:** Order tests to reflect typical API usage patterns
