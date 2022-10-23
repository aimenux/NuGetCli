using App.Extensions;
using App.Models;
using App.Services.Console;
using McMaster.Extensions.CommandLineUtils;

namespace App.Commands;

public abstract class AbstractCommand
{
    protected IConsoleService ConsoleService;

    protected AbstractCommand(IConsoleService consoleService)
    {
        ConsoleService = consoleService ?? throw new ArgumentNullException(nameof(consoleService));
    }

    public async Task<int> OnExecuteAsync(CommandLineApplication app, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!HasValidOptionsAndArguments(out var validationErrors))
            {
                ConsoleService.RenderValidationErrors(validationErrors);
                return ExitCode.Ko;
            }

            await ExecuteAsync(app, cancellationToken);
            return ExitCode.Ok;
        }
        catch (Exception ex)
        {
            ConsoleService.RenderException(ex);
            return ExitCode.Ko;
        }
    }

    protected abstract Task ExecuteAsync(CommandLineApplication app, CancellationToken cancellationToken = default);

    protected virtual bool HasValidOptionsAndArguments(out ValidationErrors validationErrors)
    {
        validationErrors = this.Validate();
        return !validationErrors.Any();
    }
}