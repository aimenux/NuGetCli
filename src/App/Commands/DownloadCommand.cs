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

    [Option("-n|--name", "Package name", CommandOptionType.SingleValue)]
    public string PackageName { get; set; }

    [Option("-v|--version", "Package version", CommandOptionType.SingleValue)]
    public string PackageVersion { get; set; }
    
    [Option("-f|--file", "Packages file", CommandOptionType.SingleValue)]
    public string PackagesFile { get; set; }

    [Option("-o|--output", "OutputDirectory", CommandOptionType.SingleValue)]
    public string OutputDirectory { get; set; } = Settings.GetWorkingDirectory();

    protected override async Task ExecuteAsync(CommandLineApplication app, CancellationToken cancellationToken = default)
    {
        var parameters = new NuGetParameters
        {
            PackageName = PackageName,
            PackageVersion = PackageVersion,
            PackagesFile = PackagesFile,
            OutputDirectory = OutputDirectory
        };
        
        await ConsoleService.RenderStatusAsync(async () =>
        {
            var nugetPackages = await _nuGetService.DownloadNugetPackagesAsync(parameters, cancellationToken);
            ConsoleService.RenderNugetPackages(nugetPackages);
        });
    }

    protected override bool HasValidOptions()
    {
        if (!Directory.Exists(OutputDirectory)) return false;

        if (!string.IsNullOrWhiteSpace(PackagesFile))
        {
            return File.Exists(PackagesFile);
        }
        
        return !string.IsNullOrWhiteSpace(PackageName)
               && !string.IsNullOrWhiteSpace(PackageVersion);
    }
}