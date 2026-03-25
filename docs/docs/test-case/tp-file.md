# Single-File Format (.tp)

<!-- markdownlint-disable MD060 -->
|   |   |
|----------------------|----------------|
| **Definition**       | An optional alternative to the multi-file format. A `.tp` file combines HTTP requests and C# scripts in one file using section markers. |
| **Naming Convention** | `<test-case-name>.tp` |
| **Purpose**         | Define test cases with all sections in one place. Both `.tp` and multi-file formats are fully supported and can co-exist in the same project; choose based on test case complexity and how you prefer to structure tests. |
| **Example Usage**   | [Health Check](https://github.com/Kros-sk/TeaPie/blob/master/demo/Tests/004-Health/001-Health-Check.tp), [Car Operations](https://github.com/Kros-sk/TeaPie/blob/master/demo/Tests/002-Cars/004-Car-Operations.tp) |
<!-- markdownlint-enable MD060 -->

The `.tp` format is an **optional alternative** to the multi-file format. Both are fully supported and can co-exist in the same project; choose based on test case complexity and how you prefer to structure tests.

## Section Markers

Sections are marked with `---` markers (case-insensitive):

<!-- markdownlint-disable MD060 -->
| Marker | Purpose |
|--------|---------|
| `--- TESTCASE <Name>` | Starts a named test case. Required when defining multiple test cases in one file. |
| `--- INIT` | Pre-request C# script (optional). Same role as `-init.csx`. |
| `--- HTTP` | HTTP request(s) (required). Same format as `.http` files. |
| `--- TEST` | Post-response C# script (optional). Same role as `-test.csx`. |
| `--- END` | Ends the current test case block. |
<!-- markdownlint-enable MD060 -->

## Single Test Case Example

Use `## TEST-EXPECT-STATUS` and other directives in the HTTP section for status code validation. In the TEST section, focus on response body and business logic validation:

```text
--- TESTCASE Health Check

--- HTTP
## TEST-EXPECT-STATUS: [200]
## TEST-HAS-BODY
GET {{ApiBaseUrl}}/health

--- TEST
tp.Test("Health response should contain status field.", async () =>
{
    dynamic responseJson = await tp.Response.GetBodyAsExpandoAsync();
    NotNull(responseJson);
    NotNull(responseJson.status);
});

--- END
```

## Multiple Test Cases in One File

A single `.tp` file can contain multiple test cases, each with its own sections:

```text
--- TESTCASE Add Another Car

--- INIT
#load "$teapie/Definitions/GenerateNewCar.csx"
var car = GenerateCar();
tp.SetVariable("AnotherCar", car.ToJsonString(), "cars");

--- HTTP
## TEST-EXPECT-STATUS: [201]
## TEST-HAS-BODY
# @name AddAnotherCarRequest
POST {{ApiBaseUrl}}{{ApiCarsSection}}
Content-Type: application/json

{{AnotherCar}}

--- TEST
await tp.Test("Newly added car should be returned with an assigned Id.", async () =>
{
    dynamic responseJson = await tp.Responses["AddAnotherCarRequest"].GetBodyAsExpandoAsync();
    NotNull(responseJson);
    True(responseJson.Id > 0);
    tp.SetVariable("AnotherCarId", (long)responseJson.Id, "cars");
});

--- END

--- TESTCASE Get All Cars

--- HTTP
## TEST-EXPECT-STATUS: [200]
## TEST-HAS-BODY
# @name GetAllCarsRequest
GET {{ApiBaseUrl}}{{ApiCarsSection}}

--- TEST
await tp.Test("Response should contain array of cars.", async () =>
{
    var body = await tp.Response.Content.ReadAsStringAsync();
    NotNull(body);
    True(body.StartsWith("[") && body.EndsWith("]"));
});

--- END
```

## Implicit Mode

If no `--- TESTCASE` marker is present, the file is treated as a single test case. The test case name is derived from the filename (e.g. `001-Health-Check.tp` → `001-Health-Check`).

## Rules

- The `--- HTTP` section is **required** for each test case.
- `--- INIT` and `--- TEST` are optional.
- Markers are **case-insensitive** (`--- testcase`, `--- http`, etc.).
- Use `--- END` to terminate each test case when defining multiple test cases.
- HTTP content follows the same conventions as [Request File](request-file.md) (named requests, variables, etc.). The `###` request separator used in `.http` files works normally inside the `--- HTTP` section without any conflict.
- For status code validation, use directives such as `## TEST-EXPECT-STATUS: [200, 201]` in the HTTP section rather than asserting in the TEST script. See [Directives](directives.md) for details.

## When to Use `.tp` vs Multi-File

<!-- markdownlint-disable MD060 -->
| Use `.tp` when… | Use multi-file when… |
|-----------------|----------------------|
| Tests are simpler and short | Scripts are longer and easier to maintain in separate files |
| You prefer everything in one place | You prefer clear separation of concerns |
| Multiple related test cases belong together | Shared scripts are reused across many test cases |
| You want fewer files to manage | You want each script to have its own file for tooling (e.g. IDE support) |
<!-- markdownlint-enable MD060 -->

Collections can mix both formats freely. Use the format that best fits each scenario.
