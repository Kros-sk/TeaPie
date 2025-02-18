
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

If you have [already installed](../README.md#how-to-install-locally) `TeaPie.Tool` just run (in demo folder):

```sh
teapie -i "./init.csx"
```

If not, use:

```sh
cd ./src/TeaPie.DotnetTool
dotnet run test "../../demo" -i "../../demo/init.csx"
```
