# Variables

TeaPie provides a **multi-level variable system**, allowing users to define and manage variables across different scopes.
Each variable name can contain **letters, digits, underscores (`_`), dollar signs (`$`), periods (`.`), and hyphens (`-`)**.

## Variable Levels

TeaPie supports **four levels of variables**, determining **priority and scope**:

| Level | Scope & Behavior |
|-------|----------------|
| **Global** | Available across all test cases and collections. These **originate from the default `$shared` environment** – [learn more](environments.md#default-environment-shared). |
| **Environment** | Available across all test cases within a collection **specific to the selected environment** – [learn more](environments.md#active-environment). |
| **Collection** | Available **during whole collection run**. |
| **Test Case** | Available **only within a specific test case**. They are deleted once the test-case execution ends. |

### **Variable Resolution Order**

TeaPie resolves variables **from the highest level to the lowest**.
For example, if a variable `foo` is defined at the **global level**, its value **can be overridden** at the **environment, collection, or test-case level**.

---

## Working with Variables

TeaPie provides **APIs** for accessing, checking, modifying, and removing variables at different levels.

### **Accessing Variables at Specific Levels**

The `TeaPie` instance (`tp`) provides access to all levels of variables:

```csharp
var globalVar = tp.GlobalVariables.Get<int>("MyGlobalVariable");
var envVar = tp.EnvironmentVariables.Get<string>("MyEnvVariable");
var collVar = tp.CollectionVariables.Get<bool>("MyCollectionVariable");
var testVar = tp.TestCaseVariables.Get<DateTime>("MyTestCaseVariable");
```

To explore the full **API of variables collections**, see [TeaPie.Variables.VariablesCollection](xref:TeaPie.Variables.VariablesCollection).

---

### **General Variable Access**

When accessing variables **without specifying a level**, TeaPie searches **from the highest level to the lowest** and returns the **first match** found.

```csharp
if (!tp.ContainsVariable("IsDevlabEnvironment")) {
    tp.SetVariable("IsDevlabEnvironment", false);
}

var myVar = tp.GetVariable("IsDevlabEnvironment");
tp.RemoveVariable("IsDevlabEnvironment");
tp.RemoveVariablesWithTag("temp"); // Removes all variables with such a tag.
```

> **Note:**
>
> - `SetVariable<T>()` **stores variables at the collection level by default**.
> - `RemoveVariable()` deletes a variable **from all levels where it exists**.

Some of these methods belong to the [TeaPie](xref:TeaPie.TeaPie) class, while others are **extension methods** available in [TeaPie.Variables.TeaPieVariablesExtensions](xref:TeaPie.Variables.TeaPieVariablesExtensions).

---

## Variable Tagging

To enhance usability, TeaPie allows **tagging variables**.
Tags help organize variables into categories and **enable bulk deletion**.

### **Example Usage**

```csharp
tp.SetVariable("PersonalNumber", 777, "test");
tp.SetVariable("Password", "2444666666", "test");
tp.SetVariable("Country", "Slovakia", "production");
tp.SetVariable("City", "Zilina", "production");

tp.RemoveVariablesWithTag("test"); // Removes 'PersonalNumber' and 'Password', but keeps 'Country' and 'City'.
```

## Request Variables

Request variables **enable data sharing across multiple requests** within the **same `.http` file**.
This feature is **limited to a single test case** and requires **named requests**.

> **Request variables are based on:**
>
> - **REST Client for Visual Studio Code (Request Variables section)**
> - **Visual Studio 2022 Release Notes (Web - Request Variables in HTTP files)**
> This syntax follows common conventions to provide a **standardized experience**.

### **Using Request Variables**

#### **1️⃣ Naming a Request**

Before accessing request variables, the request must be **named**:

```http
# @name NewBook
GET www.my-page.com
...
```

#### **2️⃣ Request Variable Syntax**

```plaintext
{{requestName.(request|response).(body|headers).(*|JPath|XPath)}}
```

#### **3️⃣ Example Usage**

```http
# Example: Extract the `id` property from `NewBook` request's JSON response
{{NewBook.response.body.$.id}}
```

This syntax enables access to **request headers and body content** of named requests.

## Variables Caching

To improve usability, **TeaPie caches variables after each run**. At the **start of each run**, it checks for an existing variable cache file. If found, **the variables are loaded into memory**.

If you want to **exclude** a variable **from being cached**, make sure to **remove** this variable before the run finishes.

To completely **disable** variable caching and loading, use the `--no-cache-variables` aliased as `--no-cache-vars` option.
