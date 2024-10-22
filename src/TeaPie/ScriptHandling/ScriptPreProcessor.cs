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
        IEnumerable<string> lines;
        _rootPath = rootPath;
        _tempFolderPath = tempFolderPath;
        _referencedScripts = referencedScripts;

        var hasLoadDirectives = scriptContent.Contains(ParsingConstants.ReferenceScriptDirective);
        var hasNugetDirectives = scriptContent.Contains(ParsingConstants.NugetDirective);

        if (hasLoadDirectives || hasNugetDirectives)
        {
            lines = scriptContent.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

            if (hasLoadDirectives)
            {
                lines = ResolveLoadDirectives(path, lines);
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
        var segments = directive.Split(new[] { ParsingConstants.ReferenceScriptDirective }, 2, StringSplitOptions.None);
        var realPath = segments[1].Trim();
        realPath = realPath.Replace("\"", string.Empty);
        realPath = ResolvePath(path, realPath);

        if (!File.Exists(realPath))
        {
            throw new FileNotFoundException($"Referenced script on path '{realPath}' was not found");
        }

        var relativePath = realPath.TrimRootPath(_rootPath, true);
        var tempPath = Path.Combine(_tempFolderPath, relativePath);

        _referencedScripts?.Add(realPath);

        return $"{ParsingConstants.ReferenceScriptDirective} \"{tempPath}\"";
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

    [GeneratedRegex(ParsingConstants.ReferenceScriptDirective)]
    private static partial Regex LoadReferenceRegex();
}
