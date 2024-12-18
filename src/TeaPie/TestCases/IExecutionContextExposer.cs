namespace TeaPie.TestCases;

internal interface IExecutionContextExposer
{
    public IReadOnlyDictionary<string, HttpRequestMessage> Requests { get; }
    public IReadOnlyDictionary<string, HttpResponseMessage> Responses { get; }
    public HttpRequestMessage? Request { get; }
    public HttpResponseMessage? Response { get; }
}
