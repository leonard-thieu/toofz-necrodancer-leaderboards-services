using System;

namespace toofz.Services
{
    public class Options
    {
        public bool ShowHelp { get; internal set; }
        public TimeSpan? UpdateInterval { get; internal set; }
        public TimeSpan? DelayBeforeGC { get; internal set; }
        public string InstrumentationKey { get; internal set; }
    }
}
