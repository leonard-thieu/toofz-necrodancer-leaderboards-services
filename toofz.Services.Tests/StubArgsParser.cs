using System.IO;
using Mono.Options;

namespace toofz.Services.Tests
{
    sealed class StubArgsParser : ArgsParser<ISettings>
    {
        public StubArgsParser(TextReader inReader, TextWriter outWriter, TextWriter errorWriter) : base(inReader, outWriter, errorWriter) { }

        protected override string EntryAssemblyFileName => Path.GetFileName(typeof(StubArgsParser).Assembly.Location);

        protected override void OnParsed(ISettings settings) { }

        protected override void OnParsing(OptionSet options) { }
    }
}
