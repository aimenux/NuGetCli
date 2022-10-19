using App.Extensions;
using App.Services.Process;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;

namespace App.Services.NuGet;

public class NuGetService : INuGetService
{
    private const int ChunkSize = 50;
    private const int TimeoutInSeconds = 90;
    private readonly IProcessService _processService;

    public NuGetService(IProcessService processService)
    {
        _processService = processService ?? throw new ArgumentNullException(nameof(processService));
    }

    public async Task<ICollection<NuGetPackage>> UploadNugetPackagesAsync(NuGetParameters parameters, CancellationToken cancellationToken)
    {
        var nugetPackagesFiles = GetNugetPackageFiles(parameters.WorkingDirectory);

        var nugetPackages = new List<NuGetPackage>();
        foreach (var files in nugetPackagesFiles.Chunk(ChunkSize))
        {
            var filesTasks = files
                .Select(x => UploadNugetPackageAsync(x, parameters, cancellationToken))
                .ToList();
            await Task.WhenAll(filesTasks);
            var results = filesTasks
                .Select(x => x.Result)
                .ToList();
            nugetPackages.AddRange(results);
        }
        return nugetPackages
            .OrderBy(x => x.Name)
            .ThenBy(x => x.Version)
            .ToList();
    }

    public async Task<ICollection<NuGetPackage>> DownloadNugetPackagesAsync(NuGetParameters parameters, CancellationToken cancellationToken)
    {
        var packagesFile = parameters.PackagesFile;

        if (string.IsNullOrWhiteSpace(packagesFile))
        {
            var packageName = parameters.PackageName;
            var packageVersion = parameters.PackageVersion;
            var nugetPackage = await DownloadNugetPackageAsync(packageName, packageVersion, parameters, cancellationToken);
            return new List<NuGetPackage> { nugetPackage };
        }

        var nugetPackages = new List<NuGetPackage>();
        var packagesToDownload = await ParseBuildLogFileAsync(packagesFile, cancellationToken);
        foreach (var packages in packagesToDownload.Chunk(ChunkSize))
        {
            var packagesTasks = packages
                .Select(x => DownloadNugetPackageAsync(x.Name, x.Version, parameters, cancellationToken))
                .ToList();
            await Task.WhenAll(packagesTasks);
            var results = packagesTasks
                .Select(x => x.Result)
                .ToList();
            nugetPackages.AddRange(results);
        }
        return nugetPackages
            .OrderBy(x => x.Name)
            .ThenBy(x => x.Version)
            .ToList();
    }

    private async Task<NuGetPackage> UploadNugetPackageAsync(string file, NuGetParameters parameters, CancellationToken cancellationToken)
    {
        var fileInfo = new FileInfo(file);
        if (fileInfo.Length == 0)
        {
            return new NotFoundNuGetPackage(file, file, $"Failed to upload nuget package {file}: Size is 0KB");
        }

        try
        {
            var options = string.IsNullOrWhiteSpace(parameters.NugetFeedKey)
                ? $"--source {parameters.NugetFeedUrl} --no-symbols --skip-duplicate --timeout {TimeoutInSeconds}"
                : $"--source {parameters.NugetFeedUrl} --api-key {parameters.NugetFeedKey} --no-symbols --skip-duplicate --timeout {TimeoutInSeconds}";
            await _processService.RunProcessAsync("dotnet", $"nuget push {file} {options}", cancellationToken);
            return new NuGetPackage(file, file);
        }
        catch (Exception ex)
        {
            return new NotFoundNuGetPackage(file, file, $"Failed to upload nuget package {file}: {ex.Message}");
        }
    }

    private async Task<NuGetPackage> DownloadNugetPackageAsync(string packageName, string packageVersion, NuGetParameters parameters, CancellationToken cancellationToken)
    {
        try
        {
            using var cache = new SourceCacheContext();
            var downloadDirectory = parameters.WorkingDirectory;
            var packageSource = new PackageSource(parameters.NugetFeedUrl);
            var repository = Repository.Factory.GetCoreV3(packageSource);
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

            var fileInfo = new FileInfo(packagePath);
            if (fileInfo.Length == 0)
            {
                return new NotFoundNuGetPackage(packageName, packageVersion, $"Failed to download nuget package {packageName} {packageVersion}: Size is 0KB");
            }

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