using System;

namespace toofz.Services
{
    public interface ISettings
    {
        /// <summary>
        /// The minimum amount of time that should pass between the start of each cycle.
        /// </summary>
        TimeSpan UpdateInterval { get; set; }
        /// <summary>
        /// The amount of time to wait after a cycle to perform garbage collection.
        /// </summary>
        TimeSpan DelayBeforeGC { get; set; }
        /// <summary>
        /// An Application Insights instrumentation key.
        /// </summary>
        string InstrumentationKey { get; set; }
        /// <summary>
        /// The number of rounds to execute a key derivation function.
        /// </summary>
        int KeyDerivationIterations { get; set; }

        /// <summary>
        /// Refreshes the application settings property values from persistent storage.
        /// </summary>
        void Reload();
        /// <summary>
        /// Stores the current values of the application settings properties.
        /// </summary>
        void Save();
    }
}
