using System.Threading;
using System.Threading.Tasks;

namespace toofz.Services.Tests
{
    sealed class SimpleWorkerRoleBase : WorkerRoleBase<ISettings>
    {
        public SimpleWorkerRoleBase(string serviceName, ISettings settings) : base(serviceName, settings) { }

        public ISettings PublicSettings => Settings;
        public int RunAsyncOverrideCallCount { get; private set; }

        protected override Task RunAsyncOverride(CancellationToken cancellationToken)
        {
            RunAsyncOverrideCallCount++;

            return Task.FromResult(0);
        }
    }
}
