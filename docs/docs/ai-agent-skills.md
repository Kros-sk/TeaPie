# AI Agent Skills

TeaPie provides **AI Agent Skills** that extend AI agent capabilities when working with TeaPie projects. These skills help AI assistants understand TeaPie's syntax, CLI commands, test structure, and best practices.

## 🤖 What are AI Agent Skills?

AI Agent Skills are specialized knowledge guides that help AI agents work more effectively with specific frameworks or technologies. The TeaPie skill provides comprehensive knowledge about:

- TeaPie framework overview and capabilities
- CLI commands and their usage
- Test case structure and organization
- Directives, variables, and functions
- Best practices for writing tests
- Examples and templates

## 📖 Documentation

For complete information about TeaPie AI Agent Skills, including how to download and use them in your projects, see the [TeaPie Skills README](https://github.com/Kros-sk/TeaPie/blob/master/.cursor/skills/README.md).

The README contains:

- Detailed explanation of what skills are
- Instructions for using skills in your project
- Prompt template for agents to download skills
- Overview of skill contents

---

## 🚀 Quick Start

To use the TeaPie skill in your project, copy and paste the following prompt to your AI agent:

```text
Download the TeaPie skill from the GitHub repository into this project.

The skill is located at: https://github.com/Kros-sk/TeaPie/tree/master/.cursor/skills/teapie/

The agent should determine the target location based on the IDE it's working in:
- For Cursor: .cursor/skills/teapie/
- For VS Code: .github/skills/teapie/
- For other IDEs: use standard conventions for that IDE

Download the entire teapie directory (including SKILL.md and all subdirectories with their contents: references/, scripts/, templates/) from the GitHub repository and copy it to the correct target location in the project.
```

The agent will automatically determine the correct location based on your IDE (e.g., `.cursor/skills/teapie/` for Cursor, `.github/skills/teapie/` for VS Code).
