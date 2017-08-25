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
        public class GetDescription
        {
            [TestMethod]
            public void TypeIsNull_ThrowsArgumentNullException()
            {
                // Arrange
                Type type = null;
                string name = nameof(SimpleSettings.UpdateInterval);

                // Act -> Assert
                Assert.ThrowsException<ArgumentNullException>(() =>
                {
                    StubArgsParser.PublicGetDescription(type, name);
                });
            }

            [TestMethod]
            public void NameIsNull_ThrowsArgumentNullException()
            {
                // Arrange
                Type type = typeof(SimpleSettings);
                string name = null;

                // Act -> Assert
                Assert.ThrowsException<ArgumentNullException>(() =>
                {
                    StubArgsParser.PublicGetDescription(type, name);
                });
            }

            [TestMethod]
            public void PropertyDoesNotExist_ThrowsArgumentNullException()
            {
                // Arrange
                Type type = typeof(SimpleSettings);
                string name = "!";

                // Act
                Assert.ThrowsException<ArgumentNullException>(() =>
                {
                    StubArgsParser.PublicGetDescription(type, name);
                });
            }

            [TestMethod]
            public void NullDescription_ReturnsNull()
            {
                // Arrange
                Type type = typeof(SimpleSettings);
                string name = nameof(SimpleSettings.NullDescription);

                // Act
                var description = StubArgsParser.PublicGetDescription(type, name);

                // Assert
                Assert.IsNull(description);
            }

            [TestMethod]
            public void MissingSettingsDescriptionAttribute_ReturnsNull()
            {
                // Arrange
                Type type = typeof(SimpleSettings);
                string name = nameof(SimpleSettings.MissingSettingsDescriptionAttribute);

                // Act
                var description = StubArgsParser.PublicGetDescription(type, name);

                // Assert
                Assert.IsNull(description);
            }
        }

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
        public class InReader
        {
            public InReader()
            {
                inReader = mockInReader.Object;
                parser = new StubArgsParser(inReader, outWriter, errorWriter);
            }

            Mock<TextReader> mockInReader = new Mock<TextReader>(MockBehavior.Strict);
            TextReader inReader;
            TextWriter outWriter = new StringWriter();
            TextWriter errorWriter = new StringWriter();
            StubArgsParser parser;

            [TestMethod]
            public void ReturnsTextReader()
            {
                // Arrange -> Act
                var reader = parser.PublicInReader;

                // Assert
                Assert.IsInstanceOfType(reader, typeof(TextReader));
            }
        }

        [TestClass]
        public class OutWriter
        {
            public OutWriter()
            {
                inReader = mockInReader.Object;
                parser = new StubArgsParser(inReader, outWriter, errorWriter);
            }

            Mock<TextReader> mockInReader = new Mock<TextReader>(MockBehavior.Strict);
            TextReader inReader;
            TextWriter outWriter = new StringWriter();
            TextWriter errorWriter = new StringWriter();
            StubArgsParser parser;

            [TestMethod]
            public void ReturnsTextWriter()
            {
                // Arrange -> Act
                var writer = parser.PublicOutWriter;

                // Assert
                Assert.IsInstanceOfType(writer, typeof(TextWriter));
            }
        }

        [TestClass]
        public class ErrorWriter
        {
            public ErrorWriter()
            {
                inReader = mockInReader.Object;
                parser = new StubArgsParser(inReader, outWriter, errorWriter);
            }

            Mock<TextReader> mockInReader = new Mock<TextReader>(MockBehavior.Strict);
            TextReader inReader;
            TextWriter outWriter = new StringWriter();
            TextWriter errorWriter = new StringWriter();
            StubArgsParser parser;

            [TestMethod]
            public void ReturnsTextWriter()
            {
                // Arrange -> Act
                var writer = parser.PublicErrorWriter;

                // Assert
                Assert.IsInstanceOfType(writer, typeof(TextWriter));
            }
        }

        [TestClass]
        public class Parse
        {
            public Parse()
            {
                inReader = mockInReader.Object;
                parser = new StubArgsParser(inReader, outWriter, errorWriter);
            }

            Mock<TextReader> mockInReader = new Mock<TextReader>(MockBehavior.Strict);
            TextReader inReader;
            TextWriter outWriter = new StringWriter();
            TextWriter errorWriter = new StringWriter();
            StubArgsParser parser;

            [TestMethod]
            public void ArgsIsNull_ThrowsArgumentNullException()
            {
                // Arrange
                string[] args = null;
                ISettings settings = new SimpleSettings();

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
                ISettings settings = new SimpleSettings();

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
                ISettings settings = new SimpleSettings();

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
                ISettings settings = new SimpleSettings();

                // Act
                parser.Parse(args, settings);
                var output = outWriter.ToString();

                // Assert
                AssertHelper.NormalizedAreEqual(@"
Usage: toofz.Services.Tests.dll [options]

options:
  --help            Shows usage information.
  --interval=VALUE  The minimum amount of time that should pass between each cycle.
  --delay=VALUE     The amount of time to wait after a cycle to perform garbage collection.
  --ikey=VALUE      An Application Insights instrumentation key.
", output);
            }

            [TestMethod]
            public void Help_Returns0()
            {
                // Arrange
                string[] args = new[] { "--help" };
                ISettings settings = new SimpleSettings();

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
                var mockSettings = new Mock<ISettings>();
                mockSettings.SetupProperty(s => s.UpdateInterval);

                // Act
                parser.Parse(args, mockSettings.Object);

                // Assert
                mockSettings.VerifySet(s => s.UpdateInterval = It.IsAny<TimeSpan>(), Times.Never);
            }

            [TestMethod]
            public void IntervalIsSpecified_SetsUpdateIntervalToInterval()
            {
                // Arrange
                string[] args = new[] { "--interval=00:10:00" };
                var mockSettings = new Mock<ISettings>();
                mockSettings.SetupProperty(s => s.UpdateInterval);

                // Act
                parser.Parse(args, mockSettings.Object);

                // Assert
                mockSettings.VerifySet(s => s.UpdateInterval = TimeSpan.FromMinutes(10));
            }

            [TestMethod]
            public void DelayIsNotSpecified_DoesNotSetDelayBeforeGC()
            {
                // Arrange
                string[] args = new string[0];
                var mockSettings = new Mock<ISettings>();
                mockSettings.SetupProperty(s => s.DelayBeforeGC);

                // Act
                parser.Parse(args, mockSettings.Object);

                // Assert
                mockSettings.VerifySet(settings => settings.DelayBeforeGC = It.IsAny<TimeSpan>(), Times.Never);
            }

            [TestMethod]
            public void DelayIsSpecified_SetsDelayBeforeGCToDelay()
            {
                // Arrange
                string[] args = new[] { "--delay=00:10:00" };
                var mockSettings = new Mock<ISettings>();
                mockSettings.SetupProperty(s => s.DelayBeforeGC);

                // Act
                parser.Parse(args, mockSettings.Object);

                // Assert
                mockSettings.VerifySet(s => s.DelayBeforeGC = TimeSpan.FromMinutes(10));
            }

            [TestMethod]
            public void IkeyIsNotSpecified_DoesNotSetInstrumentationKey()
            {
                // Arrange
                string[] args = new string[0];
                var mockSettings = new Mock<ISettings>();
                mockSettings.SetupProperty(s => s.InstrumentationKey);

                // Act
                parser.Parse(args, mockSettings.Object);

                // Assert
                mockSettings.VerifySet(s => s.InstrumentationKey = It.IsAny<string>(), Times.Never);
            }

            [TestMethod]
            public void IkeyIsSpecified_SetsInstrumentationKeyToIkey()
            {
                // Arrange
                string[] args = new[] { "--ikey=myInstrumentationKey" };
                var mockSettings = new Mock<ISettings>();
                mockSettings.SetupProperty(s => s.InstrumentationKey);

                // Act
                parser.Parse(args, mockSettings.Object);

                // Assert
                mockSettings.VerifySet(s => s.InstrumentationKey = "myInstrumentationKey");
            }

            [TestMethod]
            public void SavesSettings()
            {
                // Arrange
                string[] args = new string[0];
                var mockSettings = new Mock<ISettings>();

                // Act
                parser.Parse(args, mockSettings.Object);

                // Assert
                mockSettings.Verify(s => s.Save());
            }

            [TestMethod]
            public void Returns0()
            {
                // Arrange
                string[] args = new string[0];
                ISettings settings = new SimpleSettings();

                // Act
                var exitCode = parser.Parse(args, settings);

                // Assert
                Assert.AreEqual(0, exitCode);
            }
        }
    }
}
