# Logging

Logging is essential part of any application. The main logger is exposed as `ILogger` from `Microsoft.Extensions.Logging` and user can use it easily by accessing it via `tp` instance:

```csharp
tp.Logger.LogInformation("I understand logging in TeaPie! Yee!");
```

By default, `TeaPie` uses `Serilog` as the logging provider.

Users can adjust logging levels during application run by using these options:

- **Debug Output (`-d | --debug`)**: Displays more detailed logging.
- **Verbose Output (`-v | --verbose`)**: Displays the most detailed logging.
- **Quiet Mode (`-q | --quiet`)**: Suppresses any output.
- **Logging Options**:
  - **`--log-level`** - Sets the minimal log level for console output.
  - **`--log-file`** - Specifies a path to save logs.
  - **`--log-file-log-level`** - Sets the minimal log level for the log file.
