using App.Commands;
using App.Models;
using App.Validators;
using FluentValidation;
using FluentValidation.Results;
using McMaster.Extensions.CommandLineUtils;

namespace App.Extensions
{
    public static class ValidationExtensions
    {
        public static ValidationErrors Validate<T>(this T command) where T : AbstractCommand
        {
            return command switch
            {
                MainCommand _ => new ValidationErrors { CommandType = typeof(MainCommand) },
                UploadCommand uploadCommand => Validate(new UploadCommandValidator(), uploadCommand),
                DownloadCommand downloadCommand => Validate(new DownloadCommandValidator(), downloadCommand),
                _ => throw new ArgumentOutOfRangeException(nameof(command), typeof(T), "Unexpected command type")
            };
        }

        public static string OptionName(this ValidationFailure failure, Type commandType)
        {
            var propertyInfo = commandType
                .GetProperties()
                .Single(x => x.Name.IgnoreCaseEquals(failure.PropertyName));

            var optionAttribute = propertyInfo
                .GetCustomAttributes(true)
                .OfType<OptionAttribute>()
                .Single();

            return optionAttribute.Template;
        }

        private static ValidationErrors Validate<T>(IValidator<T> validator, T command) where T : AbstractCommand
        {
            var errors = validator
                .Validate(command)
                .Errors;
            return new ValidationErrors(errors)
            {
                CommandType = command.GetType()
            };
        }
    }
}
