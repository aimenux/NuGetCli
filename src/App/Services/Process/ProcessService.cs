using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace App.Services.Process;

public class ProcessService : IProcessService
{
    private readonly ILogger<ProcessService> _logger;

    public ProcessService(ILogger<ProcessService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task RunProcessAsync(string name, string arguments, CancellationToken cancellationToken = default)
    {
        void OutputDataReceived(object _, DataReceivedEventArgs args) => LogProcessMessage(args.Data);
        void ErrorDataReceived(object _, DataReceivedEventArgs args) => LogProcessMessage(args.Data);
        await RunProcessAsync(name, arguments, OutputDataReceived, ErrorDataReceived, cancellationToken);
    }

    private static async Task RunProcessAsync(
        string name,
        string arguments,
        DataReceivedEventHandler outputDataReceived,
        DataReceivedEventHandler errorDataReceived,
        CancellationToken cancellationToken)
    {
        var startInfo = new ProcessStartInfo
        {
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            FileName = $@"{name}",
            Arguments = $@"{arguments}"
        };

        var process = new System.Diagnostics.Process
        {
            StartInfo = startInfo
        };

        process.ErrorDataReceived += errorDataReceived;
        process.OutputDataReceived += outputDataReceived;

        process.Start();
        process.BeginErrorReadLine();
        process.BeginOutputReadLine();
        await process.WaitForExitAsync(cancellationToken);
        process.Close();
    }

    private void LogProcessMessage(string message)
    {
        if (!string.IsNullOrWhiteSpace(message))
        {
            _logger.LogTrace(message);
        }
    }
}