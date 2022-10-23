using App.Extensions;
using Buildalyzer;

namespace App.Services.NuGet;

public static class NuGetFileParser
{
    public static async Task<ICollection<NuGetPackage>> ParseFileAsync(string packagesFile, CancellationToken cancellationToken)
    {
        if (packagesFile.IgnoreCaseEndsWith("sln"))
        {
            return await ParseSolutionFileAsync(packagesFile, cancellationToken);
        }

        return await ParseBuildLogFileAsync(packagesFile, cancellationToken);
    }

    private static Task<ICollection<NuGetPackage>> ParseSolutionFileAsync(string packagesFile, CancellationToken _)
    {
        var manager = new AnalyzerManager(packagesFile);
        var packages = manager.Projects
            .SelectMany(x => x.Value.ProjectFile.PackageReferences)
            .Select(x => new NuGetPackage(x.Name, x.Version))
            .Distinct()
            .ToList();
        return Task.FromResult<ICollection<NuGetPackage>>(packages);
    }

    private static async Task<ICollection<NuGetPackage>> ParseBuildLogFileAsync(string packagesFile, CancellationToken cancellationToken)
    {
        var lines = await File.ReadAllLinesAsync(packagesFile, cancellationToken);

        var packages = new List<NuGetPackage>();
        foreach (var line in lines)
        {
            if (TryParseLine(line, out var nuGetPackage))
            {
                packages.Add(nuGetPackage);
            }
        }

        return packages
            .Distinct()
            .OrderBy(x => x.Name)
            .ThenBy(x => x.Version)
            .ToList();
    }

    private static bool TryParseLine(string line, out NuGetPackage nuGetPackage)
    {
        nuGetPackage = null;
        if (!line.IgnoreCaseContains("NotFound")) return false;
        var url = line.Split(' ').FirstOrDefault(x => x.IgnoreCaseStartWith("http"));
        if (string.IsNullOrWhiteSpace(url)) return false;
        var parts = url.Split('/').Reverse().Take(2).ToArray();
        if (parts.Length != 2) return false;
        nuGetPackage = new NuGetPackage(parts[1], parts[0]);
        return true;
    }
}