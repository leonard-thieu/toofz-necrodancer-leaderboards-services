using System;
using System.IO;
using Mono.Options;
using Moq;
using Xunit;

namespace toofz.Services.Tests
{
    public class ArgsParserTests
    {
        public ArgsParserTests()
        {
            inReader = mockInReader.Object;
            parser = new ArgsParserAdapter(inReader, outWriter, errorWriter);
        }

        private Mock<TextReader> mockInReader = new Mock<TextReader>(MockBehavior.Strict);
        private TextReader inReader;
        private TextWriter outWriter = new StringWriter();
        private TextWriter errorWriter = new StringWriter();
        private ArgsParserAdapter parser;

        public class GetDescriptionMethod
        {
            [Fact]
            public void TypeIsNull_ThrowsArgumentNullException()
            {
                // Arrange
                Type type = null;
                string name = nameof(StubSettings.UpdateInterval);

                // Act -> Assert
                Assert.Throws<ArgumentNullException>(() =>
                {
                    ArgsParserAdapter.PublicGetDescription(type, name);
                });
            }

            [Fact]
            public void NameIsNull_ThrowsArgumentNullException()
            {
                // Arrange
                Type type = typeof(StubSettings);
                string name = null;

                // Act -> Assert
                Assert.Throws<ArgumentNullException>(() =>
                {
                    ArgsParserAdapter.PublicGetDescription(type, name);
                });
            }

            [Fact]
            public void PropertyDoesNotExist_ThrowsArgumentNullException()
            {
                // Arrange
                Type type = typeof(StubSettings);
                string name = "!";

                // Act
                Assert.Throws<ArgumentNullException>(() =>
                {
                    ArgsParserAdapter.PublicGetDescription(type, name);
                });
            }

            [Fact]
            public void NullDescription_ReturnsNull()
            {
                // Arrange
                Type type = typeof(StubSettings);
                string name = nameof(StubSettings.NullDescription);

                // Act
                var description = ArgsParserAdapter.PublicGetDescription(type, name);

                // Assert
                Assert.Null(description);
            }

            [Fact]
            public void MissingSettingsDescriptionAttribute_ReturnsNull()
            {
                // Arrange
                Type type = typeof(StubSettings);
                string name = nameof(StubSettings.MissingSettingsDescriptionAttribute);

                // Act
                var description = ArgsParserAdapter.PublicGetDescription(type, name);

                // Assert
                Assert.Null(description);
            }
        }

        public class ShouldPromptForRequiredSettingMethod
        {
            [Fact]
            public void OptionIsNull_ReturnsTrue()
            {
                // Arrange
                string option = null;
                object setting = new object();

                // Act
                var shouldPrompt = ArgsParserAdapter.PublicShouldPromptForRequiredSetting(option, setting);

                // Assert
                Assert.True(shouldPrompt);
            }

            [Fact]
            public void OptionIsEmptyAndSettingIsNull_ReturnsTrue()
            {
                // Arrange
                string option = "";
                object setting = null;

                // Act
                var shouldPrompt = ArgsParserAdapter.PublicShouldPromptForRequiredSetting(option, setting);

                // Assert
                Assert.True(shouldPrompt);
            }

            [Fact]
            public void OptionIsEmptyAndSettingIsNotNull_ReturnsFalse()
            {
                // Arrange
                string option = "";
                object setting = new object();

                // Act
                var shouldPrompt = ArgsParserAdapter.PublicShouldPromptForRequiredSetting(option, setting);

                // Assert
                Assert.False(shouldPrompt);
            }

            [Fact]
            public void OptionIsNotNullOrEmpty_ReturnsFalse()
            {
                // Arrange
                string option = "not empty";
                object setting = new object();

                // Act
                var shouldPrompt = ArgsParserAdapter.PublicShouldPromptForRequiredSetting(option, setting);

                // Assert
                Assert.False(shouldPrompt);
            }
        }

        public class Constructor
        {
            [Fact]
            public void InReaderIsNull_ThrowsArgumentNullException()
            {
                // Arrange
                TextReader inReader = null;
                TextWriter outWriter = TextWriter.Null;
                TextWriter errorWriter = TextWriter.Null;

                // Act -> Assert
                Assert.Throws<ArgumentNullException>(() =>
                {
                    new ArgsParserAdapter(inReader, outWriter, errorWriter);
                });
            }

