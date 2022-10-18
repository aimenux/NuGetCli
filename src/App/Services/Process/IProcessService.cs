namespace App.Services.Process
{
    public interface IProcessService
    {
        Task RunProcessAsync(string name, string arguments, CancellationToken cancellationToken = default);
    }
}
