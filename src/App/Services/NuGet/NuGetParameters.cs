namespace App.Services.NuGet;

public class NuGetParameters
{
    public string PackageName { get; set; }
    public string PackageVersion { get; set; }
    public string PackagesFile { get; set; }
    public string OutputDirectory { get; set; }
}