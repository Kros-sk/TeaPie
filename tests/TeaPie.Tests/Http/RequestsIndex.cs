namespace TeaPie.Tests.Http;

internal static class RequestsIndex
{
    public static readonly string RootFolderName = "Requests";
    public static readonly string RootFolderRelativePath = Path.Combine("Demo", RootFolderName);
    public static readonly string RootFolderFullPath = Path.Combine(Environment.CurrentDirectory, RootFolderRelativePath);

    public static readonly string PlainGetRequestPath =
        Path.Combine(RootFolderFullPath, $"GET{Constants.RequestFileExtension}");

    public static readonly string PlainPostRequestPath =
        Path.Combine(RootFolderFullPath, $"POST{Constants.RequestFileExtension}");

    public static readonly string PlainPutRequestPath =
        Path.Combine(RootFolderFullPath, $"PUT{Constants.RequestFileExtension}");

    public static readonly string PlainPatchRequestPath =
        Path.Combine(RootFolderFullPath, $"PATCH{Constants.RequestFileExtension}");

    public static readonly string PlainDeleteRequestPath =
        Path.Combine(RootFolderFullPath, $"DELETE{Constants.RequestFileExtension}");

    public static readonly string PlainHeadRequestPath =
        Path.Combine(RootFolderFullPath, $"HEAD{Constants.RequestFileExtension}");

    public static readonly string PlainOptionsRequestPath =
        Path.Combine(RootFolderFullPath, $"OPTIONS{Constants.RequestFileExtension}");

    public static readonly string PlainTraceRequestPath =
        Path.Combine(RootFolderFullPath, $"TRACE{Constants.RequestFileExtension}");

    public static readonly string SimpleRequestPath =
        Path.Combine(RootFolderFullPath, $"SimpleRequest{Constants.RequestFileExtension}");

    public static readonly string RequestWithCommentPath =
        Path.Combine(RootFolderFullPath, $"RequestWithComment{Constants.RequestFileExtension}");

    public static readonly string RequestWithCommentsPath =
        Path.Combine(RootFolderFullPath, $"RequestWithComments{Constants.RequestFileExtension}");

    public static readonly string RequestWithCommentsAllOverFile =
        Path.Combine(RootFolderFullPath, $"RequestWithCommentsAllOverFile{Constants.RequestFileExtension}");

    public static readonly string RequestWithNamePath =
        Path.Combine(RootFolderFullPath, $"RequestWithName{Constants.RequestFileExtension}");

    public static readonly string RequestWithJsonBodyPath =
        Path.Combine(RootFolderFullPath, $"RequestWithJsonBody{Constants.RequestFileExtension}");

    public static readonly string RequestWithHeaderPath =
        Path.Combine(RootFolderFullPath, $"RequestWithHeader{Constants.RequestFileExtension}");

    public static readonly string RequestWithHeadersPath =
        Path.Combine(RootFolderFullPath, $"RequestWithHeaders{Constants.RequestFileExtension}");

    public static readonly string RequestWithBodyAndHeaderPath =
        Path.Combine(RootFolderFullPath, $"RequestWithBodyAndHeader{Constants.RequestFileExtension}");

    public static readonly string RequestWithBodyAndHeadersPath =
        Path.Combine(RootFolderFullPath, $"RequestWithBodyAndHeaders{Constants.RequestFileExtension}");

    public static readonly string RequestWithFullStructure =
        Path.Combine(RootFolderFullPath, $"RequestWithFullStructure{Constants.RequestFileExtension}");
}
