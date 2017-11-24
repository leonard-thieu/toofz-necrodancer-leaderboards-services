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
                settings = new StubSettings { KeyDerivationIterations = 1 };
                log = mockLog.Object;
            }

            private readonly Mock<Application<ISettings>> mockApp = new Mock<Application<ISettings>>();
            private readonly Application<ISettings> app;
            private ISettings settings;
            private readonly Mock<ILog> mockLog = new Mock<ILog>();
            private ILog log;
            private readonly TelemetryConfiguration telemetryConfiguration = new TelemetryConfiguration();

            [Fact]
            public void LogIsNull_ThrowsArgumentNullException()
            {
                // Arrange
                string[] args = { };
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
                settings = null;

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
                mockSettings.SetupAllProperties();
                settings = mockSettings.Object;
                settings.KeyDerivationIterations = 1;

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
                settings.InstrumentationKey = null;

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
                settings.InstrumentationKey = "myInstrumentationKey";

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

                // Act
                var ret = app.Run(args, settings, log, telemetryConfiguration);

                // Assert
                Assert.Equal(0, ret);
            }

            [Trait("Category", "Uses file system")]
            public class IntegrationTests : SettingsTestsBase<TestSettings>
            {
                public IntegrationTests() : base(TestSettings.Default)
                {
                    originalCurrentDirectory = Directory.GetCurrentDirectory();
                    // Services start with their current directory set to the system directory.
                    Directory.SetCurrentDirectory(Environment.SystemDirectory);
                }

                private readonly string originalCurrentDirectory;

                protected override void Dispose(bool disposing)
                {
                    if (disposing)
                    {
                        Directory.SetCurrentDirectory(originalCurrentDirectory);
                    }

                    base.Dispose(disposing);
                }

                [Fact]
                public void LoadsSettingsBeforeAccessingThem()
                {
                    // Arrange
                    // Create a settings file that has the instrumentation key set
                    Directory.SetCurrentDirectory(AppContext.BaseDirectory);
                    settings.LeaderboardsConnectionString = new EncryptedSecret("myConnectionString", 1);
                    settings.Save();

                    // Reset environment
                    settings.LeaderboardsConnectionString = null;
                    Directory.SetCurrentDirectory(Environment.SystemDirectory);

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
