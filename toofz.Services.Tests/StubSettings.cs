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

        public void Save() { }
    }
}
