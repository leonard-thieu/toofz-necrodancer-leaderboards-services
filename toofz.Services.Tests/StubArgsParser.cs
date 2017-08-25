using System;
using System.IO;
using Mono.Options;

namespace toofz.Services.Tests
{
    sealed class StubArgsParser : ArgsParser<Options, ISettings>
    {
        public StubArgsParser(TextReader inReader, TextWriter outWriter, TextWriter errorWriter) : base(inReader, outWriter, errorWriter) { }

        protected override string EntryAssemblyFileName { get; } = Path.GetFileName(typeof(StubArgsParser).Assembly.Location);

        protected override void OnParsed(Options options, ISettings settings)
        {
            base.OnParsed(options, settings);
        }

        protected override void OnParsing(Type settingsType, OptionSet optionSet, Options options)
        {
            base.OnParsing(settingsType, optionSet, options);
        }
    }
}
