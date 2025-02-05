using TeaPie.Testing;
using TeaPie.Xml;

namespace TeaPie.Reporting;

internal class JUnitXmlTestResultsSummaryReporter(string reportFilePath) : IReporter<TestResultsSummary>
{
    private readonly string _reportFilePath = reportFilePath;

    public void Report(TestResultsSummary report)
    {
        using var writer = new JUnitXmlWriter(_reportFilePath);

        if (report is CollectionTestResultsSummary collectionSummary)
        {
            WriteCollectionReport(writer, collectionSummary);
        }
    }

    private static void WriteCollectionReport(
        JUnitXmlWriter writer,
        CollectionTestResultsSummary collectionSummary)
    {
        writer.WriteTestSuitesRoot(
            collectionSummary.NumberOfTests,
            collectionSummary.NumberOfSkippedTests,
            collectionSummary.NumberOfFailedTests,
            collectionSummary.TimeElapsedDuringTesting,
            collectionSummary.Timestamp);

        WriteAllTestSuites(writer, collectionSummary);

        writer.EndTestSuitesRoot();
    }

    private static void WriteAllTestSuites(
        JUnitXmlWriter writer,
        CollectionTestResultsSummary collectionSummary)
    {
        foreach (var testCase in collectionSummary.TestCases.Values)
        {
            writer.WriteTestSuite(
                testCase.Name,
                testCase.NumberOfTests,
                testCase.NumberOfSkippedTests,
                testCase.NumberOfFailedTests,
                testCase.TimeElapsedDuringTesting);

            WriteTestCases(writer, testCase);

            writer.EndTestSuite();
        }
    }

    private static void WriteTestCases(
        JUnitXmlWriter writer,
        TestCaseTestResultsSummary testCaseSummary)
    {
        foreach (var testResult in testCaseSummary.TestResults)
        {
            WriteTestCase(writer, testCaseSummary, testResult);
        }
    }

    private static void WriteTestCase(
        JUnitXmlWriter writer,
        TestCaseTestResultsSummary testCaseSummary,
        TestResult testResult)
    {
        switch (testResult)
        {
            case TestResult.Passed passedTest: WritePassedTest(writer, testCaseSummary, passedTest); break;
            case TestResult.NotRun skippedTest: WriteSkippedTest(writer, testCaseSummary, skippedTest); break;
            case TestResult.Failed failedTest: WriteFailedTest(writer, testCaseSummary, failedTest); break;
        }
    }

    private static void WriteFailedTest(
        JUnitXmlWriter writer,
        TestCaseTestResultsSummary testCaseSummary,
        TestResult.Failed failedTest)
        => writer.WriteTestCase(
            testCaseSummary.Name,
            failedTest.TestName,
            failedTest.Duration,
            false,
            failedTest.ErrorMessage,
            stackTrace: failedTest.Exception?.StackTrace);

    private static void WriteSkippedTest(
        JUnitXmlWriter writer,
        TestCaseTestResultsSummary testCaseSummary,
        TestResult.NotRun skippedTest)
        => writer.WriteTestCase(
            testCaseSummary.Name,
            skippedTest.TestName,
            0.0,
            true);

    private static void WritePassedTest(
        JUnitXmlWriter writer,
        TestCaseTestResultsSummary testCaseSummary,
        TestResult.Passed passedTest)
        => writer.WriteTestCase(
            testCaseSummary.Name,
            passedTest.TestName,
            passedTest.Duration,
            false);
}
