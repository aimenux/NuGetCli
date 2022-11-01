using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Debugging;
using static App.Extensions.PathExtensions;

namespace App.Extensions;

public static class LoggingExtensions
{
    public static void AddDefaultLogger(this ILoggingBuilder loggingBuilder)
    {
        const string categoryName = Settings.PackageId;
        var services = loggingBuilder.Services;
        services.AddSingleton(serviceProvider =>
        {
            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            return loggerFactory.CreateLogger(categoryName);
        });
    }

    public static IHostBuilder AddSerilog(this IHostBuilder builder)
    {
        return builder.UseSerilog((hostingContext, serviceProvider, loggerConfiguration) =>
        {
            SelfLog.Enable(Console.Error);

            var settingsFile = GetSettingFilePath();
            if (File.Exists(settingsFile))
            {
                loggerConfiguration
                    .ReadFrom.Configuration(hostingContext.Configuration);
            }
            else
            {
                var outputTemplate = GetOutputTemplate(hostingContext.Configuration);
                loggerConfiguration
                    .WriteTo.Console(outputTemplate: outputTemplate);
            }
        });
    }

    private static string GetOutputTemplate(IConfiguration configuration)
    {
        return configuration["Serilog:WriteTo:0:Args:outputTemplate"];
    }
}