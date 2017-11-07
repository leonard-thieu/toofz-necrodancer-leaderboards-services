using System.ServiceProcess;

namespace toofz.Services
{
    internal interface IServiceBaseStatic
    {
        /// <summary>
        /// Registers the executable for a service with the Service Control Manager (SCM).
        /// </summary>
        /// <param name="service">
        /// A <see cref="ServiceBase"/> which indicates a service to start.
        /// </param>
        /// <exception cref="System.ArgumentException">
        /// <paramref name="service"/> is null.
        /// </exception>
        void Run(ServiceBase service);
    }
}
