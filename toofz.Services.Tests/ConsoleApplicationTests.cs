using System;
using System.Threading.Tasks;
using Moq;
using Xunit;

namespace toofz.Services.Tests
{
    public class ConsoleApplicationTests
    {
        public ConsoleApplicationTests()
        {
            worker = mockWorker.Object;
            parser = mockParser.Object;
            console = mockConsole.Object;
            app = new ConsoleApplication<ISettings>(worker, parser, console);
        }

        private Mock<IConsoleWorkerRole> mockWorker = new Mock<IConsoleWorkerRole>();
        private IConsoleWorkerRole worker;
        private Mock<IArgsParser<ISettings>> mockParser = new Mock<IArgsParser<ISettings>>();
        private IArgsParser<ISettings> parser;
        private Mock<IConsoleStatic> mockConsole = new Mock<IConsoleStatic>();
        private IConsoleStatic console;
        private ConsoleApplication<ISettings> app;

        public class Constructor : ConsoleApplicationTests
        {
            [Fact]
            public void ReturnsInstance()
            {
                // Arrange -> App
                var app = new ConsoleApplication<ISettings>(worker, parser, console);

                // Assert
                Assert.IsAssignableFrom<ConsoleApplication<ISettings>>(app);
            }
        }

        public class RunAsyncOverrideMethod : ConsoleApplicationTests
        {
            [Fact]
            public async Task ArgsIsNotEmpty_CallsParse()
            {
                // Arrange
                var args = new[] { "--myArg" };
                ISettings settings = new StubSettings();

                // Act
                await app.RunAsyncOverride(args, settings);

                // Assert
                mockParser.Verify(p => p.Parse(It.IsAny<string[]>(), It.IsAny<ISettings>()), Times.Once);
            }

            [Fact]
            public async Task ArgsIsNotEmpty_ReturnsExitCodeFromParse()
            {
                // Arrange
                var args = new[] { "--myArg" };
                ISettings settings = new StubSettings();
                mockParser
                    .Setup(p => p.Parse(It.IsAny<string[]>(), It.IsAny<ISettings>()))
                    .Returns(20);

                // Act
                var ret = await app.RunAsyncOverride(args, settings);

                // Assert
                Assert.Equal(20, ret);
            }

            [Fact]
            public async Task ArgsIsEmpty_DoesNotCallParse()
            {
                // Arrange
                var args = new string[0];
                ISettings settings = new StubSettings();
                mockConsole
                    .Setup(c => c.ReadKey(true))
                    .Returns(new ConsoleKeyInfo('c', ConsoleKey.C, shift: false, alt: false, control: true));

                // Act
                await app.RunAsyncOverride(args, settings);

                // Assert
                mockParser.Verify(p => p.Parse(It.IsAny<string[]>(), It.IsAny<ISettings>()), Times.Never);
            }

            [Fact]
            public async Task ArgsIsEmpty_Starts()
            {
                // Arrange
                var args = new string[0];
                ISettings settings = new StubSettings();
                mockConsole
                    .Setup(c => c.ReadKey(true))
                    .Returns(new ConsoleKeyInfo('c', ConsoleKey.C, shift: false, alt: false, control: true));

                // Act
                await app.RunAsyncOverride(args, settings);

                // Assert
                mockWorker.Verify(w => w.Start(), Times.Once);
            }

            [Fact]
            public async Task CtrlCIsPressed_Stops()
            {
                // Arrange
                var args = new string[0];
                ISettings settings = new StubSettings();
                mockConsole
                    .Setup(c => c.ReadKey(true))
                    .Returns(new ConsoleKeyInfo((char)ConsoleKey.C, ConsoleKey.C, shift: false, alt: false, control: true));

                // Act
                await app.RunAsyncOverride(args, settings);

                // Assert
                mockConsole.Verify(c => c.ReadKey(true), Times.Once);
            }

            [Fact]
            public async Task CtrlBreakIsPressed_Stops()
            {
                // Arrange
                var args = new string[0];
                ISettings settings = new StubSettings();
                mockConsole
                    .Setup(c => c.ReadKey(true))
                    .Returns(new ConsoleKeyInfo((char)ConsoleKey.Pause, ConsoleKey.Pause, shift: false, alt: false, control: true));

                // Act
                await app.RunAsyncOverride(args, settings);

                // Assert
                mockConsole.Verify(c => c.ReadKey(true), Times.Once);
            }

            [Fact(Skip = "Determine why this test is flaky.")]
            public async Task CancelKeyIsNotPressed_DoesNotStop()
            {
                // Arrange
                var args = new string[0];
                ISettings settings = new StubSettings();
                mockConsole
                    .SetupSequence(c => c.ReadKey(true))
                    .Returns(new ConsoleKeyInfo((char)ConsoleKey.Enter, ConsoleKey.Enter, shift: false, alt: false, control: false))
                    .Returns(new ConsoleKeyInfo((char)ConsoleKey.Pause, ConsoleKey.Pause, shift: false, alt: false, control: true));

                // Act
                await app.RunAsyncOverride(args, settings);

                // Assert
                mockConsole.Verify(c => c.ReadKey(true), Times.Exactly(2));
            }
        }
    }
}
