using Spectre.Console.Cli;
using TeaPie.DotnetTool;

var app = new CommandApp();
app.Configure(config =>
{
    config.SetApplicationName("tp");

    config.AddCommand<TestCommand>("test")
        .WithDescription("Runs tests from the collection at the specified path. " +
        "If no path is provided, the current directory is used.")
        .WithExample("test", "[pathToCollection]")
        .WithExample("test", "\"path\\to\\collection\"");
});

return await app.RunAsync(args);
