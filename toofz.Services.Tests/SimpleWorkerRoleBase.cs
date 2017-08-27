using System;
using System.Threading;
using System.Threading.Tasks;

namespace toofz.Services.Tests
{
    sealed class SimpleWorkerRoleBase : WorkerRoleBase<ISettings>
    {
        public SimpleWorkerRoleBase(string serviceName, ISettings settings) : base(serviceName, settings) { }

        protected override Task RunAsyncOverride(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
