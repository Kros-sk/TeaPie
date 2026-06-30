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
| `thresholds.high` | 80 | Score above this is green |
| `thresholds.low` | 50 | Score below this is red |
| `thresholds.break` | 37 | Score below this fails the build |

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

A score of ~38% is **low**. In industry practice, mutation scores typically fall into these ranges:

| Range | Assessment |
|-------|------------|
| **80%+** | Good — strong test suite that catches most regressions |
| **60–79%** | Acceptable — reasonable safety net, room to improve |
| **40–59%** | Below average — significant gaps in test assertions |
| **Below 40%** | Low — most mutations survive undetected |

The main contributors to the low score are the high **No Coverage** count (596 mutants in untested code) and the large number of **Survived** mutants (707) in areas like console rendering, pipeline orchestration, and file I/O that lack assertions. Improving coverage of core logic and adding targeted assertions will have the biggest impact.

## Thresholds

The thresholds are set to prevent regression and drive improvement:

- **`break` (37%)** — At the baseline. The score is already low, so any regression fails the CI build immediately. This ensures the mutation score can only go up.
- **`low` (50%)** — Mid-term improvement target. Reaching this means the test suite catches at least half of all mutations, a meaningful milestone.
- **`high` (80%)** — Long-term goal aligned with industry standards for a good mutation score. Scores above this are reported as green.

These thresholds should be raised as the test suite improves and the mutation score increases.
