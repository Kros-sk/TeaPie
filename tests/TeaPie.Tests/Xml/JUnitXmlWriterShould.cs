using System.Xml.Linq;
using TeaPie.Xml;
using static Xunit.Assert;

namespace TeaPie.Tests.Xml;

public class JUnitXmlWriterShould
{
    private const string TestFilePath = "TestResults.xml";

    public JUnitXmlWriterShould()
    {
        if (File.Exists(TestFilePath))
        {
            File.Delete(TestFilePath);
        }
    }

    [Fact]
    public void CreateValidXmlFile()
    {
        using (var writer = new JUnitXmlWriter(TestFilePath))
        {
            writer.WriteTestSuitesRoot(tests: 2, skipped: 1, failures: 1, timeMs: 345, timestamp: DateTime.UtcNow);
            writer.WriteTestSuite("SampleSuite", totalTests: 1, skipped: 0, failures: 0, timeMs: 123);
            writer.WriteTestCase("MyNamespace.MyTests", "SampleTest", 100, skipped: false);
            writer.EndTestSuite();
            writer.EndTestSuitesRoot();
        }

        True(File.Exists(TestFilePath));

        var doc = XDocument.Load(TestFilePath);
        NotNull(doc.Root);
        Equal("testsuites", doc.Root.Name.LocalName);
    }

    [Fact]
    public void WriteTestSuiteWithCorrectAttributes()
    {
        using (var writer = new JUnitXmlWriter(TestFilePath))
        {
            writer.WriteTestSuitesRoot();
            writer.WriteTestSuite("SuiteA", totalTests: 3, skipped: 1, failures: 1, timeMs: 500);
            writer.EndTestSuite();
            writer.EndTestSuitesRoot();
        }

        var doc = XDocument.Load(TestFilePath);
        var suite = doc.Root?.Element("testsuite");

        NotNull(suite);
        Equal("SuiteA", suite.Attribute("name")?.Value);
        Equal("3", suite.Attribute("tests")?.Value);
        Equal("1", suite.Attribute("skipped")?.Value);
        Equal("1", suite.Attribute("failures")?.Value);
        Equal("0.500", suite.Attribute("time")?.Value);
    }

    [Fact]
    public void WriteTestCaseWithCorrectAttributes()
    {
        using (var writer = new JUnitXmlWriter(TestFilePath))
        {
            writer.WriteTestSuitesRoot();
            writer.WriteTestSuite("SuiteB", totalTests: 2, skipped: 0, failures: 0, timeMs: 250);
            writer.WriteTestCase("MyNamespace.Tests", "TestMethod1", 150, skipped: false);
            writer.WriteTestCase("MyNamespace.Tests", "TestMethod2", 100, skipped: false);
            writer.EndTestSuite();
            writer.EndTestSuitesRoot();
        }

        var doc = XDocument.Load(TestFilePath);
        var testCases = doc.Descendants("testcase").ToList();

        Equal(2, testCases.Count);

        var testCase1 = testCases.First();
        Equal("MyNamespace.Tests", testCase1.Attribute("classname")?.Value);
        Equal("TestMethod1", testCase1.Attribute("name")?.Value);
        Equal("0.150", testCase1.Attribute("time")?.Value);

        var testCase2 = testCases.Last();
        Equal("TestMethod2", testCase2.Attribute("name")?.Value);
        Equal("0.100", testCase2.Attribute("time")?.Value);
    }

    [Fact]
    public void WriteFailureElementWhenTestFails()
    {
        using (var writer = new JUnitXmlWriter(TestFilePath))
        {
            writer.WriteTestSuitesRoot();
            writer.WriteTestSuite("SuiteC", totalTests: 1, failures: 1);
            writer.WriteTestCase("Tests", "FailingTest", 200, skipped: false, failureMessage: "Assertion failed");
            writer.EndTestSuite();
            writer.EndTestSuitesRoot();
        }

        var doc = XDocument.Load(TestFilePath);
        var failureElement = doc.Descendants("failure").FirstOrDefault();

        NotNull(failureElement);
        Equal("Assertion failed", failureElement.Attribute("message")?.Value);
    }

    [Fact]
    public void WriteSkippedElementForSkippedTestCase()
    {
        using (var writer = new JUnitXmlWriter(TestFilePath))
        {
            writer.WriteTestSuitesRoot();
            writer.WriteTestSuite("SuiteD", totalTests: 1, skipped: 1);
            writer.WriteTestCase("Tests", "SkippedTest", 50, skipped: true);
            writer.EndTestSuite();
            writer.EndTestSuitesRoot();
        }

        var doc = XDocument.Load(TestFilePath);
        var skippedElement = doc.Descendants("skipped").FirstOrDefault();

        NotNull(skippedElement);
    }

    [Fact]
    public void EnsureTimeFormatUsesDotAsDecimalSeparator()
    {
        using (var writer = new JUnitXmlWriter(TestFilePath))
        {
            writer.WriteTestSuitesRoot();
            writer.WriteTestSuite("SuiteE", totalTests: 1, skipped: 0, failures: 0, timeMs: 1234);
            writer.EndTestSuite();
            writer.EndTestSuitesRoot();
        }

        var doc = XDocument.Load(TestFilePath);
        var suite = doc.Root?.Element("testsuite");

        NotNull(suite);
        Contains(".", suite.Attribute("time")?.Value);
    }
}
