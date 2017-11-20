using System;
using System.IO;
using log4net;
using Microsoft.ApplicationInsights.Extensibility;
using Moq;
using toofz.Services.Tests.Properties;
using Xunit;

namespace toofz.Services.Tests
{
    public class ApplicationTests
    {
        public class RunMethod
        {
            public RunMethod()
            {
                app = mockApp.Object;
                log = mockLog.Object;
            }

            private readonly Mock<Application<ISettings>> mockApp = new Mock<Application<ISettings>>();
            private Application<ISettings> app;
            private readonly Mock<ILog> mockLog = new Mock<ILog>();
            private ILog log;
            private TelemetryConfiguration telemetryConfiguration = new TelemetryConfiguration();

            [Fact]
            public void LogIsNull_ThrowsArgumentNullException()
            {
                // Arrange
                string[] args = { };
                ISettings settings = new StubSettings();
                log = null;

                // Act -> Assert
                Assert.Throws<ArgumentNullException>(() =>
                {
                    app.Run(args, settings, log, telemetryConfiguration);
                });
            }

            [Fact]
            public void InitializesLogging()
            {
                // Arrange
                string[] args = { };
                ISettings settings = new StubSettings();

                // Act
                app.Run(args, settings, log, telemetryConfiguration);

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
                    app.Run(args, settings, log, telemetryConfiguration);
                });
            }

            [Fact]
            public void SettingsIsNull_ThrowsArgumentNullException()
            {
                // Arrange
                string[] args = { };
                ISettings settings = null;

                // Act -> Assert
                Assert.Throws<ArgumentNullException>(() =>
                {
                    app.Run(args, settings, log, telemetryConfiguration);
                });
            }

            [Fact]
            public void ReloadsSettings()
            {
                // Arrange
                string[] args = { };
                var mockSettings = new Mock<ISettings>();
                var settings = mockSettings.Object;

                // Act
                app.Run(args, settings, log, telemetryConfiguration);

                // Assert
                mockSettings.Verify(s => s.Reload(), Times.Once);
            }

            [Fact]
            public void InstrumentationKeyIsNotSet_LogsWarning()
            {
                // Arrange
                string[] args = { };
                ISettings settings = new StubSettings { InstrumentationKey = null };

                // Act
                app.Run(args, settings, log, telemetryConfiguration);

                // Assert
                mockLog.Verify(l => l.Warn("An Application Insights instrumentation key is not set. Telemetry will not be reported to Application Insights."));
            }

            [Fact]
            public void InstrumentationKeyIsSet_SetsInstrumentationKeyForTelemetry()
            {
                // Arrange
                string[] args = { };
                ISettings settings = new StubSettings { InstrumentationKey = "myInstrumentationKey" };

                // Act
                app.Run(args, settings, log, telemetryConfiguration);

                // Assert
                Assert.Equal("myInstrumentationKey", telemetryConfiguration.InstrumentationKey);
            }

            [Fact]
            public void Returns0()
            {
                // Arrange
                string[] args = { };
                ISettings settings = new StubSettings();

                // Act
                var ret = app.Run(args, settings, log, telemetryConfiguration);

                // Assert
                Assert.Equal(0, ret);
            }

            [Trait("Category", "Uses Settings")]
            [Collection(SettingsCollection.Name)]
            public class IntegrationTests
            {
                private static void ResetEnvironment()
                {
                    // Services start with their current directory set to the system directory.
                    SetCurrentDirectoryToSystemDirectory();
                }

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
                    ResetEnvironment();

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
                    settings.LeaderboardsConnectionString = new EncryptedSecret("myConnectionString", 1);
                    settings.Save();

                    // Reset environment
                    settings.LeaderboardsConnectionString = null;
                    SetCurrentDirectoryToSystemDirectory();

                    var app = new FakeApplication();
                    string[] args = { };
                    var log = Mock.Of<ILog>();
                    var telemetryConfiguration = TelemetryConfiguration.Active;

                    // Act
                    app.Run(args, settings, log, telemetryConfiguration);

                    // Assert
                    Assert.Equal("myConnectionString", settings.LeaderboardsConnectionString.Decrypt());
                }

                private class FakeApplication : Application<ISettings>
                {
                    internal override int RunOverride(string[] args, ISettings settings) => 0;
                }
            }
        }
    }
}
