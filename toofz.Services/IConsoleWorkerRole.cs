namespace toofz.Services
{
    public interface IConsoleWorkerRole
    {
        void Start(params string[] args);
        void Stop();
    }
}