            [Fact]
            public void OutWriterIsNull_ThrowsArgumentNullException()
            {
                // Arrange
                TextReader inReader = TextReader.Null;
                TextWriter outWriter = null;
                TextWriter errorWriter = TextWriter.Null;

                // Act -> Assert
                Assert.Throws<ArgumentNullException>(() =>
                {
                    new ArgsParserAdapter(inReader, outWriter, errorWriter);
                });
            }

            [Fact]
            public void ErrorWriterIsNull_ThrowsArgumentNullException()
            {
                // Arrange
                TextReader inReader = TextReader.Null;
                TextWriter outWriter = TextWriter.Null;
                TextWriter errorWriter = null;

                // Act -> Assert
                Assert.Throws<ArgumentNullException>(() =>
                {
                    new ArgsParserAdapter(inReader, outWriter, errorWriter);
                });
            }

            [Fact]
            public void ReturnsInstance()
            {
                // Arrange
                TextReader inReader = TextReader.Null;
                TextWriter outWriter = TextWriter.Null;
                TextWriter errorWriter = TextWriter.Null;

                // Act
                var parser = new ArgsParserAdapter(inReader, outWriter, errorWriter);

                // Assert
                Assert.IsAssignableFrom<ArgsParserAdapter>(parser);
            }
        }

        public class InReaderProperty : ArgsParserTests
        {
            [Fact]
            public void ReturnsTextReader()
            {
                // Arrange -> Act
                var reader = parser.PublicInReader;

                // Assert
                Assert.IsAssignableFrom<TextReader>(reader);
            }
        }

        public class OutWriterProperty : ArgsParserTests
        {
            [Fact]
            public void ReturnsTextWriter()
            {
                // Arrange -> Act
                var writer = parser.PublicOutWriter;

                // Assert
                Assert.IsAssignableFrom<TextWriter>(writer);
            }
        }

        public class ErrorWriterProperty : ArgsParserTests
        {
            [Fact]
            public void ReturnsTextWriter()
            {
                // Arrange -> Act
                var writer = parser.PublicErrorWriter;

                // Assert
                Assert.IsAssignableFrom<TextWriter>(writer);
            }
        }

        public class ParseMethod : ArgsParserTests
        {
            public ParseMethod()
            {
                settings = mockSettings.Object;
                mockSettings.SetupAllProperties();
                mockSettings.SetupProperty(s => s.KeyDerivationIterations, 1);
                mockSettings.SetupProperty(s => s.LeaderboardsConnectionString, new EncryptedSecret("a", 1));
            }

            private readonly Mock<ISettings> mockSettings = new Mock<ISettings>();
            private readonly ISettings settings;

            [Fact]
            public void ArgsIsNull_ThrowsArgumentNullException()
            {
                // Arrange
                string[] args = null;

                // Act -> Assert
                Assert.Throws<ArgumentNullException>(() =>
                {
                    parser.Parse(args, settings);
                });
            }

            [Fact]
            public void SettingsIsNull_ThrowsArgumentNullException()
            {
                // Arrange
                string[] args = { };
                ISettings settings = null;

                // Act -> Assert
                Assert.Throws<ArgumentNullException>(() =>
                {
                    parser.Parse(args, settings);
                });
            }

