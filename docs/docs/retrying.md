# Retrying

TeaPie allows users to **register named retry strategies**, which can be referenced later for request retrying. These strategies define **how and when** requests should be retried. The structure of the retry strategy is defined by the object `RetryStrategyOptions<HttpResponseMessage>` from [Polly.Core](https://github.com/App-vNext/Polly) library.

## Registering a Retry Strategy

To create a reusable retry strategy, register it in a script:

```csharp
tp.RegisterRetryStrategy("Default retry", new RetryStrategyOptions<HttpResponseMessage>
{
    MaxRetryAttempts = 3, // Maximum number of retry attempts
    Delay = TimeSpan.FromMilliseconds(500),  // Default delay between retries.
    BackoffType = DelayBackoffType.Exponential, // Defines how delay increases (e.g., Linear, Exponential)
    ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
        .HandleResult(response => response.StatusCode == HttpStatusCode.InternalServerError) // Condition determining whether a request should be retried (Retry, when status code is 500).

    // More properties can be configured (see 'Polly.Core.Retry.RetryStrategyOptions<TResult>' for more information)
});
```

## Using Retry Directives in `.http` Files

Within `.http` files, you can apply a retry strategy using **retry directives**. These directives allow fine-tuning of request retrying behavior.

```http
# @name GetCarRequest
# Applying a named retry strategy and overriding specific properties.
## RETRY-STRATEGY: Default retry   # Uses already registered "Default retry" strategy
## RETRY-MAX-ATTEMPTS: 5           # Overrides max retry attempts
## RETRY-BACKOFF-TYPE: Linear      # Changes backoff strategy for this request
## RETRY-MAX-DELAY: 00:00:03       # Limits the maximum delay between retries
## RETRY-UNTIL-STATUS: [200, 201]  # Adds condition - retry until one of given status codes is received
GET {{ApiBaseUrl}}{{ApiCarsSection}}/{{RentCarRequest.request.body.$.CarId}}
```

>💁‍♂️ These **modifications apply only to the current request** and do not alter the registered retry strategy. If **no retry strategy is selected, default one** (from `Polly.Core`) is taken and modified accordingly.

Using retry strategies ensures **more resilient test execution**, handling **temporary failures** gracefully while preventing unnecessary retries.
