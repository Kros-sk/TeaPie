# TeaPie Skills

This directory contains skills for the TeaPie framework that extend AI agent capabilities when working with TeaPie projects.

## What are skills?

Skills are specialized knowledge and guides that help AI agents work more effectively with specific frameworks or technologies. The TeaPie skill provides comprehensive knowledge about the TeaPie framework, its syntax, CLI commands, test cases, and best practices.

## Using skills in a project

If you want to use the TeaPie skill in another project, the agent can download it from this directory. The agent will automatically determine the correct target location based on the IDE it's working in:

- **Cursor**: `.cursor/skills/teapie/`
- **VS Code**: `.github/skills/teapie/`
- **Other IDEs**: The agent will decide based on standard conventions for that IDE

## Prompt for agent to download skill

```text
Download the TeaPie skill from the GitHub repository into this project.

The skill is located at: https://github.com/Kros-sk/TeaPie/tree/master/.cursor/skills/teapie/

The agent should determine the target location based on the IDE it's working in:
- For Cursor: .cursor/skills/teapie/
- For VS Code: .github/skills/teapie/
- For other IDEs: use standard conventions for that IDE

Download the entire teapie directory (including SKILL.md and all subdirectories with their contents: references/, scripts/, templates/) from the GitHub repository and copy it to the correct target location in the project.
```

## TeaPie skill contents

The TeaPie skill contains:

- Framework overview and capabilities
- Reference documentation for CLI commands, directives, variables, and functions
- Guides for creating and running tests
- Best practices for writing test cases
- Examples and templates