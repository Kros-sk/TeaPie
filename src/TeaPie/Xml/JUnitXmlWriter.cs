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
        double time = 0.0,
        DateTime? timestamp = null)
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

        if (timestamp is not null)
        {
            _writer.WriteAttributeString("timestamp", timestamp.Value.ToString("yyyy-MM-ddTHH:mm:ss"));
        }

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
        int skipped = 0,
        int failures = 0,
        double time = 0.0,
        DateTime? timestamp = null)
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
        _writer.WriteAttributeString("skipped", skipped.ToString());
        _writer.WriteAttributeString("failures", failures.ToString());
        _writer.WriteAttributeString("time", time.ToString("0.###"));

        if (timestamp is not null)
        {
            _writer.WriteAttributeString("timestamp", timestamp.ToString());
        }

        _inTestSuite = true;
    }

    public void WriteTestCase(
        string className,
        string testName,
        double time,
        bool skipped,
        string? failureMessage = null,
        string failureType = "AssertionError",
        string? stackTrace = null)
    {
        if (!_inTestSuite)
        {
            throw new InvalidOperationException("Cannot write a test case without an active test suite.");
        }

        _writer.WriteStartElement("testcase");
        _writer.WriteAttributeString("classname", className);
        _writer.WriteAttributeString("name", testName);
        _writer.WriteAttributeString("time", time.ToString("0.###"));

        if (skipped)
        {
            _writer.WriteStartElement("skipped");
            _writer.WriteEndElement();
        }
        else if (!string.IsNullOrEmpty(failureMessage))
        {
            _writer.WriteStartElement("failure");
            _writer.WriteAttributeString("message", failureMessage);
            _writer.WriteAttributeString("type", failureType);
            _writer.WriteString(failureMessage);

            if (!string.IsNullOrEmpty(stackTrace))
            {
                _writer.WriteString(stackTrace);
            }

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

    public void EndTestSuitesRoot()
    {
        if (_rootWritten)
        {
            _writer.WriteEndElement();
        }
        else
        {
            throw new InvalidOperationException("Unable to end <testsuites> element, if there is no <testsuites> element.");
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            EndTestSuite();
            _writer.WriteEndDocument();
            _writer.Dispose();
            _disposed = true;
            GC.SuppressFinalize(this);
        }
    }
}
