# Commands

On this page you can check all currently available commands with their arguments and options.

## `compile` command

|   |   |
|----------------------|----------------|
| **Command name**     | `compile`, `comp`, or `c` |
| **Purpose**          | Attempts to compile the script located at the specified path. |

**Full Syntax:**

```sh
teapie compile <path> [--log-file <file>] [--log-file-log-level <level>] [-l|--log-level <level>] [-d|--debug] [-v|--verbose] [-q|--quiet]
```

| **Argument** | **Meaning** | **Mandatory** |
|--------------|-------------|---------------|
| `path`       | Path to the script that should be compiled. | `true` |

| **Option** | **Meaning** | **Default value** |
|------------|-------------|-------------------|
| `--log-file` | Specifies the path to the file where all logs will be saved. | `null` |
| `--log-file-log-level` | Log level for the log file (only applicable if `--log-file` is set). Supported levels: `Trace`, `Debug`, `Information`, `Warning`, `Error`, `Critical`, `None`. | `Information` |
| `-l`, `--log-level` | Log level for console output. Supported levels: `Trace`, `Debug`, `Information`, `Warning`, `Error`, `Critical`, `None`. | `Information` |
| `-d`, `--debug` | Displays debug information. | `false` |
| `-v`, `--verbose` | Displays all available information, including debug details. | `false` |
| `-q`, `--quiet` | Runs the command silently, without displaying any output. | `false` |
| `-h`, `--help` | Prints help information. | – |

## `explore` command

|   |   |
|----------------------|----------------|
| **Command name**     | `explore`, `exp`, or `e` |
| **Purpose**          | Explores the collection or test case located at the given path. |

**Full Syntax:**

```sh
teapie explore [path] [--log-file <file>] [--log-file-log-level <level>] [-l|--log-level <level>] [-d|--debug] [-v|--verbose] [-q|--quiet] [--env-file <file>] [--init-script <script>]
```

| **Argument** | **Meaning** | **Mandatory** |
|--------------|-------------|---------------|
| `path`       | Path to the collection or test case to be explored. Defaults to the current directory. | `false` |

| **Option** | **Meaning** | **Default value** |
|------------|-------------|-------------------|
| `--log-file` | Specifies the path to the file where all logs will be saved. | `null` |
| `--log-file-log-level` | Log level for the log file (only applicable if `--log-file` is set). Supported levels: `Trace`, `Debug`, `Information`, `Warning`, `Error`, `Critical`, `None`. | `Information` |
| `-l`, `--log-level` | Log level for console output. Supported levels: `Trace`, `Debug`, `Information`, `Warning`, `Error`, `Critical`, `None`. | `Information` |
| `-d`, `--debug` | Displays debug information. | `false` |
| `-v`, `--verbose` | Displays all available information, including debug details. | `false` |
| `-q`, `--quiet` | Runs the command silently, without displaying any output. | `false` |
| `--env-file` | Path to the environment definition file. If not provided, the tool will use the first matching `env.json` file found in `.teapie` folder or the collection (respectively parent folder of test case). | Auto-detected |
| `-i`, `--init-script` | Path to an initialization script to be run before the first test case. If not provided, `init.csx` will be auto-discovered in the `.teapie` folder or the collection (respectively parent folder of test case). | Auto-detected |
| `-h`, `--help` | Prints help information. | – |

## `generate` command

|   |   |
|----------------------|----------------|
| **Command name** | `generate`, `gen` or `g` |
| **Purpose**         | Generates new test case with optional files according to provided options. |

**Full Syntax:**

```sh
teapie generate <test-case-name> [path] [-i|--init|--pre-request] [-t|--test|--post-response]
```

| **Argument** | **Meaning** | **Mandatory** |
|------------|-------------|-------------------|
| `test-case-name` | Sets the name for the test case that will be generated. | `true` |
| `path` | Specifies at which path test case should be generated. | `false` |

| **Option** | **Meaning** | **Default value** |
|------------|-------------|-------------------|
| `-i`, `--init`, `--pre-request` | If given (`true`) generates **pre-request script** `<test-case-name>-init.csx` | `false` |
| `-t`, `--test`, `--post-response` | If given (`true`) generates **post-response script** script `<test-case-name>-test.csx` | `false` |
| `-h`, `--help` | Prints help information. | – |

## `test` command

|   |   |
|----------------------|----------------|
| **Command name**     | `test`, `t` or **nothing** (default command for `teapie`) |
| **Purpose**          | Runs tests from the collection or test case at the specified path. If no path is provided, the current directory is used. |

**Full Syntax:**

```sh
teapie test [path] [--temp-path <path>] [-e|--env <envName>] [--env-file <file>] [-r|--report-file <file>] [-i|--init-script <script>] [--no-cache-vars] [--log-file <file>] [--log-file-log-level <level>] [-l|--log-level <level>] [-d|--debug] [-v|--verbose] [-q|--quiet]
```

| **Argument** | **Meaning** | **Mandatory** |
|--------------|-------------|---------------|
| `path`       | Path to the collection or test case which will be tested. Defaults to the current directory. | `false` |

| **Option** | **Meaning** | **Default value** |
|------------|-------------|-------------------|
| `--temp-path` | Temporary path for the application. Defaults to the system temp folder with a TeaPie sub-folder. | Auto-detected |
| `-e`, `--env` | Name of the environment on which the collection or test case will be run. | `null` |
| `--env-file` | Path to a file with environment definitions. If not provided, the tool will use the first matching `env.json` file found in `.teapie` folder or the collection (respectively parent folder of test case). | Auto-detected |
| `-r`, `--report-file` | Path to a file for generating a summary report of test results. If not specified, no report is created. | `null` |
| `-i`, `--init-script` | Path to an initialization script to run before the first test case. If not provided, `init.csx` is auto-discovered. | Auto-detected |
| `--no-cache-vars` | Disables loading and caching variables from/to file. | `false` |
| `--log-file` | Specifies the path to the file where all logs will be saved. | `null` |
| `--log-file-log-level` | Log level for the log file (only applicable if `--log-file` is set). Supported levels: `Trace`, `Debug`, `Information`, `Warning`, `Error`, `Critical`, `None`. | `Information` |
| `-l`, `--log-level` | Log level for console output. Supported levels: `Trace`, `Debug`, `Information`, `Warning`, `Error`, `Critical`, `None`. | `Information` |
| `-d`, `--debug` | Displays debug information. | `false` |
| `-v`, `--verbose` | Displays all available information, including debug details. | `false` |
| `-q`, `--quiet` | Runs the command silently, without displaying any output. | `false` |
| `-h`, `--help` | Prints help information. | – |
