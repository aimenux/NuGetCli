using System.Reflection;
using System.Text;
using System.Text.Json;
using App.Extensions;
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
        var isDownloadMode = string.IsNullOrWhiteSpace(parameters.NugetFeedUrl);

        if (isDownloadMode)
        {
            var table = new Table()
                .BorderColor(Color.White)
                .Border(TableBorder.Square)
                .Title($"[yellow]{nugetPackages.Count} package(s)[/]")
                .AddColumn(new TableColumn($"[u]Name[/]").Centered())
                .AddColumn(new TableColumn($"[u]Version[/]").Centered())
                .AddColumn(new TableColumn($"[u]Status[/]").Centered());

            foreach (var nugetPackage in nugetPackages)
            {
                var name = nugetPackage.Name;
                var version = nugetPackage.Version;
                var status = nugetPackage is not FailedNuGetPackage
                    ? Emoji.Known.CheckMarkButton
                    : Emoji.Known.CrossMark;

                table.AddRow(name.ToMarkup(), version.ToMarkup(), status.ToMarkup());
            }

            AnsiConsole.WriteLine();
            AnsiConsole.Write(table);
            AnsiConsole.WriteLine();
        }
        else
        {
            var table = new Table()
                .BorderColor(Color.White)
                .Border(TableBorder.Square)
                .Title($"[yellow]{nugetPackages.Count} package(s)[/]")
                .AddColumn(new TableColumn($"[u]Name[/]").Centered())
                .AddColumn(new TableColumn($"[u]Status[/]").Centered());

            foreach (var nugetPackage in nugetPackages)
            {
                var name = nugetPackage.Name;
                var status = nugetPackage is not FailedNuGetPackage
                    ? Emoji.Known.CheckMarkButton
                    : Emoji.Known.CrossMark;

                table.AddRow(name.ToMarkup(), status.ToMarkup());
            }

            AnsiConsole.WriteLine();
            AnsiConsole.Write(table);
            AnsiConsole.WriteLine();
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

    public static void RenderAnyException<T>(T exception) where T : Exception
    {
        const ExceptionFormats formats = ExceptionFormats.ShortenTypes
                                         | ExceptionFormats.ShortenPaths
                                         | ExceptionFormats.ShortenMethods;

        AnsiConsole.WriteLine();
        AnsiConsole.WriteException(exception, formats);
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