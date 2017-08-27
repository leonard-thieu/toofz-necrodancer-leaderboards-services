using System.ServiceProcess;

namespace toofz.Services.Tests
{
    public abstract class SimpleServiceWorkerRole : ServiceBase, IWorkerRole
    {
        public abstract void ConsoleStart(params string[] args);
    }
}
