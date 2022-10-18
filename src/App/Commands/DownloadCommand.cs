using App.Services.Console;
using App.Services.NuGet;
using McMaster.Extensions.CommandLineUtils;

namespace App.Commands;

[Command(Name = "Download", FullName = "Download nuget packages", Description = "Download nuget packages.")]
public class DownloadCommand : AbstractCommand
{
    private readonly INuGetService _nuGetService;

    public DownloadCommand(IConsoleService consoleService, INuGetService nuGetService) : base(consoleService)
    {
        _nuGetService = nuGetService ?? throw new ArgumentNullException(nameof(nuGetService));
    }

    [Option("-u|--url", "Nuget feed url", CommandOptionType.SingleValue)]
    public string NugetFeedUrl { get; set; } = Settings.DefaultNugetFeedUrl;

    [Option("--username", "Nuget feed username", CommandOptionType.SingleValue)]
    public string NugetFeedUsername { get; set; }

    [Option("--password", "Nuget feed password", CommandOptionType.SingleValue)]
    public string NugetFeedPassword { get; set; }

    [Option("-n|--name", "Package name", CommandOptionType.SingleValue)]
    public string PackageName { get; set; }

    [Option("-v|--version", "Package version", CommandOptionType.SingleValue)]
    public string PackageVersion { get; set; }

    [Option("-f|--file", "Packages file", CommandOptionType.SingleValue)]
    public string PackagesFile { get; set; }

    [Option("-d|--dir", "Packages directory", CommandOptionType.SingleValue)]
    public string WorkingDirectory { get; set; } = Settings.GetWorkingDirectory();

    protected override async Task ExecuteAsync(CommandLineApplication app, CancellationToken cancellationToken = default)
    {
        var parameters = new NuGetParameters
        {
            NugetFeedUrl = NugetFeedUrl,
            NugetFeedUsername = NugetFeedUsername,
            NugetFeedPassword = NugetFeedPassword,
            PackageName = PackageName,
            PackageVersion = PackageVersion,
            PackagesFile = PackagesFile,
            WorkingDirectory = WorkingDirectory
        };

        await ConsoleService.RenderStatusAsync(async () =>
        {
            var nugetPackages = await _nuGetService.DownloadNugetPackagesAsync(parameters, cancellationToken);
            ConsoleService.RenderNugetPackages(nugetPackages, parameters);
        });
    }

    protected override bool HasValidOptions()
    {
        if (!Directory.Exists(WorkingDirectory)) return false;

        if (string.IsNullOrWhiteSpace(NugetFeedUrl)) return false;

        if (!string.IsNullOrWhiteSpace(PackagesFile))
        {
            return File.Exists(PackagesFile);
        }

        if (!string.IsNullOrWhiteSpace(NugetFeedUsername) && string.IsNullOrWhiteSpace(NugetFeedPassword))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(NugetFeedUsername) && !string.IsNullOrWhiteSpace(NugetFeedPassword))
        {
            return false;
        }

        return !string.IsNullOrWhiteSpace(PackageName)
               && !string.IsNullOrWhiteSpace(PackageVersion);
    }
}