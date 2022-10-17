using App.Services.NuGet;

namespace App.Services.Console;

public interface IConsoleService
{
    void RenderTitle(string text);
    Task RenderStatusAsync(Func<Task> action);
    void RenderNugetPackages(ICollection<NuGetPackage> nugetPackages);
    void RenderSettingsFile(string filepath);
    void RenderException(Exception exception);
}