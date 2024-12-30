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
        ValidateContext(out var testCaseExecutionContext, out var content);

        var separatedRequests = RequestsSeparatorLineRegex().Split(content)
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
            requestExecutionContext = new(testCaseExecutionContext.TestCase.RequestsFile, testCaseExecutionContext)
            {
                RawContent = requestContent
            };

            newSteps.AddRange(
                RequestStepsFactory.CreateStepsForRequest(appContext.ServiceProvider, requestExecutionContext));
        }

        _pipeline.InsertSteps(this, [.. newSteps]);

        appContext.Logger.LogDebug(
            "Steps for all requests ({Count}) within requests file on '{Path}' were scheduled in the pipeline.",
            separatedRequests.Count(),
            testCaseExecutionContext.TestCase.RequestsFile.RelativePath);
    }

    private void ValidateContext(out TestCaseExecutionContext testCaseExecutionContext, out string content)
    {
        const string activityName = "generate steps for requests";
        ExecutionContextValidator.Validate(_testCaseExecutionContextAccessor, out testCaseExecutionContext, activityName);
        ExecutionContextValidator.ValidateParameter(
            testCaseExecutionContext.RequestsFileContent, out content, activityName, "the requests file's content");
    }

    [GeneratedRegex(HttpFileParserConstants.HttpRequestSeparatorDirectiveLineRegex, RegexOptions.IgnoreCase)]
    private static partial Regex RequestsSeparatorLineRegex();
}
