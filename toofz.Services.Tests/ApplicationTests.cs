using System;
using System.IO;
using log4net;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace toofz.Services.Tests
{
    class ApplicationTests
    {
        [TestClass]
        public class Run
        {
            public Run()
            {
                environment = mockEnvironment.Object;
                worker = mockWorker.Object;
                parser = mockParser.Object;
                serviceBase = mockServiceBase.Object;
                log = mockLog.Object;

                TelemetryConfiguration.Active.InstrumentationKey = "";
                TelemetryConfiguration.Active.DisableTelemetry = true;
            }

            Mock<IEnvironment> mockEnvironment = new Mock<IEnvironment>();
            IEnvironment environment;
            Mock<SimpleServiceWorkerRole> mockWorker = new Mock<SimpleServiceWorkerRole>();
            SimpleServiceWorkerRole worker;
            Mock<IArgsParser<ISettings>> mockParser = new Mock<IArgsParser<ISettings>>();
            IArgsParser<ISettings> parser;
            Mock<IServiceBase> mockServiceBase = new Mock<IServiceBase>();
            IServiceBase serviceBase;
            Mock<ILog> mockLog = new Mock<ILog>();
            ILog log;

            [TestMethod]
            public void InitializesLogging()
            {
                // Arrange
                string[] args = new string[0];
                ISettings settings = new SimpleSettings();

                // Act
                Application.Run(args, environment, settings, worker, parser, serviceBase, log);

                // Assert
                mockLog.Verify(l => l.Debug("Initialized logging."));
            }

            [TestMethod]
            public void ArgsIsNull_ThrowsArgumentNullException()
            {
                // Arrange
                string[] args = null;
                ISettings settings = new SimpleSettings();

                // Act -> Assert
                Assert.ThrowsException<ArgumentNullException>(() =>
                {
                    Application.Run(args, environment, settings, worker, parser, serviceBase, log);
                });
            }

            [TestMethod]
            public void EnvironmentIsNull_ThrowsArgumentNullException()
            {
                // Arrange
                string[] args = new string[0];
                environment = null;
                ISettings settings = new SimpleSettings();

                // Act -> Assert
                Assert.ThrowsException<ArgumentNullException>(() =>
                {
                    Application.Run(args, environment, settings, worker, parser, serviceBase, log);
                });
            }

            [TestMethod]
            public void WorkerIsNull_ThrowsArgumentNullException()
            {
                // Arrange
                string[] args = new string[0];
                worker = null;
                ISettings settings = new SimpleSettings();

                // Act -> Assert
                Assert.ThrowsException<ArgumentNullException>(() =>
                {
                    Application.Run(args, environment, settings, worker, parser, serviceBase, log);
                });
            }

            [TestMethod]
            public void SettingsIsNull_ThrowsArgumentNullException()
            {
                // Arrange
                string[] args = new string[0];
                ISettings settings = null;

                // Act -> Assert
                Assert.ThrowsException<ArgumentNullException>(() =>
                {
                    Application.Run(args, environment, settings, worker, parser, serviceBase, log);
                });
            }

            [TestMethod]
            public void SetsCurrentDirectoryToBaseDirectory()
            {
                // Arrange
                string[] args = new string[0];
                ISettings settings = new SimpleSettings();

                // Act
                Application.Run(args, environment, settings, worker, parser, serviceBase, log);

                // Assert
                Assert.AreEqual(AppDomain.CurrentDomain.BaseDirectory, Directory.GetCurrentDirectory());
            }

            [TestMethod]
            public void ArgsIsEmpty_DoesNotCallParse()
            {
                // Arrange
                string[] args = new string[0];
                ISettings settings = new SimpleSettings();

                // Act
                Application.Run(args, environment, settings, worker, parser, serviceBase, log);

                // Assert
                mockParser.Verify(p => p.Parse(It.IsAny<string[]>(), It.IsAny<ISettings>()), Times.Never);
            }

            [TestMethod]
            public void ArgsIsNotEmptyAndUserInteractiveIsFalse_DoesNotCallParse()
            {
                // Arrange
                string[] args = new[] { "--myArg" };
                ISettings settings = new SimpleSettings();
                mockEnvironment
                    .SetupGet(e => e.UserInteractive)
                    .Returns(false);

                // Act
                Application.Run(args, environment, settings, worker, parser, serviceBase, log);

                // Assert
                mockParser.Verify(p => p.Parse(It.IsAny<string[]>(), It.IsAny<ISettings>()), Times.Never);
            }

            [TestMethod]
            public void ArgsIsNotEmptyAndUserInteractiveIsTrueAndParserIsNull_ThrowsArgumentNullException()
            {
                // Arrange
                string[] args = new[] { "--myArg" };
                ISettings settings = new SimpleSettings();
                mockEnvironment
                    .SetupGet(e => e.UserInteractive)
                    .Returns(true);
                parser = null;

                // Act -> Assert
                Assert.ThrowsException<ArgumentNullException>(() =>
                {
                    Application.Run(args, environment, settings, worker, parser, serviceBase, log);
                });
            }

            [TestMethod]
            public void ArgsIsNotEmptyAndUserInteractiveIsTrue_CallsParse()
            {
                // Arrange
                string[] args = new[] { "--myArg" };
                ISettings settings = new SimpleSettings();
                mockEnvironment
                    .SetupGet(e => e.UserInteractive)
                    .Returns(true);

                // Act
                Application.Run(args, environment, settings, worker, parser, serviceBase, log);

                // Assert
                mockParser.Verify(p => p.Parse(It.IsAny<string[]>(), It.IsAny<ISettings>()), Times.Once);
            }

            [TestMethod]
            public void ArgsIsNotEmptyAndUserInteractiveIsTrue_ReturnsExitCodeFromParse()
            {
                // Arrange
                string[] args = new[] { "--myArg" };
                ISettings settings = new SimpleSettings();
                mockEnvironment
                    .SetupGet(e => e.UserInteractive)
                    .Returns(true);
                mockParser
                    .Setup(p => p.Parse(It.IsAny<string[]>(), It.IsAny<ISettings>()))
                    .Returns(20);

                // Act
                var ret = Application.Run(args, environment, settings, worker, parser, serviceBase, log);

                // Assert
                Assert.AreEqual(20, ret);
            }

            [TestMethod]
            public void InstrumentationKeyIsNull_LogsWarning()
            {
                // Arrange
                string[] args = new string[0];
                ISettings settings = new SimpleSettings { InstrumentationKey = null };

                // Act
                Application.Run(args, environment, settings, worker, parser, serviceBase, log);

                // Assert
                mockLog.Verify(l => l.Warn("The setting 'InstrumentationKey' is not set. Telemetry is disabled."));
            }

            [TestMethod]
            public void InstrumentationKeyIsNull_DisablesTelemetry()
            {
                // Arrange
                string[] args = new string[0];
                ISettings settings = new SimpleSettings { InstrumentationKey = null };

                // Act
                Application.Run(args, environment, settings, worker, parser, serviceBase, log);

                // Assert
                Assert.IsTrue(TelemetryConfiguration.Active.DisableTelemetry);
            }

            [TestMethod]
            public void InstrumentationKeyIsEmpty_LogsWarning()
            {
                // Arrange
                string[] args = new string[0];
                ISettings settings = new SimpleSettings { InstrumentationKey = "" };

                // Act
                Application.Run(args, environment, settings, worker, parser, serviceBase, log);

                // Assert
                mockLog.Verify(l => l.Warn("The setting 'InstrumentationKey' is not set. Telemetry is disabled."));
            }

            [TestMethod]
            public void InstrumentationKeyIsEmpty_DisablesTelemetry()
            {
                // Arrange
                string[] args = new string[0];
                ISettings settings = new SimpleSettings { InstrumentationKey = "" };

                // Act
                Application.Run(args, environment, settings, worker, parser, serviceBase, log);

                // Assert
                Assert.IsTrue(TelemetryConfiguration.Active.DisableTelemetry);
            }

            [TestMethod]
            public void InstrumentationKeyIsSet_SetsInstrumentationKeyForTelemetry()
            {
                // Arrange
                string[] args = new string[0];
                ISettings settings = new SimpleSettings { InstrumentationKey = "myInstrumentationKey" };

                // Act
                Application.Run(args, environment, settings, worker, parser, serviceBase, log);

                // Assert
                Assert.AreEqual("myInstrumentationKey", TelemetryConfiguration.Active.InstrumentationKey);
            }

            [TestMethod]
            public void InstrumentationKeyIsSet_EnablesTelemetry()
            {
                // Arrange
                string[] args = new string[0];
                ISettings settings = new SimpleSettings { InstrumentationKey = "myInstrumentationKey" };

                // Act
                Application.Run(args, environment, settings, worker, parser, serviceBase, log);

                // Assert
                Assert.IsFalse(TelemetryConfiguration.Active.DisableTelemetry);
            }

            [TestMethod]
            public void UserInteractiveIsTrue_RunsConsoleStart()
            {
                // Arrange
                string[] args = new string[0];
                ISettings settings = new SimpleSettings();
                mockEnvironment
                    .SetupGet(e => e.UserInteractive)
                    .Returns(true);

                // Act
                Application.Run(args, environment, settings, worker, parser, serviceBase, log);

                // Assert
                mockWorker.Verify(w => w.ConsoleStart(), Times.Once);
            }

            [TestMethod]
            public void Returns0()
            {
                // Arrange
                string[] args = new string[0];
                ISettings settings = new SimpleSettings();

                // Act
                var ret = Application.Run(args, environment, settings, worker, parser, serviceBase, log);

                // Assert
                Assert.AreEqual(0, ret);
            }
        }
    }
}
