using System;
using System.IO;
using log4net;
using Moq;
using toofz.Services.Tests.Properties;
using Xunit;

namespace toofz.Services.Tests
{
    public class ApplicationTests
    {
        public class Run
        {
            public Run()
            {
                app = mockApp.Object;
                log = mockLog.Object;
            }

            private readonly Mock<Application<ISettings>> mockApp = new Mock<Application<ISettings>>();
            private Application<ISettings> app;
            private readonly Mock<ILog> mockLog = new Mock<ILog>();
            private ILog log;

            [Fact]
            public void LogIsNull_ThrowsArgumentNullException()
            {
                // Arrange
                var args = new string[0];
                ISettings settings = new StubSettings();
                log = null;

                // Act -> Assert
                Assert.Throws<ArgumentNullException>(() =>
                {
                    app.Run(args, settings, log);
                });
            }

            [Fact]
            public void InitializesLogging()
            {
                // Arrange
                var args = new string[0];
                ISettings settings = new StubSettings();

                // Act
                app.Run(args, settings, log);

                // Assert
                mockLog.Verify(l => l.Debug("Initialized logging."));
            }

            [Fact]
            public void ArgsIsNull_ThrowsArgumentNullException()
            {
                // Arrange
                string[] args = null;
                ISettings settings = new StubSettings();

                // Act -> Assert
                Assert.Throws<ArgumentNullException>(() =>
                {
                    app.Run(args, settings, log);
                });
            }

            [Fact]
            public void SettingsIsNull_ThrowsArgumentNullException()
            {
                // Arrange
                var args = new string[0];
                ISettings settings = null;

                // Act -> Assert
                Assert.Throws<ArgumentNullException>(() =>
                {
                    app.Run(args, settings, log);
                });
            }

            [Fact]
            public void ReloadsSettings()
            {
                // Arrange
                var args = new string[0];
                var mockSettings = new Mock<ISettings>();
                var settings = mockSettings.Object;

                // Act
                app.Run(args, settings, log);

                // Assert
                mockSettings.Verify(s => s.Reload(), Times.Once);
            }

            [Fact]
            public void Returns0()
            {
                // Arrange
                var args = new string[0];
                ISettings settings = new StubSettings();

                // Act
                var ret = app.Run(args, settings, log);

                // Assert
                Assert.Equal(0, ret);
            }

            [Trait("Category", "Uses Settings")]
            [Collection(SettingsCollection.Name)]
            public class IntegrationTests
            {
                private static void SetCurrentDirectoryToSystemDirectory()
                {
                    Directory.SetCurrentDirectory(Environment.SystemDirectory);
                }

                private static void SetCurrentDirectoryToBaseDirectory()
                {
                    Directory.SetCurrentDirectory(AppContext.BaseDirectory);
                }

                public IntegrationTests(SettingsFixture settingsFixture)
                {
                    // Services start with their currenct directory set to the system directory.
                    SetCurrentDirectoryToSystemDirectory();

                    settings = TestSettings.Default;
                    settings.Reload();
                }

                private TestSettings settings;

                [Fact]
                public void LoadsSettingsBeforeAccessingThem()
                {
                    // Arrange
                    // Create a settings file that has the instrumentation key set
                    SetCurrentDirectoryToBaseDirectory();
                    var oldDelayBeforeGC = settings.DelayBeforeGC;
                    var newDelayBeforeGC = oldDelayBeforeGC.Add(TimeSpan.FromSeconds(5));
                    settings.DelayBeforeGC = newDelayBeforeGC;
                    settings.Save();

                    // Reset environment
                    settings.DelayBeforeGC = oldDelayBeforeGC;
                    SetCurrentDirectoryToSystemDirectory();

                    var app = new FakeApplication();
                    var args = new string[0];
                    var log = Mock.Of<ILog>();

                    // Act
                    app.Run(args, settings, log);

                    // Assert
                    Assert.Equal(newDelayBeforeGC, settings.DelayBeforeGC);
                }

                private class FakeApplication : Application<ISettings>
                {
                    internal override int RunOverride(string[] args, ISettings settings) => 0;
                }
            }
        }
    }
}
