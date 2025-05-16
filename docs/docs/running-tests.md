# Running Tests

After generating test cases and writing your tests, you can execute the **main command for testing**:

```sh
teapie
```

TeaPie supports two execution modes:

- **Collection Run** - If a **directory path** is provided, tool runs all **test cases** found in the specified folder and its subfolders.
- **Single Test-Case Run** - If a `.http` **file path** is provided, then tool executes **only that specific test case**.

Both single test case and collection runs follow these two main steps:

1. **Structure Exploration** – TeaPie scans the directory or test-case structure to identify all test cases and related files.
2. **Test Execution** – Each detected test is executed based on the provided configuration.

## Advanced Usage

For more advanced usage, here’s the full command specification:

```sh
teapie test [path-to-collection-or-test-case] [--temp-path <path-to-temporary-folder>] [-d|--debug] [-v|--verbose] [-q|--quiet] [--log-level <minimal-log-level>] [--log-file <path-to-log-file>] [--log-file-log-level <minimal-log-level-for-log-file>] [-e|--env|--environment <environment-name>] [--env-file|--environment-file <path-to-environment-file>] [-r|--report-file <path-to-report-file>] [-i|--init-script|--initialization-script <path-to-initialization-script>] [--no-cache-vars|--no-cache-variables]
```

> 💁‍♂️ You can use alias `t` or **completely omit command name**, since `test` command is considered as **default command** when launching `teapie`.

To view detailed information about each argument and option, run:

```sh
teapie --help
```

## Test Results

**Test results** can be accessed in the following ways:

- **Console output**: Displays a visually structured summary of test results in the end of application run.
- **JUnit XML report**: Use the `-r` or `--report-file` option to generate a report compatible with CI tools.
- **Exit codes**: Useful for scripting and automation:
  - `0` – Successful execution; all tests passed (or no tests were found).
  - `1` – An error occurred during application execution.
  - `2` – Execution succeeded, but some tests failed (commonly treated as a failure in CI/CD).
  - `130` – Premature termination via `Ctrl+C` (note: not fully supported yet).
