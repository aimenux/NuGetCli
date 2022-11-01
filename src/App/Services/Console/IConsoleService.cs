using App.Services.NuGet;
using App.Validators;

namespace App.Services.Console;

public interface IConsoleService
{
    void RenderTitle(string text);
    Task RenderStatusAsync(Func<Task> action);
    void RenderSettingsFile(string filepath);
    void RenderException(Exception exception);
    void RenderValidationErrors(ValidationErrors validationErrors);
    void RenderNugetPackages(ICollection<NuGetPackage> nugetPackages, NuGetParameters parameters);
}