# Contributing to TeaPie  

Thank you for considering a contribution to **TeaPie**! 🎉  
We appreciate your help in making this project better. Whether you're fixing bugs, adding features, improving documentation, or suggesting ideas, your contributions are welcome!  

## 📜 Code of Conduct

By participating in this project, you agree to follow our [Code of Conduct](CODE_OF_CONDUCT.md).  

## 🛠 How to Contribute  

### 1️⃣ Fork & Clone the Repository  

```sh
git clone https://github.com/Kros-sk/TeaPie.git
cd TeaPie
```

### 2️⃣ Set Up the Project  

Install dependencies:  

```sh
dotnet restore
```

Run tests to ensure everything works:  

```sh
dotnet test
```

### 3️⃣ Create a New Branch  

Always create a new branch for your contributions:  

```sh
git checkout -b feature/your-feature-name
```

#### **Branch Naming Convention**

Use one of the following **categories**:  

- `feature/your-feature-name`
- `bugfix/your-bugfix-name`
- `refactoring/your-refactoring-name`
- `docs/your-docs-update`

### 4️⃣ Make Your Changes  

✔ Follow the project's **code style** (naming conventions, formatting, etc.).  
✔ Ensure **unit tests** pass and write new tests if needed.  
✔ Keep changes **small and focused** (one feature or fix per PR).  
✔ Update **documentation** if applicable.  

### 5️⃣ Commit & Push  

```sh
git commit -m "Add feature X to improve Y"
git push origin feature/your-feature-name
```

### 6️⃣ Create a Pull Request  

Go to the [TeaPie Repository](https://github.com/Kros-sk/TeaPie) and create a **Pull Request (PR)**:  

- Clearly **describe your changes** and why they are needed.  
- Reference any **related issues** (e.g., `Fixes #123`).  
- Follow the PR template and **keep discussions constructive**.  

### ✅ PR Completion Checklist  

✔ Changes follow **OOP & SOLID principles**  
✔ No **warnings or errors**  
✔ If a **new feature** was added, `demo` is updated accordingly  
✔ Related **unit tests** are added or updated  
✔ **Documentation is updated** if necessary  

## 🏠 Developing Locally  

### Run `demo` Collection

The `demo` collection covers **a wide range of functionality**, making it ideal for debugging.

To run all test cases in the **demo collection**:  

```sh
dotnet run test "../../demo" # Runs 'teapie test' on the demo collection
```

To test a single case:  

```sh
teapie "./Tests/2. Cars/EditCar-req.http" --env-file "../../demo-env.json" -i "../../init.csx"
```

After **adding new features** or **bringing breaking changes**, it is **crucial to update `demo` collection** accordingly.

### Updating Documentation  

[DocFX](https://dotnet.github.io/docfx/) is used for generating documentation.  

**Documentation files** are stored in the `docs` directory, where each **section** has its own `.md` file, which is converted into an `.html` page.  

To **add a new section** or **modify the menu structure**, update the `toc.yml` file.  

---

### **Checking Documentation Changes Locally**  

#### **1️⃣ Generate the Documentation Website**

Before previewing changes, generate the site by running:  

```sh
docfx "./docs/docfx.json"
```  

This command creates the final website output inside the `_site` folder (intentionally ignored in `.gitignore`).  

#### **2️⃣ Serve the Website Locally**

To preview the generated documentation locally, start a local server:  

```sh
docfx serve _site
```  

By default, the documentation will be available at **`http://localhost:8080`**, but check the CLI output for confirmation.  

#### **3️⃣ Updating Content After Making Changes**

If you modify any `.md` file, restart the site and rebuild the documentation:  

```sh
docfx build
```  

#### **4️⃣ Reflecting API/Code Changes in Documentation**

If API changes affect the documentation, run:  

```sh
docfx metadata
docfx build
```  

### Installing the Tool Locally

For the most cases it is not required and using `dotnet run` is enough, but in the case you need to install tool locally for development purposes, follow these steps:

1. Navigate to the project directory:  

   ```sh
   cd "../src/TeaPie.DotnetTool"
   ```

2. Pack the project in **Release** mode:  

   ```sh
   dotnet pack -c Release
   ```

3. Copy the `.nupkg` file to your local NuGet feed (adjust version if needed):  

   ```sh
   copy "./bin/Release/TeaPie.Tool.1.0.0.nupkg" "path/to/your/local/nuget/feed"
   ```

4. Install the tool globally:  

   ```sh
   dotnet tool install -g TeaPie.Tool
   ```

### Setting up a Local NuGet Feed  

If you don’t have a local NuGet feed, create one:  

```sh
mkdir "path/to/your/new/local/feed/directory"
dotnet nuget add source "path/to/your/local/feed" --name LocalNuGetFeed
```

## 🤝 Contribution Guidelines  

✔ **Bug Fixes & Features** – Open an **issue first** before making major changes.  
✔ **Documentation** – Keep it **clear and consistent** with Markdown formatting.  
✔ **Testing** – Ensure tests pass and **write new tests** when necessary.  
✔ **Code Quality** – Follow best practices, keep PRs **focused**, and maintain **clean commit history**.  

## 💬 Need Help?  

If you need assistance:  

- Open an [Issue](https://github.com/Kros-sk/TeaPie/issues).  
- Start a discussion in [GitHub Discussions](https://github.com/Kros-sk/TeaPie/discussions).  
- Email: [grochal@kros.sk](mailto:grochal@kros.sk)  

We appreciate your time and effort in improving `TeaPie`! 🚀  

**Happy coding!** 💻🎯
