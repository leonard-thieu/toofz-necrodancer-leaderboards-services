using System;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using Moq;
using Xunit;

namespace toofz.Services.Tests
{
    public class IdleTests
    {
        public IdleTests()
        {
            idle = new Idle(updateInterval, startTime, mockLog.Object);
        }

        private readonly TimeSpan updateInterval = TimeSpan.FromSeconds(75);
        private readonly DateTime startTime = new DateTime(2017, 8, 27, 12, 51, 1);
        private readonly Mock<ILog> mockLog = new Mock<ILog>();
        private readonly Idle idle;

        public class StartNewMethod
        {
            [DisplayFact]
            public void ReturnsInstance()
            {
                // Arrange
                TimeSpan updateInterval = TimeSpan.Zero;

                // Act
                var idle = Idle.StartNew(updateInterval);

                // Assert
                Assert.IsAssignableFrom<Idle>(idle);
            }
        }

        public class WriteTimeRemainingMethod : IdleTests
        {
            public WriteTimeRemainingMethod()
            {
                mockLog.Setup(l => l.IsInfoEnabled).Returns(true);
            }

            [DisplayFact]
            public void TimeRemaining_WritesTimeRemaining()
            {
                // Arrange
                var from = startTime + TimeSpan.FromSeconds(60);

                // Act
                idle.WriteTimeRemaining(from);

                // Assert
                mockLog.Verify(l => l.Info("Next run takes place in 15 seconds..."));
            }

            [DisplayFact]
            public void NoTimeRemaining_WritesStartingImmediately()
            {
                // Arrange
                var from = startTime + TimeSpan.FromSeconds(90);

                // Act
                idle.WriteTimeRemaining(from);

                // Assert
                mockLog.Verify(l => l.Info("Next run starting immediately..."));
            }
        }

        public class GetTimeRemainingMethod : IdleTests
        {
            [DisplayFact]
            public void ReturnsTimeRemaining()
            {
                // Arrange
                var from = startTime + TimeSpan.FromSeconds(60);

                // Act
                var remaining = idle.GetTimeRemaining(from);

                // Assert
                Assert.Equal(TimeSpan.FromSeconds(15), remaining);
            }
        }

        public class DelayAsyncMethod : IdleTests
        {
            [DisplayFact]
            public async Task TimeRemaining_DelaysForTimeRemaining()
            {
                // Arrange
                var from = startTime + TimeSpan.FromSeconds(60);
                var mockTask = new Mock<ITaskStatic>();
                var task = mockTask.Object;
                var cancellationToken = CancellationToken.None;

                // Act
                await idle.DelayAsync(from, task, cancellationToken);

                // Assert
                mockTask.Verify(t => t.Delay(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Once);
            }

            [DisplayFact]
            public async Task NoTimeRemaining_DoesNotDelay()
            {
                // Arrange
                var from = startTime + TimeSpan.FromSeconds(90);
                var mockTask = new Mock<ITaskStatic>();
                var task = mockTask.Object;
                var cancellationToken = CancellationToken.None;

                // Act
                await idle.DelayAsync(from, task, cancellationToken);

                // Assert
                mockTask.Verify(t => t.Delay(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Never);
            }
        }
    }
}
