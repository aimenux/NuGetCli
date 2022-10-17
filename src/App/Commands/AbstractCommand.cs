using App.Services;
using App.Services.Console;
using McMaster.Extensions.CommandLineUtils;

namespace App.Commands;

public abstract class AbstractCommand
{
    protected IConsoleService ConsoleService;

    protected string CommandName => GetType().Name;

    protected AbstractCommand(IConsoleService consoleService)
    {
        ConsoleService = consoleService ?? throw new ArgumentNullException(nameof(consoleService));
    }

    public async Task OnExecuteAsync(CommandLineApplication app, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!HasValidOptions())
            {
                throw new Exception($"Invalid options for command {CommandName}");
            }

            if (!HasValidArguments())
            {
                throw new Exception($"Invalid arguments for command {CommandName}");
            }

            await ExecuteAsync(app, cancellationToken);
        }
        catch (Exception ex)
        {
            ConsoleService.RenderException(ex);
        }
    }

    protected abstract Task ExecuteAsync(CommandLineApplication app, CancellationToken cancellationToken = default);

    protected virtual bool HasValidOptions() => true;

    protected virtual bool HasValidArguments() => true;
}