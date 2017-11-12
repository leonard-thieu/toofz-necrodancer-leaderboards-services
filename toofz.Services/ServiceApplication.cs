using System;
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
