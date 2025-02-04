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
        double time = 0.0)
    {
        if (_rootWritten)
        {
            throw new InvalidOperationException("Root <testsuites> has already been written.");
        }

        _writer.WriteStartElement("testsuites");
        _writer.WriteAttributeString("tests", tests.ToString());
        _writer.WriteAttributeString("skipped", skipped.ToString());
        _writer.WriteAttributeString("failures", failures.ToString());
        _writer.WriteAttributeString("time", time.ToString("0.###"));

        _rootWritten = true;
    }

    public void WriteTestSuitesRoot()
    {
        if (!_rootWritten)
        {
            _writer.WriteStartElement("testsuites");
            _rootWritten = true;
        }
    }

    public void WriteTestSuite(
        string name,
        int totalTests = 0,
        int failures = 0,
        int errors = 0,
        double time = 0.0)
    {
        if (!_rootWritten)
        {
            WriteTestSuitesRoot();
        }

        if (_inTestSuite)
        {
            _writer.WriteEndElement();
        }

        _writer.WriteStartElement("testsuite");
        _writer.WriteAttributeString("name", name);
        _writer.WriteAttributeString("tests", totalTests.ToString());
        _writer.WriteAttributeString("failures", failures.ToString());
        _writer.WriteAttributeString("errors", errors.ToString());
        _writer.WriteAttributeString("time", time.ToString("0.###"));

        _inTestSuite = true;
    }

    public void WriteTestCase(
        string className,
        string testName,
        double time,
        string? failureMessage = null,
        string failureType = "AssertionError")
    {
        if (!_inTestSuite)
        {
            throw new InvalidOperationException("Cannot write a test case without an active test suite.");
        }

        _writer.WriteStartElement("testcase");
        _writer.WriteAttributeString("classname", className);
        _writer.WriteAttributeString("name", testName);
        _writer.WriteAttributeString("time", time.ToString("0.###"));

        if (!string.IsNullOrEmpty(failureMessage))
        {
            _writer.WriteStartElement("failure");
            _writer.WriteAttributeString("message", failureMessage);
            _writer.WriteAttributeString("type", failureType);
            _writer.WriteString(failureMessage);
            _writer.WriteEndElement();
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

    public void Dispose()
    {
        if (!_disposed)
        {
            EndTestSuite();

            if (_rootWritten)
            {
                _writer.WriteEndElement();
            }

            _writer.WriteEndDocument();
            _writer.Dispose();
            _disposed = true;
            GC.SuppressFinalize(this);
        }
    }
}
