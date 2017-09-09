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
                ISettings settings = new SimpleSettings();

                // Act -> Assert
                Assert.ThrowsException<ArgumentException>(() =>
                {
                    new SimpleWorkerRoleBase(serviceName, settings);
                });
            }

            [TestMethod]
            public void ServiceNameIsLongerThanMaxNameLength_ThrowsArgumentException()
            {
                // Arrange
                string serviceName = string.Join("", Enumerable.Repeat('a', ServiceBase.MaxNameLength + 1));
                ISettings settings = new SimpleSettings();

                // Act -> Assert
                Assert.ThrowsException<ArgumentException>(() =>
                {
                    new SimpleWorkerRoleBase(serviceName, settings);
                });
            }

            [TestMethod]
            public void ServiceNameContainsForwardSlash_ThrowsArgumentException()
            {
                // Arrange
                string serviceName = "/";
                ISettings settings = new SimpleSettings();

                // Act -> Assert
                Assert.ThrowsException<ArgumentException>(() =>
                {
                    new SimpleWorkerRoleBase(serviceName, settings);
                });
            }

            [TestMethod]
            public void ServiceNameContainsBackSlash_ThrowsArgumentException()
            {
                // Arrange
                string serviceName = @"\";
                ISettings settings = new SimpleSettings();

                // Act -> Assert
                Assert.ThrowsException<ArgumentException>(() =>
                {
                    new SimpleWorkerRoleBase(serviceName, settings);
                });
            }

            [TestMethod]
            public void SettingsIsNull_ThrowsArgumentNullException()
            {
                // Arrange
                string serviceName = "myServiceName";
                ISettings settings = null;

                // Act -> Assert
                Assert.ThrowsException<ArgumentNullException>(() =>
                {
                    new SimpleWorkerRoleBase(serviceName, settings);
                });
            }

            [TestMethod]
            public void ReturnsInstance()
            {
                // Arrange
                string serviceName = "myServiceName";
                ISettings settings = new SimpleSettings();

                // Act
                var worker = new SimpleWorkerRoleBase(serviceName, settings);

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
                string serviceName = "myServiceName";
                ISettings settings = new SimpleSettings();
                var worker = new SimpleWorkerRoleBase(serviceName, settings);

                // Act
                var settings2 = worker.PublicSettings;

                // Assert
                Assert.IsInstanceOfType(settings2, typeof(ISettings));
            }
        }

        [TestClass]
        public class RunCoreAsyncMethod
        {
            [TestMethod]
            public async Task ReloadsSettings()
            {
                // Arrange
                string serviceName = "myServiceName";
                Mock<ISettings> mockSettings = new Mock<ISettings>();
                ISettings settings = mockSettings.Object;
                var worker = new SimpleWorkerRoleBase(serviceName, settings);
                Mock<IIdle> mockIdle = new Mock<IIdle>();
                IIdle idle = mockIdle.Object;
                Mock<ILog> mockLog = new Mock<ILog>();
                ILog log = mockLog.Object;

                // Act
                await worker.RunCoreAsync(idle, log, CancellationToken.None);

                // Assert
                mockSettings.Verify(s => s.Reload(), Times.Once);
            }

            [TestMethod]
            public async Task CallsRunAsyncOverride()
            {
                // Arrange
                string serviceName = "myServiceName";
                Mock<ISettings> mockSettings = new Mock<ISettings>();
                ISettings settings = mockSettings.Object;
                var worker = new SimpleWorkerRoleBase(serviceName, settings);
                Mock<IIdle> mockIdle = new Mock<IIdle>();
                IIdle idle = mockIdle.Object;
                Mock<ILog> mockLog = new Mock<ILog>();
                ILog log = mockLog.Object;

                // Act
                await worker.RunCoreAsync(idle, log, CancellationToken.None);

                // Assert
                Assert.AreEqual(1, worker.RunAsyncOverrideCallCount);
            }

            [TestMethod]
            public async Task WritesTimeRemaining()
            {
                // Arrange
                string serviceName = "myServiceName";
                Mock<ISettings> mockSettings = new Mock<ISettings>();
                ISettings settings = mockSettings.Object;
                var worker = new SimpleWorkerRoleBase(serviceName, settings);
                Mock<IIdle> mockIdle = new Mock<IIdle>();
                IIdle idle = mockIdle.Object;
                Mock<ILog> mockLog = new Mock<ILog>();
                ILog log = mockLog.Object;

                // Act
                await worker.RunCoreAsync(idle, log, CancellationToken.None);

                // Assert
                mockIdle.Verify(i => i.WriteTimeRemaining(), Times.Once);
            }

            [TestMethod]
            public async Task DelaysForTimeRemaining()
            {
                // Arrange
                string serviceName = "myServiceName";
                Mock<ISettings> mockSettings = new Mock<ISettings>();
                ISettings settings = mockSettings.Object;
                var worker = new SimpleWorkerRoleBase(serviceName, settings);
                Mock<IIdle> mockIdle = new Mock<IIdle>();
                IIdle idle = mockIdle.Object;
                Mock<ILog> mockLog = new Mock<ILog>();
                ILog log = mockLog.Object;

                // Act
                await worker.RunCoreAsync(idle, log, CancellationToken.None);

                // Assert
                mockIdle.Verify(i => i.DelayAsync(It.IsAny<CancellationToken>()), Times.Once);
            }
        }
    }
}
