using NuGet.Common;
using NuGet.Frameworks;
using NuGet.Packaging.Core;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;

namespace TeaPie.ScriptHandling;

internal interface IScriptPreProcessor
{
    public Task<string> PrepareScript(string path, string scriptContent, List<string> referencedScripts);
}

internal class ScriptPreProcessor : IScriptPreProcessor
{
    public async Task<string> PrepareScript(string path, string scriptContent, List<string> referencedScripts)
    {
        IEnumerable<string> lines;
        var hasLoadDirectives = scriptContent.Contains(Constants.ReferenceScriptDirective);
        var hasNugetDirectives = scriptContent.Contains(Constants.NugetDirectivePrefix);

        if (hasLoadDirectives || hasNugetDirectives)
        {
            lines = scriptContent.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

            if (hasLoadDirectives)
            {
                lines = ResolveLoadDirectives(path, lines, referencedScripts);
            }

            if (hasNugetDirectives)
            {
                await ResolveNugetDirectives(lines);
                lines = lines.Where(x => !x.Contains(Constants.NugetDirectivePrefix));
            }

            scriptContent = string.Join(Environment.NewLine, lines);
        }

        // TODO: Add global script reference, if specified by user

        return scriptContent;
    }

    private static IEnumerable<string> ResolveLoadDirectives(
        string path,
        IEnumerable<string> lines,
        List<string> referencedScripts)
    {
        var currentDirectory = Path.GetDirectoryName(path);
        return lines.Select(line => ResolveLoadDirective(currentDirectory!, line!, referencedScripts));
    }

    private static string ResolveLoadDirective(string currentDirectory, string line, List<string> referencedScripts)
    {
        if (line.TrimStart().StartsWith(Constants.ReferenceScriptDirective))
        {
            var segments = line.Split(new[] { Constants.ReferenceScriptDirective }, 2, StringSplitOptions.None);
            var loadPath = segments[1].Trim();
            loadPath = loadPath.Replace("\"", string.Empty);
            loadPath = ResolvePath(currentDirectory!, loadPath);

            referencedScripts.Add(loadPath);

            return $"{Constants.ReferenceScriptDirective} \"{loadPath}\"";
        }
        return line;
    }

    private static string ResolvePath(string basePath, string relativePath)
    {
        var combinedPath = Path.Combine(basePath, relativePath);
        return Path.GetFullPath(combinedPath);
    }

    private async Task ResolveNugetDirectives(IEnumerable<string> lines)
    {
        var packagePath = $"{Environment.CurrentDirectory}/{Constants.DefaultNugetPackageFolderName}";
        var nugetPackages = ProcessNugetPackages(lines!);
        foreach (var package in nugetPackages)
        {
            await DownloadNuget(packagePath, package.PackageName!, package.Version!);
        }
    }

    private static List<NugetPackageDescription> ProcessNugetPackages(IEnumerable<string> lines)
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

    private async Task DownloadNuget(string packagePath, string packageID, string version)
    {
        var logger = NullLogger.Instance;
        var cache = new SourceCacheContext();
        var repositories = Repository.Factory.GetCoreV3(Constants.NugetApiResourcesUrl);

        var resource = await repositories.GetResourceAsync<FindPackageByIdResource>();
        var packageVersion = new NuGetVersion(version);
        var dependencyInfoResource = await repositories.GetResourceAsync<DependencyInfoResource>();
        var dependencyInfo = await dependencyInfoResource.ResolvePackage(
            new PackageIdentity(packageID, packageVersion),
            FrameworkConstants.CommonFrameworks.NetStandard20,
            cache,
            logger,
            CancellationToken.None);

        foreach (var dependency in dependencyInfo.Dependencies)
        {
            // Get the dependency information
            var dependencyPackage = new PackageIdentity(dependency.Id, dependency.VersionRange.MinVersion);

            // Resolve the package
            var resolvedDependency = await dependencyInfoResource.ResolvePackage(
                dependencyPackage,
                FrameworkConstants.CommonFrameworks.NetStandard20,
                cache,
                logger,
                CancellationToken.None);

            // Download the resolved dependency
            await DownloadPackage(resolvedDependency, repositories, packagePath, cache, logger);
        }

        var packageDownloadContext = new PackageDownloadContext(cache);
        var downloadResource = await repositories.GetResourceAsync<DownloadResource>();
        var downloadResult = await downloadResource.GetDownloadResourceResultAsync(
            new PackageIdentity(packageID, packageVersion),
            packageDownloadContext,
            packagePath,
            logger,
            CancellationToken.None);
    }

    private async Task DownloadPackage(
        PackageDependencyInfo dependencyInfo,
        SourceRepository repositories,
        string packagePath,
        SourceCacheContext cache,
        ILogger logger)
    {
        var packageDownloadContext = new PackageDownloadContext(cache);
        var downloadResource = await repositories.GetResourceAsync<DownloadResource>();
        var downloadResult = await downloadResource.GetDownloadResourceResultAsync(
            new PackageIdentity(dependencyInfo.Id, dependencyInfo.Version),
            packageDownloadContext,
            packagePath,
            logger,
            CancellationToken.None);

        // Ensure the package is saved
        if (downloadResult.Status == DownloadResourceResultStatus.Available)
        {
            await using var packageStream = downloadResult.PackageStream;
            var packageFilePath = Path.Combine(packagePath,
                $"{dependencyInfo.Id}.{dependencyInfo.Version}{Constants.NugetPackageFileExtension}");
            await using var fileStream = new FileStream(packageFilePath, FileMode.Create, FileAccess.Write);
            await packageStream.CopyToAsync(fileStream);
        }
    }

    private class NugetPackageDescription(string packageName, string version)
    {
        public string PackageName { get; set; } = packageName;
        public string Version { get; set; } = version;
    }
}
