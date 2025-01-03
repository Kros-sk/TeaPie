using Spectre.Console.Cli;
using TeaPie.DotnetTool;

var app = new CommandApp<TestCommand>();
app.Configure(config =>
{
    config.SetApplicationName("teapie");

    config.AddCommand<TestCommand>("test")
        .WithDescription("Runs tests from the collection at the specified path. " +
        "If no path is provided, the current directory is used.")
        .WithExample("test", "[pathToCollection]")
        .WithExample("test", "\"path\\to\\collection\"");

#if DEBUG
    config.PropagateExceptions();
    config.ValidateExamples();
#endif
});

return await app.RunAsync(args);
