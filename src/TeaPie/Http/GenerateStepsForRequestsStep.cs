using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using TeaPie.Http;
using TeaPie.Pipelines;

namespace TeaPie.TestCases;

internal partial class GenerateStepsForRequestsStep(ITestCaseExecutionContextAccessor accessor, IPipeline pipeline)
    : IPipelineStep
{
    private readonly IPipeline _pipeline = pipeline;
    private readonly ITestCaseExecutionContextAccessor _testCaseExecutionContextAccessor = accessor;

    public async Task Execute(ApplicationContext context, CancellationToken cancellationToken = default)
    {
        var testCaseExecutionContext = _testCaseExecutionContextAccessor.TestCaseExecutionContext
            ?? throw new NullReferenceException("Test case's execution context is null.");

        if (testCaseExecutionContext.RequestsFileContent is null)
        {
            throw new InvalidOperationException("Unable to prepare steps for requests, if the requests file's content is null.");
        }

        var separatedRequests = RequestsSeparatorLineRegex().Split(testCaseExecutionContext.RequestsFileContent)
            .Where(requestContent => !requestContent.Equals(string.Empty));

        AddStepsForRequests(context, testCaseExecutionContext, separatedRequests);

        await Task.CompletedTask;
    }

    private void AddStepsForRequests(
        ApplicationContext appContext,
        TestCaseExecutionContext testCaseExecutionContext,
        IEnumerable<string> separatedRequests)
    {
        List<IPipelineStep> newSteps = [];
        RequestExecutionContext requestExecutionContext;
        foreach (var requestContent in separatedRequests)
        {
            requestExecutionContext = new(testCaseExecutionContext.TestCase.RequestsFile)
            {
                RawContent = requestContent
            };

            AddStepsForRequest(appContext, requestExecutionContext, newSteps);
        }

        _pipeline.InsertSteps(this, [.. newSteps]);

        appContext.Logger.LogDebug(
            "Steps for all requests ({Count}) within requests file on '{Path}' were scheduled in the pipeline.",
            separatedRequests.Count(),
            testCaseExecutionContext.TestCase.RequestsFile.RelativePath);
    }

    private static void AddStepsForRequest(
        ApplicationContext appContext,
        RequestExecutionContext requestExecutionContext,
        List<IPipelineStep> newSteps)
    {
        using var scope = appContext.ServiceProvider.CreateScope();
        var provider = scope.ServiceProvider;

        var accessor = provider.GetRequiredService<IRequestExecutionContextAccessor>();
        accessor.RequestExecutionContext = requestExecutionContext;

        newSteps.Add(provider.GetStep<ParseRequestFileStep>());
        newSteps.Add(provider.GetStep<ExecuteRequestStep>());
    }

    [GeneratedRegex(HttpFileParserConstants.HttpRequestSeparatorDirectiveLineRegex, RegexOptions.IgnoreCase)]
    private static partial Regex RequestsSeparatorLineRegex();
}
