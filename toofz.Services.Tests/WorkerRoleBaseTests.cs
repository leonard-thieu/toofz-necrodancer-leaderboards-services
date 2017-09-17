using System;
using System.Linq;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace toofz.Services.Tests
{
    class WorkerRoleBaseTests
    {
        [TestClass]
        public class LogErrorMethod
        {
            [TestMethod]
            public void ExIsAggregateExceptionAndHasMultipleInnerExceptions_LogsFlattenedException()
            {
                // Arrange
                var mockLog = new Mock<ILog>();
                var log = mockLog.Object;
                var message = "myMessage";
                var inner1 = new Exception();
                var inner2 = new Exception();
                var ex = new AggregateException(inner1, inner2);

                // Act
                WorkerRoleBase<ISettings>.LogError(log, message, ex);

                // Assert
                mockLog.Verify(l => l.Error("myMessage", It.Is<Exception>(e => HasMultipleInnerExceptions(e))), Times.Once);
            }

            bool HasMultipleInnerExceptions(Exception e)
            {
                var aggr = e as AggregateException;
                if (aggr != null)
                {
                    return aggr.InnerExceptions.Count > 1;
                }

                return false;
            }

            [TestMethod]
            public void ExIsAggregateException_LogsErrorWithInnerException()
            {
                // Arrange
                var mockLog = new Mock<ILog>();
                var log = mockLog.Object;
                var message = "myMessage";
                var inner = new Exception();
                var ex = new AggregateException(inner);

                // Act
                WorkerRoleBase<ISettings>.LogError(log, message, ex);

                // Assert
                mockLog.Verify(l => l.Error("myMessage", inner), Times.Once);
            }

            [TestMethod]
            public void LogsError()
            {
                // Arrange
                var mockLog = new Mock<ILog>();
                var log = mockLog.Object;
                var message = "myMessage";
                var ex = new Exception();

                // Act
                WorkerRoleBase<ISettings>.LogError(log, message, ex);

                // Assert
                mockLog.Verify(l => l.Error("myMessage", ex), Times.Once);
            }
        }

        [TestClass]
        public class Constructor
        {
            [TestMethod]
            public void ServiceNameIsNull_ThrowsArgumentException()
            {
                // Arrange
                string serviceName = null;

                // Act -> Assert
                Assert.ThrowsException<ArgumentException>(() =>
                {
                    new EmptyWorkerRoleBase(serviceName);
                });
            }

            [TestMethod]
            public void ServiceNameIsLongerThanMaxNameLength_ThrowsArgumentException()
            {
                // Arrange
                var serviceName = string.Join("", Enumerable.Repeat('a', ServiceBase.MaxNameLength + 1));

                // Act -> Assert
                Assert.ThrowsException<ArgumentException>(() =>
                {
                    new EmptyWorkerRoleBase(serviceName);
                });
            }

            [TestMethod]
            public void ServiceNameContainsForwardSlash_ThrowsArgumentException()
            {
                // Arrange
                var serviceName = "/";

                // Act -> Assert
                Assert.ThrowsException<ArgumentException>(() =>
                {
                    new EmptyWorkerRoleBase(serviceName);
                });
            }

            [TestMethod]
            public void ServiceNameContainsBackSlash_ThrowsArgumentException()
            {
                // Arrange
                var serviceName = @"\";

                // Act -> Assert
                Assert.ThrowsException<ArgumentException>(() =>
                {
                    new EmptyWorkerRoleBase(serviceName);
                });
            }

            [TestMethod]
            public void SettingsIsNull_ThrowsArgumentNullException()
            {
                // Arrange
                ISettings settings = null;

                // Act -> Assert
                Assert.ThrowsException<ArgumentNullException>(() =>
                {
                    new EmptyWorkerRoleBase(settings);
                });
            }

            [TestMethod]
            public void ReturnsInstance()
            {
                // Arrange -> Act
                var worker = new EmptyWorkerRoleBase();

                // Assert
                Assert.IsInstanceOfType(worker, typeof(WorkerRoleBase<ISettings>));
            }
        }

        [TestClass]
        public class SettingsProperty
        {
            [TestMethod]
            public void ReturnsInstance()
            {
                // Arrange
                var worker = new WorkRoleBaseAdapter();

                // Act
                var settings = worker.PublicSettings;

                // Assert
                Assert.IsInstanceOfType(settings, typeof(ISettings));
            }

            class WorkRoleBaseAdapter : WorkerRoleBase<ISettings>
            {
                public WorkRoleBaseAdapter() : base("myServiceName", Mock.Of<ISettings>()) { }

                public ISettings PublicSettings => Settings;

                protected override Task RunAsyncOverride(CancellationToken cancellationToken) => throw new NotImplementedException();
            }
        }

        [TestClass]
        public class RunAsyncMethod
        {
            [TestMethod]
            public async Task TaskCanceledExceptionIsThrown_DoesNotThrow()
            {
                // Arrange
                var cts = new CancellationTokenSource();
                var worker = new CancellingWorkerRoleBase(cts);
                var log = Mock.Of<ILog>();
                var cancellationToken = cts.Token;

                // Act -> Assert
                await worker.RunAsync(log, cancellationToken);
            }
        }

        [TestClass]
        public class RunAsyncCoreMethod
        {
            [TestMethod]
            public async Task ReloadsSettings()
            {
                // Arrange
                var mockSettings = new Mock<ISettings>();
                var settings = mockSettings.Object;
                var worker = new EmptyWorkerRoleBase(settings);
                var idle = Mock.Of<IIdle>();
                var log = Mock.Of<ILog>();

                // Act
                await worker.RunAsyncCore(idle, log, CancellationToken.None);

                // Assert
                mockSettings.Verify(s => s.Reload(), Times.Once);
            }

            [TestMethod]
            public async Task CallsRunAsyncOverride()
            {
                // Arrange
                var worker = new MockWorkerRoleBase();
                var idle = Mock.Of<IIdle>();
                var log = Mock.Of<ILog>();

                // Act
                await worker.RunAsyncCore(idle, log, CancellationToken.None);

                // Assert
                Assert.AreEqual(1, worker.RunAsyncOverrideCallCount);
            }

            [TestMethod]
            public async Task RunAsyncOverrideThrowsTaskCanceledException_ThrowsTaskCanceledException()
            {
                // Arrange
                var cts = new CancellationTokenSource();
                var worker = new CancellingWorkerRoleBase(cts);
                var idle = Mock.Of<IIdle>();
                var log = Mock.Of<ILog>();
                var cancellationToken = cts.Token;

                // Act -> Assert
                await Assert.ThrowsExceptionAsync<TaskCanceledException>(() =>
                {
                    return worker.RunAsyncCore(idle, log, cancellationToken);
                });
            }

            [TestMethod]
            public async Task RunAsyncOverrideThrowsTypeInitializationException_ThrowsTypeInitializationException()
            {
                // Arrange
                var worker = new TypeInitializationExceptionWorkerRoleBase();
                var idle = Mock.Of<IIdle>();
                var log = Mock.Of<ILog>();

                // Act -> Assert
                await Assert.ThrowsExceptionAsync<TypeInitializationException>(() =>
                {
                    return worker.RunAsyncCore(idle, log, CancellationToken.None);
                });
            }

            [TestMethod]
            public async Task RunAsyncOverrideThrowsException_LogsError()
            {
                // Arrange
                var worker = new BrokenWorkerRoleBase();
                var idle = Mock.Of<IIdle>();
                var mockLog = new Mock<ILog>();
                var log = mockLog.Object;

                // Act
                await worker.RunAsyncCore(idle, log, CancellationToken.None);

                // Assert
                mockLog.Verify(l => l.Error("Failed to complete run due to an error.", It.IsAny<Exception>()));
            }

            [TestMethod]
            public async Task WritesTimeRemaining()
            {
                // Arrange
                var worker = new EmptyWorkerRoleBase();
                var mockIdle = new Mock<IIdle>();
                var idle = mockIdle.Object;
                var log = Mock.Of<ILog>();

                // Act
                await worker.RunAsyncCore(idle, log, CancellationToken.None);

                // Assert
                mockIdle.Verify(i => i.WriteTimeRemaining(), Times.Once);
            }

            [TestMethod]
            public async Task DelaysForTimeRemaining()
            {
                // Arrange
                var worker = new EmptyWorkerRoleBase();
                var mockIdle = new Mock<IIdle>();
                var idle = mockIdle.Object;
                var log = Mock.Of<ILog>();

                // Act
                await worker.RunAsyncCore(idle, log, CancellationToken.None);

                // Assert
                mockIdle.Verify(i => i.DelayAsync(It.IsAny<CancellationToken>()), Times.Once);
            }

            class MockWorkerRoleBase : WorkerRoleBase<ISettings>
            {
                public MockWorkerRoleBase() : base("myServiceName", Mock.Of<ISettings>()) { }

                public int RunAsyncOverrideCallCount { get; private set; }

                protected override Task RunAsyncOverride(CancellationToken cancellationToken)
                {
                    RunAsyncOverrideCallCount++;

                    return Task.FromResult(0);
                }
            }

            class TypeInitializationExceptionWorkerRoleBase : WorkerRoleBase<ISettings>
            {
                public TypeInitializationExceptionWorkerRoleBase() : base("myServiceName", Mock.Of<ISettings>()) { }

                protected override Task RunAsyncOverride(CancellationToken cancellationToken) => throw new TypeInitializationException(nameof(TypeInitializationExceptionWorkerRoleBase), new Exception());
            }

            class BrokenWorkerRoleBase : WorkerRoleBase<ISettings>
            {
                public BrokenWorkerRoleBase() : base("myServiceName", Mock.Of<ISettings>()) { }

                protected override Task RunAsyncOverride(CancellationToken cancellationToken) => throw new Exception();
            }
        }

        [TestClass]
        public class OnStopMethod
        {
            [TestMethod]
            public void StopsService()
            {
                // Arrange
                var worker = new EmptyWorkerRoleBase();
                worker.Start();

                // Act -> Assert
                worker.Stop();
            }
        }

        class EmptyWorkerRoleBase : WorkerRoleBase<ISettings>
        {
            public EmptyWorkerRoleBase() : this("myServiceName", Mock.Of<ISettings>()) { }
            public EmptyWorkerRoleBase(string serviceName) : this(serviceName, Mock.Of<ISettings>()) { }
            public EmptyWorkerRoleBase(ISettings settings) : this("myServiceName", settings) { }
            public EmptyWorkerRoleBase(string serviceName, ISettings settings) : base(serviceName, settings) { }

            protected override Task RunAsyncOverride(CancellationToken cancellationToken) => Task.Factory.StartNew(() => { }, cancellationToken);
        }

        class CancellingWorkerRoleBase : WorkerRoleBase<ISettings>
        {
            public CancellingWorkerRoleBase(CancellationTokenSource cts) : base("myServiceName", Mock.Of<ISettings>())
            {
                this.cts = cts;
            }

            readonly CancellationTokenSource cts;

            protected override Task RunAsyncOverride(CancellationToken cancellationToken)
            {
                cts.Cancel();

                return Task.Factory.StartNew(() => { }, cancellationToken);
            }
        }
    }
}
