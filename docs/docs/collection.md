# Collection

|   |   |
|----------------------|----------------|
| **Definition**       | A directory in file system which contains at least on test case. Collection should encapsulate contextually similar test cases under one roof. Each collection can have inner collection. |
| **Purpose**         | Groups test cases that are somewhat related. |
| **Example Usage**   | [Demo Collection](https://github.com/Kros-sk/TeaPie/tree/master/demo) |

## Structure

Collection is represented by directory, which **contains at least one [test case](test-case/test-case.md)**

When running a collection, you can also reference optional files:

- **[Environment File](environments.md#environment-file)** – Defines environmental variables.
- **[Initialization Script](initialization-script.md)** – Runs before executing the test case.

## Running a Collection

To execute a collection, run:

```sh
teapie
```

This will start collection run of the collection defined by current directory. If you want to execute collection located elsewhere, you can specify either relative or absolte path to collection directory:

```sh
teapie [path-to-collection]
```

To try it out, you can run a `demo` collection. Firstly, **move to demo directory** and then run just **simply run the command**.

For advanced usage, here’s the full command specification:

```sh
teapie test [path-to-collection] [--temp-path <path-to-temporary-folder>] [-d|--debug] [-v|--verbose] [-q|--quiet] [--log-level <minimal-log-level>] [--log-file <path-to-log-file>] [--log-file-log-level <minimal-log-level-for-log-file>] [-e|--env|--environment <environment-name>] [--env-file|--environment-file <path-to-environment-file>] [-r|--report-file <path-to-report-file>] [-i|--init-script|--initialization-script <path-to-initialization-script>]
```

> 💡 **Tip:** You can use the alias `t` or **omit the command name entirely**, since `test` is the **default command** when launching TeaPie.

To view detailed information about each argument and option, run:

```sh
teapie --help
```

## Exploring Collection Structure

If you only want to **inspect the collection structure** without running its tests, you can do so with the following command:

```sh
teapie explore [path-to-collection] [-d|--debug] [-v|--verbose] [-q|--quiet] [--log-level <minimal-log-level>] [--log-file <path-to-log-file>] [--log-file-log-level <minimal-log-level-for-log-file>] [--env-file|--environment-file <path-to-environment-file>] [-i|--init-script|--initialization-script <path-to-initialization-script>]
```

> 💡 **Tip:** You can use aliases `exp` or `e` to run the same command.
