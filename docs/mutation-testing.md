# Mutation Testing

TeaPie uses [Stryker.NET](https://stryker-mutator.io/docs/stryker-net/introduction/) for mutation testing to verify the effectiveness of the test suite.

## Overview

Mutation testing systematically introduces small changes (*mutants*) into the source code and checks whether the existing tests detect them. A killed mutant means the tests caught the change; a surviving mutant may indicate a gap in test assertions.

## Quick Start

```bash
# Restore the local tool
dotnet tool restore

# Run mutation testing
dotnet stryker
```

The HTML report is generated in the `StrykerOutput/` directory.

## Configuration

Stryker is configured via [`stryker-config.json`](../stryker-config.json) in the repository root. Key settings:

| Setting | Value | Description |
|---------|-------|-------------|
| `project` | `TeaPie.csproj` | Source project under test |
| `test-projects` | `TeaPie.Tests.csproj` | Test project |
| `reporters` | html, progress, cleartext | Output formats |
| `thresholds.high` | 60 | Score above this is green |
| `thresholds.low` | 40 | Score below this is red |
| `thresholds.break` | 35 | Score below this fails the build |

## CI Integration

Mutation testing runs as a **scheduled weekly workflow** (`.github/workflows/mutation-testing.yml`) every Monday at 03:00 UTC. It can also be triggered manually via `workflow_dispatch`. The HTML report is uploaded as a build artifact.

## Baseline Score

The initial baseline mutation score is **37.83%**, established with Stryker.NET 4.14.0.

| Metric | Count |
|--------|-------|
| Killed | 787 |
| Survived | 707 |
| Timeout | 6 |
| No Coverage | 596 |
| Compile Error | 145 |
| Ignored | 677 |

The relatively low score is expected for an initial baseline. Many surviving mutants are in code paths not yet covered by tests (No Coverage) or in areas like console rendering and pipeline orchestration that are harder to unit test. The score will improve as tests are strengthened over time.

## Thresholds

The thresholds are calibrated against the baseline score of 37.83%:

- **`break` (35%)** — Just below the baseline. If the mutation score drops below this, Stryker exits with a non-zero code and the CI workflow fails. This prevents significant regression.
- **`low` (40%)** — Slightly above the baseline. Scores below this are reported as red/warning, signaling a near-term improvement target.
- **`high` (60%)** — Longer-term goal for a healthy mutation score. Scores above this are reported as green.

These thresholds should be raised as the test suite improves and the mutation score increases.
