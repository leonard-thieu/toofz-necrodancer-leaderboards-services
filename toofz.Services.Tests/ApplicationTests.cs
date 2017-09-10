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
                console = mockConsole.Object;

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
            Mock<IConsole> mockConsole = new Mock<IConsole>();
            IConsole console;

            [TestMethod]
            public void InitializesLogging()
            {
                // Arrange
                string[] args = new string[0];
                ISettings settings = new SimpleSettings();

                // Act
                Application.Run(args, environment, settings, worker, parser, serviceBase, log, console);

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
                    Application.Run(args, environment, settings, worker, parser, serviceBase, log, console);
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
                    Application.Run(args, environment, settings, worker, parser, serviceBase, log, console);
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
                    Application.Run(args, environment, settings, worker, parser, serviceBase, log, console);
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
                    Application.Run(args, environment, settings, worker, parser, serviceBase, log, console);
                });
            }

            [TestMethod]
            public void SetsCurrentDirectoryToBaseDirectory()
            {
                // Arrange
                string[] args = new string[0];
                ISettings settings = new SimpleSettings();

                // Act
                Application.Run(args, environment, settings, worker, parser, serviceBase, log, console);

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
                Application.Run(args, environment, settings, worker, parser, serviceBase, log, console);

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
                Application.Run(args, environment, settings, worker, parser, serviceBase, log, console);

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
                    Application.Run(args, environment, settings, worker, parser, serviceBase, log, console);
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
                Application.Run(args, environment, settings, worker, parser, serviceBase, log, console);

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
                var ret = Application.Run(args, environment, settings, worker, parser, serviceBase, log, console);

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
                Application.Run(args, environment, settings, worker, parser, serviceBase, log, console);

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
                Application.Run(args, environment, settings, worker, parser, serviceBase, log, console);

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
                Application.Run(args, environment, settings, worker, parser, serviceBase, log, console);

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
                Application.Run(args, environment, settings, worker, parser, serviceBase, log, console);

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
                Application.Run(args, environment, settings, worker, parser, serviceBase, log, console);

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
                Application.Run(args, environment, settings, worker, parser, serviceBase, log, console);

                // Assert
                Assert.IsFalse(TelemetryConfiguration.Active.DisableTelemetry);
            }

            [TestMethod]
            public void UserInteractiveIsTrue_StartsAsConsoleApplication()
            {
                // Arrange
                string[] args = new string[0];
                ISettings settings = new SimpleSettings();
                mockEnvironment
                    .SetupGet(e => e.UserInteractive)
                    .Returns(true);
                mockConsole
                    .Setup(c => c.ReadKey(true))
                    .Returns(new ConsoleKeyInfo('c', ConsoleKey.C, shift: false, alt: false, control: true));

                // Act
                Application.Run(args, environment, settings, worker, parser, serviceBase, log, console);

                // Assert
                mockWorker.Verify(w => w.Start(), Times.Once);
            }

            [TestMethod]
            public void StartedAsConsoleApplicationAndCtrlCIsPressed_Stops()
            {
                // Arrange
                string[] args = new string[0];
                ISettings settings = new SimpleSettings();
                mockEnvironment
                    .SetupGet(e => e.UserInteractive)
                    .Returns(true);
                mockConsole
                    .Setup(c => c.ReadKey(true))
                    .Returns(new ConsoleKeyInfo((char)ConsoleKey.C, ConsoleKey.C, shift: false, alt: false, control: true));

                // Act
                Application.Run(args, environment, settings, worker, parser, serviceBase, log, console);

                // Assert
                mockConsole.Verify(c => c.ReadKey(true), Times.Once);
            }

            [TestMethod]
            public void StartedAsConsoleApplicationAndCtrlBreakIsPressed_Stops()
            {
                // Arrange
                string[] args = new string[0];
                ISettings settings = new SimpleSettings();
                mockEnvironment
                    .SetupGet(e => e.UserInteractive)
                    .Returns(true);
                mockConsole
                    .Setup(c => c.ReadKey(true))
                    .Returns(new ConsoleKeyInfo((char)ConsoleKey.Pause, ConsoleKey.Pause, shift: false, alt: false, control: true));

                // Act
                Application.Run(args, environment, settings, worker, parser, serviceBase, log, console);

                // Assert
                mockConsole.Verify(c => c.ReadKey(true), Times.Once);
            }

            [TestMethod]
            public void StartedAsConsoleApplicationAndCancelKeyIsNotPressed_DoesNotStop()
            {
                // Arrange
                string[] args = new string[0];
                ISettings settings = new SimpleSettings();
                mockEnvironment
                    .SetupGet(e => e.UserInteractive)
                    .Returns(true);
                mockConsole
                    .SetupSequence(c => c.ReadKey(true))
                    .Returns(new ConsoleKeyInfo((char)ConsoleKey.Enter, ConsoleKey.Enter, shift: false, alt: false, control: false))
                    .Returns(new ConsoleKeyInfo((char)ConsoleKey.Pause, ConsoleKey.Pause, shift: false, alt: false, control: true));

                // Act
                Application.Run(args, environment, settings, worker, parser, serviceBase, log, console);

                // Assert
                mockConsole.Verify(c => c.ReadKey(true), Times.Exactly(2));
            }

            [TestMethod]
            public void UserInteractiveIsFalseAndServiceBaseIsNull_ThrowsArgumentNullException()
            {
                // Arrange
                string[] args = new string[0];
                ISettings settings = new SimpleSettings();
                mockEnvironment
                    .SetupGet(e => e.UserInteractive)
                    .Returns(false);
                serviceBase = null;

                // Act -> Assert
                Assert.ThrowsException<ArgumentNullException>(() =>
                {
                    Application.Run(args, environment, settings, worker, parser, serviceBase, log, console);
                });
            }

            [TestMethod]
            public void UserInteractiveIsFalse_SetsCurrentDirectory()
            {
                // Arrange
                string[] args = new string[0];
                ISettings settings = new SimpleSettings();
                mockEnvironment
                    .SetupGet(e => e.UserInteractive)
                    .Returns(false);

                // Act
                Application.Run(args, environment, settings, worker, parser, serviceBase, log, console);

                // Assert
                mockEnvironment.VerifySet(e => e.CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory);
            }

            [TestMethod]
            public void UserInteractiveIsFalse_CallsRun()
            {
                // Arrange
                string[] args = new string[0];
                ISettings settings = new SimpleSettings();
                mockEnvironment
                    .SetupGet(e => e.UserInteractive)
                    .Returns(false);

                // Act
                Application.Run(args, environment, settings, worker, parser, serviceBase, log, console);

                // Assert
                mockServiceBase.Verify(s => s.Run(worker));
            }

            [TestMethod]
            public void Returns0()
            {
                // Arrange
                string[] args = new string[0];
                ISettings settings = new SimpleSettings();

                // Act
                var ret = Application.Run(args, environment, settings, worker, parser, serviceBase, log, console);

                // Assert
                Assert.AreEqual(0, ret);
            }
        }
    }
}
