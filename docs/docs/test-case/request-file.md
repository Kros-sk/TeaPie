# Request File

| | |
| --- | --- |
| **Definition** | An `.http` file which contains definition of (multiple) HTTP request(s). |
| **Naming Convention** | `<test-case-name>-req.http` |
| **Purpose** | Definition of HTTP requests which will be executed within test case. |
| **Example Usage** | [Single Request](https://github.com/Kros-sk/TeaPie/blob/master/demo/Tests/001-Customers/001-Add-Customer-req.http), [Multiple (Named) Requests](https://github.com/Kros-sk/TeaPie/blob/master/demo/Tests/002-Cars/001-Add-Car-req.http), [Advanced Request File with Directives](https://github.com/Kros-sk/TeaPie/blob/master/demo/Tests/002-Cars/002-Edit-Car-req.http), [Request File with Retry Directives](https://github.com/Kros-sk/TeaPie/blob/master/demo/Tests/003-Car-Rentals/001-Rent-Car-req.http) |

## Features

A [**request file**](https://learn.microsoft.com/en-us/aspnet/core/test/http-files?view=aspnetcore-9.0) can contain one or more HTTP requests. To separate requests, use the `###` comment line between two requests.

### Named Request

You can also name your requests for easier management by adding a metadata line just before request definition:

```http
# @name RequestName
GET https:/localhost:3001/customers
Content-Type: application/json

{
    "Id": 3,
    "FirstName": "Alice",
    "LastName": "Johnson",
    "Email": "alice.johnson@example.com"
}
```

### Variables

All variables can be used in the request file with the `{{variableName}}` notation.

>💁‍♂️ When you want to use **reference types for variables**, make sure that they override `ToString()` method. During variable resolution, `ToString()` will be called on them.

For **named requests**, you can access request and response data using the following syntax:

```http
{{requestName.(request|response).(body|headers).(*|JPath|XPath)}}

# Example that will fetch 'Id' property from 'AddNewCarRequest' request's JSON body
{{AddNewCarRequest.request.body.$.Id}}
```

This gives you comprehensive access to headers and body content of named requests.

### Templating Loops

A request block can be repeated for every item in a collection using a `{% for %}` ... `{% endfor %}` loop, turning one request definition into many independent requests:

```http
{% for partner in Partners %}
### Create partner {{ forloop.index }}: {{ partner.Name }}
# @name CreatePartner{{ forloop.index }}
## TEST-EXPECT-STATUS: [201]
POST {{ApiBaseUrl}}/partners
Content-Type: application/json

{ "name": "{{ partner.Name }}" }
{% endfor %}
```

`Partners` is a collection set beforehand (typically in the pre-request script via `tp.SetVariable(...)`). Each iteration keeps its own name, directives, retry handling and report entry, exactly as if the block had been copy-pasted N times.

See [Templating Loops](../templating-loops.md) for the full syntax, all supported collection sources, and current limitations.
