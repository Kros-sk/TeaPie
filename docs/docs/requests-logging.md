# HTTP Requests Logging

TeaPie can log detailed HTTP request and response information to a separate JSON file for better analysis of requests and debugging.

## Usage

Use the `--requests-log-file` parameter to capture structured logs:

```bash
teapie test my-requests.http --requests-log-file requests.json
```

This creates a `requests.json` file with one JSON object per line (JSONL format).

## What's Included

Each log entry contains:

- **Request details**: Name, method, URL, headers, body, file path
- **Response details**: Status code, reason phrase, headers, body, content type
- **Timing**: Start time, end time, duration in milliseconds
- **Authentication**: Provider type, whether it's default, authentication timestamp
- **Errors**: Array of any errors that occurred
- **Metadata**: Request ID, type tags, source context

## Example

```json
[
  {
    "Timestamp": "2025-12-03T14:06:09.3239022+01:00",
    "Level": "Information",
    "MessageTemplate": "{@RequestLogFileEntry}",
    "Properties": {
      "RequestLogFileEntry": {
        "RequestId": "28ccf092-0240-4d91-8acc-108ef45c8acb",
        "StartTime": "2025-12-03T13:06:09.1820864Z",
        "EndTime": "2025-12-03T13:06:09.3237782Z",
        "DurationMs": 141.6918,
        "Request": {
          "Name": "GetEditedCarRequest",
          "Method": "GET",
          "Uri": "http://localhost:3001/cars/6",
          "Headers": {
            "Authorization": "Bearer authToken"
          },
          "Body": "",
          "ContentType": "text/plain",
          "FilePath": "002-Cars\\002-Edit-Car-req.http",
          "_typeTag": "RequestInfo"
        },
        "Response": {
          "StatusCode": 200,
          "ReasonPhrase": "OK",
          "Headers": {
            "Access-Control-Allow-Origin": "*",
            "Access-Control-Allow-Methods": "GET,POST,PUT,PATCH,DELETE,HEAD,OPTIONS",
            "Access-Control-Allow-Headers": "Content-Type, Origin, Accept, Authorization, Content-Length, X-Requested-With",
            "Date": "Wed, 03 Dec 2025 13:06:09 GMT",
            "Connection": "keep-alive",
            "Keep-Alive": "timeout=5"
          },
          "Body": "{\"Id\":6,\"Brand\":\"Toyota\",\"Model\":\"RAV4\",\"EngineType\":\"3.0 TDI\",\"TransmissionType\":\"Manual\",\"PeopleCapacity\":5,\"Color\":\"silver\",\"Year\":2022,\"DrivenKilometres\":21000,\"Description\":\"\"}",
          "ContentType": "application/json",
          "ReceivedAt": "2025-12-03T13:06:09.3238287Z",
          "_typeTag": "ResponseInfo"
        },
        "Authentication": {
          "ProviderType": "OAuth2Provider",
          "IsDefault": true,
          "AuthenticatedAt": "2025-12-03T13:06:09.3238312Z",
          "_typeTag": "AuthInfo"
        },
        "Errors": [],
        "_typeTag": "RequestLogFileEntry"
      },
      "SourceContext": "HttpRequests",
      "HttpMethod": "GET",
      "Uri": "http://localhost:3001/cars/6",
      "Scope": [
        "HTTP GET http://localhost:3001/cars/6"
      ]
    }
  }
]
```
