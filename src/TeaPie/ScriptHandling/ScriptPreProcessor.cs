using TeaPie.Helpers;

namespace TeaPie.ScriptHandling;

internal interface IScriptPreProcessor
{
    public Task<string> PrepareScript(
        string path,
        string scriptContent,
        string rootPath,
        string tempFolderPath,
        List<string> referencedScripts);
}

internal class ScriptPreProcessor(INugetPackageHandler nugetPackagesHandler) : IScriptPreProcessor
{
    private List<string> _referencedScripts = [];
    private string _rootPath = string.Empty;
    private string _tempFolderPath = string.Empty;
    private readonly INugetPackageHandler _nugetPackagesHandler = nugetPackagesHandler;

    public async Task<string> PrepareScript(
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

        var hasLoadDirectives = scriptContent.Contains(Constants.ReferenceScriptDirective);
        var hasNugetDirectives = scriptContent.Contains(Constants.NugetDirectivePrefix);

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
                lines = lines.Where(x => x.Contains(Constants.NugetDirectivePrefix));
            }

            scriptContent = string.Join(Environment.NewLine, lines);
        }

        return scriptContent;
    }

    private IEnumerable<string> ResolveLoadDirectives(string path, IEnumerable<string> lines)
        => lines.Select(line => ResolveLoadDirective(path, line));

    private string ResolveLoadDirective(string path, string line)
    {
        if (line.TrimStart().StartsWith(Constants.ReferenceScriptDirective))
        {
            var segments = line.Split(new[] { Constants.ReferenceScriptDirective }, 2, StringSplitOptions.None);
            var realPath = segments[1].Trim();
            realPath = realPath.Replace("\"", string.Empty);
            realPath = ResolvePath(path, realPath);

            var relativePath = realPath.TrimRootPath(_rootPath, true);
            var tempPath = Path.Combine(_tempFolderPath, relativePath);

            _referencedScripts?.Add(realPath);

            return $"{Constants.ReferenceScriptDirective} \"{tempPath}\"";
        }

        return line;
    }

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
            if (line.TrimStart().StartsWith(Constants.NugetDirectivePrefix))
            {
                var packageInfo = line[Constants.NugetDirectivePrefix.Length..].Trim();
                packageInfo = packageInfo.Replace("\"", string.Empty);
                var parts = packageInfo.Split(',');
                if (parts.Length == 2)
                {
                    nugetPackages.Add(new(parts[0].Trim(), parts[1].Trim()));
                }
            }
        }

        return nugetPackages;
    }
}
