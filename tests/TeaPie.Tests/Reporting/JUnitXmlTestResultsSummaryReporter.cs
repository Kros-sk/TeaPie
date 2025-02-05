using System.Xml.Linq;
using TeaPie.Reporting;
using TeaPie.Testing;
using static Xunit.Assert;

namespace TeaPie.Tests.Reporting;

public class JUnitXmlTestResultsSummaryReporterShould
{
    private const string TestFilePath = "TestSummaryResults.xml";
    private static Exception _exception = new Exception("Error message");

    public JUnitXmlTestResultsSummaryReporterShould()
    {
        if (File.Exists(TestFilePath))
        {
            File.Delete(TestFilePath);
        }
    }

    [Fact]
    public void CreateValidJUnitXmlReport()
    {
        var summary = CreateMockTestSummary();

        var reporter = new JUnitXmlTestResultsSummaryReporter(TestFilePath);
        reporter.Report(summary);

        True(File.Exists(TestFilePath));

        var doc = XDocument.Load(TestFilePath);
        NotNull(doc.Root);
        Equal("testsuites", doc.Root.Name.LocalName);
    }

    [Fact]
    public void WriteTestSuitesWithCorrectAttributes()
    {
        var summary = CreateMockTestSummary();

        var reporter = new JUnitXmlTestResultsSummaryReporter(TestFilePath);
        reporter.Report(summary);

        var doc = XDocument.Load(TestFilePath);
        var suites = doc.Descendants("testsuite").ToList();

        Single(suites);
        var suite = suites[0];

        Equal("SampleTestCase", suite.Attribute("name")?.Value);
        Equal("3", suite.Attribute("tests")?.Value);
        Equal("1", suite.Attribute("skipped")?.Value);
        Equal("1", suite.Attribute("failures")?.Value);
        Equal("0.350", suite.Attribute("time")?.Value);
    }

    [Fact]
    public void WritePassedTestCasesCorrectly()
    {
        var summary = CreateMockTestSummary();

        var reporter = new JUnitXmlTestResultsSummaryReporter(TestFilePath);
        reporter.Report(summary);

        var doc = XDocument.Load(TestFilePath);
        var testCases = doc.Descendants("testcase").ToList();

        Equal(3, testCases.Count);

        var passedTest = testCases.FirstOrDefault(tc => tc.Attribute("name")?.Value == "Test1");
        NotNull(passedTest);
        Equal("SampleTestCase", passedTest.Attribute("classname")?.Value);
        Equal("0.150", passedTest.Attribute("time")?.Value);
        False(passedTest.Elements("failure").Any());
        False(passedTest.Elements("skipped").Any());
    }

    [Fact]
    public void WriteFailedTestCasesWithFailureElement()
    {
        var summary = CreateMockTestSummary();

        var reporter = new JUnitXmlTestResultsSummaryReporter(TestFilePath);
        reporter.Report(summary);

        var doc = XDocument.Load(TestFilePath);
        var failedTest = doc.Descendants("testcase")
                            .FirstOrDefault(tc => tc.Attribute("name")?.Value == "Test2");

        NotNull(failedTest);
        var failureElement = failedTest.Element("failure");
        NotNull(failureElement);
        Equal(_exception.Message, failureElement.Attribute("message")?.Value);
    }

    [Fact]
    public void WriteSkippedTestCasesWithSkippedElement()
    {
        var summary = CreateMockTestSummary();

        var reporter = new JUnitXmlTestResultsSummaryReporter(TestFilePath);
        reporter.Report(summary);

        var doc = XDocument.Load(TestFilePath);
        var skippedTest = doc.Descendants("testcase")
                             .FirstOrDefault(tc => tc.Attribute("name")?.Value == "Test3");

        NotNull(skippedTest);
        NotNull(skippedTest.Element("skipped"));
    }

    [Fact]
    public void EnsureTimeFormatUsesDotAsDecimalSeparator()
    {
        var summary = CreateMockTestSummary();

        var reporter = new JUnitXmlTestResultsSummaryReporter(TestFilePath);
        reporter.Report(summary);

        var doc = XDocument.Load(TestFilePath);
        var suite = doc.Root?.Element("testsuite");

        NotNull(suite);
        Contains(".", suite.Attribute("time")?.Value);
    }

    private static CollectionTestResultsSummary CreateMockTestSummary()
    {
        var summary = new CollectionTestResultsSummary();
        summary.AddPassedTest("SampleTestCase", new TestResult.Passed(150) { TestName = "Test1" });
        summary.AddFailedTest(
            "SampleTestCase",
            new TestResult.Failed(
                200,
                _exception.Message,
                _exception
            )
            {
                TestName = "Test2"
            });
        summary.AddSkippedTest("SampleTestCase", new TestResult.NotRun() { TestName = "Test3" });

        summary.Start();

        return summary;
    }
}
