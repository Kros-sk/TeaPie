# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

TeaPie is a lightweight CLI-based API testing framework for >=.NET 8. It executes tests defined as `.http` files with optional C# scripts (`.csx`) for pre-request setup and post-response validation. Installed as a dotnet global tool (`TeaPie.Tool`).

## Build & Test Commands

```sh
dotnet restore                    # Restore dependencies
dotnet build                      # Build all projects
dotnet build -warnaserror         # Build as CI does (warnings are errors)
dotnet test                       # Run all tests
dotnet test --filter "FullyQualifiedName~ClassName.MethodName"  # Run a single test
```

Tests use **xUnit** with **FluentAssertions** and **NSubstitute** for mocking. Test project: `tests/TeaPie.Tests/`.

CI builds with `dotnet build --configuration Release --no-restore -warnaserror` — all warnings must be resolved before merging.

## Solution Structure

- **`src/TeaPie/`** — Core library (NuGet package `TeaPie`). Contains all framework logic.
- **`src/TeaPie.DotnetTool/`** — CLI entry point (NuGet tool `TeaPie.Tool`). Thin wrapper with Spectre.Console CLI commands (`test`, `explore`, `generate`, `init`, `compile-script`, `clear-cache`).
- **`tests/TeaPie.Tests/`** — Unit tests with demo fixtures in `Demo/` subdirectories.

## Architecture

### Pipeline Pattern

The core execution model is a **pipeline of steps** (`IPipelineStep`). `ApplicationBuilder` configures DI services and assembles the pipeline via `ApplicationStepsFactory`, which creates different step sequences for different modes (test execution, structure exploration, script compilation).

Steps execute sequentially against an `ApplicationContext`. The pipeline supports dynamic step insertion during execution.

### Key Subsystems (all under `src/TeaPie/`)

- **`Pipelines/`** — Pipeline infrastructure (`IPipeline`, `IPipelineStep`, `StepsCollection`)
- **`StructureExploration/`** — Discovers and organizes test collections from the file system. Maps `.http` files and their associated scripts into a `CollectionStructure` tree.
- **`Http/`** — Parses `.http` files, executes HTTP requests, handles auth (`Auth/`), retries (`Retrying/`), headers
- **`Scripts/`** — Loads, pre-processes (resolves `#load` and `#r "nuget:..."` directives), compiles, and executes `.csx` C# scripts via Roslyn
- **`TestCases/`** — Manages test case lifecycle (init/execute/finish). A test case = `.http` file + optional `-init.csx` and `-test.csx` scripts
- **`Testing/`** — Test execution engine, test directives, assertions (extends xUnit Assert), result tracking
- **`Variables/`** — Variable system for sharing state between scripts and across requests
- **`Environments/`** — Environment configuration (env.json files, environment switching)
- **`Reporting/`** — Test result reporters (console, JUnit XML, custom)
- **`Functions/`** — Built-in functions available to scripts
- **`Templating/`** — Expands Fluid (Liquid) templates (`{% for %}`, `{% if %}`, `{% assign %}`) in `.http`/`.tp` request content after the pre-request script and before requests are split into steps. Only files containing a supported tag are templated.

### Test Case Convention

A test case is a group of files sharing a base name:

- `{name}-req.http` — The HTTP request definition (required)
- `{name}-init.csx` — Pre-request C# script (optional)
- `{name}-test.csx` — Post-response validation C# script (optional)

### DI & Service Registration

Each subsystem has its own `Setup.cs` file with extension methods for `IServiceCollection`. The root `Setup.cs` in `src/TeaPie/` orchestrates all registrations via `AddTeaPie()`.

## Code Style

- .NET 8, C# with nullable enabled, implicit usings
- Central package version management (`ManagePackageVersionsCentrally`)
- Roslynator analyzers enforced; `EnforceCodeStyleInBuild` is on
- `.editorconfig`: 4-space indent, CRLF line endings for C# files, UTF-8 with BOM
- Uses file-scoped namespaces, expression-bodied members, and pattern matching throughout

## Branch Naming

Use `feature/`, `bugfix/`, `refactoring/`, or `docs/` prefixes. Main branch is `master`.

## Documentation

DocFX-based docs live in `docs/`. Build with `docfx "./docs/docfx.json"`, serve with `docfx serve _site`.
