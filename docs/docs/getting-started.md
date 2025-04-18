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
