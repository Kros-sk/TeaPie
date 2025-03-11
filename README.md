# TeaPie - API Testing Framework

[![NuGet](https://img.shields.io/nuget/v/TeaPie)](https://www.nuget.org/packages/TeaPie/)
[![License](https://img.shields.io/github/license/Kros-sk/TeaPie)](LICENSE)
[![Build](https://github.com/Kros-sk/TeaPie/actions/workflows/pipeline.yml/badge.svg)](https://github.com/Kros-sk/TeaPie/actions)

TeaPie is a **lightweight API testing framework** designed for **automation-friendly, scriptable API testing** with `.http` files.
It provides **pre-request scripting, post-response validation, retry strategies, authentication support, and custom test directives**.

## 🚀 Features

✅ **Scriptable API Tests** – Define test cases using `.http` files
✅ **Pre-request & Post-response Scripts** – Extend tests with C# scripts
✅ **Custom Authentication Providers** – Supports OAuth2 & user-defined providers
✅ **Retry Strategies** – Customize retry logic for API failures
✅ **Environment & Initialization Scripts** – Manage test variables easily
✅ **Extensible & Open Source** – Register custom test directives, reporters, and more

## 📦 Installation

### Install via NuGet

To install **TeaPie CLI**, use the following command:

```sh
dotnet tool install -g TeaPie.Tool
```

To install the framework in your project:

```sh
dotnet add package TeaPie
```

## ⚡ Quick Start

Create a new test case:

```sh
teapie generate <test-case-name>
```

Run all test cases in a collection:

```sh
teapie test ./Tests
```

Execute a single test case:

```sh
teapie ./Tests/MyTestCase-req.http
```

For more usage details, visit the **[Wiki](https://kros-sk.github.io/TeaPie/docs/introduction.html)**.

## 📖 Documentation

📚 **Complete documentation is available in the** **[Wiki](https://kros-sk.github.io/TeaPie/docs/introduction.html)**.

## 🤝 Contributing

We welcome contributions! Please check out the [Contribution Guide](CONTRIBUTING.md) for details on how to get involved.

## 📝 License

TeaPie is licensed under the **MIT License**. See the [LICENSE](LICENSE) file for details.
