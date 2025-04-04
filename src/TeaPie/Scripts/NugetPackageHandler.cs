﻿using Microsoft.Extensions.Logging;
using NuGet.Frameworks;
using NuGet.Packaging.Core;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using System.Reflection;
using TeaPie.StructureExploration.Paths;

namespace TeaPie.Scripts;

internal interface INuGetPackageHandler
{
    Task HandleNuGetPackage(NuGetPackageDescription nugetPackage);

    Task HandleNuGetPackages(IEnumerable<NuGetPackageDescription> nugetPackages);
}

internal partial class NuGetPackageHandler(
    IPathProvider pathProvider,
    ILogger<NuGetPackageHandler> logger,
    NuGet.Common.ILogger nugetLogger)
    : INuGetPackageHandler
{
    private readonly IPathProvider _pathProvider = pathProvider;
    private readonly ILogger<NuGetPackageHandler> _logger = logger;
    private readonly NuGet.Common.ILogger _nugetLogger = nugetLogger;

    private readonly HashSet<NuGetPackageDescription> _downloadedNuGetPackages = [];
    private readonly HashSet<NuGetPackageDescription> _nugetPackagesInAssembly = [];

    public async Task HandleNuGetPackages(IEnumerable<NuGetPackageDescription> nugetPackages)
    {
        foreach (var package in nugetPackages)
        {
            await HandleNuGetPackage(package);
        }
    }

    public async Task HandleNuGetPackage(NuGetPackageDescription nugetPackage)
    {
        await DownloadNuGet(nugetPackage);
        AddNuGetDllToAssembly(nugetPackage);
    }

    private async Task DownloadNuGet(NuGetPackageDescription nugetPackage)
    {
        if (_downloadedNuGetPackages.Contains(nugetPackage))
        {
            return;
        }

        var packageId = nugetPackage.PackageName;
        var version = nugetPackage.Version;
        var cache = new SourceCacheContext();
        var repositories = Repository.Factory.GetCoreV3(ScriptsConstants.NuGetApiResourcesUrl);

        await repositories.GetResourceAsync<FindPackageByIdResource>();
        var packageVersion = new NuGetVersion(version);
        var dependencyInfoResource = await repositories.GetResourceAsync<DependencyInfoResource>();
        var dependencyInfo = await dependencyInfoResource.ResolvePackage(
            new PackageIdentity(packageId, packageVersion),
            FrameworkConstants.CommonFrameworks.NetStandard20,
            cache,
            _nugetLogger,
            CancellationToken.None)
            ?? throw new NuGetPackageNotFoundException(packageId, version);

        foreach (var dependency in dependencyInfo.Dependencies)
        {
            var dependencyPackage = new PackageIdentity(dependency.Id, dependency.VersionRange.MinVersion);

            var resolvedDependency = await dependencyInfoResource.ResolvePackage(
                dependencyPackage,
                FrameworkConstants.CommonFrameworks.NetStandard20,
                cache,
                _nugetLogger,
                CancellationToken.None);

            await DownloadPackage(resolvedDependency, repositories, cache);
        }

        var packageDownloadContext = new PackageDownloadContext(cache);
        var downloadResource = await repositories.GetResourceAsync<DownloadResource>();
        var downloadResult = await downloadResource.GetDownloadResourceResultAsync(
            new PackageIdentity(packageId, packageVersion),
            packageDownloadContext,
            _pathProvider.NuGetPackagesFolderPath,
            _nugetLogger,
            CancellationToken.None);

        if (downloadResult.Status == DownloadResourceResultStatus.NotFound)
        {
            throw new NuGetPackageNotFoundException(packageId, version);
        }

        _downloadedNuGetPackages.Add(nugetPackage);

        LogSuccessfullNuGetDownload(packageId, version);
    }

    private void AddNuGetDllToAssembly(NuGetPackageDescription nugetPackage)
    {
        if (!_nugetPackagesInAssembly.Contains(nugetPackage))
        {
            var path = FindCompatibleFrameworkPath(GetNuGetPackageLocation(nugetPackage));
            var dllPath = Directory.GetFiles(path, $"*{ScriptsConstants.LibraryFileExtension}").FirstOrDefault()
                ?? throw new InvalidOperationException($"No NuGet library for '{Path.GetFileName(path)}' framework found.");

            Assembly.LoadFrom(dllPath);
            _nugetPackagesInAssembly.Add(nugetPackage);

            LogSuccessfullNuGetAdditionToAssembly(nugetPackage.PackageName, nugetPackage.Version);
        }
    }

    private string GetNuGetPackageLocation(NuGetPackageDescription nugetPackage)
        => Path.Combine(
            _pathProvider.NuGetPackagesFolderPath, nugetPackage.PackageName.ToLower(), nugetPackage.Version.ToLower());

    private static string FindCompatibleFrameworkPath(string packagePath)
    {
        var libPath = Path.Combine(packagePath, ScriptsConstants.DefaultNuGetLibraryFolderName);
        foreach (var framework in ScriptsConstants.FrameworksPriorityList)
        {
            var frameworkPath = Path.Combine(libPath, framework);
            if (Directory.Exists(frameworkPath))
            {
                return frameworkPath;
            }
        }

        throw new InvalidOperationException("No NuGet package version with compatible framework found.");
    }

    private async Task DownloadPackage(
        PackageDependencyInfo dependencyInfo,
        SourceRepository repositories,
        SourceCacheContext cache)
    {
        var packageDownloadContext = new PackageDownloadContext(cache);
        var downloadResource = await repositories.GetResourceAsync<DownloadResource>();
        var downloadResult = await downloadResource.GetDownloadResourceResultAsync(
            new PackageIdentity(dependencyInfo.Id, dependencyInfo.Version),
            packageDownloadContext,
            _pathProvider.NuGetPackagesFolderPath,
            _nugetLogger,
            CancellationToken.None);

        if (downloadResult.Status == DownloadResourceResultStatus.Available)
        {
            await using var packageStream = downloadResult.PackageStream;
            var packageFilePath = Path.Combine(
                _pathProvider.NuGetPackagesFolderPath,
                $"{dependencyInfo.Id}.{dependencyInfo.Version}{ScriptsConstants.NuGetPackageFileExtension}");
            await using var fileStream = new FileStream(packageFilePath, FileMode.Create, FileAccess.Write);
            await packageStream.CopyToAsync(fileStream);
        }
        else if (downloadResult.Status == DownloadResourceResultStatus.NotFound)
        {
            throw new NuGetPackageNotFoundException(dependencyInfo.Id, dependencyInfo.Version.Version.ToString());
        }
    }

    [LoggerMessage("NuGet Package {name}, {version} was successfully downloaded.",
        Level = LogLevel.Trace)]
    partial void LogSuccessfullNuGetDownload(string name, string version);

    [LoggerMessage("NuGet Package {name}, {version} was successfully addded to execution assembly.",
        Level = LogLevel.Trace)]
    partial void LogSuccessfullNuGetAdditionToAssembly(string name, string version);
}

internal class NuGetPackageDescription(string packageName, string version)
{
    public string PackageName { get; set; } = packageName;
    public string Version { get; set; } = version;

    public override string ToString() => $"{PackageName}, {Version}";

    public override bool Equals(object? obj)
        => obj is not null && obj is NuGetPackageDescription other
            && PackageName.Equals(other.PackageName) && Version.Equals(other.Version);

    public override int GetHashCode() => HashCode.Combine(PackageName, Version);
}
