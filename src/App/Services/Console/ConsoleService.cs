using System.Reflection;
using System.Text;
using System.Text.Json;
using App.Extensions;
using App.Models;
using App.Services.NuGet;
using Spectre.Console;

namespace App.Services.Console;

public class ConsoleService : IConsoleService
{
    public ConsoleService()
    {
        System.Console.OutputEncoding = Encoding.UTF8;
    }

    public void RenderTitle(string text)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.Write(new FigletText(text).LeftAligned());
        AnsiConsole.WriteLine();
    }

    public async Task RenderStatusAsync(Func<Task> action)
    {
        var spinner = RandomSpinner();

        await AnsiConsole.Status()
            .StartAsync("Work is in progress ...", async ctx =>
            {
                ctx.Spinner(spinner);
                await action.Invoke();
            });
    }

    public void RenderNugetPackages(ICollection<NuGetPackage> nugetPackages, NuGetParameters parameters)
    {
        switch (parameters.Mode)
        {
            case NuGetParametersMode.Upload:
                RenderNugetPackagesForUpload(nugetPackages);
                break;
            case NuGetParametersMode.Download:
                RenderNugetPackagesForDownload(nugetPackages);
                break;
        }
    }

    public void RenderSettingsFile(string filepath)
    {
        var name = Path.GetFileName(filepath);
        var json = File.ReadAllText(filepath);
        var formattedJson = GetFormattedJson(json);
        var header = new Rule($"[yellow]({name})[/]");
        header.Centered();
        var footer = new Rule($"[yellow]({filepath})[/]");
        footer.Centered();

        AnsiConsole.WriteLine();
        AnsiConsole.Write(header);
        AnsiConsole.WriteLine(formattedJson);
        AnsiConsole.Write(footer);
        AnsiConsole.WriteLine();
    }

    public void RenderException(Exception exception) => RenderAnyException(exception);

    public void RenderValidationErrors(ValidationErrors validationErrors)
    {
        var count = validationErrors.Count;

        var commandType = validationErrors.CommandType;

        var table = new Table()
            .BorderColor(Color.White)
            .Border(TableBorder.Square)
            .Title($"[red][bold]{count} error(s)[/][/]")
            .AddColumn(new TableColumn("[u]Name[/]").Centered())
            .AddColumn(new TableColumn("[u]Message[/]").Centered())
            .Caption("[grey][bold]Invalid options/arguments[/][/]");

        foreach (var error in validationErrors)
        {
            var name = $"[bold]{error.OptionName(commandType)}[/]";
            var reason = $"[tan]{error.ErrorMessage}[/]";

            table.AddRow(name.ToMarkup(), reason.ToMarkup());
        }

        AnsiConsole.WriteLine();
        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
    }

    public static void RenderAnyException<T>(T exception) where T : Exception
    {
        const ExceptionFormats formats = ExceptionFormats.ShortenTypes
                                         | ExceptionFormats.ShortenPaths
                                         | ExceptionFormats.ShortenMethods;

        AnsiConsole.WriteLine();
        AnsiConsole.WriteException(exception, formats);
        AnsiConsole.WriteLine();
    }

    private static void RenderNugetPackagesForUpload(ICollection<NuGetPackage> nugetPackages)
    {
        var anyFailure = nugetPackages.Any(x => x is FailedNuGetPackage);

        var table = new Table()
            .BorderColor(Color.White)
            .Border(TableBorder.Square)
            .Title($"[yellow]{nugetPackages.Count} package(s)[/]")
            .AddColumn(new TableColumn($"[u]Name[/]").LeftAligned())
            .AddColumn(new TableColumn($"[u]Status[/]").WithStyle(anyFailure));

        foreach (var nugetPackage in nugetPackages)
        {
            var name = nugetPackage.Name;
            var status = nugetPackage is FailedNuGetPackage failedNuGetPackage
                ? $"{Emoji.Known.CrossMark} [grey][bold]{failedNuGetPackage.Reason}[/][/]"
                : Emoji.Known.CheckMarkButton;

            table.AddRow(name.ToMarkup(), status.ToMarkup());
        }

        AnsiConsole.WriteLine();
        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
    }

    private static void RenderNugetPackagesForDownload(ICollection<NuGetPackage> nugetPackages)
    {
        var anyFailure = nugetPackages.Any(x => x is FailedNuGetPackage);

        var table = new Table()
            .BorderColor(Color.White)
            .Border(TableBorder.Square)
            .Title($"[yellow]{nugetPackages.Count} package(s)[/]")
            .AddColumn(new TableColumn($"[u]Name[/]").LeftAligned())
            .AddColumn(new TableColumn($"[u]Version[/]").WithStyle(anyFailure))
            .AddColumn(new TableColumn($"[u]Status[/]").WithStyle(anyFailure));

        foreach (var nugetPackage in nugetPackages)
        {
            var name = nugetPackage.Name;
            var version = nugetPackage.Version;
            var status = nugetPackage is FailedNuGetPackage failedNuGetPackage
                ? $"{Emoji.Known.CrossMark} [grey][bold]{failedNuGetPackage.Reason}[/][/]"
                : Emoji.Known.CheckMarkButton;

            table.AddRow(name.ToMarkup(), version.ToMarkup(), status.ToMarkup());
        }

        AnsiConsole.WriteLine();
        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
    }

    private static string GetFormattedJson(string json)
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        using var document = JsonDocument.Parse(json);
        return JsonSerializer.Serialize(document, options);
    }

    private static Spinner RandomSpinner()
    {
        var values = typeof(Spinner.Known)
            .GetProperties(BindingFlags.Public | BindingFlags.Static)
            .Where(x => x.PropertyType == typeof(Spinner))
            .Select(x => (Spinner)x.GetValue(null))
            .ToArray();

        var index = Random.Shared.Next(values.Length);
        var value = (Spinner)values.GetValue(index);
        return value;
    }
}