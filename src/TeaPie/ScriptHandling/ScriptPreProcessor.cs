using System.Text.RegularExpressions;
using TeaPie.Helpers;
using TeaPie.Parsing;

namespace TeaPie.ScriptHandling;

internal interface IScriptPreProcessor
{
    public Task<string> ProcessScript(
        string path,
        string scriptContent,
        string rootPath,
        string tempFolderPath,
        List<string> referencedScripts);
}

internal partial class ScriptPreProcessor(INugetPackageHandler nugetPackagesHandler) : IScriptPreProcessor
{
    private List<string> _referencedScripts = [];
    private string _rootPath = string.Empty;
    private string _tempFolderPath = string.Empty;
    private readonly INugetPackageHandler _nugetPackagesHandler = nugetPackagesHandler;

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
        var hasNugetDirectives = scriptContent.Contains(ParsingConstants.NugetDirective);

        if (hasLoadDirectives || hasNugetDirectives)
        {
            lines = scriptContent.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

            if (hasLoadDirectives)
            {
                lines = ResolveLoadDirectives(path, lines);
                referencedScriptsDirectives = lines.Where(x => x.Contains(ParsingConstants.LoadScriptDirective));
                CheckAndRegisterReferencedScripts(referencedScriptsDirectives);
            }

            if (hasNugetDirectives)
            {
                await ResolveNugetDirectives(lines);
                lines = lines.Where(x => !x.Contains(ParsingConstants.NugetDirective));
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

    private async Task ResolveNugetDirectives(IEnumerable<string> lines)
        => await _nugetPackagesHandler.HandleNugetPackages(ProcessNugetPackagesDirectives(lines));

    private static List<NugetPackageDescription> ProcessNugetPackagesDirectives(IEnumerable<string> lines)
    {
        var nugetPackages = new List<NugetPackageDescription>();

        foreach (var line in lines)
        {
            if (NugetPackageRegex().IsMatch(line.Trim()))
            {
                ProcessNugetPackage(line, nugetPackages);
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

    private static string ProcessNugetPackage(string directive, List<NugetPackageDescription> listOfNugetPackages)
    {
        var packageInfo = directive[ParsingConstants.NugetDirective.Length..].Trim();
        packageInfo = packageInfo.Replace("\"", string.Empty);
        var parts = packageInfo.Split(',');
        if (parts.Length == 2)
        {
            listOfNugetPackages.Add(new(parts[0].Trim(), parts[1].Trim()));
        }

        return directive;
    }

    [GeneratedRegex(ParsingConstants.NugetDirectivePattern)]
    private static partial Regex NugetPackageRegex();

    [GeneratedRegex(ParsingConstants.LoadScriptDirective)]
    private static partial Regex LoadReferenceRegex();
}
