using System;
using System.IO;
using System.ServiceProcess;
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
            Mock<ServiceWorkerRoleBase> mockWorker = new Mock<ServiceWorkerRoleBase>();
            ServiceWorkerRoleBase worker;
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
                var args = new string[0];
                ISettings settings = new StubSettings();

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
                ISettings settings = new StubSettings();

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
                var args = new string[0];
                environment = null;
                ISettings settings = new StubSettings();

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
                var args = new string[0];
                worker = null;
                ISettings settings = new StubSettings();

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
                var args = new string[0];
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
                var args = new string[0];
                ISettings settings = new StubSettings();

                // Act
                Application.Run(args, environment, settings, worker, parser, serviceBase, log, console);

                // Assert
                Assert.AreEqual(AppDomain.CurrentDomain.BaseDirectory, Directory.GetCurrentDirectory());
            }

            [TestMethod]
            public void ArgsIsEmpty_DoesNotCallParse()
            {
                // Arrange
                var args = new string[0];
                ISettings settings = new StubSettings();

                // Act
                Application.Run(args, environment, settings, worker, parser, serviceBase, log, console);

                // Assert
                mockParser.Verify(p => p.Parse(It.IsAny<string[]>(), It.IsAny<ISettings>()), Times.Never);
            }

            [TestMethod]
            public void ArgsIsNotEmptyAndUserInteractiveIsFalse_DoesNotCallParse()
            {
                // Arrange
                var args = new[] { "--myArg" };
                ISettings settings = new StubSettings();
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
                var args = new[] { "--myArg" };
                ISettings settings = new StubSettings();
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
                var args = new[] { "--myArg" };
                ISettings settings = new StubSettings();
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
                var args = new[] { "--myArg" };
                ISettings settings = new StubSettings();
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
                var args = new string[0];
                ISettings settings = new StubSettings { InstrumentationKey = null };

                // Act
                Application.Run(args, environment, settings, worker, parser, serviceBase, log, console);

                // Assert
                mockLog.Verify(l => l.Warn("The setting 'InstrumentationKey' is not set. Telemetry is disabled."));
            }

            [TestMethod]
            public void InstrumentationKeyIsNull_DisablesTelemetry()
            {
                // Arrange
                var args = new string[0];
                ISettings settings = new StubSettings { InstrumentationKey = null };

                // Act
                Application.Run(args, environment, settings, worker, parser, serviceBase, log, console);

                // Assert
                Assert.IsTrue(TelemetryConfiguration.Active.DisableTelemetry);
            }

            [TestMethod]
            public void InstrumentationKeyIsEmpty_LogsWarning()
            {
                // Arrange
                var args = new string[0];
                ISettings settings = new StubSettings { InstrumentationKey = "" };

                // Act
                Application.Run(args, environment, settings, worker, parser, serviceBase, log, console);

                // Assert
                mockLog.Verify(l => l.Warn("The setting 'InstrumentationKey' is not set. Telemetry is disabled."));
            }

            [TestMethod]
            public void InstrumentationKeyIsEmpty_DisablesTelemetry()
            {
                // Arrange
                var args = new string[0];
                ISettings settings = new StubSettings { InstrumentationKey = "" };

                // Act
                Application.Run(args, environment, settings, worker, parser, serviceBase, log, console);

                // Assert
                Assert.IsTrue(TelemetryConfiguration.Active.DisableTelemetry);
            }

            [TestMethod]
            public void InstrumentationKeyIsSet_SetsInstrumentationKeyForTelemetry()
            {
                // Arrange
                var args = new string[0];
                ISettings settings = new StubSettings { InstrumentationKey = "myInstrumentationKey" };

                // Act
                Application.Run(args, environment, settings, worker, parser, serviceBase, log, console);

                // Assert
                Assert.AreEqual("myInstrumentationKey", TelemetryConfiguration.Active.InstrumentationKey);
            }

            [TestMethod]
            public void InstrumentationKeyIsSet_EnablesTelemetry()
            {
                // Arrange
                var args = new string[0];
                ISettings settings = new StubSettings { InstrumentationKey = "myInstrumentationKey" };

                // Act
                Application.Run(args, environment, settings, worker, parser, serviceBase, log, console);

                // Assert
                Assert.IsFalse(TelemetryConfiguration.Active.DisableTelemetry);
            }

            [TestMethod]
            public void UserInteractiveIsTrue_StartsAsConsoleApplication()
            {
                // Arrange
                var args = new string[0];
                ISettings settings = new StubSettings();
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
                var args = new string[0];
                ISettings settings = new StubSettings();
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
                var args = new string[0];
                ISettings settings = new StubSettings();
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
                var args = new string[0];
                ISettings settings = new StubSettings();
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
                var args = new string[0];
                ISettings settings = new StubSettings();
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
                var args = new string[0];
                ISettings settings = new StubSettings();
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
                var args = new string[0];
                ISettings settings = new StubSettings();
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
                var args = new string[0];
                ISettings settings = new StubSettings();

                // Act
                var ret = Application.Run(args, environment, settings, worker, parser, serviceBase, log, console);

                // Assert
                Assert.AreEqual(0, ret);
            }

            internal abstract class ServiceWorkerRoleBase : ServiceBase, IWorkerRole
            {
                public abstract void Start(params string[] args);
            }
        }
    }
}
