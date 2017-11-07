using System;
using System.IO;
using System.ServiceProcess;

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

            // Services have their starting current directory set to the system directory. The current directory must 
            // be set to the base directory so the settings file may be found.
            Directory.SetCurrentDirectory(AppContext.BaseDirectory);
        }

        private readonly ServiceBase worker;
        private readonly IServiceBaseStatic serviceBase;

        internal override int RunOverride(string[] args, TSettings settings)
        {
            serviceBase.Run(worker);

            return 0;
        }
    }
}
