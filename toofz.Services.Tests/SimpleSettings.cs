using System;
using System.Configuration;

namespace toofz.Services.Tests
{
    sealed class SimpleSettings : ISettings
    {
        [SettingsDescription("The minimum amount of time that should pass between each cycle.")]
        public TimeSpan UpdateInterval { get; set; }
        [SettingsDescription("The amount of time to wait after a cycle to perform garbage collection.")]
        public TimeSpan DelayBeforeGC { get; set; }
        [SettingsDescription(null)]
        public int NullDescription { get; set; }
        public int MissingSettingsDescriptionAttribute { get; set; }

        public void Reload()
        {
            UpdateInterval = default(TimeSpan);
            DelayBeforeGC = default(TimeSpan);
        }

        public void Save() { }
    }
}
