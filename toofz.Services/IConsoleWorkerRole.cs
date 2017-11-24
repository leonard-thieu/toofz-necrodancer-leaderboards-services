using System.Threading.Tasks;

namespace toofz.Services
{
    /// <summary>
    /// Represents a worker in a console application.
    /// </summary>
    public interface IConsoleWorkerRole
    {
        /// <summary>
        /// Signals that work has stopped due to a fault or the <see cref="ServiceBase.Stop"/> command was issued.
        /// </summary>
        Task Completion { get; }

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