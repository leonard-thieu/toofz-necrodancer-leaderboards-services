using System;
using System.Configuration;
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
            [DisplayFact(nameof(ArgumentNullException))]
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

            [DisplayFact(nameof(ArgumentNullException))]
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

            [DisplayFact(nameof(ArgumentNullException))]
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

            [DisplayFact]
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

            [DisplayFact(nameof(SettingsDescriptionAttribute))]
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
            [DisplayFact]
            public void OptionIsNull_ReturnsTrue()
            {
                // Arrange
                string option = null;

                // Act
                var shouldPrompt = ArgsParserAdapter.PublicShouldPrompt(option);

                // Assert
                Assert.True(shouldPrompt);
            }

            [DisplayFact]
            public void OptionIsNotNull_ReturnsFalse()
            {
                // Arrange
                string option = "not empty";

                // Act
                var shouldPrompt = ArgsParserAdapter.PublicShouldPrompt(option);

                // Assert
                Assert.False(shouldPrompt);
            }
        }

        public class Constructor
        {
            [DisplayFact("InReader", nameof(ArgumentNullException))]
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

            [DisplayFact("OutWriter", nameof(ArgumentNullException))]
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

            [DisplayFact("ErrorWriter", nameof(ArgumentNullException))]
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

            [DisplayFact(nameof(ArgsParser<Options, ISettings>))]
            public void ReturnsArgsParser()
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
            [DisplayFact(nameof(TextReader))]
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
            [DisplayFact(nameof(TextWriter))]
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
            [DisplayFact(nameof(TextWriter))]
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

            [DisplayFact(nameof(ArgumentNullException))]
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

            [DisplayFact(nameof(ArgumentNullException))]
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

            [DisplayFact]
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

            [DisplayFact]
            public void ExtraArg_Returns1()
            {
                // Arrange
                string[] args = { "myExtraArg" };

                // Act
                var exitCode = parser.Parse(args, settings);

                // Assert
                Assert.Equal(1, exitCode);
            }

            [DisplayFact]
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

            [DisplayFact]
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

            [DisplayFact(nameof(ISettings.UpdateInterval))]
            public void IntervalIsNotSpecified_DoesNotSetUpdateInterval()
            {
                // Arrange
                string[] args = { };

                // Act
                parser.Parse(args, mockSettings.Object);

                // Assert
                mockSettings.VerifySet(s => s.UpdateInterval = It.IsAny<TimeSpan>(), Times.Never);
            }

            [DisplayFact(nameof(ISettings.UpdateInterval))]
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

            [DisplayFact(nameof(ISettings.DelayBeforeGC))]
            public void DelayIsNotSpecified_DoesNotSetDelayBeforeGC()
            {
                // Arrange
                string[] args = { };

                // Act
                parser.Parse(args, mockSettings.Object);

                // Assert
                mockSettings.VerifySet(settings => settings.DelayBeforeGC = It.IsAny<TimeSpan>(), Times.Never);
            }

            [DisplayFact(nameof(ISettings.DelayBeforeGC))]
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

            [DisplayFact(nameof(ISettings.InstrumentationKey))]
            public void IkeyIsNotSpecified_DoesNotSetInstrumentationKey()
            {
                // Arrange
                string[] args = { };

                // Act
                parser.Parse(args, mockSettings.Object);

                // Assert
                mockSettings.VerifySet(s => s.InstrumentationKey = It.IsAny<string>(), Times.Never);
            }

            [DisplayFact(nameof(ISettings.InstrumentationKey))]
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

            [DisplayFact(nameof(ISettings.KeyDerivationIterations))]
            public void IterationsIsNotSpecified_DoesNotSetKeyDerivationIterations()
            {
                // Arrange
                string[] args = { };

                // Act
                parser.Parse(args, mockSettings.Object);

                // Assert
                mockSettings.VerifySet(s => s.KeyDerivationIterations = It.IsAny<int>(), Times.Never);
            }

            [DisplayFact(nameof(ISettings.KeyDerivationIterations))]
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

            [DisplayFact(nameof(ISettings.LeaderboardsConnectionString))]
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

            [DisplayFact(nameof(ISettings.LeaderboardsConnectionString))]
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

            [DisplayFact(nameof(ISettings.LeaderboardsConnectionString))]
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

            [DisplayFact]
            public void SavesSettings()
            {
                // Arrange
                string[] args = { };

                // Act
                parser.Parse(args, mockSettings.Object);

                // Assert
                mockSettings.Verify(s => s.Save());
            }

            [DisplayFact]
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
            [DisplayFact]
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

            [DisplayFact]
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

            [DisplayFact]
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

            public static bool PublicShouldPrompt<TOption>(TOption option)
            {
                return ShouldPrompt(option);
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

        private sealed class StubSettings : ISettings
        {
            [SettingsDescription("The minimum amount of time that should pass between each cycle.")]
            public TimeSpan UpdateInterval { get; set; }
            [SettingsDescription("The amount of time to wait after a cycle to perform garbage collection.")]
            public TimeSpan DelayBeforeGC { get; set; }
            [SettingsDescription("An Application Insights instrumentation key.")]
            public string InstrumentationKey { get; set; }
            [SettingsDescription("The number of rounds to execute a key derivation function.")]
            public int KeyDerivationIterations { get; set; }
            [SettingsDescription("The connection string used to connect to the leaderboards database.")]
            public EncryptedSecret LeaderboardsConnectionString { get; set; }
            [SettingsDescription(null)]
            public int NullDescription { get; set; }
            public int MissingSettingsDescriptionAttribute { get; set; }

            public void Reload() { }

            public void Save() { }
        }
    }
}
