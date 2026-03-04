# TeaPie - API Testing Framework

[![NuGet](https://img.shields.io/nuget/v/TeaPie)](https://www.nuget.org/packages/TeaPie.Tool/)
[![License](https://img.shields.io/github/license/Kros-sk/TeaPie)](LICENSE)
[![Build](https://github.com/Kros-sk/TeaPie/actions/workflows/pipeline.yml/badge.svg)](https://github.com/Kros-sk/TeaPie/actions)

![Logo](logo.png)

TeaPie is a **lightweight API testing framework** designed for **automation-friendly, scriptable API testing** with `.http` files.
It provides **pre-request scripting, post-response validation, retry strategies, flexible authentication, environments support and custom test directives**.

## 📖 Documentation

📚 **Complete documentation is available in the** **[Wiki](https://kros-sk.github.io/TeaPie/docs/introduction.html)**.

## 🚀 Features

✅ **Universal HTTP Requests Definition** – Define HTTP requests using `.http` files

✅ **Pre-Request & Post-Response Scripts** – Extend HTTP request with C# scripts for data

✅ **Custom Authentication Providers** – Supports OAuth2 & user-defined authentication providers

✅ **Retry Strategies** – Customize retry logic for API failures

✅ **Environment Support** – Run your tests on any environment and change it dynamically

✅ **Custom Reporting** - Apart of Console and JUnit XML reporting, you can specify own reporters

✅ **Easy Versioning** – Collections of the tests can be part of (backend) repository and changes are easily visible on PRs

✅ **Extensible & Open Source** – Project is ready-to-be extended due to its pipeline design pattern

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
teapie test demo
```

Execute a single test case:

```sh
teapie test ".\demo\Tests\002-Cars\002-Edit-Car-req.http"
```

For more usage details, visit the **[Wiki](https://kros-sk.github.io/TeaPie/docs/introduction.html)**.

## 🤝 Contributing

We welcome contributions! Please check out the [Contribution Guide](CONTRIBUTING.md) for details on how to get involved.

## 📝 License

TeaPie is licensed under the **MIT License**. See the [LICENSE](LICENSE) file for details.
