using App.Commands;
using FluentValidation;

namespace App.Validators;

public class UploadCommandValidator : AbstractValidator<UploadCommand>
{
    public UploadCommandValidator()
    {
        RuleFor(x => x.NugetFeedUrl)
            .NotEmpty().WithMessage("Nuget feed url is required");

        RuleFor(x => x.WorkingDirectory)
            .NotEmpty().WithMessage("Upload directory is required")
            .Must(Directory.Exists).WithMessage("Upload directory '{PropertyValue}' does not exist");
    }
}