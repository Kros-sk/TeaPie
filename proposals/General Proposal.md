# TeaPie (TEst API Extension)

Is framework dedicated to ease work with API testing.

## Acceptance criteria

- **CLI-based** application
- **Convention-based** by default, possibility for **parametrization**
- **Readable changes** in tests within **pull-requests**
- **Support for collections**, which will be independently runnable
- Possibility to have **pre-request** and **post-response** scripts for each _http_ request, respecitevely for each collection
- **Support** for more, switchable **environments**, with **global variables** included
- **Comprehensive logging**
- **Comprehensive reporting system**
- **Comprehensive documentation/manual**
- Easy addition of new tests/collections (e.g. by parametrized command)
- **Based** in familiar **backend technology (C#)**
- Native support for **re-trying**
- Native support for **service-bus** testing
- **Smart authorization** - e.g. _OAuth_
- **Intuitive file structure**
- **Optimized performance** – after the main logic is implemented
- **Test runner** will have current context with run-time variables
- **Hierarchical variables** with the possibility of over-write values
- Possibility to **ignore test/collection**, e.g. by custom attribute or in configuration files
- Possibility to **specify order of the tests/collections** (other than default-alphabetical)
- **Native support for seeding** before test/collection
- Possibility to run **single http request manually**
- Possibility to alter **order of the tests according to response** result/**branching**
- **Re-usable scripts**

## Definitions

**Script** - indepedently runnable piece of code. In our context, these are `.csx` files

**Request** - HTTP request, which is stored in _.http file_.

**Test Case** - logically encapsulates _pre-request script(s) HTTP request(s) within .http file, post-response script(s), testing scenarios, documentation and configuration._ None of these parts is mandatory except HTTP request. Test Case can have all its dependencies in one folder (default setting), or it can be distributed. By convention, one folder contains one Test Case, but it is possible for folder to contain more test cases altogether.

**Collection** - set of test cases, which are somehow (contextually) connected.

**End-user** - final user of the framework. Usually a programmer or QA, who writes tests for API.

## To consider

- Check possibility of adding `.md` documentation to tests (e.g. by using comments)
  - Commenting is bit trickier, since **multi-line comments** are not natively supported and you have to work-around to achieve them. When you use 3 hashtags, it automatically thinks, that you want to make request.
  - Maybe usage of double `##` can be interesting for our parser, although this is in conflict with `.md` format of **headers**, since these are also marked by hashtags
- Consider **usage of solution**, since it is the same thing as collection, only difference is in abstraction
- Consider inputs of QA from different company (Descartes)
- Random data seeding (AutoBogus)

## Important Notes

- Usage of `"."` in .http file variables is **forbidden**, so we have to **ban usage of** `"."` **for variables names.**
- For our special directives and notation within `.http file`, we will use comment starting with `##`
  - Therefore, making `.md` documentation in `.http` file is possible
- Variables will be in cascade, these are the levels:
    1. `Global Level` - global variables accessible from everywhere during whole run
    2. `Environmental Level` - switchable variables accessible from everywhere during whole run
    3. `Collection Level` - present only when running collection (not for single test-case runs)
    4. `Scope Level` - level dedicated for user-defined variables
    5. `Test-Case Level` - temporary variables which are existing only during single test-case execution (if re-trying, they are present whole time)
- Usage of `.csx` files as scripts will be possible
  - Share of current context can be achieved by sending it to `Globals` for the script
  - Cross-scripts referencing is possible, but end-user has to specify the path, although we can provide **relative paths**
  - Usage of not-yet-installed **NuGet packages** has to be work-arounded, but possible. The prototype can be found in `./Prototypes/Code Prototypes/ScriptRunnerWithNugetPackagesSupport.cs` file
- For collections it won't be possible to be within another collection. Although, folder structure is up to end-user, who can create "imaginary inner collection(s)" simply by dividing them to different folders
- In order to make work easier for end-user, the alphabetical order of pre-request script, request and post-response has to be logical, here is naming convention:
  - `test-name-init.csx`
  - `test-name-req.http`
  - `test-name-test.csx`
- It is **impossible to comment the contents of request's body** in form of `.json` in request body, which make user unable to run single `.http` file, becasue of parsing problems. Although we can make it possible to run single Test Case which contains these types of variables
- **Configuration** can be specified in `.json` file, programmatically in scripts and also in `.http` (by using directives). Configuration is merged in the same order as in the previous sentence, so configuration will be **over-written** if needed.

## Naming conventions

**kebab-case** - file's name extensions, which specify the type/purpose of the file (-prereq, -req, -postres, -script...)

## File Structure

### Test Case

Each test can be in own folder, or it can be altogether with another tests.

**Structure:**

- `test-name-config.json [OPTIONAL]`

- `test-name-doc.md [OPTIONAL]`

- `test-name-init.csx [OPTIONAL]`

- `test-name-req.http`

- `test-name-test.csx [OPTIONAL]`

**Files specification:**

`test-name-config.json [OPTIONAL]`

- describes configuration of test (re-trying options, await)
- overrides its collection's configuration
- contains variables specific for the test
- adds new variables besides environment and collection variables
- overrides values of same-name variables (located on global, environment and collection levels)
- template: [Test Configuration Template](#test-configuration-file)

`test-name-doc.md [OPTIONAL]`

- optional human-readable description of the test

`test-name-init.csx [OPTIONAL]`

- pre-request script that can alter behavior before sending a request

`test-name-req.http`

- request definition in .http format
- from the beginning, it will contain only one request, later multiple requests will be supported

`test-name-test.csx [OPTIONAL]`

- script that defines post-response behavior, mainly define tests

### Collection

Contains at least one test and can contain other collections

**Structure:**

- `Seed [OPTIONAL]`

- `Tests`

- `collection-name-config.json`

- `collection-name-doc.md [OPTIONAL]`

- `collection-vars.json [OPTIONAL]`

- `collection-name-init.csx [OPTIONAL]`

- `collection-name-test.csx [OPTIONAL]`

**Files specification:**
`Seed [OPTIONAL]`

- folder, that contains requests/tests for data initialization

`Tests`

- folder, that contains tests, respectively other inner-collections

`collection-name-config.json`

- defines configuration of the collection – tests to ignore, their order (if different from alphabetical order), re-trying options, etc.
- template: [Collection Configuration Template](#collection-configuration-file)

`collection-vars.json [OPTIONAL]`

- contains variables specific for the collection
- adds new variables besides environment variables
- overrides values of same-name variables (located on global and environment level)
- template: [Collection Variables Template](#general-variables-file)

`collection-doc.md [OPTIONAL]`

- human-readable description of collection

`collection-name-init.csx [OPTIONAL]`

- pre-request script that can alter behavior before collection run

`collection-name-test.csx [OPTIONAL]`

- script which is called after finish of the collection

### Solution [OPTIONAL]

The highest level folder, which encapsulates all desired collections altogether with environment files, shared scripts and general configuration. Collections and tests are runnable without this folder. Basically it is only logical frame over a single super-collection.

**Structure:**

- `Seed [OPTIONAL]`
- `Collections`
- `Environments`
- `Shared`
- `global-variables.json`
- `solution-configuration.json`

**Files specification:**
`Seed [OPTIONAL]`

- folder, that contains tests for data initialization for all collections/tests

`Collections`

- folder, that contains collections and tests

`Environments`

- folder, that contains supported environments
- template: [Environment Template](#environment-file)

`Shared`

- folder, that contains functionality that can be re-used all over solution (scripts)

`global-vars.json`

- contains variables which will be available across all environments
- template: [Global Variables Template](#general-variables-file)

`solution-config.json`

- defines configuration of the solution – collections/tests to ignore, their order (if different from alphabetical order), re-trying options, etc.

## Templates

### Test Configuration File

[Go Back](#test-case)

```json
{
    "retryPolicy": {
        "maxRertryAttempts": 3,
        "backoffType": "Exponential",
        "delay": "00:00:03"
    },
    "delayRequestFor": number, // in milliseconds
    "timeoutPolicy": {
        "requestTimeout": number, // in milliseconds
        "disableTimeout": boolean
    },
    "variables": [
        {
            "name": string,
            "value": any,
        },
        {
            "name": string,
            "value": any,
        },
        {
            "name": string,
            "value": any,
        }
    ]
}
```

### General Variables File

[Go Back to Test](#test-case)

[Go Back to Collection](#collection)

[Go Back to Solution](#solution-optional)

```json
[
    {
        "name": string,
        "value": any,
    },
    {
        "name": string,
        "value": any,
    },
    {
        "name": string,
        "value": any,
    },
    {
        "name": string,
        "value": any,
    }
]
```

### Collection Configuration File

[Go Back](#collection)

```json
{
    "ignore": {
        "ignoreCollections": [], // names of collections to ignore
        "ignoreTests": [] // names of tests to ignore
    },
    "testsOrder": [], // names of tests and collections in desired order, if empty or not provided, alphabetical order is applied
    {
    "retryPolicy": {
        "maxRertryAttempts": 3,
        "backoffType": "Exponential",
        "delay": "00:00:03"
    },
    "timeoutPolicy": {
        "requestTimeout": number, // in milliseconds
        "disableTimeout": boolean
    },
    "variables": [
        {
            "name": string,
            "value": any,
        },
        {
            "name": string,
            "value": any,
        },
        {
            "name": string,
            "value": any,
        }
    ]
}
```

### Environment File

[Go Back](#solution-optional)

```json
{
    "name": "",
    "version": "",
    "variables": [
        {
            "name": "",
            "value": ""
        },
        {
            "name": "",
            "value": ""
        },
        {
            "name": "",
            "value": ""
        }
    ]
}
```
