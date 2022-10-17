namespace App;

public class Settings
{
    public const string PackageId = @"NuGetCli";
    
    public const string CommandName = @"nuget-cli";
    public string NugetFeed { get; set; } = "https://api.nuget.org/v3/index.json";
    public static string GetWorkingDirectory()
    {
        const string workDir = @"C:\Logs";
        return Directory.Exists(workDir) ? workDir : @"./";
    }
}