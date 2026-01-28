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
   - `.well-known/` directory
   - Build output directories (`dist/`, `build/`, `out/`)

2. **Search strategies:**
   - Search for files matching patterns: `*openapi*.json`, `*swagger*.json`, `*api*.json`
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
- Response schemas (200, 201, 400, etc.)

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

Validate response according to OpenAPI schema:

```csharp
tp.Test("Status code should be 201 (Created).", () =>
{
    var statusCode = tp.Responses["AddCarRequest"].StatusCode();
    Equal(201, statusCode);
});

await tp.Test("Response should contain car ID.", async () =>
{
    dynamic responseJson = await tp.Responses["AddCarRequest"].GetBodyAsExpandoAsync();
    
    // Assert required properties exist
    NotNull(responseJson.Id);
    NotNull(responseJson.Brand);
    NotNull(responseJson.Model);
    
    // Assert types match schema
    True(responseJson.Id is long || responseJson.Id is int);
    True(responseJson.Brand is string);
    True(responseJson.Model is string);
    
    // Store ID for subsequent requests
    tp.SetVariable("NewCarId", responseJson.Id, "cars", "ids");
});

await tp.Test("Response Brand should match request Brand.", async () =>
{
    dynamic requestJson = await tp.Requests["AddCarRequest"].GetBodyAsExpandoAsync();
    dynamic responseJson = await tp.Responses["AddCarRequest"].GetBodyAsExpandoAsync();
    
    Equal(requestJson.Brand, responseJson.Brand);
});
```

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
GET {{ApiBaseUrl}}{{ApiCarsSection}}/{{CarId}}
```

**Test script:**
```csharp
tp.Test("Status code should be 200 (OK).", () =>
{
    Equal(200, tp.Responses["GetCarRequest"].StatusCode());
});

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
PUT {{ApiBaseUrl}}{{ApiCarsSection}}/{{CarId}}
Content-Type: application/json

{
    "Brand": "Honda",
    "Model": "Civic"
}
```

**Test script:**
```csharp
tp.Test("Status code should be 200 (OK).", () =>
{
    Equal(200, tp.Responses["EditCarRequest"].StatusCode());
});

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
DELETE {{ApiBaseUrl}}{{ApiCarsSection}}/{{CarId}}
```

**Test script:**
```csharp
tp.Test("Status code should be 204 (No Content).", () =>
{
    Equal(204, tp.Responses["DeleteCarRequest"].StatusCode());
});

tp.Test("Response should not have body.", () =>
{
    False(tp.Responses["DeleteCarRequest"].HasBody());
});
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

## Validating Responses

### Status Code Validation

```csharp
tp.Test("Status code should match OpenAPI spec.", () =>
{
    var statusCode = tp.Responses["RequestName"].StatusCode();
    // Check against expected status codes from OpenAPI
    True(statusCode == 200 || statusCode == 201);
});
```

### Response Schema Validation

```csharp
await tp.Test("Response should match OpenAPI schema.", async () =>
{
    dynamic responseJson = await tp.Responses["RequestName"].GetBodyAsExpandoAsync();
    
    // Check required properties
    NotNull(responseJson.Id);
    NotNull(responseJson.Name);
    
    // Check types
    True(responseJson.Id is long || responseJson.Id is int);
    True(responseJson.Name is string);
    
    // Check constraints (if applicable)
    if (responseJson.Id is long id)
    {
        True(id > 0);
    }
});
```

### Response Header Validation

```csharp
tp.Test("Response should have Content-Type header.", () =>
{
    var headers = tp.Responses["RequestName"].Headers();
    True(headers.ContainsKey("Content-Type"));
    Equal("application/json", headers["Content-Type"].First());
});
```

## Error Response Testing

**OpenAPI:**
```json
{
  "responses": {
    "400": {
      "description": "Bad request",
      "content": {
        "application/json": {
          "schema": {
            "type": "object",
            "properties": {
              "Error": {"type": "string"},
              "Code": {"type": "string"}
            }
          }
        }
      }
    }
  }
}
```

**Test script:**
```csharp
tp.Test("Status code should be 400 (Bad Request).", () =>
{
    Equal(400, tp.Responses["RequestName"].StatusCode());
});

await tp.Test("Error response should contain error details.", async () =>
{
    dynamic errorJson = await tp.Responses["RequestName"].GetBodyAsExpandoAsync();
    NotNull(errorJson.Error);
    NotNull(errorJson.Code);
});
```

## Best Practices

1. **Use realistic data:** Match OpenAPI schema constraints (min/max, patterns, etc.)
2. **Test all status codes:** Create tests for success and error responses
3. **Extract reusable values:** Store IDs and other values in variables for subsequent requests
4. **Validate schemas:** Check response structure matches OpenAPI specification
5. **Use named requests:** Enable request variable references between requests
6. **Follow workflow:** Order tests to reflect typical API usage patterns
