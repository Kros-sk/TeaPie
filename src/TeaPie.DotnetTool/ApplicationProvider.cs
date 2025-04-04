
using Spectre.Console.Cli;

namespace TeaPie.DotnetTool;

internal static class ApplicationProvider
{
    internal static CommandApp<TestCommand> GetApplication()
    {
        var app = new CommandApp<TestCommand>();
        app.Configure(config =>
        {
            config.SetApplicationName("teapie");

            AddCommands(config);

            ConfigureDebugIfNeeded(config);
        });

        return app;
    }

    private static void AddCommands(IConfigurator config)
    {
        AddInitCommand(config);
        AddTestCommand(config);
        AddGenerateCommand(config);
        AddExploreCommand(config);
    }

    private static void AddExploreCommand(IConfigurator config)
        => config.AddCommand<ExploreCommand>("explore")
            .WithAlias("exp")
            .WithAlias("e")
            .WithDescription("Explores collection structure and displays it.")
            .WithExample("explore", "[path]")
            .WithExample("exp", "\"path\\to\\collection\"")
            .WithExample("e", "\"path\\to\\collection\"");

    private static void AddGenerateCommand(IConfigurator config)
        => config.AddCommand<GenerateCommand>("generate")
            .WithAlias("gen")
            .WithAlias("g")
            .WithDescription("Generates files for test-case.")
            .WithExample("generate", "myTestCase", "[path]")
            .WithExample("gen", "myTestCase", "\"path\"")
            .WithExample("g", "myTestCase", "\"path\"", "-i", "FALSE", "-t", "TRUE");

    private static void AddTestCommand(IConfigurator config)
        => config.AddCommand<TestCommand>("test")
            .WithAlias("t")
            .WithDescription("Runs tests from the collection at the specified path. " +
            "If no path is provided, the current directory is used.")
            .WithExample("test", "[pathToCollection]")
            .WithExample("test", "\"path\\to\\collection\"")
            .WithExample("t", "\"path\\to\\collection\"");

    private static void AddInitCommand(IConfigurator config)
        => config.AddCommand<InitCommand>("init")
            .WithAlias("i")
            .WithDescription("Initialize working environment for TeaPie.")
            .WithExample("init")
            .WithExample("init", "[pathForTeaPieFolder]");

    private static void ConfigureDebugIfNeeded(IConfigurator config)
    {
#if DEBUG
        config.PropagateExceptions();
        config.ValidateExamples();
#endif
    }
}
