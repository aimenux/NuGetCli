using FluentValidation.Results;

namespace App.Models;

public class ValidationErrors : List<ValidationFailure>
{
    public Type CommandType { get; set; }

    public ValidationErrors()
    {
    }

    public ValidationErrors(IEnumerable<ValidationFailure> failures) : base(failures)
    {
    }
}