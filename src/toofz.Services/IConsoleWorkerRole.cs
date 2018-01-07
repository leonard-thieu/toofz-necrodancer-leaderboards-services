using System.Threading.Tasks;

namespace toofz.Services
{
    /// <summary>
    /// Represents a worker in a console application.
    /// </summary>
    public interface IConsoleWorkerRole
    {
        /// <summary>
        /// Signals that work has started.
        /// </summary>
        Task Initialization { get; }
        /// <summary>
        /// Signals that work has stopped due to a fault or <see cref="Stop"/> was called.
        /// </summary>
        Task Completion { get; }

        /// <summary>
        /// Starts the service. This method is intended to be called from console applications.
        /// </summary>
        void Start();
        /// <summary>
        /// Stops the executing service.
        /// </summary>
        void Stop();
    }
}