using System;
using System.IO;

namespace toofz.NecroDancer.Leaderboards.Services.Tests
{
    sealed class MockArgsParser : ArgsParser
    {
        public MockArgsParser(TextReader inReader, TextWriter outWriter, TextWriter errorWriter) :
            base(inReader, outWriter, errorWriter)
        {

        }

        public override int Parse<TSettings>(string[] args, TSettings settings)
        {
            throw new NotImplementedException();
        }
    }
}
