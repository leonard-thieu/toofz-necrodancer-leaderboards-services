using System;
using System.IO;
using toofz.NecroDancer.Leaderboards.Services.Tests.Properties;

namespace toofz.NecroDancer.Leaderboards.Services.Tests
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
