using System;
using System.IO;
using toofz.Services.Tests.Properties;

namespace toofz.Services.Tests
{
    sealed class MockArgsParser : ArgsParser<Settings>
    {
        public MockArgsParser(TextReader inReader, TextWriter outWriter, TextWriter errorWriter) : base(inReader, outWriter, errorWriter) { }

        public override int Parse(string[] args, Settings settings)
        {
            throw new NotImplementedException();
        }
    }
}
