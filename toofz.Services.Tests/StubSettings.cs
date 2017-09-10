using System;
using System.Configuration;

namespace toofz.Services.Tests
{
    sealed class StubSettings : ISettings
    {
        [SettingsDescription("The minimum amount of time that should pass between each cycle.")]
        public TimeSpan UpdateInterval { get; set; }
        [SettingsDescription("The amount of time to wait after a cycle to perform garbage collection.")]
        public TimeSpan DelayBeforeGC { get; set; }
        [SettingsDescription("An Application Insights instrumentation key.")]
        public string InstrumentationKey { get; set; }
        [SettingsDescription("The number of rounds to execute a key derivation function.")]
        public int KeyDerivationIterations { get; set; }
        [SettingsDescription(null)]
        public int NullDescription { get; set; }
        public int MissingSettingsDescriptionAttribute { get; set; }

        public void Reload() { }

        public void Save() { }
    }
}
