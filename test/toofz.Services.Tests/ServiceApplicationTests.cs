using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Moq;
using Xunit;

namespace toofz.Services.Tests
{
    public class ServiceApplicationTests
    {
        public ServiceApplicationTests()
        {
            app = new ServiceApplication<ISettings>(worker, mockServiceBase.Object);
        }

        private readonly WorkerRoleBase<ISettings> worker = new WorkerRoleBaseAdapter();
        private readonly Mock<IServiceBaseStatic> mockServiceBase = new Mock<IServiceBaseStatic>();
        private readonly ServiceApplication<ISettings> app;

        public class Constructor
        {
            private WorkerRoleBase<ISettings> worker = new WorkerRoleBaseAdapter();
            private IServiceBaseStatic serviceBase = Mock.Of<IServiceBaseStatic>();

            [DisplayFact(nameof(ArgumentNullException))]
            public void WorkerIsNull_ThrowsArgumentNullException()
            {
                // Arrange
                worker = null;

                // Act -> Assert
                Assert.Throws<ArgumentNullException>(() =>
                {
                    new ServiceApplication<ISettings>(worker, serviceBase);
                });
            }

            [DisplayFact]
            public void ReturnsInstance()
            {
                // Arrange -> Act
                var app = new ServiceApplication<ISettings>(worker, serviceBase);

                // Assert
                Assert.IsAssignableFrom<ServiceApplication<ISettings>>(app);
            }
        }
        public class RunAsyncOverrideMethod : ServiceApplicationTests
        {
            public RunAsyncOverrideMethod()
            {
                settings = mockSettings.Object;
            }

            private readonly Mock<ISettings> mockSettings = new Mock<ISettings>();
            private readonly ISettings settings;

            [DisplayFact(nameof(IServiceBaseStatic.Run))]
            public async Task CallsRun()
            {
                // Arrange
                mockServiceBase.Setup(s => s.Run(worker)).Callback(() => worker.Start());
                string[] args = { };

                // Act
                await app.RunAsyncOverride(args, settings);

                // Assert
                mockServiceBase.Verify(s => s.Run(worker));
            }
        }

        private class WorkerRoleBaseAdapter : WorkerRoleBase<ISettings>
        {
            public WorkerRoleBaseAdapter() : base("myServiceName", Mock.Of<ISettings>(), new TelemetryClient(), runOnce: false) { }

            protected override Task RunAsyncOverride(CancellationToken cancellationToken) => Task.FromCanceled(cancellationToken);
        }
    }
}
