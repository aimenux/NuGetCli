using App.Commands;
using FluentValidation;

namespace App.Validators;

public static class CommandValidator
{
    public static ValidationErrors Validate<TCommand>(TCommand command) where TCommand : AbstractCommand
    {
        return command switch
        {
            MainCommand _ => ValidationErrors.New<MainCommand>(),
            UploadCommand uploadCommand => Validate(new UploadCommandValidator(), uploadCommand),
            DownloadCommand downloadCommand => Validate(new DownloadCommandValidator(), downloadCommand),
            _ => throw new ArgumentOutOfRangeException(nameof(command), typeof(TCommand), "Unexpected command type")
        };
    }

    private static ValidationErrors Validate<TCommand>(IValidator<TCommand> validator, TCommand command) where TCommand : AbstractCommand
    {
        var errors = validator
            .Validate(command)
            .Errors;
        return ValidationErrors.New<TCommand>(errors);
    }
}