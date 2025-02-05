using System.Xml;

namespace TeaPie.Xml;

public class JUnitXmlWriter : IDisposable
{
    private readonly XmlWriter _writer;
    private bool _disposed;
    private bool _rootWritten;
    private bool _inTestSuite;

    public JUnitXmlWriter(string filePath)
    {
        var settings = new XmlWriterSettings { Indent = true, Encoding = System.Text.Encoding.UTF8 };
        _writer = XmlWriter.Create(filePath, settings);
        _writer.WriteStartDocument();
    }

    public void WriteTestSuitesRoot(
        int tests = 0,
        int skipped = 0,
        int failures = 0,
        double timeMs = 0.0,
        DateTime? timestamp = null)
    {
        EnsureRootNotWritten();

        _writer.WriteStartElement("testsuites");
        WriteCommonAttributes(tests, skipped, failures, timeMs, timestamp);

        _rootWritten = true;
    }

    public void WriteTestSuite(
        string name,
        int totalTests = 0,
        int skipped = 0,
        int failures = 0,
        double timeMs = 0.0,
        DateTime? timestamp = null)
    {
        EnsureRootWritten();
        ClosePreviousTestSuite();

        _writer.WriteStartElement("testsuite");
        WriteNameAttribute(name);
        WriteCommonAttributes(totalTests, skipped, failures, timeMs, timestamp);

        _inTestSuite = true;
    }

    public void WriteTestCase(
        string className,
        string testName,
        double timeMs,
        bool skipped,
        string? failureMessage = null,
        string failureType = "AssertionError",
        string? stackTrace = null)
    {
        EnsureTestSuiteWritten();

        _writer.WriteStartElement("testcase");
        WriteNameAndTimeAttributes(testName, timeMs);
        _writer.WriteAttributeString("classname", className);

        if (skipped)
        {
            _writer.WriteElementString("skipped", string.Empty);
        }
        else if (!string.IsNullOrEmpty(failureMessage))
        {
            WriteFailureElement(failureMessage, failureType, stackTrace);
        }

        _writer.WriteEndElement();
    }

    public void EndTestSuite()
    {
        if (_inTestSuite)
        {
            _writer.WriteEndElement();
            _inTestSuite = false;
        }
    }

    public void EndTestSuitesRoot()
    {
        EnsureRootWritten();
        _writer.WriteEndElement();
    }

    private void WriteCommonAttributes(int tests, int skipped, int failures, double timeMs, DateTime? timestamp)
    {
        _writer.WriteAttributeString("tests", tests.ToString());
        _writer.WriteAttributeString("skipped", skipped.ToString());
        _writer.WriteAttributeString("failures", failures.ToString());
        _writer.WriteAttributeString("time", ConvertTimeToSeconds(timeMs));

        if (timestamp is not null)
        {
            _writer.WriteAttributeString("timestamp", timestamp.Value.ToString("yyyy-MM-ddTHH:mm:ss"));
        }
    }

    private void WriteNameAttribute(string name)
        => _writer.WriteAttributeString("name", name);

    private void WriteTimeAttribute(double timeMs)
        => _writer.WriteAttributeString("time", ConvertTimeToSeconds(timeMs));

    private void WriteNameAndTimeAttributes(string name, double timeMs)
    {
        WriteNameAttribute(name);
        WriteTimeAttribute(timeMs);
    }

    private void WriteFailureElement(string message, string type, string? stackTrace)
    {
        _writer.WriteStartElement("failure");
        _writer.WriteAttributeString("message", message);
        _writer.WriteAttributeString("type", type);
        _writer.WriteString(message);

        if (!string.IsNullOrEmpty(stackTrace))
        {
            _writer.WriteString(Environment.NewLine + stackTrace);
        }

        _writer.WriteEndElement();
    }

    private static string ConvertTimeToSeconds(double timeMs)
        => (timeMs / 1000.0).ToString("f3");

    private void EnsureRootNotWritten()
    {
        if (_rootWritten)
        {
            throw new InvalidOperationException("Root <testsuites> has already been written.");
        }
    }

    private void EnsureRootWritten()
    {
        if (!_rootWritten)
        {
            throw new InvalidOperationException("Root <testsuites> must be written before writing test suites.");
        }
    }

    private void EnsureTestSuiteWritten()
    {
        if (!_inTestSuite)
        {
            throw new InvalidOperationException("Cannot write a test case without an active test suite.");
        }
    }

    private void ClosePreviousTestSuite()
    {
        if (_inTestSuite)
        {
            _writer.WriteEndElement();
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _writer.Dispose();
            _disposed = true;
            GC.SuppressFinalize(this);
        }
    }
}
