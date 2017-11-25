namespace toofz.Services
{
    /// <summary>
    /// Represents a worker in a console application.
    /// </summary>
    public interface IConsoleWorkerRole
    {
        /// <summary>
        /// Starts the service. This method is intended to be called from console applications.
        /// </summary>
        /// <param name="args">Data passed by the command line.</param>
        void Start(params string[] args);
        /// <summary>
        /// Stops the executing service.
        /// </summary>
        void Stop();
    }
}