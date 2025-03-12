# Initialization Script

|   |   |
|----------------------|----------------|
| **Definition**       | A `.csx` file that initializes essential components before a test case or collection run. |
| **Naming Convention** | `init.csx` *(can be customized if explicitly specified)* |
| **Purpose**         | Sets up necessary configurations before executing (the first) test case. |
| **Example Usage**   | [Demo Initialization Script](https://github.com/Kros-sk/TeaPie/blob/master/demo/init.csx) |

## Specification

Before executing (the first) test case, users can run an **initialization script**.
This script is used for **pre-test setup**, including:

- Setting [**environment variables**](environments.md)
- Defining [**reporters**](reporting.md)
- Configuring [**logging**](logging.md)
- Other necessary pre-execution tasks

By default, TeaPie **automatically detects** and executes the **first `init.csx` script** found in the **collection or parent folder of the test case**.

### **Custom Initialization Script**

Users can specify a **custom initialization script** instead of the default by using the following option:

```sh
-i | --init-script | --initialization-script <path-to-script>
```
