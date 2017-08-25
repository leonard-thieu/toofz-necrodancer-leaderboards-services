using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace toofz.NecroDancer.Leaderboards.Services.Tests
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
                    new MockArgsParser(inReader, outWriter, errorWriter);
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
                    new MockArgsParser(inReader, outWriter, errorWriter);
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
                    new MockArgsParser(inReader, outWriter, errorWriter);
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
                var parser = new MockArgsParser(inReader, outWriter, errorWriter);

                // Assert
                Assert.IsInstanceOfType(parser, typeof(ArgsParser));
            }
        }
    }
}
