﻿using App.Commands;
using App.Extensions;
using App.Services.Console;
using App.Services.NuGet;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace App;

public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        try
        {
            return await CreateHostBuilder(args).RunCommandLineApplicationAsync<MainCommand>(args);
        }
        catch (Exception ex)
        {
            ConsoleService.RenderAnyException(ex);
            return Settings.ExitCode.Ko;
        }
    }

    private static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((_, config) =>
            {
                config.AddJsonFile();
                config.AddUserSecrets();
                config.AddEnvironmentVariables();
                config.AddCommandLine(args);
            })
            .ConfigureLogging((_, loggingBuilder) =>
            {
                loggingBuilder.ClearProviders();
                loggingBuilder.AddDefaultLogger();
            })
            .ConfigureServices((hostingContext, services) =>
            {
                services.AddTransient<MainCommand>();
                services.AddTransient<UploadCommand>();
                services.AddTransient<DownloadCommand>();
                services.AddTransient<INuGetService, NuGetService>();
                services.AddTransient<IConsoleService, ConsoleService>();
                services.Configure<Settings>(hostingContext.Configuration.GetSection(nameof(Settings)));
            })
            .AddSerilog();
}