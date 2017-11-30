using System;
using System.Linq;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Moq;
using Xunit;

namespace toofz.Services.Tests
{
    public class WorkerRoleBaseTests
    {
        public class Constructor
        {
            [Fact]
            public void ServiceNameIsNull_ThrowsArgumentException()
            {
                // Arrange
                string serviceName = null;

                // Act -> Assert
                Assert.Throws<ArgumentException>(() =>
                {
                    new EmptyWorkerRoleBase(serviceName);
                });
            }

            [Fact]
            public void ServiceNameIsLongerThanMaxNameLength_ThrowsArgumentException()
            {
                // Arrange
                var serviceName = string.Join("", Enumerable.Repeat('a', ServiceBase.MaxNameLength + 1));

                // Act -> Assert
                Assert.Throws<ArgumentException>(() =>
                {
                    new EmptyWorkerRoleBase(serviceName);
                });
            }

            [Fact]
            public void ServiceNameContainsForwardSlash_ThrowsArgumentException()
            {
                // Arrange
                var serviceName = "/";

                // Act -> Assert
                Assert.Throws<ArgumentException>(() =>
                {
                    new EmptyWorkerRoleBase(serviceName);
                });
            }

            [Fact]
            public void ServiceNameContainsBackSlash_ThrowsArgumentException()
            {
                // Arrange
                var serviceName = @"\";

                // Act -> Assert
                Assert.Throws<ArgumentException>(() =>
                {
                    new EmptyWorkerRoleBase(serviceName);
                });
            }

            [Fact]
            public void SettingsIsNull_ThrowsArgumentNullException()
            {
                // Arrange
                ISettings settings = null;

                // Act -> Assert
                Assert.Throws<ArgumentNullException>(() =>
                {
                    new EmptyWorkerRoleBase(settings);
                });
            }

            [Fact]
            public void ReturnsInstance()
            {
                // Arrange -> Act
                var worker = new EmptyWorkerRoleBase();

                // Assert
                Assert.IsAssignableFrom<WorkerRoleBase<ISettings>>(worker);
            }
        }

        public class SettingsProperty
        {
            [Fact]
            public void ReturnsInstance()
            {
                // Arrange
                var worker = new WorkRoleBaseAdapter();

                // Act
                var settings = worker.PublicSettings;

                // Assert
                Assert.IsAssignableFrom<ISettings>(settings);
            }

            private class WorkRoleBaseAdapter : TestWorkerRoleBase
            {
                public ISettings PublicSettings => Settings;

                protected override Task RunAsyncOverride(CancellationToken cancellationToken) => throw new NotImplementedException();
            }
        }

        public class RunAsyncCoreMethod
        {
            public RunAsyncCoreMethod()
            {
                idle = mockIdle.Object;
            }

            private readonly Mock<IIdle> mockIdle = new Mock<IIdle>();
            private readonly IIdle idle;
            private readonly CancellationToken cancellationToken = CancellationToken.None;

            [Fact]
            public async Task ReloadsSettings()
            {
                // Arrange
                var mockSettings = new Mock<ISettings>();
                var settings = mockSettings.Object;
                var worker = new EmptyWorkerRoleBase(settings);

                // Act
                await worker.RunAsyncCore(idle, cancellationToken);

                // Assert
                mockSettings.Verify(s => s.Reload(), Times.Once);
            }

            [Fact]
            public async Task CallsRunAsyncOverride()
            {
                // Arrange
                var worker = new MockWorkerRoleBase();

                // Act
                await worker.RunAsyncCore(idle, cancellationToken);

                // Assert
                Assert.Equal(1, worker.RunAsyncOverrideCallCount);
            }

            [Fact]
            public async Task WritesTimeRemaining()
            {
                // Arrange
                var worker = new EmptyWorkerRoleBase();

                // Act
                await worker.RunAsyncCore(idle, cancellationToken);

                // Assert
                mockIdle.Verify(i => i.WriteTimeRemaining(), Times.Once);
            }

            [Fact]
            public async Task DelaysForTimeRemaining()
            {
                // Arrange
                var worker = new EmptyWorkerRoleBase();

                // Act
                await worker.RunAsyncCore(idle, cancellationToken);

                // Assert
                mockIdle.Verify(i => i.DelayAsync(It.IsAny<CancellationToken>()), Times.Once);
            }

            private class MockWorkerRoleBase : TestWorkerRoleBase
            {
                public int RunAsyncOverrideCallCount { get; private set; }

                protected override Task RunAsyncOverride(CancellationToken cancellationToken)
                {
                    RunAsyncOverrideCallCount++;

                    return Task.CompletedTask;
                }
            }
        }

        public class OnStopMethod
        {
            [Fact]
            public void StopsService()
            {
                // Arrange
                var worker = new EmptyWorkerRoleBase();
                worker.Start();

                // Act -> Assert
                worker.Stop();
            }
        }

        private class EmptyWorkerRoleBase : WorkerRoleBase<ISettings>
        {
            public EmptyWorkerRoleBase() : this("myServiceName", Mock.Of<ISettings>()) { }
            public EmptyWorkerRoleBase(string serviceName) : this(serviceName, Mock.Of<ISettings>()) { }
            public EmptyWorkerRoleBase(ISettings settings) : this("myServiceName", settings) { }
            public EmptyWorkerRoleBase(string serviceName, ISettings settings) : base(serviceName, settings, new TelemetryClient()) { }

            protected override Task RunAsyncOverride(CancellationToken cancellationToken) => Task.Factory.StartNew(() => { }, cancellationToken);
        }

        private abstract class TestWorkerRoleBase : WorkerRoleBase<ISettings>
        {
            protected TestWorkerRoleBase() : base("myServiceName", Mock.Of<ISettings>(), new TelemetryClient()) { }
        }
    }
}
