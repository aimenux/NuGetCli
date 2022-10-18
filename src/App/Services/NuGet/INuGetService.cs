namespace App.Services.NuGet;

public interface INuGetService
{
    Task<ICollection<NuGetPackage>> UploadNugetPackagesAsync(NuGetParameters parameters, CancellationToken cancellationToken = default);
    Task<ICollection<NuGetPackage>> DownloadNugetPackagesAsync(NuGetParameters parameters, CancellationToken cancellationToken = default);
}