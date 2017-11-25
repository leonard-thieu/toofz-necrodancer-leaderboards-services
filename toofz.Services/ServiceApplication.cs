using System;
using System.ServiceProcess;
using System.Threading;

namespace toofz.Services
{
    internal sealed class ServiceApplication<TSettings> : Application<TSettings>
        where TSettings : ISettings
    {
        public ServiceApplication(
            ServiceBase worker,
            IServiceBaseStatic serviceBase)
        {
            this.worker = worker ?? throw new ArgumentNullException(nameof(worker));
            this.serviceBase = serviceBase;
        }

        private readonly ServiceBase worker;
        private readonly IServiceBaseStatic serviceBase;

        internal override int RunOverride(string[] args, TSettings settings)
        {
            // Watching on a separate thread so that exceptions can be observed.
            var serviceRunWatcher = new Thread(() => serviceBase.Run(worker)) { IsBackground = true };
            serviceRunWatcher.Start();

            return 0;
        }
    }
}
