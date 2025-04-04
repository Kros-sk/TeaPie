﻿namespace TeaPie.Testing;

public class CollectionTestResultsSummary(string name = "") : TestResultsSummary
{
    public string Name { get; } = name;

    private readonly Dictionary<string, TestCaseTestResultsSummary> _testCases = [];
    public IReadOnlyDictionary<string, TestCaseTestResultsSummary> TestCases => _testCases;

    internal void AddSkippedTest(string testCaseName, TestResult.NotRun skippedTestResult)
    {
        GetTestCase(testCaseName).AddSkippedTest(skippedTestResult);
        AddSkippedTest(skippedTestResult);
    }

    internal void AddPassedTest(string testCaseName, TestResult.Passed passedTestResult)
    {
        GetTestCase(testCaseName).AddPassedTest(passedTestResult);
        AddPassedTest(passedTestResult);
    }

    internal void AddFailedTest(string testCaseName, TestResult.Failed failedTestResult)
    {
        GetTestCase(testCaseName).AddFailedTest(failedTestResult);
        AddFailedTest(failedTestResult);
    }

    private TestCaseTestResultsSummary GetTestCase(string testCaseName)
    {
        if (!_testCases.TryGetValue(testCaseName, out var testCase))
        {
            testCase = new TestCaseTestResultsSummary(testCaseName);
            _testCases[testCaseName] = testCase;
        }

        return testCase;
    }
}
