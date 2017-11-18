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
        public class StartNewMethod
        {
            [Fact]
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

        public class WriteTimeRemainingMethod
        {
            [Fact]
            public void TimeRemaining_WritesTimeRemaining()
            {
                // Arrange
                TimeSpan updateInterval = TimeSpan.FromSeconds(75);
                DateTime startTime = new DateTime(2017, 8, 27, 12, 51, 1);
                var mockLog = new Mock<ILog>();
                var log = mockLog.Object;
                var idle = new Idle(updateInterval, startTime, log);
                var from = startTime + TimeSpan.FromSeconds(60);

                // Act
                idle.WriteTimeRemaining(from);

                // Assert
                mockLog.Verify(l => l.Info("Next run takes place in 15 seconds..."));
            }

            [Fact]
            public void NoTimeRemaining_WritesStartingImmediately()
            {
                // Arrange
                TimeSpan updateInterval = TimeSpan.FromSeconds(75);
                DateTime startTime = new DateTime(2017, 8, 27, 12, 51, 1);
                var mockLog = new Mock<ILog>();
                var log = mockLog.Object;
                var idle = new Idle(updateInterval, startTime, log);
                var from = startTime + TimeSpan.FromSeconds(90);

                // Act
                idle.WriteTimeRemaining(from);

                // Assert
                mockLog.Verify(l => l.Info("Next run starting immediately..."));
            }
        }

        public class GetTimeRemainingMethod
        {
            [Fact]
            public void ReturnsTimeRemaining()
            {
                // Arrange
                TimeSpan updateInterval = TimeSpan.FromSeconds(75);
                DateTime startTime = new DateTime(2017, 8, 27, 12, 51, 1);
                var mockLog = new Mock<ILog>();
                var log = mockLog.Object;
                var idle = new Idle(updateInterval, startTime, log);
                var from = startTime + TimeSpan.FromSeconds(60);

                // Act
                var remaining = idle.GetTimeRemaining(from);

                // Assert
                Assert.Equal(TimeSpan.FromSeconds(15), remaining);
            }
        }

        public class DelayAsyncMethod
        {
            [Fact]
            public async Task TimeRemaining_DelaysForTimeRemaining()
            {
                // Arrange
                TimeSpan updateInterval = TimeSpan.FromSeconds(75);
                DateTime startTime = new DateTime(2017, 8, 27, 12, 51, 1);
                var mockLog = new Mock<ILog>();
                var log = mockLog.Object;
                var idle = new Idle(updateInterval, startTime, log);
                var from = startTime + TimeSpan.FromSeconds(60);
                var mockTask = new Mock<ITask>();
                var task = mockTask.Object;
                var cancellationToken = CancellationToken.None;

                // Act
                await idle.DelayAsync(from, task, cancellationToken);

                // Assert
                mockTask.Verify(t => t.Delay(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Once);
            }

            [Fact]
            public async Task NoTimeRemaining_DoesNotDelay()
            {
                // Arrange
                TimeSpan updateInterval = TimeSpan.FromSeconds(75);
                DateTime startTime = new DateTime(2017, 8, 27, 12, 51, 1);
                var mockLog = new Mock<ILog>();
                var log = mockLog.Object;
                var idle = new Idle(updateInterval, startTime, log);
                var from = startTime + TimeSpan.FromSeconds(90);
                var mockTask = new Mock<ITask>();
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
