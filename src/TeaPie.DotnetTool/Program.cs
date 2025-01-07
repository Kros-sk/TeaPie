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

    config.AddCommand<GenerateCommand>("generate")
        .WithAlias("gen")
        .WithAlias("g")
        .WithDescription("Generates files for test case.")
        .WithExample("generate", "myTestCase", "[path]")
        .WithExample("gen", "myTestCase", "\"path\"")
        .WithExample("g", "myTestCase", "\"path\"", "-i", "FALSE", "-t", "TRUE");

#if DEBUG
    config.PropagateExceptions();
    config.ValidateExamples();
#endif
});

return await app.RunAsync(args);
