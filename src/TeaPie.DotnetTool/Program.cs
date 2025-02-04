using Spectre.Console.Cli;
using TeaPie.DotnetTool;

var app = new CommandApp<TestCommand>();
app.Configure(config =>
{
    config.SetApplicationName("teapie");

    config.AddCommand<TestCommand>("test")
        .WithAlias("t")
        .WithDescription("Runs tests from the collection at the specified path. " +
        "If no path is provided, the current directory is used.")
        .WithExample("test", "[pathToCollection]")
        .WithExample("test", "\"path\\to\\collection\"")
        .WithExample("t", "\"path\\to\\collection\"");

    config.AddCommand<GenerateCommand>("generate")
        .WithAlias("gen")
        .WithAlias("g")
        .WithDescription("Generates files for test-case.")
        .WithExample("generate", "myTestCase", "[path]")
        .WithExample("gen", "myTestCase", "\"path\"")
        .WithExample("g", "myTestCase", "\"path\"", "-i", "FALSE", "-t", "TRUE");

    config.AddCommand<ExploreCommand>("explore")
        .WithAlias("exp")
        .WithAlias("e")
        .WithDescription("Explores collection structure and displays it.")
        .WithExample("explore", "[path]")
        .WithExample("exp", "\"path\\to\\collection\"")
        .WithExample("e", "\"path\\to\\collection\"");

#if DEBUG
    config.PropagateExceptions();
    config.ValidateExamples();
#endif
});

return await app.RunAsync(args);
