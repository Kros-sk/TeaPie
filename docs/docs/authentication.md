# Authentication

To provide maximum flexibility, an **authentication interceptor** is applied to all outgoing HTTP requests. The **authentication provider** to be used within interceptor **can be specified in scripts** or directly within **request files**.

## Available Authentication Providers

Currently, **TeaPie** supports two authentication providers:

- `None` - **No authentication** is performed. This is the **default behavior** for all requests.
- `OAuth2` - A commonly used authentication method, **natively supported** by the tool.

To use **OAuth2**, it must be configured before executing requests:

```csharp
tp.ConfigureOAuth2Provider(OAuth2OptionsBuilder.Create()
    .WithAuthUrl(tp.GetVariable<string>("AuthServerUrl")) // Required parameter.
    .WithGrantType("client_credentials") // Required parameter.
    .WithClientId("test-client")
    .WithClientSecret("test-secret")
    .AddParameter("custom_parameter", "true") // Add custom parameters if needed.
    .WithAccessTokenVariableName("MyAccessToken") // Access token will be stored to the variable with given name, so it will be accessible during application run. Such a variable is not cached since it is marked with 'secret' and 'no-cache' tags.
    .Build()
);
```

## Registering a Custom Authentication Provider

To use a custom authentication provider, **register it before usage**:

```csharp
tp.RegisterAuthProvider(
    "MyAuth",
    new MyAuthProvider(tp.ApplicationContext)
        .ConfigureOptions(new MyAuthProviderOptions { AuthUrl = authUrl })
);
```

If the **registered provider should also be the default**, use this method instead:

```csharp
tp.RegisterDefaultAuthProvider(
    "MyAuth",
    new MyAuthProvider(tp.ApplicationContext)
        .ConfigureOptions(new MyAuthProviderOptions { AuthUrl = authUrl })
);
```

## Setting a Default Authentication Provider

To specify which **registered authentication provider** should be used for all requests, set it as the default:

```csharp
tp.SetDefaultAuthProvider("MyAuth"); // Sets 'MyAuth' as the default authentication provider.
```

For `OAuth2` there is built-in method:

```csharp
tp.SetOAuth2AsDefaultAuthProvider();
```

If no authentication provider is explicitly set as default, requests will **default to "None"**, meaning no authentication is applied.

## Using a Specific Authentication Provider for a Request

Some requests may require a different authentication mechanism than the default.
To **assign a specific authentication provider** for a request, use this directive in a `.http` file:

```http
## AUTH-PROVIDER: MyAuth
POST {{ApiBaseUrl}}{{ApiCarsSection}}
Content-Type: application/json

...
```

## Disabling Authentication

By default, no authentication is performed. However, if a default authentication provider is set, it applies to all requests.
To **disable authentication** for a specific request, use the pre-defined authentication provider 'None':

```http
## AUTH-PROVIDER: None
POST {{ApiBaseUrl}}{{ApiCarsSection}}
Content-Type: application/json

...
```
