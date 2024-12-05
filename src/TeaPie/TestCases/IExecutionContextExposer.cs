namespace TeaPie.TestCases;

internal interface IExecutionContextExposer
{
    public Dictionary<string, HttpRequestMessage> Requests { get; }
    public Dictionary<string, HttpResponseMessage> Responses { get; }
    public HttpRequestMessage? Request { get; }
    public HttpResponseMessage? Response { get; }
}
