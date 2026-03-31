# Getting Started

## 📦 Installation

To install **TeaPie CLI**, run:

```sh
dotnet tool install -g TeaPie.Tool
```

To integrate tool within your project, use:

```sh
dotnet add package TeaPie
```

---

## 🤖 Or try your AI Agent

You can use your AI agent to automatically set up TeaPie in your project. Copy and paste the following prompt:

```text
Set up TeaPie in this project by following these steps:

1. Install TeaPie.Tool globally (if not already installed):
   dotnet tool install -g TeaPie.Tool

2. Download TeaPie skills into this project:
   teapie install-skills

3. Initialize TeaPie configuration:
   teapie init

4. Ask the user if they want to create their first TeaPie tests. If yes, use the TeaPie skill knowledge to help create appropriate test cases based on their project structure and needs.
```

---

## ⚡ Quick Start

### **1️⃣ Create a New Test Case**

```sh
teapie generate <test-case-name>
```

To learn more about **test cases**, visit the [Test Case page](./test-case/test-case.md).

### **2️⃣ Run All Test Cases in a Collection**

```sh
teapie test ./demo/Tests
```

Not sure what a **collection** is? Read [about collections](collection.md).

### **3️⃣ Execute a Single Test Case**

```sh
teapie test "./demo/Tests/002-Cars/002-Edit-Car-req.http"
```

Whether you want to run single test case or collection, [read more](running-tests.md).
