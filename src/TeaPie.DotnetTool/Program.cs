using Spectre.Console.Cli;
using TeaPie.DotnetTool;

var app = new CommandApp();
app.Configure(config =>
{
    config.SetApplicationName("tp");

    config.AddCommand<TestCommand>("test")
        .WithDescription("Command for running tests within collection on given path. " +
        "If no path is given, current directory is chosen.")
        .WithExample("test", "[pathToCollection]")
        .WithExample("test", "\"path\\to\\collection\"");
});

return await app.RunAsync(args);
