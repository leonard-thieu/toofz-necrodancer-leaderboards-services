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
            app = new ServiceApplication<ISettings>(worker, mockServiceBase.Object);
        }

        private readonly WorkerRoleBase<ISettings> worker = new WorkerRoleBaseAdapter();
        private readonly Mock<IServiceBaseStatic> mockServiceBase = new Mock<IServiceBaseStatic>();
        private readonly ServiceApplication<ISettings> app;

        public class Constructor
        {
            private WorkerRoleBase<ISettings> worker = new WorkerRoleBaseAdapter();
            private IServiceBaseStatic serviceBase = Mock.Of<IServiceBaseStatic>();

            [Fact]
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
            }

            private readonly string currentDirectory;
            private readonly ISettings settings = new StubSettings();

            public void Dispose()
            {
                Directory.SetCurrentDirectory(currentDirectory);
            }

            [Fact]
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
            public WorkerRoleBaseAdapter() : base("myServiceName", new StubSettings(), new TelemetryClient()) { }

            protected override Task RunAsyncOverride(CancellationToken cancellationToken) => Task.FromCanceled(cancellationToken);
        }
    }
}
