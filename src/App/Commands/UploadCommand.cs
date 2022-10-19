﻿using App.Services.Console;
using App.Services.NuGet;
using McMaster.Extensions.CommandLineUtils;

namespace App.Commands;

[Command(Name = "Upload", FullName = "Upload nuget packages", Description = "Upload nuget packages.")]
public class UploadCommand : AbstractCommand
{
    private readonly INuGetService _nuGetService;

    public UploadCommand(IConsoleService consoleService, INuGetService nuGetService) : base(consoleService)
    {
        _nuGetService = nuGetService ?? throw new ArgumentNullException(nameof(nuGetService));
    }

    [Option("-u|--url", "Nuget feed url", CommandOptionType.SingleValue)]
    public string NugetFeedUrl { get; set; } = Settings.DefaultNugetFeedUrl;

    [Option("-k|--key", "Nuget feed key", CommandOptionType.SingleValue)]
    public string NugetFeedKey { get; set; }

    [Option("-d|--dir", "Packages directory", CommandOptionType.SingleValue)]
    public string WorkingDirectory { get; set; } = Settings.GetWorkingDirectory();

    protected override async Task ExecuteAsync(CommandLineApplication app, CancellationToken cancellationToken = default)
    {
        var parameters = new NuGetParameters
        {
            NugetFeedUrl = NugetFeedUrl,
            NugetFeedKey = NugetFeedKey,
            WorkingDirectory = WorkingDirectory,
            Mode = NuGetParametersMode.Upload
        };

        await ConsoleService.RenderStatusAsync(async () =>
        {
            var nugetPackages = await _nuGetService.UploadNugetPackagesAsync(parameters, cancellationToken);
            ConsoleService.RenderNugetPackages(nugetPackages, parameters);
        });
    }

    protected override bool HasValidOptions()
    {
        return Directory.Exists(WorkingDirectory)
               && !string.IsNullOrWhiteSpace(NugetFeedUrl);
    }
}