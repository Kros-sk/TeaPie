# Directives

To speed processes up, `TeaPie` comes with various directives. They can be used either in `.http` **[request files](#directives-in-http-files)** or `.csx` **[scripts](#directives-in-csx-scripts)**.

## Directives in `.csx` Scripts

### `#load` Directive

|   |   |
|----------------------|----------------|
| **Syntax** | `#load "<path-to-script>"` |
| **Example Usage**         | `#load "./Definitions/GenerateNewCar.csx"` or [Pre-Request Script with Load Directive](https://github.com/Kros-sk/TeaPie/blob/master/demo/Tests/002-Cars/001-AddCar-init.csx)  |
| **Purpose**         | Referencing another `.csx` script to use its functionality. |
| **Parameters**         | `path-to-script` - *absolute* or *relative* path (relative to parent directory of the script) to another script |
| **❗IMPORTANT❗**         | Referenced script is **automatically executed**. For this reason, rather encapsulate logic in methods, to prevent unwanted execution. |

### `#nuget` Directive

|   |   |
|----------------------|----------------|
| **Syntax** | `#nuget "<package-name>, <version>"` |
| **Example Usage** | `#nuget "AutoBogus, 2.13.1"` or [Script with Nuget Directive](https://github.com/Kros-sk/TeaPie/blob/master/demo/.teapie/Definitions/CarFaker.csx) |
| **Purpose** | Installs a NuGet package to be used in the scripts. |
| **Parameters** | `package-name` - The NuGet package ID. `version` – Version of NuGet package to be installed. |
| **❗IMPORTANT❗** | Even though NuGet packages are installed globally across all scripts, you must use the `using` directive to access them in the code. |

## Directives in `.http` Files

Directives in `.http` files can be divided scope-wise as follows:

- [Authentication Directives](#authentication-directives)
- [Retrying Directives](#retrying-directives)
- [Testing Directives](#testing-directives)

### Authentication Directives

#### `## AUTH-PROVIDER` Directive

|   |   |
|----------------------|----------------|
| **Syntax** | `## AUTH-PROVIDER: [provider-name]` |
| **Example Usage** | `## AUTH-PROVIDER: MyAuth` or `## AUTH-PROVIDER: None` or [Request File with Authentication Directive](https://github.com/Kros-sk/TeaPie/blob/master/demo/Tests/002-Cars/002-Edit-Car-req.http) |
| **Purpose** | Specifies the authentication provider to be used for a request. If no provider is explicitly set, requests default to `"None"` (no authentication). |
| **Parameters** | `provider-name` – The name of the authentication provider to be used for the request. Use `"None"` to disable authentication for a specific request. `OAuth2` is supported natively, but requires prior configuration of its options. |

### Retrying Directives

Retrying directives and their **modifications of retrying strategies** apply **only to the current request** and do not alter the registered retry strategy.

If no retry strategy is explicitly selected, the **default strategy from `Polly.Core`** is used and modified accordingly.

#### `## RETRY-STRATEGY` Directive

|   |   |
|----------------------|----------------|
| **Syntax** | `## RETRY-STRATEGY: <strategy-name>` |
| **Example Usage** | `## RETRY-STRATEGY: Default retry` or [Request File with Retrying Directives](https://github.com/Kros-sk/TeaPie/blob/master/demo/Tests/003-Car-Rentals/001-Rent-Car-req.http) |
| **Purpose** | Selects a predefined retry strategy by name and apply it to the request. |
| **Parameters** | `strategy-name` – The name of a previously registered retry strategy. |

#### `## RETRY-UNTIL-STATUS` Directive

|   |   |
|----------------------|----------------|
| **Syntax** | `## RETRY-UNTIL-STATUS: <status-codes>` |
| **Example Usage** | `## RETRY-UNTIL-STATUS: [200, 201]` or [Request File with Retrying Directives](https://github.com/Kros-sk/TeaPie/blob/master/demo/Tests/003-Car-Rentals/001-Rent-Car-req.http) |
| **Purpose** | Retries the request until one of the specified HTTP status codes is received. |
| **Parameters** | `status-codes` – A list of acceptable status codes that should stop the retry process. |

#### `## RETRY-MAX-ATTEMPTS` Directive

|   |   |
|----------------------|----------------|
| **Syntax** | `## RETRY-MAX-ATTEMPTS: <number>` |
| **Example Usage** | `## RETRY-MAX-ATTEMPTS: 5` or [Request File with Retrying Directives](https://github.com/Kros-sk/TeaPie/blob/master/demo/Tests/003-Car-Rentals/001-Rent-Car-req.http) |
| **Purpose** | Sets the maximum number of retry attempts for the request. |
| **Parameters** | `number` – The maximum number of retries allowed. |

#### `## RETRY-BACKOFF-TYPE` Directive

|   |   |
|----------------------|----------------|
| **Syntax** | `## RETRY-BACKOFF-TYPE: <type>` |
| **Example Usage** | `## RETRY-BACKOFF-TYPE: Linear` or [Request File with Retrying Directives](https://github.com/Kros-sk/TeaPie/blob/master/demo/Tests/003-Car-Rentals/001-Rent-Car-req.http) |
| **Purpose** | Defines the backoff strategy applied between retries. |
| **Parameters** | `type` – Can be `Constant`, `Linear`, `Exponential`, or another strategy supported by [DelayBackoffType](https://www.pollydocs.org/api/Polly.DelayBackoffType.html). |

#### `## RETRY-MAX-DELAY` Directive

|   |   |
|----------------------|----------------|
| **Syntax** | `## RETRY-MAX-DELAY: <hh:mm:ss.fff>` |
| **Example Usage** | `## RETRY-MAX-DELAY: 00:00:03` |
| **Purpose** | Sets the maximum allowed delay between retries. |
| **Parameters** | `hh:mm:ss.fff` – The maximum delay time before retrying a failed request. |

### Testing Directives

#### `## TEST-EXPECT-STATUS` Directive

|   |   |
|----------------------|----------------|
| **Syntax** | `## TEST-EXPECT-STATUS: [status-codes]` |
| **Example Usage** | `## TEST-EXPECT-STATUS: [200, 201]` or [Request File with Testing Directives](https://github.com/Kros-sk/TeaPie/blob/master/demo/Tests/002-Cars/002-Edit-Car-req.http) |
| **Purpose** | Ensures the response status code matches any value in the provided array. |
| **Parameters** | `status-codes` – A list of expected HTTP status codes (as integers). |

#### `## TEST-HAS-BODY` Directive

|   |   |
|----------------------|----------------|
| **Syntax** | `## TEST-HAS-BODY` or `## TEST-HAS-BODY: <should-have-body>` |
| **Example Usage** | `## TEST-HAS-BODY` (Equivalent to `## TEST-HAS-BODY: True`) or [Request File with Testing Directives](https://github.com/Kros-sk/TeaPie/blob/master/demo/Tests/002-Cars/002-Edit-Car-req.http) |
| **Purpose** | Checks if the response contains a body. |
| **Parameters** | `should-have-body` - optional parameter, which determines whether body should or shouldn't contain body. If not specified, `true` is default value. |

#### `## TEST-HAS-HEADER` Directive

|   |   |
|----------------------|----------------|
| **Syntax** | `## TEST-HAS-HEADER: <header-name>` |
| **Example Usage** | `## TEST-HAS-HEADER: Content-Type` or [Request File with Testing Directives](https://github.com/Kros-sk/TeaPie/blob/master/demo/Tests/002-Cars/002-Edit-Car-req.http) |
| **Purpose** | Verifies that the specified header is present in the response. |
| **Parameters** | `header-name` – The name of the HTTP header to check. |

#### Custom Testing Directive

|   |   |
|----------------------|----------------|
| **Syntax** | `## TEST-<directive-name>: [parameter1]; [parameter2]; ...` |
| **Example Usage** | `## TEST-SUCCESSFUL-STATUS: True` or [Request File with Testing Directives](https://github.com/Kros-sk/TeaPie/blob/master/demo/Tests/002-Cars/002-Edit-Car-req.http) |
| **Purpose** | Allows users to define **custom testing directives** with a unique name and an optional list of parameters of various types. |
| **Parameters** | `directive-name` – The custom directive name, appended after the `TEST-` prefix. `parameter[index]` – Optional parameters, delimited by `;`, supporting multiple data types. A directive can have zero or multiple parameters and they can have custom names. |
| **Registration** | Custom test directives must be registered in the script before first use. [Registration Example (CUSTOM TEST DIRECTIVE section)](https://github.com/Kros-sk/TeaPie/blob/master/demo/init.csx). |
