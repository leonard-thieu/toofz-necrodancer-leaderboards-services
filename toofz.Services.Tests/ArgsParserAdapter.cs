using System;
using System.IO;
using Mono.Options;

namespace toofz.Services.Tests
{
    sealed class ArgsParserAdapter : ArgsParser<Options, ISettings>
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

        public TextReader PublicInReader { get => InReader; }
        public TextWriter PublicOutWriter { get => OutWriter; }
        public TextWriter PublicErrorWriter { get => ErrorWriter; }

        protected override string EntryAssemblyFileName { get; } = Path.GetFileName(typeof(ArgsParserAdapter).Assembly.Location);

        protected override void OnParsed(Options options, ISettings settings)
        {
            base.OnParsed(options, settings);
        }

        protected override void OnParsing(Type settingsType, OptionSet optionSet, Options options)
        {
            base.OnParsing(settingsType, optionSet, options);
        }

        public string PublicReadOption(string prompt)
        {
            return ReadOption(prompt);
        }
    }
}
