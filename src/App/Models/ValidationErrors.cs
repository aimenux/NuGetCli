namespace App.Models;

public class ValidationErrors : List<ValidationError>
{
    public void Add(string name, string error)
    {
        Add(new ValidationError(name, error));
    }
}