using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using TeaPie.Extensions;
using TeaPie.Parsing;

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

internal partial class ScriptPreProcessor(INuGetPackageHandler nugetPackagesHandler, ILogger<ScriptPreProcessor> logger)
    : IScriptPreProcessor
{
    private List<string> _referencedScripts = [];
    private string _rootPath = string.Empty;
    private string _tempFolderPath = string.Empty;

    private readonly INuGetPackageHandler _nugetPackagesHandler = nugetPackagesHandler;
    private readonly ILogger<ScriptPreProcessor> _logger = logger;

    public async Task<string> ProcessScript(
        string path,
        string scriptContent,
        string rootPath,
        string tempFolderPath,
        List<string> referencedScripts)
    {
        IEnumerable<string> lines, referencedScriptsDirectives;

        _rootPath = rootPath;
        _tempFolderPath = tempFolderPath;
        _referencedScripts = referencedScripts;

        var hasLoadDirectives = scriptContent.Contains(ParsingConstants.LoadScriptDirective);
        var hasNuGetDirectives = scriptContent.Contains(ParsingConstants.NuGetDirective);

        if (hasLoadDirectives || hasNuGetDirectives)
        {
            lines = scriptContent.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

            if (hasLoadDirectives)
            {
                lines = ResolveLoadDirectives(path, lines);
                referencedScriptsDirectives = lines.Where(x => x.Contains(ParsingConstants.LoadScriptDirective));
                CheckAndRegisterReferencedScripts(referencedScriptsDirectives);

                LogResolvedLoadDirectives(path);
            }

            if (hasNuGetDirectives)
            {
                await ResolveNuGetDirectives(lines);
                lines = lines.Where(x => !x.Contains(ParsingConstants.NuGetDirective));

                LogResolvedNuGetDirectives(path);
            }

            scriptContent = string.Join(Environment.NewLine, lines);
        }

        return scriptContent;
    }

    private void CheckAndRegisterReferencedScripts(IEnumerable<string> referencedScriptsDirectives)
    {
        foreach (var scriptPath in referencedScriptsDirectives)
        {
            var tempPath = GetPathFromLoadDirective(scriptPath);
            var realPath = tempPath.TrimRootPath(_tempFolderPath, false);
            realPath = _rootPath.MergeWith(realPath);

            if (!File.Exists(realPath))
            {
                throw new FileNotFoundException($"Referenced script on path '{realPath}' was not found");
            }

            _referencedScripts.Add(realPath);
        }
    }

    private IEnumerable<string> ResolveLoadDirectives(string path, IEnumerable<string> lines)
        => lines.Select(line => ResolveLoadDirective(path, line));

    private string ResolveLoadDirective(string path, string line)
        => LoadReferenceRegex().IsMatch(line) ? ProcessLoadDirective(line, path) : line;

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

    private string ProcessLoadDirective(string directive, string path)
    {
        var realPath = GetPathFromLoadDirective(directive);
        realPath = realPath.Replace("\"", string.Empty);
        realPath = ResolvePath(path, realPath);

        var relativePath = realPath.TrimRootPath(_rootPath, true);
        var tempPath = Path.Combine(_tempFolderPath, relativePath);

        return $"{ParsingConstants.LoadScriptDirective} \"{tempPath}\"";
    }

    private static string GetPathFromLoadDirective(string directive)
    {
        var segments = directive.Split(new[] { ParsingConstants.LoadScriptDirective }, 2, StringSplitOptions.None);
        var path = segments[1].Trim();
        return path.Replace("\"", string.Empty);
    }

    private static string ProcessNuGetPackage(string directive, List<NuGetPackageDescription> listOfNuGetPackages)
    {
        var packageInfo = directive[ParsingConstants.NuGetDirective.Length..].Trim();
        packageInfo = packageInfo.Replace("\"", string.Empty);
        var parts = packageInfo.Split(',');
        if (parts.Length == 2)
        {
            listOfNuGetPackages.Add(new(parts[0].Trim(), parts[1].Trim()));
        }

        return directive;
    }

    [GeneratedRegex(ParsingConstants.NuGetDirectivePattern)]
    private static partial Regex NuGetPackageRegex();

    [GeneratedRegex(ParsingConstants.LoadScriptDirective)]
    private static partial Regex LoadReferenceRegex();

    [LoggerMessage("Load-script directives were resolved for the script on path '{scriptPath}'.", Level = LogLevel.Trace)]
    partial void LogResolvedLoadDirectives(string scriptPath);

    [LoggerMessage("NuGet package directives were resolved for the script on path '{scriptPath}'.", Level = LogLevel.Trace)]
    partial void LogResolvedNuGetDirectives(string scriptPath);
}
