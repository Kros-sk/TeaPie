using System.Text.RegularExpressions;
using TeaPie.StructureExploration.Paths;

namespace TeaPie.Scripts;

internal partial class NuGetDirectiveLineResolver(INuGetPackageHandler nugetPackagesHandler) : IScriptLineResolver
{
    private readonly INuGetPackageHandler _nugetPackagesHandler = nugetPackagesHandler;

    public bool CanResolve(string line) => NuGetPackageRegex().IsMatch(line.Trim());

    public async Task<string> ResolveLine(string line, ScriptPreProcessContext context)
    {
        var nuGetPackage = ParseDirective(line);
        await _nugetPackagesHandler.HandleNuGetPackage(nuGetPackage);

        return string.Empty;
    }

    private static NuGetPackageDescription ParseDirective(string directive)
    {
        var packageInfo = directive[ScriptPreProcessorConstants.NuGetDirective.Length..].Trim();
        packageInfo = packageInfo.TrimQuotes();

        var parts = packageInfo.Split(',');
        if (parts.Length == 2)
        {
            return new(parts[0].Trim(), parts[1].Trim());
        }

        throw new InvalidOperationException($"NuGet directive is in incorrect format '{directive}'.");
    }

    [GeneratedRegex(ScriptPreProcessorConstants.NuGetDirectivePattern)]
    private static partial Regex NuGetPackageRegex();
}
