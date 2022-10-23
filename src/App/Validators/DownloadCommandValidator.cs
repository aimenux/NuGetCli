using App.Commands;
using FluentValidation;

namespace App.Validators;

public class DownloadCommandValidator : AbstractValidator<DownloadCommand>
{
    public DownloadCommandValidator()
    {
        RuleFor(x => x.NugetFeedUrl)
            .NotEmpty().WithMessage("Nuget feed url is required");

        RuleFor(x => x.WorkingDirectory)
            .NotEmpty().WithMessage("Download directory is required")
            .Must(Directory.Exists).WithMessage("Download directory '{PropertyValue}' does not exist");

        When(x => !string.IsNullOrWhiteSpace(x.PackagesFile), () =>
        {
            RuleFor(x => x.PackagesFile)
                .Must(File.Exists).WithMessage("Packages file '{PropertyValue}' does not exist");
        });

        When(x => string.IsNullOrWhiteSpace(x.PackagesFile), () =>
        {
            RuleFor(x => x.PackageName)
                .NotEmpty().WithMessage("Package name is required");

            RuleFor(x => x.PackageVersion)
                .NotEmpty().WithMessage("Package version is required");
        });
    }
}