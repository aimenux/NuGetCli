using System.Reflection;
using App.Models;
using App.Services.Console;
using McMaster.Extensions.CommandLineUtils;
using static App.Extensions.PathExtensions;

namespace App.Commands;

[Command(Name = Settings.CommandName, FullName = "NuGet cli", Description = "A net global tool helping to upload/download nuget packages.")]
[Subcommand(typeof(UploadCommand), typeof(DownloadCommand))]
[VersionOptionFromMember(MemberName = nameof(GetVersion))]
public class MainCommand : AbstractCommand
{
    public MainCommand(IConsoleService consoleService) : base(consoleService)
    {
    }

    [Option("-s|--settings", "Show settings information.", CommandOptionType.NoValue)]
    public bool ShowSettings { get; set; }

    protected override Task ExecuteAsync(CommandLineApplication app, CancellationToken cancellationToken = default)
    {
        if (ShowSettings)
        {
            var filepath = GetSettingFilePath();
            ConsoleService.RenderSettingsFile(filepath);
        }
        else
        {
            ConsoleService.RenderTitle(Settings.CommandName);
            app.ShowHelp();
        }

        return Task.CompletedTask;
    }

    private static string GetVersion()
    {
        return typeof(MainCommand)
            .Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion;
    }
}