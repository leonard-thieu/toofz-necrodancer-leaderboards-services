using System;
using System.Threading;
using System.Threading.Tasks;

namespace toofz.Services
{
    internal sealed class ServiceApplication<TSettings> : Application<TSettings>
        where TSettings : ISettings
    {
        public ServiceApplication(
            WorkerRoleBase<TSettings> worker,
            IServiceBaseStatic serviceBase)
        {
            this.worker = worker ?? throw new ArgumentNullException(nameof(worker));
            this.serviceBase = serviceBase;
        }

        private readonly WorkerRoleBase<TSettings> worker;
        private readonly IServiceBaseStatic serviceBase;

        internal override async Task<int> RunAsyncOverride(string[] args, TSettings settings)
        {
            // Watching on a separate thread so that exceptions can be observed.
            var serviceRunWatcher = new Thread(() => serviceBase.Run(worker)) { IsBackground = true };
            serviceRunWatcher.Start();

            await worker.Initialization.ConfigureAwait(false);
            await worker.Completion.ConfigureAwait(false);

            return 0;
        }
    }
}
