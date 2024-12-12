namespace TeaPie.Reporting;

internal interface IReporter
{
    void ReportTestStart(string testName);

    void ReportTestSuccess(string testName);

    void ReportTestFailure(string testName, string errorMessage);

    void ReportTestCaseSummary(int passed, int failed, int skipped);
}
