using Microsoft.Extensions.Logging;
using Spectre.Console;
using System.Text.RegularExpressions;
using TeaPie.StructureExploration;

namespace TeaPie.Scripts;

internal interface IScriptPreProcessor
{
    Task<string> ProcessScript(
        string path,
        string scriptContent,
        string rootPath,
        string tempFolderPath,
        List<string> referencedScripts);
}

internal partial class ScriptPreProcessor(
    INuGetPackageHandler nugetPackagesHandler,
    ILogger<ScriptPreProcessor> logger,
    IPathProvider pathProvider)
    : IScriptPreProcessor
{
    private List<string> _referencedScripts = [];

    private readonly INuGetPackageHandler _nugetPackagesHandler = nugetPackagesHandler;
    private readonly ILogger<ScriptPreProcessor> _logger = logger;
    private readonly IPathProvider _pathProvider = pathProvider;

    public async Task<string> ProcessScript(
        string scriptPath,
        string scriptContent,
        string rootPath,
        string tempFolderPath,
        List<string> referencedScripts)
    {
        IEnumerable<string> lines;

        _referencedScripts = referencedScripts;

        var hasLoadDirectives = scriptContent.Contains(ScriptPreProcessorConstants.LoadScriptDirective);
        var hasNuGetDirectives = scriptContent.Contains(ScriptPreProcessorConstants.NuGetDirective);

        if (hasLoadDirectives || hasNuGetDirectives)
        {
            lines = scriptContent.Split([Environment.NewLine], StringSplitOptions.None);

            if (hasLoadDirectives)
            {
                lines = ResolveLoadDirectives(scriptPath, lines);
            }

            if (hasNuGetDirectives)
            {
                lines = await ResolveNuGetDirectives(scriptPath, lines);
            }

            scriptContent = string.Join(Environment.NewLine, lines);
        }

        return scriptContent;
    }

    private async Task<IEnumerable<string>> ResolveNuGetDirectives(string path, IEnumerable<string> lines)
    {
        await ResolveNuGetDirectives(lines);
        lines = lines.Where(x => !x.Contains(ScriptPreProcessorConstants.NuGetDirective));

        LogResolvedNuGetDirectives(path);
        return lines;
    }

    private void CheckAndRegisterReferencedScripts(IEnumerable<string> referencedScriptsDirectives)
    {
        foreach (var scriptPath in referencedScriptsDirectives)
        {
            var realPath = _pathProvider.ComputeRealPath(GetPathFromLoadDirective(scriptPath), Path.GetDirectoryName(scriptPath)!);

            if (!System.IO.File.Exists(realPath))
            {
                throw new FileNotFoundException($"Referenced script on path '{realPath}' was not found");
            }

            _referencedScripts.Add(realPath);
        }
    }

    private IEnumerable<string> ResolveLoadDirectives(
        string scriptPath,
        IEnumerable<string> lines)
    {
        lines = lines.Select(line => ResolveLoadDirective(scriptPath, line));
        var referencedScriptsDirectives = lines.Where(x => x.Contains(ScriptPreProcessorConstants.LoadScriptDirective));
        CheckAndRegisterReferencedScripts(referencedScriptsDirectives);

        LogResolvedLoadDirectives(scriptPath);
        return lines;
    }

    private string ResolveLoadDirective(string scriptPath, string line)
        => LoadReferenceRegex().IsMatch(line) ? ProcessLoadDirective(line, scriptPath) : line;

    private static string ResolvePath(string basePath, string relativePath)
    {
        var combinedPath = Path.Combine(basePath, relativePath);
        return Path.GetFullPath(combinedPath);
    }

    private async Task ResolveNuGetDirectives(IEnumerable<string> lines)
        => await _nugetPackagesHandler.HandleNuGetPackages(ProcessNuGetPackagesDirectives(lines));

    private static List<NuGetPackageDescription> ProcessNuGetPackagesDirectives(IEnumerable<string> lines)
    {
        var nugetPackages = new List<NuGetPackageDescription>();

        foreach (var line in lines)
        {
            if (NuGetPackageRegex().IsMatch(line.Trim()))
            {
                ProcessNuGetPackage(line, nugetPackages);
            }
        }

        return nugetPackages;
    }

    private string ProcessLoadDirective(string directive, string scriptPath)
    {
        var referencedPath = GetPathFromLoadDirective(directive);
        var tempPath = _pathProvider.ComputeTemporaryPath(referencedPath, Path.GetDirectoryName(scriptPath)!);

        // var tempPath = GetTempPath(scriptPath, referencedPath);

        return $"{ScriptPreProcessorConstants.LoadScriptDirective} \"{tempPath}\"";
    }

    private string GetTempPath(string scriptPath, string referencedPath)
    {
        var relativePath = ExtractRelativePath(scriptPath, referencedPath);
        return Path.Combine(_pathProvider.TempFolderPath, relativePath);
    }

    private string ExtractRelativePath(string scriptPath, string referencedPath)
    {
        if (referencedPath.StartsWith(ScriptsConstants.TeaPieFolderPathReference))
        {
            return referencedPath.Replace(
                ScriptsConstants.TeaPieFolderPathReference, Constants.TeaPieFolderName, StringComparison.OrdinalIgnoreCase);
        }
        else
        {
            referencedPath = ResolveRealPath(scriptPath, referencedPath);
            return referencedPath.TrimRootPath(_pathProvider.RootPath, true);
        }
    }

    private static string ResolveRealPath(string path, string realPath)
    {
        realPath = realPath.Normalize();

        var directoryPath = Path.GetDirectoryName(path) ?? string.Empty;

        return ResolvePath(directoryPath, realPath);
    }

    private static string GetPathFromLoadDirective(string directive)
    {
        var segments = directive.Split(ScriptPreProcessorConstants.LoadScriptDirective, 2, StringSplitOptions.None);
        var path = segments[1].Trim();

        return path.Replace("\"", string.Empty);
    }

    private static string ProcessNuGetPackage(string directive, List<NuGetPackageDescription> listOfNuGetPackages)
    {
        var packageInfo = directive[ScriptPreProcessorConstants.NuGetDirective.Length..].Trim();
        packageInfo = packageInfo.Replace("\"", string.Empty);
        var parts = packageInfo.Split(',');
        if (parts.Length == 2)
        {
            listOfNuGetPackages.Add(new(parts[0].Trim(), parts[1].Trim()));
        }

        return directive;
    }

    [GeneratedRegex(ScriptPreProcessorConstants.NuGetDirectivePattern)]
    private static partial Regex NuGetPackageRegex();

    [GeneratedRegex(ScriptPreProcessorConstants.LoadScriptDirective)]
    private static partial Regex LoadReferenceRegex();

    [LoggerMessage("Load-script directives were resolved for the script on path '{scriptPath}'.", Level = LogLevel.Trace)]
    partial void LogResolvedLoadDirectives(string scriptPath);

    [LoggerMessage("NuGet package directives were resolved for the script on path '{scriptPath}'.", Level = LogLevel.Trace)]
    partial void LogResolvedNuGetDirectives(string scriptPath);
}
