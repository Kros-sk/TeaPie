using NuGet.Common;
using NuGet.Frameworks;
using NuGet.Packaging.Core;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using TeaPie.Exceptions;

namespace TeaPie.ScriptHandling;

internal interface INugetPackageHandler
{
    public Task HandleNugetPackages(List<NugetPackageDescription> nugetPackages);
}

internal class NugetPackageHandler : INugetPackageHandler
{
    public async Task HandleNugetPackages(List<NugetPackageDescription> nugetPackages)
    {
        var packagePath = $"{Environment.CurrentDirectory}/{Constants.DefaultNugetPackageFolderName}";
        foreach (var package in nugetPackages)
        {
            await DownloadNuget(packagePath, package.PackageName, package.Version);
        }
    }

    private async Task DownloadNuget(string packagePath, string packageID, string version)
    {
        var logger = NullLogger.Instance;
        var cache = new SourceCacheContext();
        var repositories = Repository.Factory.GetCoreV3(Constants.NugetApiResourcesUrl);

        await repositories.GetResourceAsync<FindPackageByIdResource>();
        var packageVersion = new NuGetVersion(version);
        var dependencyInfoResource = await repositories.GetResourceAsync<DependencyInfoResource>();
        var dependencyInfo = await dependencyInfoResource.ResolvePackage(
            new PackageIdentity(packageID, packageVersion),
            FrameworkConstants.CommonFrameworks.NetStandard20,
            cache,
            logger,
            CancellationToken.None)
            ?? throw new NugetPackageNotFoundException(packageID, version);

        foreach (var dependency in dependencyInfo.Dependencies)
        {
            var dependencyPackage = new PackageIdentity(dependency.Id, dependency.VersionRange.MinVersion);

            var resolvedDependency = await dependencyInfoResource.ResolvePackage(
                dependencyPackage,
                FrameworkConstants.CommonFrameworks.NetStandard20,
                cache,
                logger,
                CancellationToken.None);

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

        if (downloadResult.Status == DownloadResourceResultStatus.NotFound)
        {
            throw new NugetPackageNotFoundException(packageID, version);
        }
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

        if (downloadResult.Status == DownloadResourceResultStatus.Available)
        {
            await using var packageStream = downloadResult.PackageStream;
            var packageFilePath = Path.Combine(packagePath,
                $"{dependencyInfo.Id}.{dependencyInfo.Version}{Constants.NugetPackageFileExtension}");
            await using var fileStream = new FileStream(packageFilePath, FileMode.Create, FileAccess.Write);
            await packageStream.CopyToAsync(fileStream);
        }
    }
}

internal class NugetPackageDescription(string packageName, string version)
{
    public string PackageName { get; set; } = packageName;
    public string Version { get; set; } = version;
}
