# Technologies

Here are chosen technologies for TeaPie framework:

- `C# .NET` - main technology on which whole application is based
- `.csx scripts` - scripts, which will be written by end-user
- `CSharpScript` - NuGet from Roslyn, which can handle and execute `.csx` scriots
- `.http request files` - end-user will write HTTP request in these files
- `Spectre.Console` - library used for creating CLI application
- `dotnet tool` - platform on which framework will be available
- `Polly` - library for retrying
- `Microsoft.Logging.Abstraction.ILogger` - interface for logging
  - `Serilog` - default logger implementation
- `Scriban` - possible templating language, e.g. for variables collecting
- `JUnit XML` - default format for reporting service
- `JSON` - default format for configuration files and DTOs
