﻿using App.Services.Console;
using App.Services.NuGet;
using McMaster.Extensions.CommandLineUtils;

namespace App.Commands;

[Command(Name = "Download", FullName = "Download nuget packages", Description = "Download nuget packages from nuget feed to directory.")]
public class DownloadCommand : AbstractCommand
{
    private readonly INuGetService _nuGetService;

    public DownloadCommand(INuGetService nuGetService, IConsoleService consoleService) : base(consoleService)
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
}