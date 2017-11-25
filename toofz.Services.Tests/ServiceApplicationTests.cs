using System;
using System.IO;
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
            serviceBase = mockServiceBase.Object;
        }

        private readonly WorkerRoleBase<ISettings> worker = new WorkerRoleBaseAdapter();
        private readonly Mock<IServiceBaseStatic> mockServiceBase = new Mock<IServiceBaseStatic>();
        private readonly IServiceBaseStatic serviceBase;

        public class Constructor : ServiceApplicationTests
        {
            [Fact]
            public void ReturnsInstance()
            {
                // Arrange -> Act
                var app = new ServiceApplication<ISettings>(worker, serviceBase);

                // Assert
                Assert.IsAssignableFrom<ServiceApplication<ISettings>>(app);
            }
        }

        [Trait("Category", "Uses file system")]
        [Collection("Uses file system")]
        public class RunAsyncOverrideMethod : ServiceApplicationTests, IDisposable
        {
            public RunAsyncOverrideMethod()
            {
                currentDirectory = Directory.GetCurrentDirectory();
                app = new ServiceApplication<ISettings>(worker, serviceBase);
            }

            private readonly string currentDirectory;
            private readonly ServiceApplication<ISettings> app;

            public void Dispose()
            {
                Directory.SetCurrentDirectory(currentDirectory);
            }

            [Fact(Skip = "How did this work before?")]
            public async Task CallsRun()
            {
                // Arrange
                string[] args = { };
                ISettings settings = new StubSettings();

                // Act
                await app.RunAsyncOverride(args, settings);

                // Assert
                mockServiceBase.Verify(s => s.Run(worker));
            }
        }

        private class WorkerRoleBaseAdapter : WorkerRoleBase<ISettings>
        {
            public WorkerRoleBaseAdapter()
                : base("myServiceName", new StubSettings(), new TelemetryClient()) { }

            protected override Task RunAsyncOverride(CancellationToken cancellationToken)
            {
                Stop();

                return Task.CompletedTask;
            }
        }

    }
}
