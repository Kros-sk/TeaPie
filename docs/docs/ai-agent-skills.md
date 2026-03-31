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

For complete information about TeaPie AI Agent Skills, including how to download and use them in your projects, see the [TeaPie Skills README](https://github.com/Kros-sk/TeaPie/blob/master/.claude/skills/README.md).

The README contains:

- Detailed explanation of what skills are
- Instructions for using skills in your project
- Prompt template for agents to download skills
- Overview of skill contents

---

## 🚀 Quick Start

If you already have TeaPie installed, you can add skills to your project using the CLI command:

```sh
teapie install-skills
```

This will download the skills into `.claude/skills/teapie/` in your project.

You can also ask your AI agent to set everything up. Copy and paste the following prompt:

```text
Install TeaPie.Tool globally (if not already installed) and use the `teapie install-skills` command to download TeaPie skills into this project.

1. Install TeaPie.Tool:
   dotnet tool install -g TeaPie.Tool

2. Download skills:
   teapie install-skills
```
