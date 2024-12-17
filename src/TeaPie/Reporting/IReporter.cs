namespace TeaPie.Reporting;

internal interface IReporter
{
    void ReportTestStart(string testName, string path);

    void ReportTestSuccess(string testName, long duration);

    void ReportTestFailure(string testName, string errorMessage, long duration);
}
