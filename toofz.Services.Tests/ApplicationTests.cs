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
                var args = new string[0];
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
                var args = new string[0];
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
                var args = new string[0];
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
                var args = new string[0];
                var mockSettings = new Mock<ISettings>();
                var settings = mockSettings.Object;

                // Act
                app.Run(args, settings, log, telemetryConfiguration);

                // Assert
                mockSettings.Verify(s => s.Reload(), Times.Once);
            }

            [Fact]
            public void InstrumentationKeyIsNull_LogsWarning()
            {
                // Arrange
                var args = new string[0];
                ISettings settings = new StubSettings { InstrumentationKey = null };

                // Act
                app.Run(args, settings, log, telemetryConfiguration);

                // Assert
                mockLog.Verify(l => l.Warn("The setting 'InstrumentationKey' is not set. Telemetry is disabled."));
            }

            [Fact]
            public void InstrumentationKeyIsNull_DisablesTelemetry()
            {
                // Arrange
                var args = new string[0];
                ISettings settings = new StubSettings { InstrumentationKey = null };

                // Act
                app.Run(args, settings, log, telemetryConfiguration);

                // Assert
                Assert.True(telemetryConfiguration.DisableTelemetry);
            }

            [Fact]
            public void InstrumentationKeyIsEmpty_LogsWarning()
            {
                // Arrange
                var args = new string[0];
                ISettings settings = new StubSettings { InstrumentationKey = "" };

                // Act
                app.Run(args, settings, log, telemetryConfiguration);

                // Assert
                mockLog.Verify(l => l.Warn("The setting 'InstrumentationKey' is not set. Telemetry is disabled."));
            }

            [Fact]
            public void InstrumentationKeyIsEmpty_DisablesTelemetry()
            {
                // Arrange
                var args = new string[0];
                ISettings settings = new StubSettings { InstrumentationKey = "" };

                // Act
                app.Run(args, settings, log, telemetryConfiguration);

                // Assert
                Assert.True(telemetryConfiguration.DisableTelemetry);
            }

            [Fact]
            public void InstrumentationKeyIsSet_SetsInstrumentationKeyForTelemetry()
            {
                // Arrange
                var args = new string[0];
                ISettings settings = new StubSettings { InstrumentationKey = "myInstrumentationKey" };

                // Act
                app.Run(args, settings, log, telemetryConfiguration);

                // Assert
                Assert.Equal("myInstrumentationKey", telemetryConfiguration.InstrumentationKey);
            }

            [Fact]
            public void InstrumentationKeyIsSet_EnablesTelemetry()
            {
                // Arrange
                var args = new string[0];
                ISettings settings = new StubSettings { InstrumentationKey = "myInstrumentationKey" };

                // Act
                app.Run(args, settings, log, telemetryConfiguration);

                // Assert
                Assert.False(telemetryConfiguration.DisableTelemetry);
            }

            [Fact]
            public void Returns0()
            {
                // Arrange
                var args = new string[0];
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
                    // Services start with their currenct directory set to the system directory.
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
                    var args = new string[0];
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
