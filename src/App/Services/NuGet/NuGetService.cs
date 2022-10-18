using App.Extensions;
using App.Services.Process;
using Microsoft.Extensions.Options;
using NuGet.Common;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;

namespace App.Services.NuGet;

public class NuGetService : INuGetService
{
    private readonly IProcessService _processService;
    private readonly IOptions<Settings> _options;

    public NuGetService(IProcessService processService, IOptions<Settings> options)
    {
        _processService = processService ?? throw new ArgumentNullException(nameof(processService));
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public async Task<ICollection<NuGetPackage>> UploadNugetPackagesAsync(NuGetParameters parameters, CancellationToken cancellationToken = default)
    {
        var files = GetNugetPackageFiles(parameters.WorkingDirectory);

        var options = string.IsNullOrWhiteSpace(parameters.NugetFeedKey)
            ? $"--source {parameters.NugetFeedUrl} --skip-duplicate"
            : $"--source {parameters.NugetFeedUrl} --api-key {parameters.NugetFeedKey} --skip-duplicate";

        var nugetPackages = new List<NuGetPackage>();

        foreach (var file in files)
        {
            try
            {
                await _processService.RunProcessAsync("dotnet", $"nuget push {file} {options}", cancellationToken);
                nugetPackages.Add(new NuGetPackage(file, file));
            }
            catch (Exception ex)
            {
                nugetPackages.Add(new NotFoundNuGetPackage(file, file, ex.Message));
            }
        }

        return nugetPackages;
    }

    public async Task<ICollection<NuGetPackage>> DownloadNugetPackagesAsync(NuGetParameters parameters, CancellationToken cancellationToken = default)
    {
        var packagesFile = parameters.PackagesFile;

        if (string.IsNullOrWhiteSpace(packagesFile))
        {
            var packageName = parameters.PackageName;
            var packageVersion = parameters.PackageVersion;
            var nugetPackage = await DownloadNugetPackageAsync(packageName, packageVersion, parameters.WorkingDirectory, cancellationToken);
            return new List<NuGetPackage> { nugetPackage };
        }

        const int chunkSize = 100;
        var nugetPackages = new List<NuGetPackage>();
        var packagesToDownload = await ParseBuildLogFileAsync(packagesFile, cancellationToken);
        foreach (var packages in packagesToDownload.Chunk(chunkSize))
        {
            var packagesTasks = packages
                .Select(x => DownloadNugetPackageAsync(x.Name, x.Version, parameters.WorkingDirectory, cancellationToken))
                .ToList();
            await Task.WhenAll(packagesTasks);
            var results = packagesTasks
                .Select(x => x.Result)
                .ToList();
            nugetPackages.AddRange(results);
        }
        return nugetPackages;
    }

    private async Task<NuGetPackage> DownloadNugetPackageAsync(string packageName, string packageVersion, string downloadDirectory, CancellationToken cancellationToken)
    {
        try
        {
            using var cache = new SourceCacheContext();
            var repository = Repository.Factory.GetCoreV3(_options.Value.NugetFeed);
            var resource = await repository.GetResourceAsync<FindPackageByIdResource>(cancellationToken);
            var packagePath = Path.GetFullPath(Path.Combine(downloadDirectory, $"{packageName}.{packageVersion}.nupkg"));
            await using var packageStream = File.OpenWrite(packagePath);
            var ok = await resource.CopyNupkgToStreamAsync(
                packageName,
                new NuGetVersion(packageVersion),
                packageStream,
                cache,
                NullLogger.Instance,
                cancellationToken);

            return ok
                ? new NuGetPackage(packageName, packageVersion)
                : new NotFoundNuGetPackage(packageName, packageVersion, $"Failed to download nuget package {packageName} {packageVersion}");
        }
        catch (Exception ex)
        {
            return new NotFoundNuGetPackage(packageName, packageVersion, $"Failed to download nuget package {packageName} {packageVersion}: {ex.Message}");
        }
    }

    private static async Task<ICollection<NuGetPackage>> ParseBuildLogFileAsync(string packagesFile, CancellationToken cancellationToken)
    {
        var lines = await File.ReadAllLinesAsync(packagesFile, cancellationToken);

        var packages = new List<NuGetPackage>();
        foreach (var line in lines)
        {
            if (TryParseLine(line, out var nuGetPackage))
            {
                packages.Add(nuGetPackage);
            }
        }

        return packages
            .Distinct()
            .OrderBy(x => x.Name)
            .ThenBy(x => x.Version)
            .ToList();
    }

    private static bool TryParseLine(string line, out NuGetPackage nuGetPackage)
    {
        nuGetPackage = null;
        if (!line.IgnoreCaseContains("NotFound")) return false;
        var url = line.Split(' ').FirstOrDefault(x => x.IgnoreCaseStartWith("http"));
        if (string.IsNullOrWhiteSpace(url)) return false;
        var parts = url.Split('/').Reverse().Take(2).ToArray();
        if (parts.Length != 2) return false;
        nuGetPackage = new NuGetPackage(parts[1], parts[0]);
        return true;
    }

    private static ICollection<string> GetNugetPackageFiles(string directory)
    {
        var directoryOptions = new EnumerationOptions
        {
            RecurseSubdirectories = true
        };

        var files = Directory
            .GetFiles(directory, "*.nupkg", directoryOptions)
            .Distinct()
            .OrderBy(x => x)
            .ToList();

        return files;
    }
}