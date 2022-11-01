using Microsoft.Extensions.Configuration;
using static App.Extensions.PathExtensions;

namespace App.Extensions;

public static class ConfigurationExtensions
{
    public static void AddJsonFile(this IConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.SetBasePath(GetDirectoryPath());
        var environment = Environment.GetEnvironmentVariable("ENVIRONMENT");
        configurationBuilder.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        configurationBuilder.AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true);
    }

    public static void AddUserSecrets(this IConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.AddUserSecrets(typeof(Program).Assembly);
    }
}