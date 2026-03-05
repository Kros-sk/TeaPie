using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using TeaPie;
using VerifyXunit;

namespace TeaPie.Tests.Integration;

[Trait("Category", "Integration")]
public sealed class DemoProjectShould : IAsyncLifetime
{
    private IContainer _mockoonContainer = null!;
    private string _solutionRoot = null!;
    private string _demoTestsPath = null!;
    private string _tempEnvFilePath = null!;
    private int _mappedPort;

    public Task InitializeAsync()
    {
        _solutionRoot = FindSolutionRoot();
        _demoTestsPath = Path.Combine(_solutionRoot, "demo", "Tests");
        _tempEnvFilePath = Path.GetTempFileName();

        var mockoonConfigPath = Path.Combine(_solutionRoot, "demo", "server", "CarRentalServer.json");

        _mockoonContainer = new ContainerBuilder()
            .WithImage("mockoon/cli:latest")
            .WithResourceMapping(mockoonConfigPath, "/data/")
            .WithEntrypoint("mockoon-cli")
            .WithCommand("start", "-X", "-d", "/data/CarRentalServer.json", "-p", "3001")
            .WithPortBinding(3001, true)
            .WithWaitStrategy(Wait.ForUnixContainer()
                .UntilHttpRequestIsSucceeded(r => r
                    .ForPath("/health")
                    .ForPort(3001)))
            .Build();

        return _mockoonContainer.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _mockoonContainer.DisposeAsync();
        if (File.Exists(_tempEnvFilePath))
        {
            File.Delete(_tempEnvFilePath);
        }
    }

    [Fact]
    public async Task ProduceExpectedReport()
    {
        _mappedPort = _mockoonContainer.GetMappedPublicPort(3001);

        CreateTempEnvFile(_mappedPort);

        var reportFilePath = Path.GetTempFileName();
        try
        {
            var application = ApplicationBuilder.Create(collectionRun: true)
                .WithPath(_demoTestsPath)
                .WithReportFile(reportFilePath)
                .WithEnvironmentFile(_tempEnvFilePath)
                .WithEnvironment("local")
                .WithVariablesCaching(false)
                .WithDefaultPipeline()
                .Build();

            await application.Run();

            var reportContent = await File.ReadAllTextAsync(reportFilePath);

            await Verifier.Verify(reportContent)
                .ScrubInlineDateTimes("yyyy-MM-ddTHH:mm:ss")
                .AddScrubber(ScrubTimeAttributes)
                .AddScrubber(ScrubDemoPath)
                .AddScrubber(ScrubCarYearInTestName)
                .AddScrubber(ScrubCarBrandInTestName)
                .AddScrubber(ScrubDirectiveIndices)
                .AddScrubber(ScrubStackTracePaths);
        }
        finally
        {
            if (File.Exists(reportFilePath))
            {
                File.Delete(reportFilePath);
            }
        }
    }

    private void CreateTempEnvFile(int port)
    {
        var baseUrl = $"http://localhost:{port}";
        var env = new Dictionary<string, object>
        {
            ["$shared"] = new Dictionary<string, string>
            {
                ["ApiBaseUrl"] = baseUrl,
                ["AuthServerUrl"] = $"{baseUrl}/auth/token",
                ["ApiCustomersSection"] = "/customers",
                ["ApiCarsSection"] = "/cars",
                ["ApiCarRentalSection"] = "/rental",
            },
            ["local"] = new Dictionary<string, object>
            {
                ["ApiBaseUrl"] = baseUrl,
                ["AuthServerUrl"] = $"{baseUrl}/auth/token",
                ["DebugMode"] = true,
            },
        };

        var json = JsonSerializer.Serialize(env, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_tempEnvFilePath, json);
    }

    private static void ScrubTimeAttributes(StringBuilder builder)
    {
        var result = Regex.Replace(
            builder.ToString(),
            @"time=""[\d.]+""",
            "time=\"0.000\"");
        builder.Clear().Append(result);
    }

    private void ScrubDemoPath(StringBuilder builder)
    {
        var result = builder.ToString().Replace(_demoTestsPath, "DEMO_TESTS");
        builder.Clear().Append(result);
    }

    private static void ScrubCarYearInTestName(StringBuilder builder)
    {
        var result = Regex.Replace(
            builder.ToString(),
            @"'(?<brand>[^']+), (?<year>\d{4})'",
            "'CAR_MODEL, 0000'");
        builder.Clear().Append(result);
    }

    private static void ScrubCarBrandInTestName(StringBuilder builder)
    {
        var result = Regex.Replace(
            builder.ToString(),
            @"Newly added car should have '[^']+' brand\.",
            "Newly added car should have 'BRAND' brand.");
        builder.Clear().Append(result);
    }

    private static void ScrubDirectiveIndices(StringBuilder builder)
    {
        // Only scrub directive indices like [1]-[99], not status codes like [200], [201], [404]
        var result = Regex.Replace(
            builder.ToString(),
            @"\[[1-9]\d{0,1}\]",
            "[#]");
        builder.Clear().Append(result);
    }

    private static void ScrubStackTracePaths(StringBuilder builder)
    {
        // Match path to TeaPie source files (Registrator.cs, Tester.cs) in any format:
        // {UserProfile}..., {SolutionDirectory}..., or raw filesystem path
        var result = Regex.Replace(
            builder.ToString(),
            @" in [^:]+src/TeaPie/Testing/(?:Registrator|Tester)\.cs",
            " in {SourcePath}");
        builder.Clear().Append(result);
    }

    private static string FindSolutionRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null && !File.Exists(Path.Combine(dir.FullName, "TeaPie.sln")))
        {
            dir = dir.Parent;
        }

        return dir?.FullName ?? throw new InvalidOperationException("Solution root not found");
    }
}
