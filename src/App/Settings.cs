﻿namespace App;

public class Settings
{
    public const string PackageId = @"NuGetCli";

    public const string CommandName = @"NuGetCli";

    public const string DefaultNugetFeedUrl = "https://api.nuget.org/v3/index.json";

    public static string GetWorkingDirectory()
    {
        const string workDir = @"C:\Logs";
        return Directory.Exists(workDir) ? workDir : @"./";
    }

    public static class ExitCode
    {
        public const int Ok = 0;
        public const int Ko = -1;
    }
}