# Test Case

|   |   |
|----------------------|----------------|
| **Definition**       | The fundamental unit representing a single test scenario that can be executed independently. It consists of either a `.http` file with optional supporting scripts (multi-file format) or a `.tp` file (single-file format) that define setup, execution, and validation of API tests. Test cases can be grouped into a collection. |
| **Purpose**         | Encapsulates a single test scenario (not only) for isolated execution. |
| **Example Usage**   | [Request File of a Complete Test Case](https://github.com/Kros-sk/TeaPie/blob/master/demo/Tests/002-Cars/001-Add-Car-req.http) |

## Structure

### Request File

Each test case is represented by a **`.http` file**, referred to as the **[Request File](request-file.md)**.
It must contain **at least one HTTP request**, following [**these conventions**](https://learn.microsoft.com/en-us/aspnet/core/test/http-files?view=aspnetcore-9.0).

### Optional Scripts

For more complex test cases, these additional scripts can be included:

- [**Pre-Request Script**](pre-request-script.md) – A `.csx` script for **data setup and initialization** before executing HTTP requests.
- [**Post-Response Script**](post-response-script.md) – A `.csx` script for **validating API responses** and performing assertions - testing.

### Additional Optional Files

When running a test case, you can also reference:

- **[Environment File](../environments.md#environment-file)** – Defines environmental variables.
- **[Initialization Script](../initialization-script.md)** – Runs before executing the test case.

### Alternative: Single-File Format (`.tp`)

TeaPie supports two equivalent ways to define test cases. Use whichever fits your project better.

The [**Single-File Format (`.tp`)**](tp-file.md) combines HTTP requests and C# scripts in one file using section markers. It is an optional alternative to the multi-file format. Both formats are fully supported and can **co-exist in the same collection** — you can freely mix `.http`-based and `.tp`-based test cases within a single project without any configuration changes.

## Running a Test Case

To execute a test case, run:

```sh
teapie <path-to-request-file>
```

To try it out, you can run a test case from the `demo` collection.
The following examples demonstrate both formats:

```sh
# Multi-file format
teapie "./Tests/002-Cars/002-Edit-Car-req.http"

# Single-file format (.tp)
teapie "./Tests/002-Cars/004-Car-Operations.tp"
```

For advanced usage, here’s the full command specification:

```sh
teapie test <path-to-test-case> [--temp-path <path-to-temporary-folder>] [-d|--debug] [-v|--verbose] [-q|--quiet] [--log-level <minimal-log-level>] [--log-file <path-to-log-file>] [--log-file-log-level <minimal-log-level-for-log-file>] [-e|--env|--environment <environment-name>] [--env-file|--environment-file <path-to-environment-file>] [-r|--report-file <path-to-report-file>] [-i|--init-script|--initialization-script <path-to-initialization-script>] [--no-cache-vars|--no-cache-variables]
```

> 💡 **Tip:** You can use the alias `t` or **omit the command name entirely**, since `test` is the **default command** when launching TeaPie.

To view detailed information about each argument and option, run:

```sh
teapie --help
```

## Scaffolding

To create a new test case, use:

```sh
teapie generate <test-case-name> [path] [-i|--init|--pre-request] [-t|--test|--post-response]
```

> 💡 **Shortcut:** You can use aliases `gen` or `g` instead of `generate`.

This command generates the multi-file format in the specified path (or the current directory if no path is provided):

- [**Pre-Request Script**](pre-request-script.md): `<test-case-name>-init.csx`
- [**Request File**](request-file.md): `<test-case-name>-req.http`
- [**Post-Response Script**](post-response-script.md): `<test-case-name>-test.csx`

To **disable pre-request or post-response script generation**, set the `-i` and `-t` options to `false`.

To scaffold a **single-file format (`.tp`)** test case instead, add the `--single-file` flag:

```sh
teapie generate <test-case-name> [path] [-i|--init|--pre-req] [-t|--test|--post-res] [-s|--single-file]
```

When `--single-file` is used, a `.tp` file is generated with the appropriate `### HTTP` section and optional `### INIT` and `### TEST` sections based on `-i` and `-t` flags. See [Single-File Format (`.tp`)](tp-file.md) for details.

## Exploring Test Case Structure

To inspect the structure of a test case **without executing it**, run:

```sh
teapie explore <path-to-test-case> [-d|--debug] [-v|--verbose] [-q|--quiet] [--log-level <minimal-log-level>] [--log-file <path-to-log-file>] [--log-file-log-level <minimal-log-level-for-log-file>] [--env-file|--environment-file <path-to-environment-file>] [-i|--init-script|--initialization-script <path-to-initialization-script>]
```

The path can point to a request file (`.http`) or a single-file test case (`.tp`).

> 💡 **Shortcut:** You can use aliases `exp` or `e` instead of `explore`.
