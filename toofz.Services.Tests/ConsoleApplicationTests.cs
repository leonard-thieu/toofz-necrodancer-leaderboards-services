﻿using System;
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
            [Fact]
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

            [Fact]
            public async Task ArgsIsNotEmpty_CallsParse()
            {
                // Arrange
                string[] args = { "--myArg" };

                // Act
                await app.RunAsyncOverride(args, settings);

                // Assert
                mockParser.Verify(p => p.Parse(args, settings), Times.Once);
            }

            [Fact]
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

            [Fact]
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

            [Fact]
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

            [Fact]
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

            [Fact]
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

            [Fact]
            public async Task CancelKeyIsNotPressed_DoesNotStop()
            {
                // Arrange
                string[] args = { };
                var i = 0;
                mockConsole
                    .Setup(c => c.ReadKey(true))
                    .Callback(() => completionTcs.SetResult(true))
                    .Returns(() =>
                    {
                        switch (i)
                        {
                            case 0:
                                return new ConsoleKeyInfo((char)ConsoleKey.Enter, ConsoleKey.Enter, shift: false, alt: false, control: false);
                            case 1:
                                completionTcs.SetResult(true);
                                return new ConsoleKeyInfo((char)ConsoleKey.Pause, ConsoleKey.Pause, shift: false, alt: false, control: true);
                            default:
                                throw new InvalidOperationException($"Setup called {i} times but only expected 2.");
                        }
                    });

                // Act
                await app.RunAsyncOverride(args, settings);

                // Assert
                mockConsole.Verify(c => c.ReadKey(true), Times.Exactly(2));
            }
        }
    }
}
