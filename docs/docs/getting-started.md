# Getting Started

Since `TeaPie.Tool` is not on NuGet store yet, the easiest way to launch this tool is:

1. Move to the **project folder**:

   ```sh
   cd ./src/TeaPie.DotnetTool
   ```

2. Run test of the `demo` **collection**:

   ```sh
   dotnet run test "../../demo" # this will run 'teapie test' command on demo collection
   ```

   Alternatively, you can run just **single test case**:

   ```sh
    dotnet run test "../../demo/Tests/2. Cars/EditCar-req.http" -i "../../demo/init.csx" --env-file "../../demo-env.json"
   ```

You can **learn more** about how to use this tool either in [Usage section](usage) or by checking attached [demo](./demo/).
