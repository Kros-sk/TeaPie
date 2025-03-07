
# Demo Setup

All necessary information about the `TeaPie` tool from the user's perspective can be found [here](../README.md).

## Installation

1. Ensure you have `npm` installed. If not, follow the [npm installation guidance](https://docs.npmjs.com/downloading-and-installing-node-js-and-npm).

2. Install the `mockoon-cli` tool globally:

    ```sh
    npm install -g @mockoon/cli
    ```

## Start Server

To make this demo work, you need to **start the server first**:

```sh
mockoon-cli start --data CarRentalServer.json
```

By default, the server runs on [http://localhost:3001](http://localhost:3001).

If you want to **change the port** or modify other server properties, you can:

- Use the [Mockoon Desktop App](https://mockoon.com/) for a graphical interface.
- Manually edit the fields in the [server configuration file](CarRentalServer.json).

## Run tests

There are two run modes:

- Either you run whole `demo` **collection**
- Or you run just **single test case**

### Collection Run

If you have [already installed](../README.md#how-to-install-locally) `TeaPie.Tool` just run (in `demo` folder):

```sh
teapie
```

If not, use:

```sh
cd ./src/TeaPie.DotnetTool
dotnet run test "../../demo" -i "../../demo/init.csx"
```

### Single Test Case Run

You can choose which test case should be run, but this one is quite representative, since it contain more features included:

```sh
teapie "./Tests/2. Cars/EditCar-req.http" --env-file "../../demo-env.json" -i "../../init.csx
```

If you still don't have installed tool, use this alternative:

```sh
cd ./src/TeaPie.DotnetTool
dotnet run test "../../demo/Tests/2. Cars/EditCar-req.http" -i "../../demo/init.csx" --env-file "../../demo-env.json"
```
