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

### Claude Code plugin

In Claude Code, install the TeaPie plugin from the repository marketplace:

```text
/plugin install teapie --marketplace Kros-sk/TeaPie
```

On older Claude Code versions, add the marketplace first, then install the plugin:

```text
/plugin marketplace add Kros-sk/TeaPie
/plugin install teapie@teapie
```

The plugin is updated together with the repository, so you don't need to download the skill again after a new release.

### CLI command

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

## 📄 Documentation for LLMs

The documentation is also published as [llms.txt](https://www.teapie.fun/llms.txt), a short index that AI agents can read to find the relevant pages without crawling the whole site.