            [Fact]
            public void ExtraArg_ShowsError()
            {
                // Arrange
                string[] args = { "myExtraArg" };

                // Act
                parser.Parse(args, settings);
                var error = errorWriter.ToString();

                // Assert
                Assert.Equal(@"toofz.Services.Tests.dll: 'myExtraArg' is not a valid option.
", error, ignoreLineEndingDifferences: true);
            }

            [Fact]
            public void ExtraArg_Returns1()
            {
                // Arrange
                string[] args = { "myExtraArg" };

                // Act
                var exitCode = parser.Parse(args, settings);

                // Assert
                Assert.Equal(1, exitCode);
            }

            [Fact]
            public void Help_ShowsHelp()
            {
                // Arrange
                string[] args = { "--help" };
                var settings = new StubSettings();

                // Act
                parser.Parse(args, settings);
                var output = outWriter.ToString();

                // Assert
                Assert.Equal(@"
Usage: toofz.Services.Tests.dll [options]

options:
  --help                Shows usage information.
  --interval=VALUE      The minimum amount of time that should pass between each cycle.
  --delay=VALUE         The amount of time to wait after a cycle to perform garbage collection.
  --ikey=VALUE          An Application Insights instrumentation key.
  --iterations=VALUE    The number of rounds to execute a key derivation function.
  --connection[=VALUE]  The connection string used to connect to the leaderboards database.
  --optional[=VALUE]    This option is optional.
", output, ignoreLineEndingDifferences: true);
            }

            [Fact]
            public void Help_Returns0()
            {
                // Arrange
                string[] args = { "--help" };

                // Act
                var exitCode = parser.Parse(args, settings);

                // Assert
                Assert.Equal(0, exitCode);
            }

            #region UpdateInterval

            [Fact]
            public void IntervalIsNotSpecified_DoesNotSetUpdateInterval()
            {
                // Arrange
                string[] args = { };

                // Act
                parser.Parse(args, mockSettings.Object);

                // Assert
                mockSettings.VerifySet(s => s.UpdateInterval = It.IsAny<TimeSpan>(), Times.Never);
            }

            [Fact]
            public void IntervalIsSpecified_SetsUpdateIntervalToInterval()
            {
                // Arrange
                string[] args = { "--interval=00:10:00" };

                // Act
                parser.Parse(args, mockSettings.Object);

                // Assert
                mockSettings.VerifySet(s => s.UpdateInterval = TimeSpan.FromMinutes(10));
            }

            #endregion

            #region DelayBeforeGC

            [Fact]
            public void DelayIsNotSpecified_DoesNotSetDelayBeforeGC()
            {
                // Arrange
                string[] args = { };

                // Act
                parser.Parse(args, mockSettings.Object);

                // Assert
                mockSettings.VerifySet(settings => settings.DelayBeforeGC = It.IsAny<TimeSpan>(), Times.Never);
            }

            [Fact]
            public void DelayIsSpecified_SetsDelayBeforeGCToDelay()
            {
                // Arrange
                string[] args = { "--delay=00:10:00" };

                // Act
                parser.Parse(args, mockSettings.Object);

                // Assert
                mockSettings.VerifySet(s => s.DelayBeforeGC = TimeSpan.FromMinutes(10));
            }

            #endregion

            #region InstrumentationKey

            [Fact]
            public void IkeyIsNotSpecified_DoesNotSetInstrumentationKey()
            {
                // Arrange
                string[] args = { };

                // Act
                parser.Parse(args, mockSettings.Object);

                // Assert
                mockSettings.VerifySet(s => s.InstrumentationKey = It.IsAny<string>(), Times.Never);
            }

            [Fact]
            public void IkeyIsSpecified_SetsInstrumentationKeyToIkey()
            {
                // Arrange
                string[] args = { "--ikey=myInstrumentationKey" };

                // Act
                parser.Parse(args, mockSettings.Object);

                // Assert
                mockSettings.VerifySet(s => s.InstrumentationKey = "myInstrumentationKey");
            }

            #endregion

            #region KeyDerivationIterations

            [Fact]
            public void IterationsIsNotSpecified_DoesNotSetKeyDerivationIterations()
            {
                // Arrange
                string[] args = { };

                // Act
                parser.Parse(args, mockSettings.Object);

                // Assert
                mockSettings.VerifySet(s => s.KeyDerivationIterations = It.IsAny<int>(), Times.Never);
            }

            [Fact]
            public void IterationsIsSpecified_SetsKeyDerivationIterationsToIterations()
            {
                // Arrange
                string[] args = { "--iterations=20000" };

                // Act
                parser.Parse(args, mockSettings.Object);

                // Assert
                mockSettings.VerifySet(s => s.KeyDerivationIterations = 20000);
            }

            #endregion

            #region LeaderboardsConnectionString

            [Fact]
            public void ConnectionIsSpecified_SetsLeaderboardsConnectionString()
            {
                // Arrange
                string[] args = { "--connection=myConnectionString" };

                // Act
                parser.Parse(args, settings);

                // Assert
                var encrypted = new EncryptedSecret("myConnectionString", 1);
                Assert.Equal(encrypted.Decrypt(), settings.LeaderboardsConnectionString.Decrypt());
            }

            [Fact]
            public void ConnectionFlagIsSpecified_PromptsUserForConnectionAndSetsLeaderboardsConnectionString()
            {
                // Arrange
                string[] args = { "--connection" };
                mockInReader
                    .SetupSequence(r => r.ReadLine())
                    .Returns("myConnectionString");

                // Act
                parser.Parse(args, settings);

                // Assert
                var encrypted = new EncryptedSecret("myConnectionString", 1);
                Assert.Equal(encrypted.Decrypt(), settings.LeaderboardsConnectionString.Decrypt());
            }

            [Fact]
            public void ConnectionFlagIsNotSpecifiedAndLeaderboardsConnectionStringIsNotSet_SetsLeaderboardsConnectionStringToDefault()
            {
                // Arrange
                string[] args = { };
                settings.LeaderboardsConnectionString = null;

                // Act
                parser.Parse(args, settings);

                // Assert
                var encrypted = new EncryptedSecret(ArgsParser<Options, ISettings>.DefaultLeaderboardsConnectionString, 1);
                Assert.Equal(encrypted.Decrypt(), settings.LeaderboardsConnectionString.Decrypt());
            }

            [Fact]
            public void ConnectionFlagIsNotSpecifiedAndLeaderboardsConnectionStringIsSet_DoesNotSetLeaderboardsConnectionString()
            {
                // Arrange
                string[] args = { };
                mockSettings.SetupProperty(s => s.LeaderboardsConnectionString, new EncryptedSecret("a", 1));
                var settings = mockSettings.Object;

                // Act
                parser.Parse(args, settings);

                // Assert
                mockSettings.VerifySet(s => s.LeaderboardsConnectionString = It.IsAny<EncryptedSecret>(), Times.Never);
            }

            #endregion

            [Fact]
            public void SavesSettings()
            {
                // Arrange
                string[] args = { };

                // Act
                parser.Parse(args, mockSettings.Object);

                // Assert
                mockSettings.Verify(s => s.Save());
            }

            [Fact]
            public void Returns0()
            {
                // Arrange
                string[] args = { };

                // Act
                var exitCode = parser.Parse(args, settings);

                // Assert
                Assert.Equal(0, exitCode);
            }
        }

        public class ReadOptionMethod : ArgsParserTests
        {
            [Fact]
            public void ReadsOptionAndOptionIsNotNullOrEmpty_ReturnsOption()
            {
                // Arrange
                string prompt = "Value";
                mockInReader
                    .SetupSequence(r => r.ReadLine())
                    .Returns("value");

                // Act
                var option = parser.PublicReadOption(prompt);

                // Assert
                Assert.Equal("value", option);
            }

            [Fact]
            public void ReadsOptionAndOptionIsNull_PromptsAgain()
            {
                // Arrange
                string prompt = "Value";
                mockInReader
                    .SetupSequence(r => r.ReadLine())
                    .Returns(null)
                    .Returns("value");

                // Act
                var option = parser.PublicReadOption(prompt);

                // Assert
                Assert.Equal("value", option);
            }

            [Fact]
            public void ReadsOptionAndOptionIsEmpty_PromptsAgain()
            {
                // Arrange
                string prompt = "Value";
                mockInReader
                    .SetupSequence(r => r.ReadLine())
                    .Returns("")
                    .Returns("value");

                // Act
                var option = parser.PublicReadOption(prompt);

                // Assert
                Assert.Equal("value", option);
            }
        }

        private class ArgsParserAdapter : ArgsParser<Options, ISettings>
        {
            public static string PublicGetDescription(Type type, string propName)
            {
                return GetDescription(type, propName);
            }

            public static bool PublicShouldPromptForRequiredSetting(string option, object setting)
            {
                return ShouldPromptForRequiredSetting(option, setting);
            }

            public ArgsParserAdapter(TextReader inReader, TextWriter outWriter, TextWriter errorWriter) : base(inReader, outWriter, errorWriter) { }

            public TextReader PublicInReader => InReader;
            public TextWriter PublicOutWriter => OutWriter;
            public TextWriter PublicErrorWriter => ErrorWriter;

            protected override string EntryAssemblyFileName { get; } = Path.GetFileName(typeof(ArgsParserAdapter).Assembly.Location);

            protected override void OnParsing(Type settingsType, OptionSet optionSet, Options options)
            {
                base.OnParsing(settingsType, optionSet, options);

                optionSet.Add("optional:", "This option is optional.", optional => { });
            }

            protected override void OnParsed(Options options, ISettings settings)
            {
                base.OnParsed(options, settings);
            }

            public string PublicReadOption(string prompt)
            {
                return ReadOption(prompt);
            }
        }
    }
}
