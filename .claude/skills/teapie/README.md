# TeaPie skill

![TeaPie](icon.png)

This plugin teaches Claude how to work with [TeaPie](https://github.com/Kros-sk/TeaPie), an open-source CLI framework for API integration testing on .NET. TeaPie tests are plain `.http` files (or single-file `.tp` files) with optional C# scripts for pre-request setup and post-response assertions, so they live next to your backend code and show up in pull requests.

With the plugin installed, Claude can:

- set up TeaPie in a repository (`teapie init`, environments, `init.csx`),
- generate test cases for your API endpoints, including from an OpenAPI specification,
- write assertions with test directives or C# scripts,
- run tests with `teapie test` and explain or fix failures,
- find existing tests for an endpoint and keep test case numbering in order.

## Install

In Claude Code:

```text
/plugin install teapie --marketplace Kros-sk/TeaPie
```

## Requirements

- [.NET 8 or newer](https://dotnet.microsoft.com/download)
- TeaPie CLI: `dotnet tool install -g TeaPie.Tool`

## What the plugin runs and sends

The plugin contains instructions, reference documents, request templates and four local Python helper scripts (`scripts/`) that read and rename test files in your repository. The plugin itself doesn't send any data anywhere.

When Claude follows the instructions, it runs the `teapie` and `dotnet` commands on your machine. `dotnet tool install` downloads TeaPie from NuGet, `teapie install-skills` downloads this skill from GitHub, and `teapie test` sends the HTTP requests defined in your tests to the API endpoints you configure in your environment files.

## Documentation

- Full documentation: [www.teapie.fun](https://www.teapie.fun/)
- Source code and issues: [github.com/Kros-sk/TeaPie](https://github.com/Kros-sk/TeaPie)

## License

MIT, see the [repository license](https://github.com/Kros-sk/TeaPie/blob/master/LICENSE).
