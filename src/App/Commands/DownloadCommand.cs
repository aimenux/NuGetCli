using App.Models;
using App.Services.Console;
using App.Services.NuGet;
using McMaster.Extensions.CommandLineUtils;

namespace App.Commands;

[Command(Name = "Download", FullName = "Download nuget packages", Description = "Download nuget packages from nuget feed to directory.")]
public class DownloadCommand : AbstractCommand
{
    private readonly INuGetService _nuGetService;

    public DownloadCommand(IConsoleService consoleService, INuGetService nuGetService) : base(consoleService)
    {
        _nuGetService = nuGetService ?? throw new ArgumentNullException(nameof(nuGetService));
    }

    [Option("-u|--url", "Nuget feed url", CommandOptionType.SingleValue)]
    public string NugetFeedUrl { get; set; } = Settings.DefaultNugetFeedUrl;

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
            PackageName = PackageName,
            PackageVersion = PackageVersion,
            PackagesFile = PackagesFile,
            WorkingDirectory = WorkingDirectory,
            Mode = NuGetParametersMode.Download
        };

        await ConsoleService.RenderStatusAsync(async () =>
        {
            var nugetPackages = await _nuGetService.DownloadNugetPackagesAsync(parameters, cancellationToken);
            ConsoleService.RenderNugetPackages(nugetPackages, parameters);
        });
    }

    protected override bool HasValidOptionsAndArguments(out ValidationErrors validationErrors)
    {
        validationErrors = new ValidationErrors();

        if (string.IsNullOrWhiteSpace(NugetFeedUrl))
        {
            validationErrors.Add("-u|--url", "Feed url is mandatory");
        }

        if (!Directory.Exists(WorkingDirectory))
        {
            validationErrors.Add("-d|--dir", $"Directory '{WorkingDirectory}' does not exist");
        }

        if (!string.IsNullOrWhiteSpace(PackagesFile))
        {
            if (!File.Exists(PackagesFile))
            {
                validationErrors.Add("-f|--file", $"File '{PackagesFile}' does not exist");
            }
        }
        else
        {
            if (string.IsNullOrWhiteSpace(PackageName))
            {
                validationErrors.Add("-n|--name", "Package name is mandatory");
            }

            if (string.IsNullOrWhiteSpace(PackageVersion))
            {
                validationErrors.Add("-v|--version", "Package version is mandatory");
            }
        }

        return !validationErrors.Any();
    }
}