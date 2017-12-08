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
            app = new ConsoleApplication<ISettings>(mockWorker.Object, mockParser.Object, mockConsole.Object);
        }

        private readonly Mock<IConsoleWorkerRole> mockWorker = new Mock<IConsoleWorkerRole>();
        private readonly Mock<IArgsParser<ISettings>> mockParser = new Mock<IArgsParser<ISettings>>();
        private readonly Mock<IConsoleStatic> mockConsole = new Mock<IConsoleStatic>();
        private readonly ConsoleApplication<ISettings> app;

        public class Constructor
        {
            [DisplayFact]
            public void ReturnsInstance()
            {
                // Arrange
                var worker = Mock.Of<IConsoleWorkerRole>();
                var parser = Mock.Of<IArgsParser<ISettings>>();
                var console = Mock.Of<IConsoleStatic>();

                // Act
                var app = new ConsoleApplication<ISettings>(worker, parser, console);

                // Assert
                Assert.IsAssignableFrom<ConsoleApplication<ISettings>>(app);
            }
        }

        public class RunAsyncOverrideMethod : ConsoleApplicationTests
        {
            public RunAsyncOverrideMethod()
            {
                completionTcs = new TaskCompletionSource<bool>();
                mockWorker.Setup(w => w.Completion).Returns(completionTcs.Task);
            }

            private readonly ISettings settings = new StubSettings();
            private readonly TaskCompletionSource<bool> completionTcs;

            [DisplayFact(nameof(IArgsParser<ISettings>.Parse))]
            public async Task ArgsIsNotEmpty_CallsParse()
            {
                // Arrange
                string[] args = { "--myArg" };

                // Act
                await app.RunAsyncOverride(args, settings);

                // Assert
                mockParser.Verify(p => p.Parse(args, settings), Times.Once);
            }

            [DisplayFact(nameof(IArgsParser<ISettings>.Parse))]
            public async Task ArgsIsNotEmpty_ReturnsExitCodeFromParse()
            {
                // Arrange
                string[] args = { "--myArg" };
                mockParser.Setup(p => p.Parse(args, settings)).Returns(20);

                // Act
                var ret = await app.RunAsyncOverride(args, settings);

                // Assert
                Assert.Equal(20, ret);
            }

            [DisplayFact(nameof(IArgsParser<ISettings>.Parse))]
            public async Task ArgsIsEmpty_DoesNotCallParse()
            {
                // Arrange
                string[] args = { };
                mockConsole
                    .Setup(c => c.ReadKey(true))
                    .Callback(() => completionTcs.SetResult(true))
                    .Returns(new ConsoleKeyInfo('c', ConsoleKey.C, shift: false, alt: false, control: true));

                // Act
                await app.RunAsyncOverride(args, settings);

                // Assert
                mockParser.Verify(p => p.Parse(args, settings), Times.Never);
            }

            [DisplayFact]
            public async Task ArgsIsEmpty_Starts()
            {
                // Arrange
                string[] args = { };
                mockConsole
                    .Setup(c => c.ReadKey(true))
                    .Callback(() => completionTcs.SetResult(true))
                    .Returns(new ConsoleKeyInfo('c', ConsoleKey.C, shift: false, alt: false, control: true));

                // Act
                await app.RunAsyncOverride(args, settings);

                // Assert
                mockWorker.Verify(w => w.Start(), Times.Once);
            }

            [DisplayFact("CtrlC")]
            public async Task CtrlCIsPressed_Stops()
            {
                // Arrange
                string[] args = { };
                mockConsole
                    .Setup(c => c.ReadKey(true))
                    .Callback(() => completionTcs.SetResult(true))
                    .Returns(new ConsoleKeyInfo((char)ConsoleKey.C, ConsoleKey.C, shift: false, alt: false, control: true));

                // Act
                await app.RunAsyncOverride(args, settings);

                // Assert
                mockConsole.Verify(c => c.ReadKey(true), Times.Once);
            }

            [DisplayFact("CtrlBreak")]
            public async Task CtrlBreakIsPressed_Stops()
            {
                // Arrange
                string[] args = { };
                mockConsole
                    .Setup(c => c.ReadKey(true))
                    .Callback(() => completionTcs.SetResult(true))
                    .Returns(new ConsoleKeyInfo((char)ConsoleKey.Pause, ConsoleKey.Pause, shift: false, alt: false, control: true));

                // Act
                await app.RunAsyncOverride(args, settings);

                // Assert
                mockConsole.Verify(c => c.ReadKey(true), Times.Once);
            }

            [DisplayFact]
            public async Task CancelKeyIsNotPressed_DoesNotStop()
            {
                // Arrange
                string[] args = { };
                var i = 0;
                mockConsole
                    .Setup(c => c.ReadKey(true))
                    .Returns(() =>
                    {
                        ConsoleKeyInfo keyInfo;

                        switch (i)
                        {
                            case 0:
                                keyInfo = new ConsoleKeyInfo((char)ConsoleKey.Spacebar, ConsoleKey.Spacebar, shift: false, alt: false, control: false);
                                Assert.False(ConsoleApplication<ISettings>.IsCancelKeyPress(keyInfo));
                                break;
                            case 1:
                                completionTcs.SetResult(true);
                                keyInfo = new ConsoleKeyInfo((char)ConsoleKey.Pause, ConsoleKey.Pause, shift: false, alt: false, control: true);
                                Assert.True(ConsoleApplication<ISettings>.IsCancelKeyPress(keyInfo));
                                break;
                            default:
                                throw new InvalidOperationException($"Setup called {i} times but only expected 2.");
                        }

                        i++;

                        return keyInfo;
                    });

                // Act
                await app.RunAsyncOverride(args, settings);

                // Assert
                mockConsole.Verify(c => c.ReadKey(true), Times.Exactly(2));
            }
        }
    }
}
