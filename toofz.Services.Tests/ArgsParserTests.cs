using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using toofz.TestsShared;

namespace toofz.Services.Tests
{
    class ArgsParserTests
    {
        [TestClass]
        public class Constructor
        {
            [TestMethod]
            public void InReaderIsNull_ThrowsArgumentNullException()
            {
                // Arrange
                TextReader inReader = null;
                TextWriter outWriter = TextWriter.Null;
                TextWriter errorWriter = TextWriter.Null;

                // Act -> Assert
                Assert.ThrowsException<ArgumentNullException>(() =>
                {
                    new StubArgsParser(inReader, outWriter, errorWriter);
                });
            }

            [TestMethod]
            public void OutWriterIsNull_ThrowsArgumentNullException()
            {
                // Arrange
                TextReader inReader = TextReader.Null;
                TextWriter outWriter = null;
                TextWriter errorWriter = TextWriter.Null;

                // Act -> Assert
                Assert.ThrowsException<ArgumentNullException>(() =>
                {
                    new StubArgsParser(inReader, outWriter, errorWriter);
                });
            }

            [TestMethod]
            public void ErrorWriterIsNull_ThrowsArgumentNullException()
            {
                // Arrange
                TextReader inReader = TextReader.Null;
                TextWriter outWriter = TextWriter.Null;
                TextWriter errorWriter = null;

                // Act -> Assert
                Assert.ThrowsException<ArgumentNullException>(() =>
                {
                    new StubArgsParser(inReader, outWriter, errorWriter);
                });
            }

            [TestMethod]
            public void ReturnsInstance()
            {
                // Arrange
                TextReader inReader = TextReader.Null;
                TextWriter outWriter = TextWriter.Null;
                TextWriter errorWriter = TextWriter.Null;

                // Act
                var parser = new StubArgsParser(inReader, outWriter, errorWriter);

                // Assert
                Assert.IsInstanceOfType(parser, typeof(StubArgsParser));
            }
        }

        [TestClass]
        public class Parse
        {
            public Parse()
            {
                inReader = mockInReader.Object;
                parser = new StubArgsParser(inReader, outWriter, errorWriter);

                mockSettings
                    .SetupAllProperties();
                settings = mockSettings.Object;
            }

            Mock<TextReader> mockInReader = new Mock<TextReader>(MockBehavior.Strict);
            TextReader inReader;
            TextWriter outWriter = new StringWriter();
            TextWriter errorWriter = new StringWriter();
            StubArgsParser parser;
            Mock<ISettings> mockSettings = new Mock<ISettings>();
            ISettings settings;

            [TestMethod]
            public void ArgsIsNull_ThrowsArgumentNullException()
            {
                // Arrange
                string[] args = null;

                // Act -> Assert
                Assert.ThrowsException<ArgumentNullException>(() =>
                {
                    parser.Parse(args, settings);
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
                    parser.Parse(args, settings);
                });
            }

            [TestMethod]
            public void ExtraArg_ShowsError()
            {
                // Arrange
                string[] args = new[] { "myExtraArg" };
                settings = new StubSettings();

                // Act
                parser.Parse(args, settings);
                var error = errorWriter.ToString();

                // Assert
                AssertHelper.NormalizedAreEqual(@"toofz.Services.Tests.dll: 'myExtraArg' is not a valid option.
", error);
            }

            [TestMethod]
            public void ExtraArg_Returns1()
            {
                // Arrange
                string[] args = new[] { "myExtraArg" };

                // Act
                var exitCode = parser.Parse(args, settings);

                // Assert
                Assert.AreEqual(1, exitCode);
            }

            [TestMethod]
            public void Help_ShowsHelp()
            {
                // Arrange
                string[] args = new[] { "--help" };

                // Act
                parser.Parse(args, settings);
                var output = outWriter.ToString();

                // Assert
                AssertHelper.NormalizedAreEqual(@"
Usage: toofz.Services.Tests.dll [options]

options:
  --help            Shows usage information.
  --interval=VALUE  
  --delay=VALUE     
", output);
            }

            [TestMethod]
            public void Help_Returns0()
            {
                // Arrange
                string[] args = new[] { "--help" };

                // Act
                var exitCode = parser.Parse(args, settings);

                // Assert
                Assert.AreEqual(0, exitCode);
            }

            [TestMethod]
            public void IntervalIsNotSpecified_DoesNotSetUpdateInterval()
            {
                // Arrange
                string[] args = new string[0];
                mockSettings.SetupProperty(settings => settings.UpdateInterval);

                // Act
                parser.Parse(args, settings);

                // Assert
                mockSettings.VerifySet(settings => settings.UpdateInterval = It.IsAny<TimeSpan>(), Times.Never);
            }

            [TestMethod]
            public void IntervalIsSpecified_SetsUpdateIntervalToInterval()
            {
                // Arrange
                string[] args = new[] { "--interval=00:10:00" };
                var interval = TimeSpan.FromSeconds(20);
                mockSettings.SetupProperty(settings => settings.UpdateInterval, interval);

                // Act
                parser.Parse(args, settings);

                // Assert
                Assert.AreEqual(TimeSpan.FromMinutes(10), settings.UpdateInterval);
            }

            [TestMethod]
            public void DelayIsNotSpecified_DoesNotSetDelayBeforeGC()
            {
                // Arrange
                string[] args = new string[0];
                mockSettings.SetupProperty(settings => settings.DelayBeforeGC);

                // Act
                parser.Parse(args, settings);

                // Assert
                mockSettings.VerifySet(settings => settings.DelayBeforeGC = It.IsAny<TimeSpan>(), Times.Never);
            }

            [TestMethod]
            public void DelayIsSpecified_SetsDelayBeforeGCToDelay()
            {
                // Arrange
                string[] args = new[] { "--delay=00:10:00" };
                var delay = TimeSpan.FromSeconds(20);
                mockSettings.SetupProperty(settings => settings.DelayBeforeGC, delay);

                // Act
                parser.Parse(args, settings);

                // Assert
                Assert.AreEqual(TimeSpan.FromMinutes(10), settings.DelayBeforeGC);
            }

            [TestMethod]
            public void SavesSettings()
            {
                // Arrange
                string[] args = new string[0];

                // Act
                parser.Parse(args, settings);

                // Assert
                mockSettings.Verify(s => s.Save());
            }

            [TestMethod]
            public void Returns0()
            {
                // Arrange
                string[] args = new string[0];

                // Act
                var exitCode = parser.Parse(args, settings);

                // Assert
                Assert.AreEqual(0, exitCode);
            }
        }
    }
}
