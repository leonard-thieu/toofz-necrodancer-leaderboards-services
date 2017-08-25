using System;
using System.Threading;
using System.Threading.Tasks;

namespace toofz.Services.Tests
{
    sealed class SimpleWorkerRoleBase : WorkerRoleBase<ISettings>
    {
        public SimpleWorkerRoleBase(string serviceName) : base(serviceName) { }

        public override ISettings Settings => throw new NotImplementedException();

        protected override Task RunAsyncOverride(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
