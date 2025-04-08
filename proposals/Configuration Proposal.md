# Configuration

Configuration for Test Runner can be present in three places:

1. `Configuration file` - by convention it is file with the name that contain postfix `*-config.json`
2. `Scripts` - end-user will be able to programmatically alter the configuration inside pre-request and post-response scripts
3. `Request File Directives` - request `.http` file supports various directives, which can change the configuration

Framework will take configuration from these places in indicated order. Each configuration place is able to re-write previously loaded configuration (not entirely necessary, usually only some parts).

Configuration will have few segments:

- `Retrying`
- `Delay`
- `Order`

## Retrying

For retrying we will use [Polly](https://github.com/App-vNext/Polly) framework, which supports all necessary functionality for retrying.

### Retry Policy

Standard notation for retrying policy is as follows:

```json
{
  "retryPolicy": {
     "maxRertryAttempts": 3,
     "backoffType": "Exponential",
     "delay": "00:00:03"
  }
}
```

As mentioned above, we will let user to set configuration programmatically, too:

```csharp
tp.RetryPolicy = new RetryStrategyOptions
{
    BackoffType = DelayBackoffType.Exponential,
    MaxRetryAttempts = 4,
    Delay = TimeSpan.FromSeconds(3),
};
```

If end user want to alter retry policy within request `.http` file, he can do it like this:

```http
### Request Name
## RETRY-MAX-ATTEMPTS: 5
## RETRY-MAX-BACKOFFTYPE: Exponential
## RETRY-MAX-DELAY: 00:00:03
GET {{URL}}
```

### Retrying Functions

When writing API tests, there are few situations that occur frequently, so it is useful to have support for them available:

```csharp
public void RetryUntilStatusCode(params int[] statusCodes) // Will retry request until one of the 'statusCodes' is returned.
public void RetryUntilTestsPass(params int[] statusCodes) // Will retry request until all of the tests are successful
public void Retry() // This function will retry current request(s)
```

These functions look like they will possibly never end - for prevention there is upper limit of re-tries defined in **retry policy**. This doesn't apply on `Retry()` function, since it is up to user, when he will invoke this function - **so be careful with this one!**

End-user can trigger conditional retrying in .http as well:

```http
### Request Name 1
## RETRY-UNTIL-TESTS-PASS
GET {{URL}}

### Request Name 2
## RETRY-UNTIL-STATUS-CODE: [200, 404]
GET {{URL}}
```

## Delay and Timeout

When testing API, sometimes it is useful to simulate some kind of latency. To achieve this, end-user will be able to configure it according his needs:

```json
{
    "delayRequestFor": number // in milliseconds
}
```

Sometimes it is one of the key measurements to check, if the response is sent in reasonable time. So it is great to have possibility to set timeout explicitly. On the other hand, some requests can take more time to evaluate, but their response is critical, so you want to disable timeout for this one:

```json
{
    "timeoutPolicy": {
        "requestTimeout": number, // in milliseconds
        "disableTimeout": boolean
    }
}
```

Programmatically, it would look like this:

```csharp
tp.DelayRequest(2200);
tp.SetTimeout(1200);
tp.DisableTimeout();
tp.EnableTimeout();
```

End-user can set these rules also in the `.http` file:

```http
### Request Name 3
## DELAY: 2200
GET {{URL}}

### Request Name 4
## TIMEOUT: 1200
GET {{URL}}

### Request Name 5
## DISABLE-TIMEOUT
GET {{URL}}
```

## Test Cases Order

Normally, all tests run in alphabetical order. If user want to specify the order or exclude some test cases, he can do it in configuration file as follows:

```json
{
    "order": [], // desired order of test cases, enumerated by names
    "skip": [] // names of test cases to be ignored
}
```

Programmatically, it can be achieved in initial script, like this:

```csharp
tp.SetRunner(runner => {
    runner.SetOrder(
        "Test 1",
        "Test 25",
        "Auto Test");

    runner.Skip(
        "Ignor Me Test",
        "Test 0"
    );
})
```

If end-user want to disable one specific request, he can do it easily by adding directive:

```http
### Request Name
## SKIP
GET {{URL}}
```
