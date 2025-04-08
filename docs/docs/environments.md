# Environments

Environments are a crucial part of automating tests, allowing you to define variables for different scenarios. **TeaPie** supports environments to enhance flexibility and efficiency.

## Environment File

|   |   |
|----------------------|----------------|
| **Definition**       | A `.json` file which contains definitions of environments (named sets of variables). |
| **Naming Convention** | `env.json` *(can be customized if explicitly specified)* |
| **Purpose**         | Defines all available environments, on which can application run. |
| **Example Usage**   | [Demo Environment File](https://github.com/Kros-sk/TeaPie/blob/master/demo/.teapie/env.json) |

To use environments, firstly you must define them in a JSON **environment file**. By default, the tool uses the **first found file** within `.teapie` folder (if exists) or collection (depth-first algorithm) with name `env.json`, respectively first found file in the parent directory of provided test case, when running single test case. However, you can specify a custom environment file by using the following option:

```sh
--env-file|--environment-file <path-to-environment-file>
```

This is example, of how environment file can look like:

```json
{
    "$shared": {
        "ApiBaseUrl": "http://my-car-rental-company.com",
        "ApiCustomersSection": "/customers",
        "ApiCarsSection": "/cars",
        "ApiCarRentalSection": "/rental"
    },
    "local": {
        "ApiBaseUrl": "http://localhost:3001", // Override $shared's variable
        "DebugMode": true // Environment-specific variable
    }
}
```

Each environment is defined by its **name** and **set of variables**.

## Default Environment (`$shared`)

Each environment file **should include** a `$shared` environment, which serves as the **default environment**. Key points about `$shared`:

- **Global Variables**: Variables from `$shared` are always stored in `tp.GlobalVariables`.
- **Environment Variables**: Variables from `$shared` are added to `tp.EnvironmentVariables` only if `$shared` is **selected as the active environment**.
- **Overwriting**: Other environments can override variables defined in `$shared`.

This approach was inspired by [Rest Client for Visual Studio Code](https://marketplace.visualstudio.com/items?itemName=humao.rest-client#environments).

## Active Environment

To specify the environment for running tests, use the `-e` option followed by the environment name:

```sh
-e local
```

> You can also use aliases `--env` and `--environment` for the same purpose.

There are some scenarios where you want to **switch environments in the code** (`.csx` scripts). There you can use:

```csharp
tp.SetEnvironment("local");
```
