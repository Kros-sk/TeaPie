using System.Text.RegularExpressions;
using TeaPie.StructureExploration;
using TeaPie.StructureExploration.Paths;
using File = TeaPie.StructureExploration.File;

namespace TeaPie.Scripts;

internal partial class LoadDirectiveLineResolver(
    IPathResolver pathResolver,
    TemporaryPathResolver tempPathResolver,
    IExternalFileRegistry externalFileRegistry,
    IPathProvider pathProvider)
    : IScriptLineResolver
{
    private readonly IPathResolver _pathResolver = pathResolver;
    private readonly IPathProvider _pathProvider = pathProvider;
    private readonly TemporaryPathResolver _tempPathResolver = tempPathResolver;
    private readonly IExternalFileRegistry _externalFileRegistry = externalFileRegistry;

    public bool CanResolve(string line) => LoadReferenceRegex().IsMatch(line);

    public async Task<string> ResolveLine(string line, ScriptPreProcessContext context)
    {
        var scriptDirectory = Path.GetDirectoryName(context.Script.File.Path)!;
        var referencedPath = GetPathFromLoadDirective(line);

        var realPath = GetRealPath(scriptDirectory, referencedPath);
        var tempPath = _tempPathResolver.ResolvePath(realPath, scriptDirectory);

        RegisterFile(context, realPath, tempPath);

        await Task.CompletedTask;

        return $"{ScriptPreProcessorConstants.LoadScriptDirective} \"{tempPath}\"";
    }

    private string GetRealPath(string scriptDirectory, string referencedPath)
    {
        var realPath = _pathResolver.ResolvePath(referencedPath, scriptDirectory);
        if (!System.IO.File.Exists(realPath))
        {
            throw new InvalidOperationException($"Referenced script on path '{realPath}' doesn't exist.");
        }

        return realPath;
    }

    private void RegisterFile(ScriptPreProcessContext context, string realPath, string tempPath)
    {
        var isExternal = !File.BelongsTo(realPath, _pathProvider.RootPath);

        if (isExternal)
        {
            _externalFileRegistry.Register(realPath, new ExternalFile(realPath));
        }

        context.AddScriptReference(new(realPath, tempPath, isExternal));
    }

    private static string GetPathFromLoadDirective(string directive)
    {
        var segments = directive.Split(ScriptPreProcessorConstants.LoadScriptDirective, 2, StringSplitOptions.None);
        return segments[1].Trim().TrimQuotes().NormalizeSeparators();
    }

    [GeneratedRegex(ScriptPreProcessorConstants.LoadScriptDirective)]
    private static partial Regex LoadReferenceRegex();
}
