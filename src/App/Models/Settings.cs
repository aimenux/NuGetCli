namespace App.Models;

public class Settings
{
    public const string PackageId = @"NuGetCli";

    public const string CommandName = @"nuget-cli";

    public const string DefaultNugetFeedUrl = "https://api.nuget.org/v3/index.json";

    public static string GetWorkingDirectory()
    {
        const string workDir = @"C:\Logs";
        return Directory.Exists(workDir) ? workDir : @"./";
    }
}